# GUI, Layout & Atmosphere Rework — Autonomous Agent Prompt

**Extends:** `x-docs/visual-parity-prompt.md` — that document defines the feedback loop, Agent Bridge commands, MCP tooling, safety rails, stall detection, and changelog protocol. All of those apply here. This prompt overrides only the **mission, rubric, and pass sequence**.

## Mission

The game is portrait (9:16) and stays that way. But the current layout is broken — the HUD bar is comically oversized relative to the gameboard, the atmosphere is a purple haze instead of noir carnival darkness, and objects are scattered too far apart in world space. Fix the proportions, compact the GUI, and clean up the atmosphere so the portrait frame looks like a polished mobile game.

## Reference Screenshots

Study these — they show the current state and its problems:

| File | What it shows |
|------|---------------|
| `x-docs/reference-screenshots/scene-view-close-purple-haze.png` | Scene view: balloon board with purple hazy atmosphere bleeding below. The "atmosphere" is diffuse purple everywhere below the board instead of dark noir. Neon light gizmos visible. |
| `x-docs/reference-screenshots/scene-view-zoomed-out-scale.png` | Zoomed WAY out: entire game board is tiny like an ant. A dart ammo object is far to the left. White axis lines show massive scale mismatch. Objects placed too far apart in world space. |
| `x-docs/reference-screenshots/scene-view-gui-bar-oversized.png` | Zoomed out showing GUI canvas: the HUD top bar is enormous relative to game content. Canvas wireframe extends far beyond the game world. "SCORE 0", "ROOM 1" etc are massively oversized for the gameplay area. |
| `x-docs/reference-screenshots/game-view-layout-issues.png` | Game view in play mode (the actual player experience): giant cyan horizontal line across top of balloons, oversized HUD bar at top, balloon board only fills ~35% of screen width (center-left), massive black void on right side, launch lane below. The portrait content doesn't fill its own viewport well. |
| `x-docs/reference/visual-refernce.png` | The original target reference — study the proportions and layout. |

## The Problems (Priority Order)

### P0: Oversized HUD / GUI Proportions
**Current:** The HUD canvas uses 1080x1920 reference resolution with a top bar at 68px height, but the bar and its elements (ROOM badge, SCORE counter, dart count) are disproportionately large relative to the actual gameboard below. The HUD dominates the top of the screen instead of being a thin, elegant status strip. There's also a giant cyan horizontal line across the top row of balloons that shouldn't be there.

**Target:** Compact portrait HUD that leaves maximum room for gameplay. Think polished mobile portrait games — thin top status bar, small but readable info, the gameboard is the hero, not the chrome around it.

### P1: Purple Atmosphere Haze
**Current:** The AtmosphereController creates mist particles with purple/magenta/cyan color oscillation. Combined with fog layers from EnvironmentBuilder and neon light bleed, the result is a diffuse purple haze below the board that doesn't read as "noir carnival" — it reads as "broken rendering."

**Target:** Dark, moody noir carnival atmosphere. The area around the board should feel like a dimly lit arcade/carnival booth. No purple bleed. Blacks should be BLACK, with only intentional neon accent lighting from the rig.

### P2: Scale / Spatial Mismatch
**Current:** The game board is 8x8 world units centered at origin. But other objects (dart ammo, UI elements) are positioned far away in world space. When you zoom out in Scene View, the board is tiny relative to the world. This suggests stray objects or incorrect positioning.

**Target:** All game objects should be spatially compact. The board, launch area, and dart staging should be close together. No stray objects floating far from the play area. Scene View should show a tidy, organized scene at a reasonable zoom level.

### P3: Viewport Fill / Proportions
**Current:** The balloon board + launch lane don't fill the 9:16 portrait viewport well. There's wasted black space on the sides and the gameboard feels small relative to the screen. The content should feel tight and full — the board should be the dominant visual element.

**Target:** The board, frame, launch lane, and HUD should fill the portrait viewport proportionally. Minimal wasted space. The camera ortho size and board dimensions should be tuned so the game content fills the available portrait frame.

## Before You Write Any Code

Read these in order:

| # | What | Path | Why |
|---|------|------|-----|
| 1 | **Original loop prompt** | `x-docs/visual-parity-prompt.md` | Defines feedback loop, Agent Bridge, MCP, safety rails, stall detection. All apply. |
| 2 | **This prompt** | (you're reading it) | Overrides mission, rubric, pass sequence for this phase. |
| 3 | **Visual parity progress** | `x-docs/visual-parity-progress.md` | 7 passes of prior work, current 7.65/10 score, lessons learned. |
| 4 | **Reference screenshots** | `x-docs/reference-screenshots/` | The 4 screenshots showing current problems. |
| 5 | **Visual reference** | `x-docs/reference/visual-refernce.png` | Target aesthetic. |
| 6 | **Project standards** | `CLAUDE.md` | Hard limits, architecture, Agent Bridge commands. |
| 7 | **Recent changelogs** | `x-docs/changelogs/` | Read the 2-3 most recent. |

## The Loop

Same as `x-docs/visual-parity-prompt.md` — READ → PLAN → EXECUTE → VERIFY → SCORE → WRITE → DECIDE. Use `./agent-bridge.sh compile`, `health`, `gameplay`, `dart-test` as defined there. All safety rails (stall detection, token budget, anti-patterns) apply.

## Audit Rubric (overrides the visual-parity rubric for this phase)

Score each category 1-10. Compute weighted total.

### 1. HUD & GUI Proportions (weight: 25%)
- Is the HUD compact — thin top bar that doesn't dominate the screen?
- Is essential info (score, room, darts) readable but small?
- Are HUD elements properly sized relative to the gameboard area?
- No stray visual artifacts (cyan lines, oversized elements)?
- Does it feel like a polished mobile portrait game?

### 2. Atmosphere & Mood (weight: 25%)
- Is the atmosphere dark and moody (noir carnival), NOT purple-hazed?
- Are neon accents visible as intentional lighting, not diffuse bleed?
- Is the background behind/around the board dark (near-black)?
- Does the environment feel like a dimly lit arcade booth?
- No purple/magenta haze bleeding below or around the board?

### 3. Viewport Fill & Proportions (weight: 20%)
- Does the gameboard fill the portrait viewport well (minimal wasted space)?
- Is the board the dominant visual element, not the HUD?
- Is the launch lane proportional to the board (not too tall)?
- Does the camera ortho size frame everything tightly?

### 4. Spatial Coherence (weight: 15%)
- Are all game objects (board, lane, darts) spatially compact?
- No stray objects positioned far from the game area?
- Does Scene View show a tidy scene at reasonable zoom?
- Are world-space coordinates reasonable?

### 5. Integration & No Regressions (weight: 15%)
- Do all existing visual systems (balloons, paint, VFX) still work?
- Are physics trajectories still correct (30/60/100% hit different rows)?
- No visual regressions from the 7.65/10 visual parity baseline?
- Does the overall composition feel cohesive?

**Weighted total:**
```
total = hud×0.25 + atmosphere×0.25 + viewport×0.20 + spatial×0.15 + integration×0.15
```

## Recommended Pass Sequence

### Pass 1: Identify & Remove the Cyan Line + Stray Objects
**Goal:** Quick wins — remove the giant cyan horizontal line visible in the game view, find and fix stray objects positioned far from the game area.

**Investigation:**
- The cyan horizontal line across the top row of balloons in the game view screenshot needs to be identified. It could be a debug line, a guide line, a slingshot visual, or a UI element rendered at the wrong position. Find it and remove/fix it.
- Check Scene hierarchy for stray objects far from origin (the dart ammo object visible in the zoomed-out screenshot).
- Clean up any debug visuals or mispositioned elements.

**Key files to investigate:**
- `LaunchLaneVisuals.cs` — has guide lines
- `SlingshotVisuals.cs` — has rubber band line and guide lines
- `EnvironmentBuilder.cs` — creates panels and frame
- Scene hierarchy via `./agent-bridge.sh validate` or MCP

**Verification:** `./agent-bridge.sh gameplay` — cyan line should be gone, scene should be cleaner.

### Pass 2: HUD Compaction (P0)
**Goal:** Shrink the HUD so the gameboard is the hero.

**Changes:**
- `InGameHUD.Builder.cs`: Reduce top bar height (68px → 36-40px). Shrink font sizes. Make room badge smaller. Tighten margins.
- `GameConstants.cs`: Reduce `HUD_TOP_MARGIN` (48 → 20-24px), `HUD_SIDE_MARGIN` (36 → 16-20px).
- Consider whether some HUD elements can be repositioned or combined to save space.

**Design target:** The HUD should occupy <5% of screen height in portrait. Room badge, score, and dart count should be visible but not dominant.

**Verification:** HUD is ScreenSpaceOverlay so won't appear in `./agent-bridge.sh gameplay` captures. Verify via code review or MCP screenshot if Unity is open.

### Pass 3: Atmosphere Cleanup (P1)
**Goal:** Transform the purple haze into dark noir carnival atmosphere.

**Changes:**
- `AtmosphereController.cs`: Drastically reduce mist opacity (alpha 0.12 → 0.03-0.04). Replace purple/magenta color oscillation with very subtle dark blue-gray `(0.08, 0.08, 0.12, 0.03)`. Or disable entirely and see if the scene looks better without it.
- `EnvironmentBuilder.cs`: Reduce or remove `FogLayerTop` and `FogLayerBottom` panels — these semi-transparent quads contribute to the purple tint. If needed, replace with much darker, less opaque versions.
- `NeonLightRig.cs`: Verify light ranges don't bleed into areas where they create purple haze. Consider reducing intensity or range further. Lights should create focused accent pools on the balloon board, not diffuse wash below it.

**Target aesthetic:** "Dimly lit carnival booth at night." Only color from:
1. The balloons themselves (vivid, saturated)
2. The neon light rig (focused accent spots)
3. The floor stripes in the launch area

Everything else should be dark — black or near-black.

**Verification:** `./agent-bridge.sh gameplay` — no purple haze, dark moody feel.

### Pass 4: Viewport Proportions (P3)
**Goal:** Tune camera and board dimensions so the game fills the portrait viewport well.

**Tuning levers:**
- `CAMERA_ORTHO_SIZE` (currently 10) — reducing this zooms in, making the board fill more of the viewport.
- `BOARD_LEFT/RIGHT/TOP/BOTTOM` — board position and size.
- `LANE_TOP/LANE_BOTTOM` — launch area height (currently 8 units tall, same as the board — could be compressed).
- Camera Y position — moving camera up/down shifts what's centered in the viewport.

**Key insight:** The launch lane is 8 units tall (same height as the board!) but it's mostly empty space with just a few guide lines. Compressing the lane height would let the board take up more of the viewport. The slingshot pull mechanic doesn't need 8 units of vertical space.

**Verification:** `./agent-bridge.sh gameplay` + `./agent-bridge.sh dart-test` — viewport fills well AND physics still work.

### Pass 5: Integration & Polish
**Goal:** Verify everything works together. Fix regressions.

**Checklist:**
- [ ] `./agent-bridge.sh compile` → 0 errors
- [ ] `./agent-bridge.sh health` → 0 violations
- [ ] `./agent-bridge.sh dart-test` → distinct trajectories at 30/60/100%
- [ ] `./agent-bridge.sh gameplay` → compact HUD, dark atmosphere, no purple haze, board fills viewport
- [ ] Balloon visuals still look correct
- [ ] Paint system still works
- [ ] Screen effects still work
- [ ] No stray objects in scene

## Key Files

| File | What to change | Priority |
|------|---------------|----------|
| `InGameHUD.Builder.cs` | Top bar height, font sizes, margins, element layout | Pass 2 |
| `GameConstants.cs` | HUD margins, camera ortho size, lane dimensions | Pass 2, 4 |
| `AtmosphereController.cs` | Mist density/color/opacity — reduce or disable | Pass 3 |
| `EnvironmentBuilder.cs` | Fog layers — reduce or remove | Pass 3 |
| `NeonLightRig.cs` | Light ranges/intensity — more focused | Pass 3 |
| `BalloonCamera.cs` | Ortho size, camera Y position | Pass 4 |
| `LaunchLaneVisuals.cs` | Investigate cyan line, lane decorations | Pass 1 |
| `SlingshotVisuals.cs` | Investigate cyan line, slingshot visuals | Pass 1 |

## Anti-Patterns

All anti-patterns from `x-docs/visual-parity-prompt.md` apply. Additional:

1. **Do NOT change the portrait orientation.** The game is 9:16 portrait and stays that way.
2. **Do NOT regress the visual parity work.** Physics, balloon rendering, paint VFX, dart visuals were tuned over 7 passes to 7.65/10. Protect that score.
3. **URP materials need explicit transparency setup.** Every `new Material(shader)` for particles/VFX needs `_Surface=1`, blend modes, `_ZWrite=0`, `_SURFACE_TYPE_TRANSPARENT` keyword. See Pass 7 changelog.
4. **The launch lane might not need to be 8 units tall.** The pull mechanic works in screen space (touch input), not world space. Compressing the lane vertically is a safe way to give the board more viewport real estate.

## Progress Log

Create and maintain `x-docs/gui-layout-pass/progress.md` using the same format as `x-docs/visual-parity-progress.md`. Include pass summary table, per-pass details, lessons learned.

## Done Criteria

You are done when ALL of these are true:

1. `./agent-bridge.sh compile` → 0 errors
2. `./agent-bridge.sh health` → 0 violations
3. `./agent-bridge.sh dart-test` → physics still correct (30/60/100% hit different rows)
4. `./agent-bridge.sh gameplay` → compact HUD, dark atmosphere, no purple haze, board fills viewport
5. Self-audit weighted score ≥ 8.0/10
6. Progress log updated with final pass results
7. Changelog written to `x-docs/changelogs/`
8. Codebase clean — no half-finished work, no commented-out code
