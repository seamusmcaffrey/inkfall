# Visual Parity Orchestration

You are an **orchestrating agent** for the Inkfall Unity project. Your mission: make this game look identical to the visual reference. You do this by iteratively capturing the game, scoring it against the reference, dispatching specialists to close gaps, and repeating until parity is achieved.

**Do not stop after one pass. Keep iterating until every scored item is satisfied.**

---

# Your Tools

You have three feedback systems. Always prefer whichever is fastest and most available:

## Agent Bridge (always available)

```bash
# Non-visual checks (batch mode, fast)
./agent-bridge.sh compile       # Must pass 0 errors before any visual check
./agent-bridge.sh health        # Must pass 0 new violations at all times
./agent-bridge.sh validate      # Scene structure: camera, layers, components

# Visual capture (interactive mode, ~60–90s, requires full GPU)
./agent-bridge.sh gameplay      # Boot → Play Mode → START RUN → capture PNG
./agent-bridge.sh dart-test     # Fire darts at 30/60/100% pull → capture before/after PNGs
./agent-bridge.sh screenshot    # Scene View static render

# Check what's available
./agent-bridge.sh status
```

All output goes to `Logs/agent-feedback/` and `Logs/agent-feedback/screenshots/`.

## MCP Servers (when Unity is open — instant, ~1–2s)

Check availability first: `./agent-bridge.sh status`

If Unity is open with MCP running, **always use MCP instead of agent-bridge for visual work** — it is 50× faster.

**uLoopMCP** — screenshots, arbitrary C# execution in the Editor, play mode control:
- Available via the `mcp__*` tools in your tool set
- Use for: runtime screenshots, executing diagnostic C# snippets, toggling Play Mode

**unityMCP / Unity-MCP (Ivan Murzak, 52-tool bridge)** — full scene access:
- Scene graph inspection, GameObject lookup, component read/write
- Material property modification, texture assignment
- Console log reading
- Script reading and modification
- Package and settings management
- Start via: Unity → Window → AI Game Developer → Configure (select Claude Code), then Window → MCP for Unity → Start Server

When Unity is open and MCP is running, your visual workflow is:
1. Make code/asset change
2. Use MCP to take a screenshot (instant)
3. Compare to reference
4. Repeat

## Assets Library (4,119 pre-downloaded assets — use these first before generating anything)

```bash
cd /Users/seamus/Documents/unity-assets-index

# Text/semantic search (finds assets matching a description)
python3 search.py text "cork board texture"
python3 search.py text "wood panel dark"
python3 search.py text "paint splatter particle effect"
python3 search.py text "neon glow particle"
python3 search.py text "industrial metal frame"
python3 search.py text "round glossy balloon"
python3 search.py text "coin currency icon"

# Browse by type
python3 search.py browse --type texture --temp warm
python3 search.py browse --type prefab
python3 search.py stats
```

The catalog is at `/Users/seamus/Documents/unity-assets-index/output/catalog.json`. Each asset has: `path`, `name`, `asset_type`, `source_pack`, `thumbnail_path`, `dominant_colors`, `style_label`.

**When you find a relevant asset:**
1. Read its `path` from the catalog — this is its path inside the Unity Asset Store package
2. The packages live at `~/Library/Unity/Asset Store-5.x/`
3. Copy or reference the asset into `Assets/` — the asset is already paid for and downloaded

Before generating any new texture, shader property, or prefab from scratch, **search the library first**.

---

# The Visual Target

Reference image: `/Users/seamus/Documents/prometheus/inkfall/x-docs/reference/visual-refernce.png`

This image shows four panels — the complete target aesthetic, **"graffiti noir carnival"**:

## Panel 1 — Active Gameplay

- **Balloon shapes**: Standard round teardrop balloons (the classic party balloon silhouette) — NOT hearts, stars, animals, or abstract shapes
- **Balloon surface**: Glossy 3D, strong specular highlight (bright white point), vivid saturated colors — blue, purple, teal, green, hot pink/magenta, gold, black
- **Balloon emblems**: Each balloon type has a distinct metallic icon painted on it — crown, sun/paint-burst, shield, anchor, warning triangle
- **Grid**: 4–5 columns × 4–5 rows, packed tight, staggered slightly
- **Board**: Dark cork/wood texture background with a weathered metal frame around the perimeter
- **Paint drips**: Neon green paint dripping down the board surface — persistent, thick, luminous
- **HUD bar** at top: Left = "ROOM 12" steel-plate badge. Center = "SCORE: 1,450 / TARGET: 3,000" with a progress bar below it. Right = "DARTS: 3" and "CURRENCY: 120" with a coin icon. Dark industrial panel background.
- **Floor**: Wooden bowling lane stripes receding in perspective below the board
- **Dart in flight**: Chrome dart mid-trajectory
- **Atmosphere**: Fog, edge glow, spotlight rays

