## 2026-03-15 16:04

## Perspective camera, 3D dart flight, collision fix, dev mode skip, atmosphere removal

## Issues identified (if relevant)
1. Orthographic camera produced a flat scene with no depth cues despite imported 3D balloon assets.
2. After switching to perspective camera, everything sat at Z=0 so the view looked identical to orthographic.
3. After pushing balloons to Z=4 and adding forward dart velocity, darts flew through balloons without hitting them.
4. Root cause: dart CapsuleCollider was disabled until Y-velocity peak (a 2D-era mechanic). By peak, the dart had already passed through the Z=4 balloon plane.
5. AtmosphereController particle mist/haze layers obscured the board with dark semi-transparent particles.
6. Title screen and room intro screens slowed dev iteration.

## Issue resolution implemented (if there was an issue)
1. **Perspective camera**: BalloonCamera switched from orthographic to perspective (60 FOV), with adaptive Z-distance calculation to frame TARGET_WORLD_WIDTH across aspect ratios.
2. **3D depth**: Balloons placed at Z=4 (BOARD_Z), darts launch from Z=0 with forward velocity (DART_FORWARD_SPEED=8). Environment (cork board, back wall, frame, walls) repositioned to board Z depth. Splatters placed at board Z.
3. **Collision fix**: Removed peak-gating — CapsuleCollider now always enabled on launch. Removed _hasPassedPeak field and CheckPeakOverlap fallback. With 4 units of Z separation, there's no risk of hitting balloons at launch position.
4. **Atmosphere nuked**: AtmosphereController gutted to empty MonoBehaviour. No more mist/haze particles.
5. **Dev mode**: Added DEV_SKIP_INTRO flag — skips title screen, room intro, fade delays, and intro timer. Set to false to restore normal flow.
6. **3D dart rotation**: Changed from 2D atan2 Z-rotation to full 3D LookRotation with mesh-axis correction.
7. **Aim assist**: Increased AIM_ASSIST_MAX_DISTANCE from 5.5 to 12 to account for Z depth.

## File(s) modified:
- `Assets/Scripts/BalloonGame/Visuals/BalloonCamera.cs`
- `Assets/Scripts/BalloonGame/GameConstants.cs`
- `Assets/Scripts/BalloonGame/Darts/DartController.cs`
- `Assets/Scripts/BalloonGame/Darts/DartLauncher.cs`
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs`
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.cs`
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs`
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Frame.cs`
- `Assets/Scripts/BalloonGame/Visuals/AtmosphereController.cs`
- `Assets/Scripts/BalloonGame/Visuals/PostProcessingSetup.cs`
- `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs`
- `Assets/Scripts/BalloonGame/VFX/PersistentSplatterVFX.cs`
- `Assets/Scripts/BalloonGame/Editor/SceneBuilder.Environment.cs`
- `Assets/Scripts/BalloonGame/Editor/AgentBridge/SceneValidator.cs`
- `Assets/Scripts/BalloonGame/Editor/AgentBridge/ScreenshotCapture.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/TitleScreen.cs`
- `Assets/Scripts/BalloonGame/Roguelite/RunManager.cs`
- `Assets/Scripts/BalloonGame/Managers/BalloonGameManager.cs`
- `CLAUDE.md`
