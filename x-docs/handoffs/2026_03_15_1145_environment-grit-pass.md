# Environment & Atmosphere Grit Pass — Agent Prompt

## Mission

Transform Inkshot from a flat "developer balloon wall" into the dark, moody, neon-lit carnival from `x-docs/reference/visual-refernce.png`. Focus exclusively on **Environment & Framing**, **Balloon Visuals (perspective/depth)**, and **Atmosphere**. Do not touch physics, input, UI logic, or game mechanics. Work autonomously — do not ask questions, do not stop short.

## Your Eyes: The Current State

The game currently looks like a brightly-lit grid of flat, uniform balloons pinned to a flat brown rectangle. There is no depth, no atmosphere, no drama. Specifically:

- **Cork board** is a flat uniform brown quad — no texture variation, no weathering, no grain
- **Metal frame** reads as thin gray lines — no depth, no bolts visible, no industrial weight
- **Neon lights exist in code** but their effect is barely visible — balloons look uniformly lit from the front
- **Balloons have zero depth perspective** — the top row looks the same size as the bottom row, creating a flat wall rather than a board receding into space
- **Atmosphere is invisible** — mist particles are alpha 0.03, functionally transparent
- **Launch lane** is a black void with thin colored stripes — no floor texture, no depth
- **No visible vignetting** — edges of the screen are as bright as the center
- **Balloon colors are saturated but uniformly lit** — no shadow side, no rim highlights catching neon light

## The Reference Target

Study `x-docs/reference/visual-refernce.png` carefully. The environment shows:

1. **A dark, moody cork board** — not flat brown but a rich, textured surface with visible grain and wear
2. **Heavy industrial metal frame** — thick, weathered, with clearly visible corner bolts that look 3D
3. **Dramatic neon lighting** — magenta and cyan light sources create colored shadows and rim highlights on balloon edges. The lighting is NOT uniform — it creates pools of color across the board
4. **Strong perspective depth** — balloons at the top of the board are noticeably smaller than those at the bottom, creating a sense that the board recedes away from the viewer
5. **Atmospheric fog/mist** — visible wisps of colored mist drifting across the scene, especially in the launch lane area
6. **Dark vignetting** — screen edges fade to near-black, focusing attention on the center
7. **Neon paint splatters** — vivid glowing marks on the cork board from popped balloons
8. **Floor stripes in the launch lane** — multi-colored neon stripes on a dark floor with visible depth

## Scope — What To Fix (In Priority Order)

### Priority 1: Perspective Depth on Balloons

The most impactful single change. Currently `PERSPECTIVE_MIN_SCALE = 0.88f` and `PERSPECTIVE_SCALE_RANGE = 0.12f` in `GameConstants.cs`, meaning top-row balloons are only 88% the size of bottom-row ones. This is barely perceptible.

**Target:** Top row should be noticeably smaller (~70-75% of bottom row size). The grid should read as a wall receding into the distance.

**Files:**
- `Assets/Scripts/BalloonGame/GameConstants.cs` — `PERSPECTIVE_MIN_SCALE`, `PERSPECTIVE_SCALE_RANGE`, `PERSPECTIVE_HORIZONTAL_PINCH`, `PERSPECTIVE_VERTICAL_COMPRESSION`
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.cs` — applies the perspective scaling per row

**Approach:**
1. Reduce `PERSPECTIVE_MIN_SCALE` to ~0.72 (top row = 72% of bottom)
2. Increase `PERSPECTIVE_HORIZONTAL_PINCH` to ~0.10 (columns converge slightly toward top)
3. Increase `PERSPECTIVE_VERTICAL_COMPRESSION` to ~0.12 (rows closer together at top)
4. Verify with `./agent-bridge.sh gameplay` — the grid should look like a wall viewed at an angle

### Priority 2: Atmospheric Fog & Mist Visibility

Currently `AtmosphereController.cs` creates mist with alpha 0.02-0.03 — invisible. The reference shows clearly visible fog wisps.

**Files:**
- `Assets/Scripts/BalloonGame/Visuals/AtmosphereController.cs` — particle settings and colors

**Approach:**
1. Increase mist alpha from 0.03 to 0.08-0.12 (visible but not obscuring)
2. Increase mist particle size from 2.5 to 3.5-4.0
3. Make the color oscillation more pronounced (more magenta/cyan variation)
4. Add a second particle layer with larger, slower particles for "background haze"
5. Consider additive blending instead of alpha blend for neon glow-through effect

### Priority 3: Neon Lighting Impact

The `NeonLightRig.cs` has 9 lights (2 directional + 7 point lights) but their visual impact is minimal. The BalloonLit shader and URP settings may be dampening the effect.

**Files:**
- `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs` — light positions, colors, intensities, ranges
- `Assets/Shaders/BalloonLit.shader` — how balloons respond to lights
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.Materials.cs` — balloon material properties

