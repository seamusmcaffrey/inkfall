# Asset Library Indexer — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a metadata database of all downloaded Unity Asset Store packages — extractable without importing into any project — producing a searchable JSON catalog with thumbnails, color palettes, style tags, and CLIP/DINOv2 embeddings for semantic search and style-aware filtering.

**Architecture:** A Python pipeline (no Unity required) that cracks open `.unitypackage` tarballs, extracts metadata from `.meta`/`.mat`/`.prefab` YAML files and embedded `preview.png` thumbnails, then runs local ML models (CLIP, DINOv2) for embeddings and style clustering. Output is a portable `asset-catalog.json` + thumbnails folder + FAISS vector index. A follow-up Unity Editor script fills in runtime-only data (vertex counts, audio durations) for assets imported into a project.

**Tech Stack:** Python 3.11+, PyYAML, Pillow, scikit-learn, open_clip (MIT), DINOv2 (Apache 2.0), FAISS, PyTorch (MPS backend for Apple Silicon)

**Cost Boundary:** Phases 1-8 are fully local with zero API costs. Phase 8 estimates what a vision LLM pass would cost and stops for approval.

**Location of everything:** `/Users/seamus/Documents/unity-assets-index/` — standalone project, not tied to any Unity project

**Location of .unitypackage cache:** `~/Library/Unity/Asset Store-5.x/` (26 packages currently downloaded)

---

## Key Discovery: .unitypackage Files Are Tarballs

Each `.unitypackage` is a gzipped tarball with this structure per asset:

```
<guid>/
  pathname      — text file: "Assets/Publisher/Models/thing.fbx"
  asset         — the actual file (fbx, png, mat, prefab, etc.)
  asset.meta    — Unity import settings YAML
  preview.png   — 128x128 publisher-rendered thumbnail (not always present)
```

This means we can extract metadata, thumbnails, and parse YAML-based assets (materials, prefabs, shaders) **without Unity running**. Preview coverage varies by pack (19-85% of assets have thumbnails).

---

## File Structure

Everything lives in `/Users/seamus/Documents/unity-assets-index/`:

```
unity-assets-index/
  requirements.txt          — Python dependencies
  catalog.py                — Shared data model: AssetEntry dataclass, JSON serialization
  extract.py                — Phase 1-2: crack .unitypackage, extract metadata + thumbnails
  parse_materials.py        — Phase 3: deep-parse .mat YAML for colors, shaders, properties
  parse_prefabs.py          — Phase 3: deep-parse .prefab YAML for components, hierarchy
  palette.py                — Phase 4: extract dominant color palettes from thumbnails
  embed.py                  — Phase 5: CLIP + DINOv2 embeddings on thumbnails
  cluster.py                — Phase 6: style clustering + compatibility scoring
  search.py                 — Query interface: text search, similarity, browse by filter
  estimate_api.py           — Phase 8: count assets needing descriptions, estimate cost

  output/                   — All generated data
    catalog.json            — The master catalog
    thumbnails/             — Extracted preview PNGs, named by GUID
    embeddings/             — FAISS index + GUID mapping
    clusters.json           — Style cluster assignments
    cost-estimate.json      — Phase 8 output
```

---

## Phase 1: Project Scaffolding

### Task 1.1: Create directory structure and dependencies

**Files:**
- Create: `requirements.txt`
- Create: `catalog.py`

- [ ] **Step 1: Create the tools directory and requirements.txt**

```
requirements.txt:

PyYAML>=6.0
Pillow>=10.0
scikit-learn>=1.3
numpy>=1.24
```

Note: torch, open_clip, faiss are Phase 5 dependencies — added later to avoid heavy install upfront.

- [ ] **Step 2: Create the shared data model (catalog.py)**

```python
# catalog.py
"""Shared data model for asset catalog entries."""

import json
from dataclasses import dataclass, field, asdict
from pathlib import Path
from typing import Optional


@dataclass
class AssetEntry:
    """Single asset in the catalog."""
    guid: str
    path: str                                  # e.g. "Assets/ithappy/Food_FREE/Meshes/drink_002.fbx"
    name: str                                  # e.g. "drink_002"
    extension: str                             # e.g. ".fbx"
    asset_type: str                            # "model", "texture", "material", "prefab", "audio", "shader", "script", "scene", "other"
    source_pack: str                           # e.g. "Food FREE - Low Poly 3D Models Pack"
    source_publisher: str                      # e.g. "ithappy"
    has_thumbnail: bool = False
    thumbnail_path: Optional[str] = None       # relative path in output/thumbnails/

    # From .meta file (Tier 1)
    meta_texture_type: Optional[str] = None    # "Default", "NormalMap", "Sprite", etc.
    meta_srgb: Optional[bool] = None
    meta_max_texture_size: Optional[int] = None
    meta_animation_type: Optional[str] = None  # "None", "Legacy", "Generic", "Humanoid"
    meta_mesh_compression: Optional[int] = None
    meta_is_readable: Optional[bool] = None

    # From .mat YAML (Tier 1 — materials only)
    mat_shader_guid: Optional[str] = None
    mat_shader_name: Optional[str] = None       # resolved if shader is in same package
    mat_colors: dict = field(default_factory=dict)         # {"_BaseColor": [r,g,b,a], ...}
    mat_floats: dict = field(default_factory=dict)         # {"_Metallic": 0.5, ...}
    mat_texture_slots: dict = field(default_factory=dict)  # {"_BaseMap": "guid", ...}
    mat_keywords: list = field(default_factory=list)       # ["_EMISSION", "_NORMALMAP"]
    mat_render_queue: Optional[int] = None
    mat_surface_type: Optional[str] = None     # "Opaque" or "Transparent"

    # From .prefab YAML (Tier 1 — prefabs only)
    prefab_components: list = field(default_factory=list)  # ["MeshFilter", "MeshRenderer", "BoxCollider", ...]
    prefab_child_count: int = 0
    prefab_has_particles: bool = False
    prefab_has_animation: bool = False

    # From color palette extraction (Phase 4)
    dominant_colors: list = field(default_factory=list)    # [[r,g,b], ...] top 5

    # From embeddings (Phase 5)
    clip_embedding_id: Optional[int] = None
    dino_embedding_id: Optional[int] = None

    # From clustering (Phase 6)
    style_cluster: Optional[int] = None
    style_label: Optional[str] = None          # "low-poly", "pixel-art", "pbr-realistic", etc.
    color_temperature: Optional[str] = None    # "warm", "cool", "neutral"


@dataclass
class AssetCatalog:
    """The full asset catalog."""
    version: int = 1
    generated: str = ""
    total_assets: int = 0
    total_packs: int = 0
    packs: dict = field(default_factory=dict)  # pack_name -> {publisher, asset_count, path}
    assets: list = field(default_factory=list)  # list of AssetEntry dicts

    def save(self, path: Path):
        path.parent.mkdir(parents=True, exist_ok=True)
        data = asdict(self)
        # Filter None values for cleaner JSON
        data["assets"] = [
            {k: v for k, v in a.items() if v is not None and v != [] and v != {}}
            for a in data["assets"]
        ]
        with open(path, "w") as f:
            json.dump(data, f, indent=2)
        print(f"Saved catalog: {self.total_assets} assets from {self.total_packs} packs -> {path}")

    @classmethod
    def load(cls, path: Path) -> "AssetCatalog":
        with open(path) as f:
            data = json.load(f)
        catalog = cls(**{k: v for k, v in data.items() if k != "assets"})
        catalog.assets = data.get("assets", [])
        return catalog
```

- [ ] **Step 3: Verify Python environment**

