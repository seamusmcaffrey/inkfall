# Changelog

## Asset-Based Visual Overhaul — 2026-03-15

### Strategic Direction Change
The project's visual pipeline was previously 100% procedural — zero imported 3D models, zero pre-made textures. Every surface (cork board, metal frame, floor, balloons, darts) was generated at runtime from Perlin noise and code-built meshes. After 5+ visual/layout passes and experimenting with perspective simulation (row-based scaling/pinch in both orientations), we hit a ceiling: **flat procedural textures on Unity primitives viewed through an orthographic camera will never feel like looking into a real room.** The decision was made to pivot to imported 3D assets and prepare for a perspective camera.

### What Changed

**Imported Loafbrr Balloon Asset Pack (free, URP-compatible)**
- Downloaded "Balloons" by Loafbrr from Unity Asset Store (16.9 MB, 69 files)
- Contains 49 balloon mesh variants (letters, numbers, shapes) in a single FBX
- Includes ORM Shader Graph with PBR textures (diffuse, normal, ORM maps at 1K)
- 9 material variants: Gold (metallic), Silver (chrome), and 7 tinted White variants
- Assets located at `Assets/LoafbrrAssets/Balloons/`

**Balloon Prefab Override System**
- Added `balloonPrefabOverride` (GameObject) and `balloonMeshOverride` (Mesh) fields to `GameConfigSO`
- Modified `BalloonWall.Spawning.cs` → `CreateBalloonObject()` to instantiate from prefab when set, falling back to procedural mesh when null
- Added component guards (null checks before AddComponent) so prefab components aren't duplicated
- BalloonEmblem (procedural overlay) skipped when using prefab — not needed with 3D assets
- Currently wired to `Balloon_Balloon` prefab (basic round balloon shape)

**Per-Type Material Overrides via Loafbrr Materials**
- All 8 `BalloonTypeSO` assets updated with `materialOverride` pointing to Loafbrr materials:
  - StandardRed → Balloon_White Variant (pink-red)
  - StandardBlue → Balloon_White Variant 4 (blue)
  - StandardYellow → Balloon_White Variant 1 (warm yellow)
  - StandardGreen → Balloon_White Variant 2 (green)
  - StandardPurple → Balloon_White Variant 3 (purple)
  - GoldBalloon → Balloon_Gold (metallic gold)
  - HazardBalloon → Balloon_Silver (chrome)
  - PaintBalloon → Balloon_White Variant 5 (deep red)
- Material assignment in `BalloonWall.cs` updated: applies `materialOverride` when set, skips PropertyBlock color override to preserve PBR material properties
- `ApplyAtmosphericFade` now skips balloons with `materialOverride` so PBR shading isn't stomped

**Balloon Scale Adjustment**
- `BALLOON_MAX_WIDTH` changed from 0.90 → 3.15, `BALLOON_MAX_HEIGHT` from 1.0 → 3.5
- Imported balloon mesh is ~5x smaller than procedural mesh at equivalent scale

**Lighting Simplified for Asset Evaluation**
- `NeonLightRig.cs` replaced: 9 dramatic neon lights → 3 neutral directional lights (main/fill/back) with warm-neutral colors
- Added cleanup logic to destroy old child lights before rebuilding rig
- `PostProcessingSetup.cs` disabled at Awake (bloom 5.0, vignette 0.62, chromatic aberration removed)
- `EnvironmentBuilder.Atmosphere.cs` emptied: fog overlays and vignette quads disabled
- All effects preserved in code and can be re-enabled when room aesthetic is finalized

### What's Next

Two follow-up task prompts written as standalone files:
- `PROMPT-perspective-camera.md` — Switch from orthographic to perspective camera (do this first)
- `PROMPT-room-environment.md` — Build a 3D room/booth environment around the board

### Known Issues
- Gold metallic material not visually distinct under current neutral lighting — may need environment reflections or adjusted light angles
- Balloon scale constants are now large (3.15 × 3.5) because imported mesh native size differs from procedural — should be normalized when prefab pipeline stabilizes
- Old neon light GameObjects may persist in saved scenes from previous sessions

## Task: Build Inkshot Balloon-Dart Prototype and Stabilize Gameplay - 2026-03-13

