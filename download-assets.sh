#!/usr/bin/env bash
# ──────────────────────────────────────────────────────────────────
# download-assets.sh — Fetch free game assets for Inkshot
#
# Three tiers:
#   1. DIRECT DOWNLOADS — curl from author sites (Kenney, Demigiant)
#   2. GIT PACKAGES     — clone repos / add UPM entries
#   3. ASSET STORE      — opens browser tabs for one-click "Add to My Assets"
# ──────────────────────────────────────────────────────────────────
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "$0")" && pwd)"
THIRD_PARTY="$PROJECT_ROOT/Assets/ThirdParty"
DOWNLOADS="$THIRD_PARTY/_Downloads"
MANIFEST="$PROJECT_ROOT/Packages/manifest.json"

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
RED='\033[0;31m'
NC='\033[0m'

mkdir -p "$DOWNLOADS"

log()  { echo -e "${GREEN}[✓]${NC} $1"; }
warn() { echo -e "${YELLOW}[!]${NC} $1"; }
info() { echo -e "${CYAN}[→]${NC} $1"; }
err()  { echo -e "${RED}[✗]${NC} $1"; }

# ──────────────────────────────────────────────────────────────────
# TIER 1: Direct Downloads (no auth required)
# ──────────────────────────────────────────────────────────────────
download_direct() {
    local url="$1"
    local name="$2"
    local dest="$3"
    local filename
    filename="$(basename "$url")"

    if [ -d "$dest" ] && [ "$(ls -A "$dest" 2>/dev/null)" ]; then
        warn "$name already exists at $dest — skipping"
        return 0
    fi

    info "Downloading $name..."
    if curl -fSL --progress-bar -o "$DOWNLOADS/$filename" "$url"; then
        mkdir -p "$dest"
        unzip -qo "$DOWNLOADS/$filename" -d "$dest"
        log "$name downloaded and extracted to $dest"
    else
        err "Failed to download $name from $url"
        return 1
    fi
}

echo ""
echo "════════════════════════════════════════════════════════════"
echo "  TIER 1: Direct Downloads (Kenney CC0 + DOTween)"
echo "════════════════════════════════════════════════════════════"
echo ""

# Kenney UI Pack — 430+ UI elements, CC0
download_direct \
    "https://kenney.nl/media/pages/assets/ui-pack/af874291da-1718203990/kenney_ui-pack.zip" \
    "Kenney UI Pack" \
    "$THIRD_PARTY/KenneyUI"

# Kenney Game Icons — 105+ icons, CC0
download_direct \
    "https://kenney.nl/media/pages/assets/game-icons/94af1f5c0b-1677661579/kenney_game-icons.zip" \
    "Kenney Game Icons" \
    "$THIRD_PARTY/KenneyGameIcons"

# Kenney Particle Pack — 80 particle textures, CC0
download_direct \
    "https://kenney.nl/media/pages/assets/particle-pack/1dd3d4cbe2-1677578741/kenney_particle-pack.zip" \
    "Kenney Particle Pack" \
    "$THIRD_PARTY/KenneyParticles"

# Kenney Input Prompts — 1280+ controller/keyboard glyphs, CC0
download_direct \
    "https://kenney.nl/media/pages/assets/input-prompts/67c675026d-1763810066/kenney_input-prompts_1.4.1.zip" \
    "Kenney Input Prompts" \
    "$THIRD_PARTY/KenneyInputPrompts"

# Kenney Fonts — 11 free game fonts, CC0
download_direct \
    "https://kenney.nl/media/pages/assets/kenney-fonts/3492f8d47e-1677661710/kenney_kenney-fonts.zip" \
    "Kenney Fonts" \
    "$THIRD_PARTY/KenneyFonts"

# DOTween Free — tweening engine from Demigiant
download_direct \
    "https://dotween.demigiant.com/downloads/DOTween_1_2_825.zip" \
    "DOTween Free v1.2.825" \
    "$THIRD_PARTY/DOTween"

echo ""
echo "════════════════════════════════════════════════════════════"
echo "  TIER 2: Git / UPM Packages"
echo "════════════════════════════════════════════════════════════"
echo ""