Run: `python3 --version && pip3 install PyYAML Pillow scikit-learn numpy`
Expected: Python 3.11+ confirmed, packages installed.

- [ ] **Step 4: Commit scaffolding**

```bash
git add requirements.txt catalog.py
git commit -m "chore: scaffold asset indexer tool with data model"
```

---

## Phase 2: Package Extraction — Crack Open .unitypackage Files

### Task 2.1: Build the extraction pipeline

**Files:**
- Create: `extract.py`

This is the core Phase 1-2 script. It:
1. Finds all `.unitypackage` files in the Asset Store cache
2. Extracts `pathname`, `asset.meta`, and `preview.png` from each
3. Parses `.meta` YAML for type-specific metadata
4. Saves thumbnails to `output/thumbnails/`
5. Builds the initial catalog

- [ ] **Step 1: Write extract.py**

```python
# extract.py
"""Extract metadata and thumbnails from .unitypackage files without Unity."""

import os
import sys
import tarfile
from datetime import datetime
from pathlib import Path

import yaml

# Ensure sibling modules are importable regardless of CWD
sys.path.insert(0, str(Path(__file__).resolve().parent))

from catalog import AssetCatalog, AssetEntry

CACHE_DIR = Path.home() / "Library/Unity/Asset Store-5.x"
OUTPUT_DIR = Path(__file__).resolve().parent / "output"
THUMBNAIL_DIR = OUTPUT_DIR / "thumbnails"

TEXTURE_TYPE_MAP = {0: "Default", 1: "NormalMap", 2: "EditorGUI", 3: "Cookie",
                    5: "Advanced", 6: "Lightmap", 7: "Cursor", 8: "Sprite",
                    10: "Cookie", 11: "Lightmap"}
ANIMATION_TYPE_MAP = {0: "None", 1: "Legacy", 2: "Generic", 3: "Humanoid"}

EXTENSION_TO_TYPE = {
    ".fbx": "model", ".obj": "model", ".blend": "model", ".dae": "model",
    ".png": "texture", ".jpg": "texture", ".jpeg": "texture", ".tga": "texture",
    ".psd": "texture", ".tif": "texture", ".tiff": "texture", ".bmp": "texture",
    ".mat": "material",
    ".prefab": "prefab",
    ".wav": "audio", ".ogg": "audio", ".mp3": "audio", ".aiff": "audio",
    ".shader": "shader", ".shadergraph": "shader", ".hlsl": "shader",
    ".cs": "script",
    ".unity": "scene",
    ".asset": "scriptable_object",
    ".anim": "animation", ".controller": "animation",
    ".ttf": "font", ".otf": "font",
    ".json": "data", ".xml": "data", ".txt": "data",
    ".inputactions": "data",
}


def find_packages() -> list[tuple[Path, str, str]]:
    """Find all .unitypackage files. Returns (path, publisher, pack_name)."""
    packages = []
    for pkg_path in CACHE_DIR.rglob("*.unitypackage"):
        publisher = pkg_path.parent.parent.name
        pack_name = pkg_path.stem
        packages.append((pkg_path, publisher, pack_name))
    packages.sort(key=lambda x: x[2])
    return packages


def extract_package(pkg_path: Path, publisher: str, pack_name: str) -> list[AssetEntry]:
    """Extract all asset entries from a single .unitypackage."""
    entries = []
    guid_data = {}  # guid -> {pathname, meta_yaml, has_preview}

    with tarfile.open(pkg_path, "r:gz") as tar:
        for member in tar.getmembers():
            parts = member.name.split("/")
            if len(parts) != 2:
                continue
            guid, filename = parts

            if guid not in guid_data:
                guid_data[guid] = {}

            if filename == "pathname":
                f = tar.extractfile(member)
                if f:
                    raw = f.read().decode("utf-8", errors="replace").strip()
                    # pathname can have trailing null bytes
                    guid_data[guid]["pathname"] = raw.split("\x00")[0].strip()

            elif filename == "asset.meta":
                f = tar.extractfile(member)
                if f:
                    try:
                        meta = yaml.safe_load(f.read())
                        guid_data[guid]["meta"] = meta
                    except yaml.YAMLError:
                        pass

            elif filename == "preview.png":
                f = tar.extractfile(member)
                if f:
                    preview_data = f.read()
                    if len(preview_data) > 100:  # skip empty/tiny previews
                        guid_data[guid]["preview"] = preview_data

    # Build AssetEntry for each asset with a valid pathname
    for guid, data in guid_data.items():
        pathname = data.get("pathname")
        if not pathname:
            continue

        ext = Path(pathname).suffix.lower()
        name = Path(pathname).stem
        asset_type = EXTENSION_TO_TYPE.get(ext, "other")

        entry = AssetEntry(
            guid=guid,
            path=pathname,
            name=name,
            extension=ext,
            asset_type=asset_type,
            source_pack=pack_name,
            source_publisher=publisher,
        )

        # Extract .meta metadata
        meta = data.get("meta")
        if meta and isinstance(meta, dict):
            parse_meta(entry, meta)

        # Save thumbnail
        if "preview" in data:
            entry.has_thumbnail = True
            thumb_path = THUMBNAIL_DIR / f"{guid}.png"
            thumb_path.parent.mkdir(parents=True, exist_ok=True)
            thumb_path.write_bytes(data["preview"])
            entry.thumbnail_path = f"thumbnails/{guid}.png"

        entries.append(entry)

    return entries


def parse_meta(entry: AssetEntry, meta: dict):
    """Extract type-specific metadata from .meta YAML."""
    # Texture metadata
    if entry.asset_type == "texture":
        ti = meta.get("TextureImporter", {})
        if ti:
            tt = ti.get("textureType")
            if tt is not None:
                entry.meta_texture_type = TEXTURE_TYPE_MAP.get(tt, f"Unknown({tt})")
            mipmaps = ti.get("mipmaps", {})
            entry.meta_srgb = bool(mipmaps.get("sRGBTexture", 1))
            entry.meta_max_texture_size = ti.get("maxTextureSize")

    # Model metadata
    elif entry.asset_type == "model":
        mi = meta.get("ModelImporter", {})
        if mi:
            at = mi.get("animationType")
            if at is not None:
                entry.meta_animation_type = ANIMATION_TYPE_MAP.get(at, f"Unknown({at})")
            meshes = mi.get("meshes", {})
            entry.meta_mesh_compression = meshes.get("meshCompression")
            entry.meta_is_readable = bool(meshes.get("isReadable", 0))

    # Audio metadata
    elif entry.asset_type == "audio":
        ai = meta.get("AudioImporter", {})
        if ai:
            entry.meta_is_readable = True  # just flag it exists


def main():
    print(f"Scanning: {CACHE_DIR}")
    packages = find_packages()
    print(f"Found {len(packages)} packages\n")

    catalog = AssetCatalog(
        generated=datetime.now().isoformat(),
    )

    for pkg_path, publisher, pack_name in packages:
        print(f"  Extracting: {pack_name} ({publisher})...", end=" ", flush=True)
        entries = extract_package(pkg_path, publisher, pack_name)
        print(f"{len(entries)} assets, {sum(1 for e in entries if e.has_thumbnail)} thumbnails")

        catalog.packs[pack_name] = {
            "publisher": publisher,
            "asset_count": len(entries),
            "package_path": str(pkg_path),
        }

        from dataclasses import asdict
        catalog.assets.extend([asdict(e) for e in entries])

    catalog.total_assets = len(catalog.assets)
    catalog.total_packs = len(catalog.packs)

    catalog.save(OUTPUT_DIR / "catalog.json")
    print(f"\nThumbnails saved to: {THUMBNAIL_DIR}")
    print(f"Catalog saved to: {OUTPUT_DIR / 'catalog.json'}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Run extraction and verify output**

Run: `cd /Users/seamus/Documents/unity-assets-index && python3extract.py`
Expected: Processes 26 packages, extracts metadata and thumbnails, writes `output/catalog.json`.

- [ ] **Step 3: Spot-check the catalog**

Run: `python3 -c "import json; c=json.load(open('output/catalog.json')); print(f'Packs: {c[\"total_packs\"]}, Assets: {c[\"total_assets\"]}'); print('Types:', {t: sum(1 for a in c['assets'] if a.get('asset_type')==t) for t in set(a.get('asset_type','?') for a in c['assets'])})" `
Expected: Asset counts per type, no crashes.

- [ ] **Step 4: Commit**

```bash
git add extract.py
git commit -m "feat: asset indexer — extract metadata and thumbnails from .unitypackage files"
```

---

## Phase 3: Deep Material and Prefab Parsing

### Task 3.1: Parse .mat files from inside packages

**Files:**
- Create: `parse_materials.py`

Materials are YAML-serialized in `.unitypackage` and contain shader references, colors, texture slots, keywords, and surface type — all extractable without Unity.

- [ ] **Step 1: Write parse_materials.py**

```python
# parse_materials.py
"""Deep-parse .mat asset files from .unitypackage tarballs for shader/color/property data."""