### Scene Setup Updates
- Added [Assets/Scripts/BalloonGame/GameConstants.cs](Assets/Scripts/BalloonGame/GameConstants.cs), [Assets/Scripts/BalloonGame/BalloonCamera.cs](Assets/Scripts/BalloonGame/BalloonCamera.cs), and [Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs](Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs) to build and tune the new `InkshotScene`.
- Generated [Assets/Scenes/InkshotScene.unity](Assets/Scenes/InkshotScene.unity) with portrait orthographic camera, frame walls, back wall, lane floor, launch origin, slingshot controller, balloon wall, and game manager roots.
- Created new wall and floor materials plus wall physics assets under `Assets/Materials/`.
- Added an explicit environment layer constant and assigned wall/floor scene objects to it so projectiles use the existing physics matrix correctly.

### Balloon Wall Updates
- Added [Assets/Scripts/BalloonGame/BalloonData.cs](Assets/Scripts/BalloonGame/BalloonData.cs), [Assets/Scripts/BalloonGame/BalloonNode.cs](Assets/Scripts/BalloonGame/BalloonNode.cs), and [Assets/Scripts/BalloonGame/BalloonWall.cs](Assets/Scripts/BalloonGame/BalloonWall.cs).
- Implemented runtime generation of an 8x9 balloon grid with five colors, row-based scale changes, horizontal pinch, atmospheric darkening, and kinematic rigidbodies for Unity collision.
- Adjusted spacing from edge-based placement to inset slot centers so the balloon wall fits the frame more cleanly.
- Added safe wall clearing/regeneration logic for restarts and editor-side scene rebuilds.

### Slingshot and Dart Updates
- Added [Assets/Scripts/BalloonGame/SlingshotInput.cs](Assets/Scripts/BalloonGame/SlingshotInput.cs) and [Assets/Scripts/BalloonGame/SlingshotVisuals.cs](Assets/Scripts/BalloonGame/SlingshotVisuals.cs) for drag-to-aim input, rubber-band visualization, and trajectory preview using the Input System.
- Added [Assets/Scripts/BalloonGame/DartLauncher.cs](Assets/Scripts/BalloonGame/DartLauncher.cs) and [Assets/Scripts/BalloonGame/DartController.cs](Assets/Scripts/BalloonGame/DartController.cs) for dart spawning, Rigidbody launch, velocity-facing rotation, bounce/stop handling, and lifecycle cleanup.
- Reworked the dart collider setup to use a single root `CapsuleCollider` with continuous collision detection after inconsistent wall contact during testing.
- Added targeted debug logs for `DART WALL BOUNCE`, `DART TOP HIT`, and `DART STOP reason=...` to support manual validation.

### Game Loop, Scoring, and HUD Updates
- Added [Assets/Scripts/BalloonGame/BalloonGameManager.cs](Assets/Scripts/BalloonGame/BalloonGameManager.cs), [Assets/Scripts/BalloonGame/ScoreManager.cs](Assets/Scripts/BalloonGame/ScoreManager.cs), and [Assets/Scripts/BalloonGame/GameHUD.cs](Assets/Scripts/BalloonGame/GameHUD.cs).
- Implemented room state flow for intro, ready, dart in flight, cleared, and failed states with `4` starting darts, `3000` target score, and `R` restart.
- Wired balloon pops to scoring and room progression through event-based updates.
- Built a runtime HUD showing score, target, darts remaining, and clear/fail messages.

### Errors Encountered and Solutions Applied
- Fixed a Unity 6 runtime font error in `GameHUD` by switching from invalid built-in `Arial.ttf` to `LegacyRuntime.ttf` and making HUD creation resilient when references are missing.
- Fixed missing wall collisions by diagnosing the project's physics collision matrix: `Projectiles` collided with `Enemies` and `Enviroment`, but not with `Default`, so frame walls on `Default` were ignored until moved to the environment layer.
- Removed an intermediate child-collider relay approach after it complicated collision behavior and replaced it with a simpler root-collider dart.

### Validation and Current State
- Manual validation during this thread confirmed scene generation, balloon spawning, dart launching, balloon pops, score increments, side-wall bounce, dart consumption, and fail-state flow.
- Unity batch validation completed successfully after the final fixes with no new compile errors. The only remaining warning is the pre-existing deprecation in [Assets/Scripts/Weapons/Targets/TargetChooser.cs](Assets/Scripts/Weapons/Targets/TargetChooser.cs), which was not modified.
- No automated gameplay tests were added. `dotnet` was not available in this environment, so compile validation was done through Unity batchmode only.
- Remaining polish work for a follow-up thread: smoother aim/arc tuning, better practical validation of top-wall stop under real throws, and an art/lighting pass to push the scene closer to the target reference.
