## 2026-03-15 16:30

## 3D room environment and natural dart launch positioning

### Room Environment
- Built a 5-plane carnival booth room (back wall, two side walls, floor, ceiling) using rotated Quads in a new `EnvironmentBuilder.Room.cs` partial class
- Added baseboard trim (Cubes) along back and side walls for detail
- Added procedural stucco/plaster wall texture (`GenerateWallTexture`) with Perlin noise grain and vertical gradient
- Replaced flat 3-light directional rig with booth-appropriate lighting: warm key + cool fill + rim directionals, overhead spotlight aimed at the board, ambient point light near room entrance
- Removed old flat background elements: `BuildFloorArea()` (BackWall quad, LaneFloor quad, lane stripes) and `BuildLaneGuides()` — replaced by room geometry
- Room constants: `ROOM_BACK_Z=6`, `ROOM_FRONT_Z=-2`, `ROOM_HALF_WIDTH=7`, `ROOM_FLOOR_Y=-1.5`, `ROOM_CEILING_Y=10.5`

### Dart Launch Repositioning
- Moved `LAUNCH_POSITION` from `(0, -2.5, 0)` to `(0, -0.5, -8)` — near floor level, 12 units from the board, like standing at a carnival booth counter
- Increased `DART_FORWARD_SPEED` from 8 to 14 to cover the longer throw distance in ~0.85 seconds
- Added upward-only velocity clamp (`velocity.y = Max(0)`) so darts can never fire into the floor — applied after AimAssist
- Fixed `ScreenToWorld` to project touches to the launch Z plane (Z=-8) instead of Z=0
- Increased `AIM_ASSIST_MAX_DISTANCE` from 12 to 20 to cover the longer throw distance
- Added editor gizmo (yellow wireframe sphere + red crosshair + "DART LAUNCH" label) on `SlingshotInput` so the launch point is visible in Scene view during both edit and play mode

### Bug Fix
- `DartPhysicsTestRunner` was firing darts with `Vector3.up * speed` (no Z velocity) — darts never reached the board. Fixed to include `DART_FORWARD_SPEED` in the Z component.

## Issues identified (if relevant)
- DartPhysicsTestRunner had a pre-existing bug where test darts had no forward Z velocity, meaning `./agent-bridge.sh dart-test` darts would fly straight up and never reach the balloon board. This was masked when launch was at Z=0 (close to board) but became obvious at Z=-8.
- ScreenToWorld was projecting touches to the Z=0 plane while the launch position was at Z=-8, causing the activation zone to be misaligned with the visual launch point in screen space.

## Issue resolution implemented (if there was an issue)
- Added `GameConstants.DART_FORWARD_SPEED` as the Z component in DartPhysicsTestRunner velocity construction.
- Changed ScreenToWorld to use `Abs(camera.z - LAUNCH_POSITION.z)` for the projection distance.
- Moved the Y-clamp to after AimAssist so slerp can't reintroduce negative Y velocity.

## File(s) modified:
- `Assets/Scripts/BalloonGame/GameConstants.cs`
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs`
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Room.cs` (new)
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Room.cs.meta` (new)
- `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs`
- `Assets/Scripts/BalloonGame/Visuals/ProceduralTextures.cs`
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs`
- `Assets/Scripts/BalloonGame/Darts/DartPhysicsTestRunner.cs`