import json
import re
import tarfile
from pathlib import Path

import yaml

# Fix imports: ensure catalog.py is importable from project root
import sys
sys.path.insert(0, str(Path(__file__).resolve().parent))

from catalog import AssetCatalog

CACHE_DIR = Path.home() / "Library/Unity/Asset Store-5.x"
OUTPUT_DIR = Path(__file__).resolve().parent / "output"

SURFACE_TYPE_MAP = {0.0: "Opaque", 1.0: "Transparent"}


def strip_unity_yaml(raw: bytes) -> str:
    """Strip Unity-specific YAML directives and tags that crash yaml.safe_load."""
    text = raw.decode("utf-8", errors="replace")
    text = re.sub(r'^%.*$', '', text, flags=re.MULTILINE)
    text = re.sub(r'--- !u!\d+ &\d+', '---', text)
    return text


def extract_mat_from_package(pkg_path: Path) -> dict[str, dict]:
    """Extract and parse all .mat files from a .unitypackage. Returns guid -> parsed data."""
    results = {}
    guid_data = {}

    with tarfile.open(pkg_path, "r:gz") as tar:
        for member in tar.getmembers():
            parts = member.name.split("/")
            if len(parts) != 2:
                continue
            guid, filename = parts

            if guid not in guid_data:
                guid_data[guid] = {}

            if filename == "pathname":
                f = tar.extractfile(member)
                if f:
                    raw = f.read().decode("utf-8", errors="replace").strip()
                    guid_data[guid]["pathname"] = raw.split("\x00")[0].strip()

            elif filename == "asset":
                f = tar.extractfile(member)
                if f:
                    guid_data[guid]["asset_bytes"] = f.read()

    for guid, data in guid_data.items():
        pathname = data.get("pathname", "")
        if not pathname.endswith(".mat"):
            continue
        asset_bytes = data.get("asset_bytes")
        if not asset_bytes:
            continue

        try:
            cleaned = strip_unity_yaml(asset_bytes)
            # Unity .mat files are multi-document YAML; load all and find Material
            docs = list(yaml.safe_load_all(cleaned))
            mat = None
            for doc in docs:
                if isinstance(doc, dict) and "Material" in doc:
                    mat = doc["Material"]
                    break
            if not mat:
                continue
            parsed = parse_material(mat)
            results[guid] = parsed
        except (yaml.YAMLError, KeyError, TypeError):
            continue

    return results


def parse_material(mat: dict) -> dict:
    """Extract structured data from a Material YAML block."""
    result = {}

    # Shader reference
    shader_ref = mat.get("m_Shader", {})
    if isinstance(shader_ref, dict):
        result["shader_guid"] = shader_ref.get("guid", "")

    # Colors
    colors = {}
    for tex_env_list in [mat.get("m_Colors", [])]:
        if isinstance(tex_env_list, list):
            for item in tex_env_list:
                if isinstance(item, dict):
                    for key, val in item.items():
                        if isinstance(val, dict) and "r" in val:
                            colors[key] = [
                                round(val.get("r", 0), 4),
                                round(val.get("g", 0), 4),
                                round(val.get("b", 0), 4),
                                round(val.get("a", 1), 4),
                            ]
    if colors:
        result["colors"] = colors

    # Float properties
    floats = {}
    for float_list in [mat.get("m_Floats", [])]:
        if isinstance(float_list, list):
            for item in float_list:
                if isinstance(item, dict):
                    for key, val in item.items():
                        if isinstance(val, (int, float)):
                            floats[key] = round(float(val), 4)
    if floats:
        result["floats"] = floats

    # Surface type
    surface = floats.get("_Surface")
    if surface is not None:
        result["surface_type"] = SURFACE_TYPE_MAP.get(surface, f"Unknown({surface})")

    # Texture slots
    tex_slots = {}
    for tex_list in [mat.get("m_TexEnvs", [])]:
        if isinstance(tex_list, list):
            for item in tex_list:
                if isinstance(item, dict):
                    for key, val in item.items():
                        if isinstance(val, dict):
                            tex_ref = val.get("m_Texture", {})
                            if isinstance(tex_ref, dict):
                                tex_guid = tex_ref.get("guid", "")
                                if tex_guid and tex_guid != "0" * 32:
                                    tex_slots[key] = tex_guid
    if tex_slots:
        result["texture_slots"] = tex_slots

    # Keywords
    keywords = mat.get("m_ValidKeywords", [])
    if keywords:
        result["keywords"] = keywords

    # Render queue
    rq = mat.get("m_CustomRenderQueue")
    if rq is not None and rq != -1:
        result["render_queue"] = rq

    return result


def enrich_catalog():
    """Load existing catalog and enrich material entries with deep-parsed data."""
    catalog_path = OUTPUT_DIR / "catalog.json"
    catalog = AssetCatalog.load(catalog_path)

    # Build GUID -> asset index for fast lookup
    guid_to_idx = {}
    for i, asset in enumerate(catalog.assets):
        guid_to_idx[asset["guid"]] = i

    packages = list(CACHE_DIR.rglob("*.unitypackage"))
    enriched = 0

    for pkg_path in sorted(packages):
        mat_data = extract_mat_from_package(pkg_path)
        for guid, parsed in mat_data.items():
            if guid in guid_to_idx:
                idx = guid_to_idx[guid]
                asset = catalog.assets[idx]
                asset["mat_shader_guid"] = parsed.get("shader_guid")
                asset["mat_colors"] = parsed.get("colors", {})
                asset["mat_floats"] = parsed.get("floats", {})
                asset["mat_texture_slots"] = parsed.get("texture_slots", {})
                asset["mat_keywords"] = parsed.get("keywords", [])
                asset["mat_render_queue"] = parsed.get("render_queue")
                asset["mat_surface_type"] = parsed.get("surface_type")
                enriched += 1

    print(f"Enriched {enriched} materials with deep-parsed data")
    catalog.save(catalog_path)


if __name__ == "__main__":
    enrich_catalog()