## Panel 2 — Perk Draft

- Board visible above with popped rows showing empty metal ring hooks
- Three perk cards laid out horizontally at the bottom:
  - Card: dark background, rarity border (white = Common, blue = Rare, purple = Epic)
  - Upper portion: large bold icon (paint splash / crossed darts / money bag with stars)
  - Card name in caps: PAINT COMBO / RICOCHET PATH / GOLD RUSH
  - 1-line description text
  - Rarity label at card bottom

## Panel 3 — Paint Explosion VFX

- Massive neon green + hot pink paint explosion covering the center of the board
- Wet, organic, dramatically oversized — multiple balloons caught in the blast
- Paint streaks and drips visible
- Darts visible inside the explosion

## Panel 4 — Room Transition Screen

- Full dark screen, atmospheric bowling alley perspective fading to black in background
- Large bold white text: "ROOM 13" then "THE NEON MAZE"
- "TARGET SCORE: 4,000" below
- Framed preview panel showing 4 upcoming balloon type icons (gold, crown, paint-burst, warning)
- 2 small perk icon badges at the bottom

---

# Current State — What It Looks Like Now

A screenshot taken 2026-03-22 shows the game in its current state. **Known critical gaps:**

1. **Balloon mesh shapes are completely wrong.** Current meshes are hearts, stars, balloon animals (dog/poodle forms), and bow-tie/ribbon shapes. The reference requires standard round teardrop balloons. Fix `BalloonMeshGenerator.cs` — it must produce a proper rounded teardrop silhouette.

2. **No balloon emblems visible.** The metallic icons (crown, shield, anchor, etc.) should be clearly visible on each balloon face.

3. **Balloons are matte/flat.** No specular highlight visible. The shader tuning is insufficient.

4. **HUD is barely readable.** Labels exist but have no industrial panel backing, no room badge, no styled typography.

5. **No paint drip effects on the board.** The persistent splatter/drip system is not producing visible output.

6. **No atmospheric environment.** Board background lacks cork/wood texture and metal frame. No fog or neon glow visible.

7. **Floor lane is plain/dark.** Wooden bowling lane stripes are not visible.

8. **Paint explosion VFX insufficient.** Pop effects need to be dramatically larger and neon-colored.

---

# Your Iteration Loop

## Iteration Structure

Each iteration follows this structure:

```
CAPTURE → SCORE → DISPATCH → VERIFY → REPEAT
```

Never move to the next iteration until health passes.

## Step 1 — Capture Current State

```bash
./agent-bridge.sh compile   # must be 0 errors
./agent-bridge.sh gameplay  # capture gameplay PNG
./agent-bridge.sh dart-test # capture dart physics PNGs
```

Read every output PNG. Read the reference: `/Users/seamus/Documents/prometheus/inkfall/x-docs/reference/visual-refernce.png`

## Step 2 — Score Against Reference

Score each category 0–10 (0 = completely wrong, 10 = matches reference exactly). Record scores in a running table you maintain in your scratchpad:

| Category | Iter 1 | Iter 2 | Iter 3 | Target |
|---|---|---|---|---|
| Balloon shape (round teardrop) | | | | 10 |
| Balloon surface (gloss, specular) | | | | 10 |
| Balloon emblems (visible, distinct) | | | | 10 |
| Balloon color palette | | | | 10 |
| Board (cork/wood texture + metal frame) | | | | 10 |
| Paint drips (persistent, neon) | | | | 10 |
| Atmosphere (fog, neon glow, spotlights) | | | | 10 |
| Floor (bowling lane perspective) | | | | 10 |
| HUD (room badge, score, darts, currency) | | | | 10 |
| Paint explosion VFX | | | | 10 |
| Perk card UI | | | | 10 |
| Room transition screen | | | | 10 |
| **TOTAL** | **/120** | **/120** | **/120** | **120** |

**Iteration goal**: Increase total score each iteration. Stop when total ≥ 108/120 (90%) with no category below 7.

## Step 3 — Dispatch Specialists

For each category scoring < 8, create a dispatch task. Launch independent dispatches in parallel.