# NaughtyAttributes — Inspector extensions via UPM
if grep -q "com.dbrizov.naughtyattributes" "$MANIFEST" 2>/dev/null; then
    warn "NaughtyAttributes already in manifest.json — skipping"
else
    info "Adding NaughtyAttributes to manifest.json via UPM..."
    # Use python to safely insert into JSON
    python3 -c "
import json
with open('$MANIFEST', 'r') as f:
    m = json.load(f)
m['dependencies']['com.dbrizov.naughtyattributes'] = 'https://github.com/dbrizov/NaughtyAttributes.git#upm'
with open('$MANIFEST', 'w') as f:
    json.dump(m, f, indent=2)
    f.write('\n')
"
    log "NaughtyAttributes added to manifest.json"
fi

# Unity Localization — first-party, via UPM
if grep -q "com.unity.localization" "$MANIFEST" 2>/dev/null; then
    warn "Unity Localization already in manifest.json — skipping"
else
    info "Adding Unity Localization to manifest.json..."
    python3 -c "
import json
with open('$MANIFEST', 'r') as f:
    m = json.load(f)
m['dependencies']['com.unity.localization'] = '1.5.3'
with open('$MANIFEST', 'w') as f:
    json.dump(m, f, indent=2)
    f.write('\n')
"
    log "Unity Localization added to manifest.json"
fi

# ProBuilder — first-party, via UPM
if grep -q "com.unity.probuilder" "$MANIFEST" 2>/dev/null; then
    warn "ProBuilder already in manifest.json — skipping"
else
    info "Adding ProBuilder to manifest.json..."
    python3 -c "
import json
with open('$MANIFEST', 'r') as f:
    m = json.load(f)
m['dependencies']['com.unity.probuilder'] = '6.0.7'
with open('$MANIFEST', 'w') as f:
    json.dump(m, f, indent=2)
    f.write('\n')
"
    log "ProBuilder added to manifest.json"
fi

# URP Toon Shader — GitHub clone
TOON_DIR="$THIRD_PARTY/URPToonShader"
if [ -d "$TOON_DIR" ] && [ "$(ls -A "$TOON_DIR" 2>/dev/null)" ]; then
    warn "URP Toon Shader already exists — skipping"
else
    info "Cloning URP Toon Shader from GitHub..."
    if git clone --depth 1 https://github.com/ChiliMilk/URP_Toon.git "$TOON_DIR" 2>/dev/null; then
        rm -rf "$TOON_DIR/.git"
        log "URP Toon Shader cloned to $TOON_DIR"
    else
        err "Failed to clone URP Toon Shader"
    fi
fi

echo ""
echo "════════════════════════════════════════════════════════════"
echo "  TIER 3: Asset Store (opens browser for one-click import)"
echo "════════════════════════════════════════════════════════════"
echo ""