```

- [ ] **Step 2: Run material enrichment**

Run: `cd /Users/seamus/Documents/unity-assets-index && python3parse_materials.py`
Expected: Enriches material entries in catalog.json with color/shader/keyword data.

- [ ] **Step 3: Spot-check a material entry**

Run: `python3 -c "import json; c=json.load(open('output/catalog.json')); mats=[a for a in c['assets'] if a.get('mat_colors')]; print(f'{len(mats)} materials with colors'); print(json.dumps(mats[0], indent=2) if mats else 'none')"`

- [ ] **Step 4: Commit**

```bash
git add parse_materials.py
git commit -m "feat: asset indexer — deep-parse material YAML for colors, shaders, keywords"
```

### Task 3.2: Parse .prefab files for component inventory

**Files:**
- Create: `parse_prefabs.py`

- [ ] **Step 1: Write parse_prefabs.py**

```python
# parse_prefabs.py
"""Deep-parse .prefab asset files from .unitypackage tarballs for component/hierarchy data."""

import json
import re
import sys
import tarfile
from pathlib import Path

import yaml

sys.path.insert(0, str(Path(__file__).resolve().parent))

from catalog import AssetCatalog

CACHE_DIR = Path.home() / "Library/Unity/Asset Store-5.x"
OUTPUT_DIR = Path(__file__).resolve().parent / "output"

# Unity YAML type tags -> component names
UNITY_TYPE_MAP = {
    "1": "GameObject", "4": "Transform", "20": "Camera",
    "23": "MeshRenderer", "25": "Renderer", "33": "MeshFilter",
    "54": "Rigidbody", "56": "CapsuleCollider", "64": "MeshCollider",
    "65": "BoxCollider", "108": "Light", "111": "Animation",
    "114": "MonoBehaviour", "120": "LineRenderer", "136": "CapsuleCollider",
    "137": "SkinnedMeshRenderer", "198": "ParticleSystem",
    "199": "ParticleSystemRenderer", "205": "LODGroup",
    "212": "SpriteRenderer", "224": "AudioSource", "225": "AudioListener",
    "1001": "PrefabInstance",
    "95": "Animator",
}


def extract_prefabs_from_package(pkg_path: Path) -> dict[str, dict]:
    """Extract and parse all .prefab files from a .unitypackage."""
    results = {}
    guid_data = {}

    with tarfile.open(pkg_path, "r:gz") as tar:
        for member in tar.getmembers():
            parts = member.name.split("/")
            if len(parts) != 2:
                continue
            guid, filename = parts

            if guid not in guid_data:
                guid_data[guid] = {}

            if filename == "pathname":
                f = tar.extractfile(member)
                if f:
                    raw = f.read().decode("utf-8", errors="replace").strip()
                    guid_data[guid]["pathname"] = raw.split("\x00")[0].strip()

            elif filename == "asset":
                f = tar.extractfile(member)
                if f:
                    guid_data[guid]["asset_bytes"] = f.read()

    for guid, data in guid_data.items():
        pathname = data.get("pathname", "")
        if not pathname.endswith(".prefab"):
            continue
        asset_bytes = data.get("asset_bytes")
        if not asset_bytes:
            continue

        parsed = parse_prefab_raw(asset_bytes)
        if parsed:
            results[guid] = parsed

    return results


def parse_prefab_raw(raw_bytes: bytes) -> dict:
    """Parse prefab YAML using regex (multi-document YAML is tricky with PyYAML)."""
    text = raw_bytes.decode("utf-8", errors="replace")

    # Find all type tags: --- !u!<type_id> &<file_id>
    type_tags = re.findall(r"--- !u!(\d+) &\d+", text)

    components = set()
    has_particles = False
    has_animation = False
    child_count = 0

    for tag in type_tags:
        comp_name = UNITY_TYPE_MAP.get(tag, f"UnityType_{tag}")
        if comp_name in ("GameObject", "Transform", "PrefabInstance"):
            if comp_name == "Transform":
                child_count += 1
            continue
        components.add(comp_name)
        if comp_name == "ParticleSystem":
            has_particles = True
        if comp_name in ("Animator", "Animation"):
            has_animation = True

    # child_count - 1 because root transform isn't a child
    child_count = max(0, child_count - 1)

    return {
        "components": sorted(components),
        "child_count": child_count,
        "has_particles": has_particles,
        "has_animation": has_animation,
    }


def enrich_catalog():
    """Load existing catalog and enrich prefab entries."""
    catalog_path = OUTPUT_DIR / "catalog.json"
    catalog = AssetCatalog.load(catalog_path)

    guid_to_idx = {}
    for i, asset in enumerate(catalog.assets):
        guid_to_idx[asset["guid"]] = i

    packages = list(CACHE_DIR.rglob("*.unitypackage"))
    enriched = 0

    for pkg_path in sorted(packages):
        prefab_data = extract_prefabs_from_package(pkg_path)
        for guid, parsed in prefab_data.items():
            if guid in guid_to_idx:
                idx = guid_to_idx[guid]
                asset = catalog.assets[idx]
                asset["prefab_components"] = parsed["components"]
                asset["prefab_child_count"] = parsed["child_count"]
                asset["prefab_has_particles"] = parsed["has_particles"]
                asset["prefab_has_animation"] = parsed["has_animation"]
                enriched += 1

    print(f"Enriched {enriched} prefabs with component data")
    catalog.save(catalog_path)


if __name__ == "__main__":
    enrich_catalog()
```

- [ ] **Step 2: Run prefab enrichment**

Run: `cd /Users/seamus/Documents/unity-assets-index && python3parse_prefabs.py`
Expected: Enriches prefab entries with component lists, child counts, particle/animation flags.

- [ ] **Step 3: Spot-check**

Run: `python3 -c "import json; c=json.load(open('output/catalog.json')); prefs=[a for a in c['assets'] if a.get('prefab_components')]; print(f'{len(prefs)} prefabs enriched'); [print(f'  {p[\"name\"]}: {p[\"prefab_components\"]}') for p in prefs[:5]]"`

- [ ] **Step 4: Commit**

```bash
git add parse_prefabs.py
git commit -m "feat: asset indexer — deep-parse prefab YAML for components and hierarchy"
```

---

## Phase 4: Color Palette Extraction

### Task 4.1: Extract dominant colors from thumbnails

**Files:**
- Create: `tools/asset-indexer/palette.py`

Uses k-means clustering on thumbnail pixels to extract dominant colors. Also computes color temperature (warm/cool/neutral).

- [ ] **Step 1: Write palette.py**

```python
# tools/asset-indexer/palette.py
"""Extract dominant color palettes from asset thumbnails."""

import json
import sys
from pathlib import Path

import numpy as np
from PIL import Image
from sklearn.cluster import MiniBatchKMeans

sys.path.insert(0, str(Path(__file__).resolve().parent))

from catalog import AssetCatalog

OUTPUT_DIR = Path(__file__).resolve().parent / "output"
THUMBNAIL_DIR = OUTPUT_DIR / "thumbnails"
NUM_COLORS = 5


def extract_palette(image_path: Path, n_colors: int = NUM_COLORS) -> list[list[int]]:
    """Extract dominant colors from an image using k-means."""
    img = Image.open(image_path).convert("RGBA")
    pixels = np.array(img)

    # Filter out transparent and near-black background pixels
    mask = (pixels[:, :, 3] > 128) & (
        (pixels[:, :, 0] > 15) | (pixels[:, :, 1] > 15) | (pixels[:, :, 2] > 15)
    )
    rgb_pixels = pixels[:, :, :3][mask]

    if len(rgb_pixels) < n_colors * 10:
        return []

    kmeans = MiniBatchKMeans(n_clusters=n_colors, n_init=3, random_state=42)
    kmeans.fit(rgb_pixels)

    # Sort by cluster size (most dominant first)
    labels, counts = np.unique(kmeans.labels_, return_counts=True)
    order = np.argsort(-counts)
    centers = kmeans.cluster_centers_[order]

    return [[int(c) for c in color] for color in centers]


