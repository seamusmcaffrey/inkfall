## 2026-03-16 20:30

## Aim-point ballistic model, split-origin dart spawn, and physics sync fix

## Issues identified (if relevant)
1. Old speed-based velocity model produced Vy values (10-40 m/s) wildly mismatched with board geometry (~5-15 needed). Darts overshot or undershot depending on pull strength.
2. Darts spawned at floor level (LAUNCH_POSITION) instead of mid-screen, making the slingshot feel disconnected from dart flight.
3. Even after changing spawn position to FIRE_ORIGIN, darts still appeared at floor level. Root cause: ObjectPool keeps darts parented to a pool container, and when `Rigidbody.isKinematic` flips to `false` in `Launch()`, the physics engine snaps to its cached internal position rather than the updated `transform.position`.
4. Rigidbody linear damping (0.1) caused ~8% speed loss over flight, producing systematic aim-low bias of ~0.4 units.
5. Gravity too weak (-12) meant darts were still rising at board impact for center shots.

## Issue resolution implemented (if there was an issue)
1. **Aim-point ballistic model**: Pull direction/strength maps to a target point on the board plane. Ballistic velocity is computed from `FIRE_ORIGIN` to that aim point accounting for gravity and forward speed. Extracted to `SlingshotInput.Ballistics.cs` partial class.
2. **Split origin architecture**: `LAUNCH_POSITION` (0, -0.5, -8) remains the slingshot touch zone at screen bottom. New `FIRE_ORIGIN` (0, 2.5, -4) is where darts actually spawn at screen center. Slingshot visuals use LAUNCH_POSITION; trajectory/dart use FIRE_ORIGIN.
3. **Physics sync fix**: Darts are now unparented from pool container on spawn (`SetParent(null, false)`). Both `transform.position` AND `Rigidbody.position` are explicitly set before `isKinematic` is disabled, ensuring the physics engine starts from the correct world position.
4. **Damping reduction**: Linear damping reduced from 0.1 to 0.02 to eliminate speed loss bias.
5. **Gravity tuned**: Changed from -12 to -14 m/s² so dart arc peaks slightly before board impact.
6. **Aim assist reworked**: Changed from angle-based velocity nudging to distance-based aim-point snapping within `AIM_ASSIST_SNAP_RADIUS` on the board plane.
7. **Scene View dart tester**: New editor tool (`SceneViewDartTester.cs`) for click-drag dart testing in Scene View during Play Mode.
8. **Gizmo update**: SlingshotInput now draws two gizmos — yellow "SLINGSHOT INPUT" at pull zone, cyan "DART FIRE ORIGIN" at spawn point.

## File(s) modified:
- `Assets/Scripts/BalloonGame/GameConstants.cs` — Added FIRE_ORIGIN, BOARD_CENTER_X/Y, AIM_RANGE_X/Y, AIM_ASSIST_SNAP_RADIUS, AIM_CLAMP_MARGIN; tuned DART_GRAVITY to -14, TRAJECTORY_DURATION to 1.2
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs` — Made partial, updated gizmos to show both origins
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.Ballistics.cs` — NEW: aim-point model (ComputeAimPoint, ComputeBallisticVelocity, PreviewVelocity)
- `Assets/Scripts/BalloonGame/Darts/DartLauncher.cs` — Spawn at FIRE_ORIGIN, unparent from pool, explicit Rigidbody position sync, damping 0.02
- `Assets/Scripts/BalloonGame/Darts/DartController.cs` — Rigidbody position sync before isKinematic flip in Launch()
- `Assets/Scripts/BalloonGame/Darts/DartPhysicsTestRunner.cs` — Updated to use aim-point model and FIRE_ORIGIN
- `Assets/Scripts/BalloonGame/AimAssist.cs` — Added AdjustAimPoint() for distance-based board-plane snapping
- `Assets/Scripts/BalloonGame/Visuals/SlingshotVisuals.cs` — Split origin: band uses LAUNCH_POSITION, trajectory uses FIRE_ORIGIN
- `Assets/Scripts/BalloonGame/Visuals/SlingshotVisuals.Dots.cs` — Board-plane clipping for trajectory dots
- `Assets/Scripts/BalloonGame/Data/GameConfigSO.cs` — Legacy tooltips on minLaunchSpeed/maxLaunchSpeed
- `Assets/Scripts/BalloonGame/Editor/SceneViewDartTester.cs` — NEW: Scene View dart firing tool