# Asset Store packages — these require adding to your Unity account via browser
# then importing through Unity's Package Manager
ASSET_STORE_URLS=(
    # VFX & Particles
    "https://assetstore.unity.com/packages/vfx/particles/cartoon-fx-remaster-free-109565|Cartoon FX Remaster Free"
    "https://assetstore.unity.com/packages/vfx/particles/hit-impact-effects-free-218385|Hit Impact Effects FREE"

    # UI & Icons
    "https://assetstore.unity.com/packages/2d/gui/icons/clean-vector-icons-132084|Clean Vector Icons"
    "https://assetstore.unity.com/packages/2d/gui/fantasy-wooden-gui-free-103811|Fantasy Wooden GUI Free"
    "https://assetstore.unity.com/packages/2d/gui/icons/ux-flat-icons-free-202525|UX Flat Icons Free"

    # Textures & Materials
    "https://assetstore.unity.com/packages/2d/textures-materials/free-stylized-pbr-textures-pack-111778|FREE Stylized PBR Textures"
    "https://assetstore.unity.com/packages/2d/textures-materials/25-free-stylized-textures-215655|25+ Free Stylized Textures"

    # Skyboxes
    "https://assetstore.unity.com/packages/2d/textures-materials/sky/allsky-free-10-sky-skybox-set-146014|AllSky Free"
    "https://assetstore.unity.com/packages/2d/textures-materials/sky/fantasy-skybox-free-18353|Fantasy Skybox FREE"
    "https://assetstore.unity.com/packages/vfx/shaders/free-skybox-extended-shader-107400|FREE Skybox Extended Shader"

    # Shaders
    "https://assetstore.unity.com/packages/vfx/shaders/urp-simple-toon-shader-243515|(URP) Simple Toon Shader"

    # Audio & SFX
    "https://assetstore.unity.com/packages/audio/sound-fx/free-casual-game-sfx-pack-54116|FREE Casual Game SFX Pack"
    "https://assetstore.unity.com/packages/audio/sound-fx/free-sound-effects-pack-155235|Free Sound Effects Pack"

    # 3D Models (Synty Starter Pack also free at syntystore.com — opens separately)
    "https://syntystore.com/products/polygon-starter-pack|POLYGON Starter Pack (Synty) — FREE from Synty Store"
    "https://assetstore.unity.com/packages/3d/environments/landscapes/low-poly-simple-nature-pack-162153|Low-Poly Simple Nature Pack"
    "https://assetstore.unity.com/packages/3d/environments/lowpoly-environment-nature-free-medieval-fantasy-series-187052|Lowpoly Environment Nature Free"

    # 2D
    "https://assetstore.unity.com/packages/2d/free-2d-mega-pack-177430|Free 2D Mega Pack"
)

echo "The following Asset Store packages need to be added to your Unity account"
echo "via browser, then imported in Unity's Package Manager."
echo ""
echo "Options:"
echo "  [a] Open ALL in browser tabs (recommended — add to account, import in Unity)"
echo "  [l] List URLs only (copy/paste yourself)"
echo "  [s] Skip"
echo ""
read -rp "Choice [a/l/s]: " choice

case "$choice" in
    a|A)
        info "Opening ${#ASSET_STORE_URLS[@]} Asset Store pages..."
        for entry in "${ASSET_STORE_URLS[@]}"; do
            url="${entry%%|*}"
            name="${entry##*|}"
            open "$url"
            echo "  → $name"
            sleep 0.5  # stagger browser tabs
        done
        echo ""
        log "All tabs opened. Click 'Add to My Assets' on each page."
        log "Then in Unity: Window → Package Manager → My Assets → Download & Import"
        ;;
    l|L)
        echo ""
        for entry in "${ASSET_STORE_URLS[@]}"; do
            url="${entry%%|*}"
            name="${entry##*|}"
            echo "  $name"
            echo "    $url"
            echo ""
        done
        ;;
    s|S)
        warn "Skipped Asset Store packages"
        ;;
    *)
        warn "Unknown choice — skipping"
        ;;
esac

echo ""
echo "════════════════════════════════════════════════════════════"
echo "  Cleanup"
echo "════════════════════════════════════════════════════════════"
echo ""

# Clean up downloaded zips
read -rp "Delete downloaded zip files from _Downloads? [y/N]: " cleanup
if [[ "$cleanup" == "y" || "$cleanup" == "Y" ]]; then
    rm -rf "$DOWNLOADS"
    log "Cleaned up _Downloads"
else
    info "Zip files kept at $DOWNLOADS"
fi

echo ""
echo "════════════════════════════════════════════════════════════"
echo "  Summary"
echo "════════════════════════════════════════════════════════════"
echo ""
echo "  Direct downloads:  Assets/ThirdParty/"
ls -d "$THIRD_PARTY"/*/ 2>/dev/null | while read -r d; do
    echo "    $(basename "$d")/"
done
echo ""
echo "  UPM packages added to manifest.json:"
echo "    - NaughtyAttributes (Inspector extensions)"
echo "    - Unity Localization (first-party)"
echo "    - ProBuilder (in-editor mesh editing)"
echo ""
echo "  Next steps:"
echo "    1. Open Unity — it will resolve UPM packages automatically"
echo "    2. Import Asset Store packages via Package Manager → My Assets"
echo "    3. Run DOTween setup: Tools → Demigiant → DOTween Utility Panel"
echo "    4. Move ThirdParty assets to appropriate project folders as needed"
echo ""
log "Done!"
