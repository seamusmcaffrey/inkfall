## 2026-03-16 14:48

## Unified dart origin, downward-only pull, and slingshot aim model overhaul

## Issues identified (if relevant)

1. **Dart spawn disconnected from slingshot**: `FIRE_ORIGIN` (0, 2.5, -4) was a separate constant from `LAUNCH_POSITION` (0, -0.5, -8). The dart materialized at a completely different screen position than where the slingshot was drawn, creating visual dissonance.

2. **Bottom-row targeting required dragging UP**: With the old directional aim model, hitting the bottom of the board required pulling the slingshot upward (inverted), which felt unnatural — the user had to move their thumb above the slingshot anchor.

3. **Board too low relative to slingshot**: `BOARD_BOTTOM=0` was only 0.5 units above `LAUNCH_POSITION.y=-0.5`, meaning every shot required significant upward velocity even for the lowest targets. No natural downward arcs were possible.

4. **Aim too twitchy**: The aim point snapped instantly to finger position, making precision targeting difficult on mobile.

## Issue resolution implemented (if there was an issue)

### Phase 1: Unified dart origin (removed FIRE_ORIGIN)
- Removed `FIRE_ORIGIN` constant entirely — all systems now use `LAUNCH_POSITION` as the single dart spawn/trajectory/aim-assist origin.
- Updated 8 files: `DartLauncher`, `SlingshotInput.Ballistics`, `SlingshotVisuals`, `AimAssist`, `DartPhysicsTestRunner`, `SceneViewDartTester`, `SlingshotInput`, `GameConstants`.
- Retuned physics for the longer flight path (12 units forward from Z=-8 to Z=4):
  - `DART_FORWARD_SPEED`: 14 → 12 (1.0s flight time)
  - `DART_GRAVITY`: -14 → -18 (stronger gravity for pronounced arcs)

### Phase 2: Board raised + slingshot repositioned
- `BOARD_BOTTOM`: 0 → 2, `BOARD_TOP`: 8 → 10 (shifted board up 2 units)
- `LAUNCH_POSITION.y`: -0.5 → 0.5 (raised slingshot 1 unit, `ROOM_FLOOR_Y + 2`)
- This gives natural arcs to all board positions and keeps the slingshot in the lower screen area.

### Phase 3: Downward-only pull with magnitude-based aim
- **Clamped pull to downward-only**: `rawPull.y = Mathf.Min(rawPull.y, 0f)` — physically impossible to drag the slingshot above its anchor point.
- **New aim Y model**: Pull magnitude (how far down you pull) maps to board height via `Lerp(virtualBottom, BOARD_TOP, power)`. Gentle pull = bottom row, strong pull = top row. Replaces the old directional Y mapping.
- **X aim unchanged**: Lateral pull direction still controls left/right aim (inverted, slingshot-style).
- Defensive double-clamp in `ComputeAimPoint` protects external callers like `SceneViewDartTester`.

### Phase 4: Minimum lob arc
- Added `MIN_LOB_HEIGHT = 1.5` — enforces a minimum peak height above origin so every shot has a visible upward arc before descending to its target.
- Ballistic solver recomputes flight time when lob override is active, adjusting forward speed so the dart still hits the exact target on the descending arc.

### Phase 5: Bottom-row shelf + power curve + aim smoothing
- Added `AIM_VERTICAL_SHELF = 4` — extends the aim Lerp below the board bottom, creating a wide range of gentle pulls that all target the bottom row without compressing the upper range.
- `pullSpeedExponent`: 1.45 → 3.0 — steep power curve means light pulls produce very low power, giving fine control over bottom-row targeting.
- Added `AIM_SMOOTH_SPEED = 5` — pull vector lerps toward finger position instead of snapping, making aim drift feel deliberate and controllable.

### Tuning knobs summary
| Constant | Value | Purpose |
|----------|-------|---------|
| `DART_FORWARD_SPEED` | 12 | Z velocity, determines flight time |
| `DART_GRAVITY` | -18 | Arc curvature |
| `MIN_LOB_HEIGHT` | 1.5 | Minimum arc peak above origin |
| `AIM_VERTICAL_SHELF` | 4 | Bottom-row dead zone width |
| `AIM_SMOOTH_SPEED` | 5 | Aim interpolation rate (lower = slower) |
| `pullSpeedExponent` | 3.0 | Power curve steepness (higher = more bottom-row range) |
| `MAX_PULL_DISTANCE` | 1.8 | Physical drag distance for full pull |
| `TRAJECTORY_DURATION` | 0.7 | How much of the arc the preview shows |

## File(s) modified:
- `Assets/Scripts/BalloonGame/GameConstants.cs` — Removed FIRE_ORIGIN, raised board, added MIN_LOB_HEIGHT, AIM_VERTICAL_SHELF, AIM_SMOOTH_SPEED; tuned DART_FORWARD_SPEED, DART_GRAVITY, TRAJECTORY_DURATION
- `Assets/Scripts/BalloonGame/Data/GameConfigSO.cs` — pullSpeedExponent 1.45 → 3.0
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs` — Downward-only pull clamp, aim smoothing via Lerp
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.Ballistics.cs` — Magnitude-based aim Y model, min lob arc solver, defensive pull clamp in ComputeAimPoint
- `Assets/Scripts/BalloonGame/Darts/DartLauncher.cs` — FIRE_ORIGIN → LAUNCH_POSITION
- `Assets/Scripts/BalloonGame/Darts/DartPhysicsTestRunner.cs` — FIRE_ORIGIN → LAUNCH_POSITION
- `Assets/Scripts/BalloonGame/Visuals/SlingshotVisuals.cs` — Unified origin for band + trajectory
- `Assets/Scripts/BalloonGame/AimAssist.cs` — FIRE_ORIGIN → LAUNCH_POSITION
- `Assets/Scripts/BalloonGame/Editor/SceneViewDartTester.cs` — FIRE_ORIGIN → LAUNCH_POSITION