def classify_temperature(colors: list[list[int]]) -> str:
    """Classify overall color temperature as warm/cool/neutral."""
    if not colors:
        return "neutral"

    # Convert to HSV-like hue analysis
    warm_weight = 0
    cool_weight = 0

    for r, g, b in colors:
        # Simple heuristic: red/yellow channel dominance = warm, blue = cool
        warmth = (r * 1.2 + g * 0.5) - (b * 1.5)
        if warmth > 30:
            warm_weight += 1
        elif warmth < -30:
            cool_weight += 1

    if warm_weight > cool_weight + 1:
        return "warm"
    elif cool_weight > warm_weight + 1:
        return "cool"
    return "neutral"


def enrich_catalog():
    """Add color palettes to all assets with thumbnails."""
    catalog_path = OUTPUT_DIR / "catalog.json"
    catalog = AssetCatalog.load(catalog_path)

    enriched = 0
    total_with_thumb = 0

    for asset in catalog.assets:
        thumb_path_str = asset.get("thumbnail_path")
        if not thumb_path_str:
            continue
        total_with_thumb += 1

        thumb_path = OUTPUT_DIR / thumb_path_str
        if not thumb_path.exists():
            continue

        try:
            palette = extract_palette(thumb_path)
            if palette:
                asset["dominant_colors"] = palette
                asset["color_temperature"] = classify_temperature(palette)
                enriched += 1
        except Exception as e:
            print(f"  Warning: failed palette for {asset.get('name')}: {e}")

    print(f"Extracted palettes for {enriched}/{total_with_thumb} thumbnailed assets")
    catalog.save(catalog_path)


if __name__ == "__main__":
    enrich_catalog()
```

- [ ] **Step 2: Run palette extraction**

Run: `cd /Users/seamus/Documents/unity-assets-index && python3palette.py`
Expected: Extracts dominant colors for all assets with thumbnails. Should take <60 seconds for ~1000 thumbnails.

- [ ] **Step 3: Spot-check colors**

Run: `python3 -c "import json; c=json.load(open('output/catalog.json')); colored=[a for a in c['assets'] if a.get('dominant_colors')]; print(f'{len(colored)} assets with palettes'); [print(f'  {a[\"name\"]}: {a[\"color_temperature\"]} {a[\"dominant_colors\"][:2]}') for a in colored[:5]]"`

- [ ] **Step 4: Commit**

```bash
git add tools/asset-indexer/palette.py
git commit -m "feat: asset indexer — extract dominant color palettes from thumbnails"
```

---

## Phase 5: CLIP and DINOv2 Embeddings (Local, No API Cost)

### Task 5.1: Install ML dependencies and build embedding pipeline

**Files:**
- Modify: `requirements.txt` (add torch, open_clip, faiss)
- Create: `embed.py`

Both CLIP and DINOv2 run locally on Apple Silicon via PyTorch MPS backend. No API calls.

- [ ] **Step 1: Install ML dependencies**

Run:
```bash
pip3 install torch torchvision --index-url https://download.pytorch.org/whl/cpu
pip3 install open_clip_torch faiss-cpu
```

Note: `--index-url cpu` keeps install small. MPS acceleration still works for inference on Apple Silicon. If the user has a GPU-capable Mac, the default pip install with MPS support also works.

- [ ] **Step 2: Update requirements.txt**

Append to `requirements.txt`:
```
torch>=2.1
torchvision>=0.16
open_clip_torch>=2.24
faiss-cpu>=1.7
```

- [ ] **Step 3: Write embed.py**

```python
# embed.py
"""Generate CLIP and DINOv2 embeddings for asset thumbnails. Runs locally, no API costs."""

import json
import sys
from pathlib import Path

import numpy as np
import torch
from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parent))

from catalog import AssetCatalog

OUTPUT_DIR = Path(__file__).resolve().parent / "output"
THUMBNAIL_DIR = OUTPUT_DIR / "thumbnails"
EMBEDDINGS_DIR = OUTPUT_DIR / "embeddings"


def get_device():
    if torch.backends.mps.is_available():
        return torch.device("mps")
    elif torch.cuda.is_available():
        return torch.device("cuda")
    return torch.device("cpu")


def load_clip_model(device):
    """Load CLIP ViT-B/32 (smaller, faster for initial indexing)."""
    import open_clip
    model, _, preprocess = open_clip.create_model_and_transforms(
        "ViT-B-32", pretrained="laion2b_s34b_b79k"
    )
    model = model.to(device).eval()
    tokenizer = open_clip.get_tokenizer("ViT-B-32")
    return model, preprocess, tokenizer


def load_dino_model(device):
    """Load DINOv2 ViT-S/14 (smallest variant, good for style)."""
    model = torch.hub.load("facebookresearch/dinov2", "dinov2_vits14")
    model = model.to(device).eval()

    from torchvision import transforms
    preprocess = transforms.Compose([
        transforms.Resize(224, interpolation=transforms.InterpolationMode.BICUBIC),
        transforms.CenterCrop(224),
        transforms.ToTensor(),
        transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
    ])
    return model, preprocess


def embed_thumbnails(catalog_path: Path, model_name: str = "both"):
    """Embed all thumbnails and save FAISS indices."""
    import faiss

    catalog = AssetCatalog.load(catalog_path)
    device = get_device()
    print(f"Device: {device}")

    # Collect assets with thumbnails
    thumb_assets = []
    for i, asset in enumerate(catalog.assets):
        thumb_path_str = asset.get("thumbnail_path")
        if thumb_path_str:
            full_path = OUTPUT_DIR / thumb_path_str
            if full_path.exists():
                thumb_assets.append((i, asset, full_path))

    print(f"Found {len(thumb_assets)} assets with thumbnails")

    if not thumb_assets:
        print("No thumbnails to embed.")
        return

    EMBEDDINGS_DIR.mkdir(parents=True, exist_ok=True)

    # Build GUID ordering once (shared across both models)
    guid_order = [asset["guid"] for _, asset, _ in thumb_assets]

    # --- CLIP Embeddings ---
    if model_name in ("clip", "both"):
        print("\nLoading CLIP ViT-B/32...")
        clip_model, clip_preprocess, clip_tokenizer = load_clip_model(device)

        clip_vectors = []

        print("Embedding with CLIP...", flush=True)
        with torch.no_grad():
            for idx, (cat_idx, asset, thumb_path) in enumerate(thumb_assets):
                img = Image.open(thumb_path).convert("RGB")
                img_tensor = clip_preprocess(img).unsqueeze(0).to(device)
                feat = clip_model.encode_image(img_tensor)
                feat = feat / feat.norm(dim=-1, keepdim=True)
                clip_vectors.append(feat.cpu().numpy().flatten())

                asset["clip_embedding_id"] = idx

                if (idx + 1) % 100 == 0:
                    print(f"  {idx + 1}/{len(thumb_assets)}", flush=True)

        clip_matrix = np.array(clip_vectors, dtype=np.float32)
        clip_index = faiss.IndexFlatIP(clip_matrix.shape[1])  # inner product = cosine sim on normalized
        clip_index.add(clip_matrix)
        faiss.write_index(clip_index, str(EMBEDDINGS_DIR / "clip.index"))
        print(f"CLIP index saved: {clip_matrix.shape}")

        del clip_model, clip_preprocess
        torch.mps.empty_cache() if device.type == "mps" else None

    # --- DINOv2 Embeddings ---
    if model_name in ("dino", "both"):
        print("\nLoading DINOv2 ViT-S/14...")
        dino_model, dino_preprocess = load_dino_model(device)

        dino_vectors = []

        print("Embedding with DINOv2...", flush=True)
        with torch.no_grad():
            for idx, (cat_idx, asset, thumb_path) in enumerate(thumb_assets):
                img = Image.open(thumb_path).convert("RGB")
                img_tensor = dino_preprocess(img).unsqueeze(0).to(device)
                feat = dino_model(img_tensor)
                feat = feat / feat.norm(dim=-1, keepdim=True)
                dino_vectors.append(feat.cpu().numpy().flatten())

                asset["dino_embedding_id"] = idx

                if (idx + 1) % 100 == 0:
                    print(f"  {idx + 1}/{len(thumb_assets)}", flush=True)

        dino_matrix = np.array(dino_vectors, dtype=np.float32)
        dino_index = faiss.IndexFlatIP(dino_matrix.shape[1])
        dino_index.add(dino_matrix)
        faiss.write_index(dino_index, str(EMBEDDINGS_DIR / "dino.index"))
        print(f"DINOv2 index saved: {dino_matrix.shape}")

    # Save GUID ordering (maps embedding index -> GUID)
    with open(EMBEDDINGS_DIR / "guid_order.json", "w") as f:
        json.dump(guid_order, f)

    catalog.save(catalog_path)
    print("\nDone. Catalog updated with embedding IDs.")