**Approach:**
1. Increase neon light intensities (currently 0.6-1.6, try 2.0-3.0 for point lights)
2. Increase neon light ranges (currently 5-6, try 8-10 to reach more of the board)
3. Check that BalloonLit shader actually responds to additional lights (URP per-object light limit may need adjustment)
4. Verify balloon materials have sufficient smoothness/metallic for specular highlights
5. Add slight color tinting to the key light — warm amber instead of pure white
6. Consider adding rim light contribution to the BalloonLit shader if not already present

### Priority 4: Cork Board & Frame Depth

The cork board is a flat brown quad. The frame is thin gray cubes. Neither reads as "industrial noir."

**Files:**
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs` — board and frame construction
- `Assets/Scripts/BalloonGame/GameConstants.cs` — board/frame dimensions

**Approach:**
1. Darken the cork board color — less flat brown, more dark chocolate (currently `0.22, 0.14, 0.08`, try `0.12, 0.07, 0.04`)
2. Add subtle noise/grain to the cork by using a procedural texture or vertex color variation
3. Increase frame thickness from 0.45 to 0.55-0.65 for more visual weight
4. Make frame darker and more metallic (currently flat gray `0.30, 0.27, 0.24` — try `0.18, 0.16, 0.14`)
5. Make bolt size larger (currently 0.20 — try 0.28-0.32) with brighter highlight color
6. Consider adding a subtle edge bevel effect by layering a slightly smaller, brighter frame piece on top

### Priority 5: Screen Vignetting & Atmosphere Quads

The fog overlay quads and vignette quads exist but are nearly invisible (alpha 0.12-0.15).

**Files:**
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs` — `BuildAtmosphere()` method

**Approach:**
1. Increase vignette alpha from 0.12 to 0.25-0.35
2. Make vignette quads wider (currently 3 units) — overlap more of the board edges
3. Increase fog layer alpha from 0.15 to 0.20-0.25
4. Add a gradient effect by stacking multiple fog layers with decreasing alpha

### Priority 6: Launch Lane Floor

The launch lane is a flat black rectangle with thin colored stripes. It should feel like a dark floor with neon accent lines.