Use the `Task` tool with `subagent_type` matching the agent name from the roster below.

## Step 4 — Verify Each Fix

After all dispatched agents complete:

```bash
./agent-bridge.sh health    # must be 0 new violations
./agent-bridge.sh compile   # must be 0 errors
./agent-bridge.sh gameplay  # capture new screenshot
```

Re-score. If score increased, advance to next iteration. If a category didn't improve, re-diagnose and re-dispatch with a more specific brief.

## Step 5 — Continue Until Done

Repeat Steps 1–4. Track score improvements across iterations. You are done when:
- Total score ≥ 108/120 and no category < 7
- `./agent-bridge.sh health` returns zero violations
- `./agent-bridge.sh compile` returns zero errors

Then commit: `polish: visual parity pass — match reference aesthetic`

---

# Agent Roster

Use the `Task` tool with `subagent_type` set to the agent name.

| Category | Primary Agent | Support |
|---|---|---|
| Balloon mesh shape | `unity-specialist` | `technical-artist` |
| Balloon surface shader (gloss, specular, fresnel) | `unity-shader-specialist` | `technical-artist` |
| Balloon emblem/icon overlay | `unity-shader-specialist` | `technical-artist` |
| Balloon color palette and material tuning | `technical-artist` | `art-director` |
| Board environment (cork, frame, texture) | `technical-artist` | `unity-specialist` |
| Paint drip persistent effects | `technical-artist` | `unity-shader-specialist` |
| Neon lighting and atmosphere | `unity-shader-specialist` | `technical-artist` |
| Floor/lane perspective rendering | `technical-artist` | — |
| HUD layout and industrial styling | `unity-ui-specialist` | `art-director` |
| Perk card UI (rarity, icons, layout) | `unity-ui-specialist` | `art-director` |
| Room transition screen | `unity-ui-specialist` | `art-director` |
| Paint explosion VFX scale and color | `technical-artist` | `unity-shader-specialist` |
| Dart mesh and chrome material | `unity-shader-specialist` | `technical-artist` |
| Asset sourcing from library | `technical-artist` | — |
| Architecture/standards violations | `unity-specialist` | — |

---

# Dispatch Template

Each agent dispatch must be fully self-contained. Use this template:

```
You are being dispatched as [agent-name] to fix a specific visual gap in the Inkfall Unity project.

## Context

PROJECT ROOT: /Users/seamus/Documents/prometheus/inkfall
REFERENCE IMAGE: /Users/seamus/Documents/prometheus/inkfall/x-docs/reference/visual-refernce.png
VISUAL PARITY PLAN: /Users/seamus/Documents/prometheus/inkfall/x-docs/visual-parity-plan.md
ASSETS LIBRARY: /Users/seamus/Documents/unity-assets-index (4,119 pre-downloaded Unity assets)
  Search: cd /Users/seamus/Documents/unity-assets-index && python3 search.py text "your query"

## Gap to Fix

CURRENT STATE: [Describe exactly what is wrong — include score from scoring table]
TARGET STATE: [Describe what the reference shows for this element]
SCORE TO BEAT: [current score] → must reach ≥ 8/10

## Files to Examine

[List all relevant .cs, .shader, .mat, .asset, .unity files]

## Your Task

1. Read the reference image description above
2. Search the assets library for any relevant textures, materials, or prefabs before creating anything new
3. Examine the listed files to understand the current implementation
4. Diagnose the root cause — what specifically causes this gap?
5. Propose your fix (what files change and how)
6. Search assets library for anything you need before writing new code/assets
7. Implement the fix, respecting all project standards below
8. Run: ./agent-bridge.sh health — report result
9. Run: ./agent-bridge.sh compile — report result
10. Report: what changed, what the root cause was, confidence that the gap is closed

## Agent Bridge (use this to verify your fix)

./agent-bridge.sh health       # must show 0 new violations
./agent-bridge.sh compile      # must show 0 errors

If Unity is open: use MCP tools for instant visual feedback instead of re-running agent-bridge gameplay.

## Project Standards (hard limits — violations block the build)

- No file over 300 lines (Editor/UI: 200–250, VFX/visual: 200)
- No Debug.Log without #if UNITY_EDITOR
- No magic numbers — use GameConstants.cs constants or ScriptableObject fields
- No GetComponent in Update/FixedUpdate/LateUpdate — cache in Awake
- No FindObjectOfType in Update loops
- All VFX event-driven via EventBus<T>
- Pool all spawned objects via ObjectPool — never Instantiate/Destroy in gameplay
- Subscribe EventBus in OnEnable, unsubscribe in OnDisable
- URP 17.3.0 on Unity 6 — all shaders must be URP-compatible
- Shaders must support SRP Batcher: use UnityPerMaterial CBUFFER
- Partial classes named: TypeName.Category.cs (e.g. BalloonWall.Materials.cs)
```