if __name__ == "__main__":
    model = sys.argv[1] if len(sys.argv) > 1 else "both"
    embed_thumbnails(OUTPUT_DIR / "catalog.json", model_name=model)
```

- [ ] **Step 4: Run embeddings (expect 5-15 min on Apple Silicon)**

Run: `cd /Users/seamus/Documents/unity-assets-index && python3embed.py both`
Expected: Downloads CLIP and DINOv2 models on first run (~500MB total), embeds all thumbnails, saves FAISS indices.

- [ ] **Step 5: Verify indices**

Run: `python3 -c "import faiss; idx=faiss.read_index('output/embeddings/clip.index'); print(f'CLIP index: {idx.ntotal} vectors, dim={idx.d}'); idx2=faiss.read_index('output/embeddings/dino.index'); print(f'DINOv2 index: {idx2.ntotal} vectors, dim={idx2.d}')"`

- [ ] **Step 6: Commit**

```bash
git add embed.py requirements.txt
git commit -m "feat: asset indexer — CLIP + DINOv2 embeddings with FAISS index"
```

---

## Phase 6: Style Clustering and Compatibility Scoring

### Task 6.1: Cluster assets by visual style

**Files:**
- Create: `tools/asset-indexer/cluster.py`

- [ ] **Step 1: Write cluster.py**

```python
# tools/asset-indexer/cluster.py
"""Cluster assets by visual style using DINOv2 embeddings + heuristic signals."""

import json
import sys
from pathlib import Path

import faiss
import numpy as np
from sklearn.cluster import KMeans

sys.path.insert(0, str(Path(__file__).resolve().parent))

from catalog import AssetCatalog

OUTPUT_DIR = Path(__file__).resolve().parent / "output"
EMBEDDINGS_DIR = OUTPUT_DIR / "embeddings"

# Style labels assigned by analyzing cluster centroids
STYLE_LABELS = {
    # These get auto-assigned based on cluster characteristics
    # Fallback labels for manual refinement
}


def load_embeddings(model: str = "dino") -> tuple[np.ndarray, list[str]]:
    """Load FAISS index and GUID ordering."""
    index = faiss.read_index(str(EMBEDDINGS_DIR / f"{model}.index"))
    vectors = np.zeros((index.ntotal, index.d), dtype=np.float32)
    for i in range(index.ntotal):
        vectors[i] = index.reconstruct(i)

    with open(EMBEDDINGS_DIR / "guid_order.json") as f:
        guid_order = json.load(f)

    return vectors, guid_order


def cluster_by_style(n_clusters: int = 8):
    """Cluster assets by DINOv2 embeddings (best for style discrimination)."""
    vectors, guid_order = load_embeddings("dino")

    print(f"Clustering {len(vectors)} assets into {n_clusters} style groups...")
    kmeans = KMeans(n_clusters=n_clusters, n_init=10, random_state=42)
    labels = kmeans.fit_predict(vectors)

    # Load catalog and assign clusters
    catalog_path = OUTPUT_DIR / "catalog.json"
    catalog = AssetCatalog.load(catalog_path)

    guid_to_cluster = dict(zip(guid_order, [int(l) for l in labels]))

    # Analyze each cluster
    cluster_info = {}
    for cluster_id in range(n_clusters):
        mask = labels == cluster_id
        member_guids = [g for g, l in zip(guid_order, labels) if l == cluster_id]

        # Find members in catalog
        members = [a for a in catalog.assets if a["guid"] in set(member_guids)]

        # Summarize cluster
        packs = set(a.get("source_pack", "") for a in members)
        types = {}
        for a in members:
            t = a.get("asset_type", "other")
            types[t] = types.get(t, 0) + 1

        # Heuristic style label from member properties
        style_label = infer_style_label(members)

        cluster_info[cluster_id] = {
            "size": int(mask.sum()),
            "packs": sorted(packs),
            "type_distribution": types,
            "style_label": style_label,
        }
        print(f"  Cluster {cluster_id} ({style_label}): {mask.sum()} assets from {len(packs)} packs")

    # Write cluster assignments to catalog
    for asset in catalog.assets:
        guid = asset["guid"]
        if guid in guid_to_cluster:
            cluster_id = guid_to_cluster[guid]
            asset["style_cluster"] = cluster_id
            asset["style_label"] = cluster_info[cluster_id]["style_label"]

    catalog.save(catalog_path)

    # Save cluster details
    with open(OUTPUT_DIR / "clusters.json", "w") as f:
        json.dump(cluster_info, f, indent=2)
    print(f"\nCluster details saved to {OUTPUT_DIR / 'clusters.json'}")


def infer_style_label(members: list[dict]) -> str:
    """Heuristic: infer a style label from cluster member properties."""
    # Check for PBR indicators
    has_pbr = sum(1 for m in members if "_NORMALMAP" in m.get("mat_keywords", []))
    has_emission = sum(1 for m in members if "_EMISSION" in m.get("mat_keywords", []))

    # Check color temperatures
    temps = [m.get("color_temperature") for m in members if m.get("color_temperature")]
    warm_pct = temps.count("warm") / max(len(temps), 1)
    cool_pct = temps.count("cool") / max(len(temps), 1)

    # Check texture types
    sprites = sum(1 for m in members if m.get("meta_texture_type") == "Sprite")
    normals = sum(1 for m in members if m.get("meta_texture_type") == "NormalMap")

    total = max(len(members), 1)

    if sprites / total > 0.3:
        return "2d-sprites"
    if has_pbr / total > 0.2 and normals / total > 0.1:
        return "pbr-realistic"
    if warm_pct > 0.6:
        return "warm-stylized"
    if cool_pct > 0.6:
        return "cool-stylized"

    # Default to a generic label
    dominant_type = max(
        set(m.get("asset_type", "other") for m in members),
        key=lambda t: sum(1 for m in members if m.get("asset_type") == t),
        default="mixed"
    )
    return f"{dominant_type}-mixed"


