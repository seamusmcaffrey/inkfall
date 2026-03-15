## 2026-03-15 10:03

## GUI Layout Passes 3-4: Aggressive lane compression, HUD shrink, scene cleanup

## Issues identified (if relevant)
- Launch lane consumed ~35% of viewport, making the balloon board feel small relative to the screen
- HUD elements (top bar, badge, combo display, dart tray, RunHUD) were still oversized for a mobile portrait layout
- Stray root-level scene objects (LaunchOrigin, BackWall, LaneFloor, Directional Light) cluttered the scene hierarchy
- LaunchOrigin baked at y=-7.5 in scene YAML, mismatched with runtime position
- Neon accent lights were over-dimmed in prior passes, losing noir carnival character
- ComboDisplay used 48-96pt font range, disproportionately large

## Issue resolution implemented (if there was an issue)
- **Lane compression**: LANE_BOTTOM -5.0→-3.5, LAUNCH_POSITION y -3.5→-2.5, reducing lane from 4.2→2.7 units
- **Camera re-center**: CAMERA_Y_CENTER 1.5→2.5, shifting viewport weight toward the board (now fills ~80%)
- **HUD shrink**: TopBar 36→28px, Badge 30→22px, ProgressBar 6→4px, DartTray 240×44→180×32, all fonts reduced ~20% further, margins tightened (HUD_TOP_MARGIN 16→8, HUD_SIDE_MARGIN 16→10)
- **ComboDisplay**: BaseFontSize 48→32, MaxFontSize 96→64, glow panel 280×120→200×80
- **RunHUD**: Font sizes 24→16, color muted from InkCyan to (0, 0.55, 0.55, 0.7)
- **Stray root cleanup**: Added "LaunchOrigin" to EnvironmentBuilder.StaleVisualRoots destroy list
- **Scene fix**: Updated LaunchOrigin baked position in InkshotScene.unity from y=-7.5 to y=-2.5
- **Neon rebalance**: Neon intensities 1.2→1.4, mid 0.7→0.8, AmberBottom repositioned to y=-2.5
- **Side vignette**: Added subtle dark vignette panels at viewport edges for noir depth
- **Floor stripes**: Reduced from 6 to 4 with proportional spacing for shorter lane
- **Physics**: maxLaunchSpeed 18→19, MAX_PULL_DISTANCE 2.0→1.8, AIM_ACTIVATION_RADIUS 3.0→2.5
- **Verification**: compile 0 errors, health 0 violations, dart-test shows distinct trajectories at 30/60/100% pull

## File(s) modified:
- `Assets/Scripts/BalloonGame/GameConstants.cs`
- `Assets/Scripts/BalloonGame/UI/InGameHUD.cs`
- `Assets/Scripts/BalloonGame/UI/InGameHUD.Builder.cs`
- `Assets/Scripts/BalloonGame/UI/ComboDisplay.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/RunHUD.cs`
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs`
- `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs`
- `Assets/ScriptableObjects/GameConfig.asset`
- `Assets/Scenes/InkshotScene.unity`