---

# Key File Map

## Balloon Shape
```
Assets/Scripts/BalloonGame/Visuals/BalloonMeshGenerator.cs   # CRITICAL: mesh shape generation
```

## Balloon Shaders and Materials
```
Assets/Shaders/BalloonLit.shader              # Primary surface shader
Assets/Shaders/BalloonEmblem.shader           # Emblem overlay shader
Assets/Materials/BalloonBlue.mat
Assets/Materials/BalloonGreen.mat
Assets/Materials/BalloonPurple.mat
Assets/Materials/BalloonRed.mat
Assets/Materials/BalloonYellow.mat
```

## Balloon System
```
Assets/Scripts/BalloonGame/Balloons/BalloonWall.cs            # Grid orchestration
Assets/Scripts/BalloonGame/Balloons/BalloonWall.Materials.cs  # Per-type material assignment
```

## VFX
```
Assets/Scripts/BalloonGame/VFX/               # Paint splash, splatter, drips
Assets/Scripts/BalloonGame/ScreenFX/          # Screen shake, vignette, slow-mo
```

## Environment
```
Assets/Scripts/BalloonGame/Visuals/           # Board, floor, lighting rig, camera
```

## UI
```
Assets/Scripts/BalloonGame/UI/                # HUD, overlays, screens
Assets/Scripts/BalloonGame/Roguelite/UI/      # Perk selection, room intro
```

## Configuration
```
Assets/ScriptableObjects/GameConfig.asset     # Runtime tuning (all numbers live here)
Assets/Settings/Rendering/InkshotPipeline.asset
Assets/Scenes/InkshotScene.unity
ProjectSettings/QualitySettings.asset
```

---

# Visual Quality Checklist

Check each against the reference. Score 10 only when it is genuinely indistinguishable.

**Balloon Shape**
- [ ] Standard round teardrop silhouette — no hearts, stars, animals, or abstract shapes
- [ ] Balloons are plump and full — not thin or elongated

**Balloon Surface**
- [ ] Strong specular highlight (bright white point visible on surface)
- [ ] Vivid saturated colors: blue, purple, teal, green, hot pink, gold, black
- [ ] Slight rim/fresnel glow visible at balloon edges
- [ ] Gold balloon visibly metallic and shiny
- [ ] Black/hazard balloon dark and low-gloss

**Balloon Emblems**
- [ ] Each balloon type displays a distinct metallic icon (crown, sun/burst, shield, anchor, warning triangle)
- [ ] Emblems are readable at normal viewing distance
- [ ] Emblem color contrasts with balloon body color

**Board and Environment**
- [ ] Dark cork/wood texture visible on board background
- [ ] Weathered metal frame around board perimeter
- [ ] Neon green paint drips visible on board surface (persistent, thick)
- [ ] Floor shows bowling lane stripe perspective below the board
- [ ] Fog/atmosphere visible at frame edges
- [ ] Neon lighting (green, magenta, cyan) contributes color and glow

**HUD**
- [ ] "ROOM X" badge top-left (steel-plate style, dark industrial)
- [ ] "SCORE: X,XXX / TARGET: X,XXX" centered with progress bar
- [ ] "DARTS: X" top-right
- [ ] "CURRENCY: XXX" with coin icon
- [ ] All HUD elements on dark panel backing — not floating on air

**VFX — Paint Explosion**
- [ ] Explosion is dramatically large (covers multiple balloons)
- [ ] Neon green + hot pink colors
- [ ] Includes paint drip/streak elements
- [ ] Darts visible within the blast area

**Perk Cards**
- [ ] Dark card backgrounds with rarity-colored borders (white/blue/purple)
- [ ] Large readable icon in upper card area
- [ ] Card name in bold caps
- [ ] Description text legible
- [ ] Three cards shown side-by-side

**Room Transition Screen**
- [ ] Full-screen dark moody backdrop
- [ ] "ROOM X" headline large, white, bold
- [ ] Room name subtitle visible
- [ ] Target score displayed
- [ ] Balloon type preview panel present
- [ ] Perk icon badges shown at bottom