def compute_compatibility(guid_a: str, guid_b: str, model: str = "dino") -> float:
    """Compute style compatibility score between two assets (0-1, higher = more compatible)."""
    vectors, guid_order = load_embeddings(model)
    guid_to_idx = {g: i for i, g in enumerate(guid_order)}

    if guid_a not in guid_to_idx or guid_b not in guid_to_idx:
        return 0.0

    va = vectors[guid_to_idx[guid_a]]
    vb = vectors[guid_to_idx[guid_b]]

    # Cosine similarity (vectors are already normalized)
    return float(np.dot(va, vb))


if __name__ == "__main__":
    cluster_by_style(n_clusters=8)
```

- [ ] **Step 2: Run clustering**

Run: `cd /Users/seamus/Documents/unity-assets-index && python3cluster.py`
Expected: Assigns each thumbnailed asset to one of 8 style clusters. Prints cluster summaries.

- [ ] **Step 3: Review cluster quality**

Run: `python3 -c "import json; c=json.load(open('output/clusters.json')); [print(f'Cluster {k}: {v[\"style_label\"]} — {v[\"size\"]} assets from {len(v[\"packs\"])} packs') for k,v in sorted(c.items())]"`

If clusters are too coarse (everything in 2-3 groups) or too fine (each pack is its own cluster), adjust `n_clusters` in `cluster.py` and re-run.

- [ ] **Step 4: Commit**

```bash
git add tools/asset-indexer/cluster.py
git commit -m "feat: asset indexer — DINOv2-based style clustering with compatibility scoring"
```

---

## Phase 7: Search Interface

### Task 7.1: Build the query tool

**Files:**
- Create: `search.py`

- [ ] **Step 1: Write search.py**

```python
# search.py
"""Query interface for the asset catalog. Supports text search, similarity, and filtered browse."""

import json
import sys
from pathlib import Path

import faiss
import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))

from catalog import AssetCatalog

OUTPUT_DIR = Path(__file__).resolve().parent / "output"
EMBEDDINGS_DIR = OUTPUT_DIR / "embeddings"


def load_catalog():
    return AssetCatalog.load(OUTPUT_DIR / "catalog.json")


def load_index_and_guids(model: str = "clip"):
    index = faiss.read_index(str(EMBEDDINGS_DIR / f"{model}.index"))
    with open(EMBEDDINGS_DIR / "guid_order.json") as f:
        guid_order = json.load(f)
    return index, guid_order


def text_search(query: str, top_k: int = 10):
    """Search by text using CLIP text-to-image similarity."""
    import open_clip
    import torch

    index, guid_order = load_index_and_guids("clip")
    catalog = load_catalog()
    guid_to_asset = {a["guid"]: a for a in catalog.assets}

    model, _, preprocess = open_clip.create_model_and_transforms(
        "ViT-B-32", pretrained="laion2b_s34b_b79k"
    )
    tokenizer = open_clip.get_tokenizer("ViT-B-32")
    model.eval()

    with torch.no_grad():
        text_tokens = tokenizer([query])
        text_feat = model.encode_text(text_tokens)
        text_feat = text_feat / text_feat.norm(dim=-1, keepdim=True)
        query_vec = text_feat.cpu().numpy().astype(np.float32)

    scores, indices = index.search(query_vec, top_k)

    print(f"\nText search: \"{query}\" (top {top_k})\n")
    for rank, (score, idx) in enumerate(zip(scores[0], indices[0])):
        if idx < 0:
            continue
        guid = guid_order[idx]
        asset = guid_to_asset.get(guid, {})
        print(f"  {rank+1}. [{score:.3f}] {asset.get('name', '?')} ({asset.get('asset_type', '?')}) — {asset.get('source_pack', '?')}")


def similar(guid: str, top_k: int = 10, model: str = "dino"):
    """Find visually similar assets using DINOv2 embeddings."""
    index, guid_order = load_index_and_guids(model)
    catalog = load_catalog()
    guid_to_asset = {a["guid"]: a for a in catalog.assets}
    guid_to_idx = {g: i for i, g in enumerate(guid_order)}

    if guid not in guid_to_idx:
        print(f"GUID {guid} not found in embeddings.")
        return

    vec = index.reconstruct(guid_to_idx[guid]).reshape(1, -1)
    scores, indices = index.search(vec, top_k + 1)  # +1 to skip self

    source = guid_to_asset.get(guid, {})
    print(f"\nSimilar to: {source.get('name', '?')} ({source.get('source_pack', '?')})\n")
    for rank, (score, idx) in enumerate(zip(scores[0], indices[0])):
        if idx < 0 or guid_order[idx] == guid:
            continue
        g = guid_order[idx]
        asset = guid_to_asset.get(g, {})
        print(f"  {rank}. [{score:.3f}] {asset.get('name', '?')} ({asset.get('asset_type', '?')}) — {asset.get('source_pack', '?')}")


def browse(asset_type: str = None, pack: str = None, cluster: int = None,
           temperature: str = None, limit: int = 20):
    """Browse assets with filters."""
    catalog = load_catalog()

    results = catalog.assets
    if asset_type:
        results = [a for a in results if a.get("asset_type") == asset_type]
    if pack:
        results = [a for a in results if pack.lower() in a.get("source_pack", "").lower()]
    if cluster is not None:
        results = [a for a in results if a.get("style_cluster") == cluster]
    if temperature:
        results = [a for a in results if a.get("color_temperature") == temperature]

    print(f"\nBrowse results: {len(results)} assets (showing {min(limit, len(results))})\n")
    for a in results[:limit]:
        extras = []
        if a.get("style_label"):
            extras.append(a["style_label"])
        if a.get("color_temperature"):
            extras.append(a["color_temperature"])
        extra_str = f" [{', '.join(extras)}]" if extras else ""
        print(f"  {a.get('name', '?')} ({a.get('asset_type', '?')}) — {a.get('source_pack', '?')}{extra_str}")


def stats():
    """Print catalog statistics."""
    catalog = load_catalog()
    print(f"\nAsset Catalog Stats")
    print(f"  Total assets: {catalog.total_assets}")
    print(f"  Total packs: {catalog.total_packs}")

    types = {}
    for a in catalog.assets:
        t = a.get("asset_type", "other")
        types[t] = types.get(t, 0) + 1
    print(f"\n  By type:")
    for t, c in sorted(types.items(), key=lambda x: -x[1]):
        print(f"    {t}: {c}")

    thumbnailed = sum(1 for a in catalog.assets if a.get("has_thumbnail"))
    palettes = sum(1 for a in catalog.assets if a.get("dominant_colors"))
    embedded = sum(1 for a in catalog.assets if a.get("clip_embedding_id") is not None)
    clustered = sum(1 for a in catalog.assets if a.get("style_cluster") is not None)
    print(f"\n  Coverage:")
    print(f"    Thumbnails: {thumbnailed}")
    print(f"    Color palettes: {palettes}")
    print(f"    CLIP embeddings: {embedded}")
    print(f"    Style clusters: {clustered}")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage:")
        print("  python search.py stats")
        print("  python search.py text 'low poly tree'")
        print("  python search.py similar <guid>")
        print("  python search.py browse --type model --pack nature --cluster 2 --temp warm")
        sys.exit(0)

    cmd = sys.argv[1]

    if cmd == "stats":
        stats()
    elif cmd == "text":
        text_search(" ".join(sys.argv[2:]))
    elif cmd == "similar":
        similar(sys.argv[2])
    elif cmd == "browse":
        kwargs = {}
        args = sys.argv[2:]
        i = 0
        while i < len(args):
            if args[i] == "--type":
                kwargs["asset_type"] = args[i+1]; i += 2
            elif args[i] == "--pack":
                kwargs["pack"] = args[i+1]; i += 2
            elif args[i] == "--cluster":
                kwargs["cluster"] = int(args[i+1]); i += 2
            elif args[i] == "--temp":
                kwargs["temperature"] = args[i+1]; i += 2
            elif args[i] == "--limit":
                kwargs["limit"] = int(args[i+1]); i += 2
            else:
                i += 1
        browse(**kwargs)