**Files:**
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs` — `BuildFloorArea()` and `BuildLaneGuides()`

**Approach:**
1. Give the floor a slightly lighter color than pure black — very dark gray-brown
2. Make the colored stripes thicker (currently 0.10 height — try 0.15-0.20)
3. Add subtle glow quads behind the stripes (slightly larger, same color, lower alpha)
4. Lane guide lines (currently 0.06 wide cyan) should be more prominent

## Workflow

Follow the standard visual parity feedback loop from `x-docs/visual-parity-prompt.md`:

```
1. READ    — this handoff, reference image, changelogs
2. PLAN    — pick one priority at a time
3. EXECUTE — implement in focused phases (1-3 files per phase)
4. VERIFY  — ./agent-bridge.sh compile && ./agent-bridge.sh health
5. VERIFY  — ./agent-bridge.sh gameplay → read the output PNG
6. COMPARE — side-by-side with x-docs/reference/visual-refernce.png
7. ITERATE — if closer, move to next priority. If not, adjust values.
```

**One visual change at a time.** Change perspective scaling, verify. Then change atmosphere, verify. Don't batch.

**Kill stuck Unity before agent-bridge commands:**
```bash
pgrep -x Unity | head -1 | xargs -r kill 2>/dev/null; rm -f Temp/UnityLockfile
```

## Verification Commands

```bash
./agent-bridge.sh compile    # Must pass with 0 errors before moving on
./agent-bridge.sh health     # Must introduce no new violations
./agent-bridge.sh gameplay   # Captures game screenshot — THIS IS YOUR EYES
```

The gameplay screenshot lands at `Logs/agent-feedback/screenshots/gameplay_*.png`. **Read it after every visual change.** If you haven't looked at a screenshot, you are coding blind.

**Note:** `gameplay` uses Camera.Render + RenderTexture at 1080x1920 portrait. It captures the game world accurately but does NOT capture ScreenSpaceOverlay UI. Score environment by what you see in the screenshot, not by reading code.

## Hard Constraints

From `CLAUDE.md` — these are non-negotiable:

- No file over its line limit (Visuals: 200, GameConstants: 150, Balloons: 300)
- No magic numbers — use `GameConstants.cs` or ScriptableObject fields
- No `Debug.Log` without `#if UNITY_EDITOR`
- Zero compilation errors, zero health violations
- All VFX event-driven via EventBus, pooled via ObjectPool
- Visual changes MUST be verified via `./agent-bridge.sh gameplay`

## Key Files Quick Reference

| File | Lines | What |
|------|-------|------|
| `GameConstants.cs` | 150 | Perspective constants, board dims — AT LINE LIMIT |
| `EnvironmentBuilder.cs` | 200 | Cork, frame, floor, fog, vignettes — AT LINE LIMIT |
| `EnvironmentBuilder.Cleanup.cs` | 54 | Partial class for cleanup |
| `NeonLightRig.cs` | 47 | All neon/key/fill lights |
| `AtmosphereController.cs` | 75 | Mist particle system |
| `BalloonWall.cs` | 192 | Grid spawning + perspective scaling |
| `BalloonWall.Materials.cs` | 138 | Per-type balloon material setup |
| `BalloonLit.shader` | ? | Custom balloon shader (URP) |

**GameConstants.cs and EnvironmentBuilder.cs are at their line limits.** If you need to add constants, you must remove or consolidate existing ones. If EnvironmentBuilder needs more code, extract into a new partial class file (e.g., `EnvironmentBuilder.Atmosphere.cs`).

## What NOT To Touch

- Physics, input, dart launching, SlingshotInput
- UI screens, HUD layout, canvas hierarchy
- Game logic, scoring, combo system, perk system
- Camera viewport/aspect ratio (just completed — working correctly)
- File structure or architecture

## Done Criteria

1. `./agent-bridge.sh compile` → 0 errors
2. `./agent-bridge.sh health` → 0 violations
3. Gameplay screenshot shows:
   - Visible perspective depth on balloon grid (top row clearly smaller)
   - Visible atmospheric mist/fog
   - Neon-colored rim highlights on balloon edges
   - Darker, moodier cork board
   - Heavier metal frame with visible bolts
   - Screen-edge vignetting
4. Side-by-side comparison with reference shows clear improvement in "atmosphere" category
5. Codebase is clean — no commented-out code, no TODO markers

## Anti-Patterns (From Prior Passes)

1. Don't change capture method mid-session — use `./agent-bridge.sh gameplay` consistently
2. Don't batch visual changes — one change, one screenshot, one comparison
3. Don't spend >20 min on a single issue — try brute force, then document and move on
4. Don't over-tune values by reading code — iterate visually. Try a value, screenshot, adjust
5. Don't change things you can't verify — if the screenshot doesn't show it, don't score it
6. Agent-bridge fails? Kill Unity first: `pgrep -x Unity | head -1 | xargs -r kill 2>/dev/null; rm -f Temp/UnityLockfile`
