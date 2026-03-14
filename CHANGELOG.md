# Changelog

## Task: Build Inkshot Balloon-Dart Prototype and Stabilize Gameplay - 2026-03-13

### Scene Setup Updates
- Added [Assets/Scripts/BalloonGame/GameConstants.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/GameConstants.cs), [Assets/Scripts/BalloonGame/BalloonCamera.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/BalloonCamera.cs), and [Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs) to build and tune the new `InkshotScene`.
- Generated [Assets/Scenes/InkshotScene.unity](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scenes/InkshotScene.unity) with portrait orthographic camera, frame walls, back wall, lane floor, launch origin, slingshot controller, balloon wall, and game manager roots.
- Created new wall and floor materials plus wall physics assets under `Assets/Materials/`.
- Added an explicit environment layer constant and assigned wall/floor scene objects to it so projectiles use the existing physics matrix correctly.

### Balloon Wall Updates
- Added [Assets/Scripts/BalloonGame/BalloonData.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/BalloonData.cs), [Assets/Scripts/BalloonGame/BalloonNode.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/BalloonNode.cs), and [Assets/Scripts/BalloonGame/BalloonWall.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/BalloonWall.cs).
- Implemented runtime generation of an 8x9 balloon grid with five colors, row-based scale changes, horizontal pinch, atmospheric darkening, and kinematic rigidbodies for Unity collision.
- Adjusted spacing from edge-based placement to inset slot centers so the balloon wall fits the frame more cleanly.
- Added safe wall clearing/regeneration logic for restarts and editor-side scene rebuilds.

### Slingshot and Dart Updates
- Added [Assets/Scripts/BalloonGame/SlingshotInput.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/SlingshotInput.cs) and [Assets/Scripts/BalloonGame/SlingshotVisuals.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/SlingshotVisuals.cs) for drag-to-aim input, rubber-band visualization, and trajectory preview using the Input System.
- Added [Assets/Scripts/BalloonGame/DartLauncher.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/DartLauncher.cs) and [Assets/Scripts/BalloonGame/DartController.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/DartController.cs) for dart spawning, Rigidbody launch, velocity-facing rotation, bounce/stop handling, and lifecycle cleanup.
- Reworked the dart collider setup to use a single root `CapsuleCollider` with continuous collision detection after inconsistent wall contact during testing.
- Added targeted debug logs for `DART WALL BOUNCE`, `DART TOP HIT`, and `DART STOP reason=...` to support manual validation.

### Game Loop, Scoring, and HUD Updates
- Added [Assets/Scripts/BalloonGame/BalloonGameManager.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/BalloonGameManager.cs), [Assets/Scripts/BalloonGame/ScoreManager.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/ScoreManager.cs), and [Assets/Scripts/BalloonGame/GameHUD.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/BalloonGame/GameHUD.cs).
- Implemented room state flow for intro, ready, dart in flight, cleared, and failed states with `4` starting darts, `3000` target score, and `R` restart.
- Wired balloon pops to scoring and room progression through event-based updates.
- Built a runtime HUD showing score, target, darts remaining, and clear/fail messages.

### Errors Encountered and Solutions Applied
- Fixed a Unity 6 runtime font error in `GameHUD` by switching from invalid built-in `Arial.ttf` to `LegacyRuntime.ttf` and making HUD creation resilient when references are missing.
- Fixed missing wall collisions by diagnosing the project's physics collision matrix: `Projectiles` collided with `Enemies` and `Enviroment`, but not with `Default`, so frame walls on `Default` were ignored until moved to the environment layer.
- Removed an intermediate child-collider relay approach after it complicated collision behavior and replaced it with a simpler root-collider dart.

### Validation and Current State
- Manual validation during this thread confirmed scene generation, balloon spawning, dart launching, balloon pops, score increments, side-wall bounce, dart consumption, and fail-state flow.
- Unity batch validation completed successfully after the final fixes with no new compile errors. The only remaining warning is the pre-existing deprecation in [Assets/Scripts/Weapons/Targets/TargetChooser.cs](/Users/seamus/Downloads/AutoAimBowAndArrow-master/Assets/Scripts/Weapons/Targets/TargetChooser.cs), which was not modified.
- No automated gameplay tests were added. `dotnet` was not available in this environment, so compile validation was done through Unity batchmode only.
- Remaining polish work for a follow-up thread: smoother aim/arc tuning, better practical validation of top-wall stop under real throws, and an art/lighting pass to push the scene closer to the target reference.