```

- [ ] **Step 2: Test all search modes**

Run:
```bash
cd /Users/seamus/Documents/prometheus/inkfall
python3 search.py stats
python3 search.py text "wooden barrel"
python3 search.py browse --type model --limit 10
python3 search.py browse --temp warm --type texture --limit 10
```

- [ ] **Step 3: Commit**

```bash
git add search.py
git commit -m "feat: asset indexer — search interface with text, similarity, and filtered browse"
```

---

## Phase 8: API Cost Estimation [STOP POINT]

### Task 8.1: Estimate vision LLM costs for natural language descriptions

**Files:**
- Create: `estimate_api.py`

This phase does NOT call any API. It counts assets that would benefit from LLM descriptions and calculates estimated cost.

- [ ] **Step 1: Write estimate_api.py**

```python
# estimate_api.py
"""Estimate API costs for vision LLM asset descriptions. Does NOT call any API."""

import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

from catalog import AssetCatalog

OUTPUT_DIR = Path(__file__).resolve().parent / "output"

# Pricing estimates (as of March 2026, subject to change)
# Claude: ~$0.015 per image (Sonnet, 128x128 thumbnail + short prompt)
# GPT-4o: ~$0.01 per image (similar)
COST_PER_IMAGE_CLAUDE = 0.015
COST_PER_IMAGE_GPT4O = 0.010

# We'd batch 10 thumbnails per API call with a grid image
BATCH_SIZE = 10
COST_PER_BATCH_CLAUDE = 0.04  # grid image is larger but one call
COST_PER_BATCH_GPT4O = 0.03


def estimate():
    catalog = AssetCatalog.load(OUTPUT_DIR / "catalog.json")

    # Assets with thumbnails that would benefit from descriptions
    describable = [
        a for a in catalog.assets
        if a.get("has_thumbnail") and a.get("asset_type") in (
            "model", "texture", "material", "prefab", "scriptable_object"
        )
    ]

    # Assets without thumbnails that need Unity Editor rendering first
    no_thumb_visual = [
        a for a in catalog.assets
        if not a.get("has_thumbnail") and a.get("asset_type") in (
            "model", "texture", "material", "prefab"
        )
    ]

    total_describable = len(describable)
    total_batches = (total_describable + BATCH_SIZE - 1) // BATCH_SIZE

    estimate_data = {
        "total_assets": catalog.total_assets,
        "assets_with_thumbnails": sum(1 for a in catalog.assets if a.get("has_thumbnail")),
        "visual_assets_needing_description": total_describable,
        "visual_assets_without_thumbnails": len(no_thumb_visual),
        "api_batches_needed": total_batches,
        "cost_estimates": {
            "individual_calls": {
                "claude_sonnet": f"${total_describable * COST_PER_IMAGE_CLAUDE:.2f}",
                "gpt4o": f"${total_describable * COST_PER_IMAGE_GPT4O:.2f}",
            },
            "batched_10_per_call": {
                "claude_sonnet": f"${total_batches * COST_PER_BATCH_CLAUDE:.2f}",
                "gpt4o": f"${total_batches * COST_PER_BATCH_GPT4O:.2f}",
            },
        },
        "what_you_get": [
            "Natural language description per asset (e.g. 'Low-poly wooden barrel with metal bands, medieval style')",
            "Theme tags (medieval, sci-fi, nature, urban, fantasy, etc.)",
            "Art style classification (low-poly, pixel-art, PBR, toon, hand-painted)",
            "Suggested use cases (prop, environment, character, UI element, etc.)",
            "Quality assessment (production-ready, placeholder, needs work)",
        ],
        "what_you_already_have_without_api": [
            "Structured metadata (type, resolution, vertex hints, shader info)",
            "Color palettes (dominant colors, warm/cool temperature)",
            "CLIP embeddings (text-to-asset search)",
            "DINOv2 embeddings (visual similarity search)",
            "Style clusters (auto-grouped by visual style)",
            "Material properties (shader, colors, keywords, transparency)",
            "Prefab components (what Unity components each prefab uses)",
        ],
    }

    output_path = OUTPUT_DIR / "cost-estimate.json"
    with open(output_path, "w") as f:
        json.dump(estimate_data, f, indent=2)

    print("\n=== API Cost Estimate ===\n")
    print(f"Total assets in catalog: {catalog.total_assets}")
    print(f"Assets with thumbnails: {estimate_data['assets_with_thumbnails']}")
    print(f"Visual assets needing LLM description: {total_describable}")
    print(f"Visual assets without thumbnails (need Unity render first): {len(no_thumb_visual)}")
    print(f"\nBatched API calls needed: {total_batches} (10 per call)")
    print(f"\nEstimated cost (batched):")
    print(f"  Claude Sonnet: {estimate_data['cost_estimates']['batched_10_per_call']['claude_sonnet']}")
    print(f"  GPT-4o:        {estimate_data['cost_estimates']['batched_10_per_call']['gpt4o']}")
    print(f"\nEstimated cost (individual):")
    print(f"  Claude Sonnet: {estimate_data['cost_estimates']['individual_calls']['claude_sonnet']}")
    print(f"  GPT-4o:        {estimate_data['cost_estimates']['individual_calls']['gpt4o']}")
    print(f"\nSaved to: {output_path}")


if __name__ == "__main__":
    estimate()
```

- [ ] **Step 2: Run the estimate**

Run: `cd /Users/seamus/Documents/unity-assets-index && python3estimate_api.py`
Expected: Prints cost breakdown. Present to user for approval before proceeding to actual API calls.

- [ ] **Step 3: Commit**

```bash
git add estimate_api.py
git commit -m "feat: asset indexer — API cost estimator for vision LLM descriptions"
```

---

## Phase Summary

| Phase | What | Cost | Output |
|-------|------|------|--------|
| 1 | Scaffolding + data model | $0 | `catalog.py`, `requirements.txt` |
| 2 | Extract metadata + thumbnails from .unitypackage | $0 | `catalog.json`, `thumbnails/` |
| 3 | Deep-parse materials + prefabs | $0 | Enriched `catalog.json` |
| 4 | Color palette extraction | $0 | Dominant colors + temperature per asset |
| 5 | CLIP + DINOv2 embeddings | $0 | FAISS indices for text + visual search |
| 6 | Style clustering | $0 | 8 style clusters with labels |
| 7 | Search interface | $0 | CLI tool: text search, similarity, browse |
| 8 | Cost estimate | $0 | `cost-estimate.json` with API pricing |
| **9** | **Vision LLM descriptions** | **$TBD** | **BLOCKED — awaiting user approval** |

**After Phase 8, you will have:**
- Full metadata for every asset across 26 packages
- Thumbnails extracted from publisher previews
- Color palettes and temperature classification
- CLIP text-to-asset semantic search ("find me a wooden barrel")
- DINOv2 visual similarity search ("find assets that look like this one")
- Auto-generated style clusters ("these packs go together visually")
- A CLI to query all of the above

**What Phase 9 (API cost) would add:**
- Human-readable descriptions per asset
- Theme/genre tags
- Art style labels refined by LLM judgment
- Quality assessments
