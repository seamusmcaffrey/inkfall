## 2026-03-14 12:00

## Comprehensive gameplay audit and fix pass — UI, shaders, physics, VFX

## Issues identified (if relevant)

Multiple interconnected issues prevented the game from being playable:

1. **UI buttons unresponsive**: No `EventSystem` in the scene; `StandaloneInputModule` incompatible with New Input System; `FadeOverlay` (sort order 900) blocking raycasts while invisible; duplicate scene-saved UI children with dead `onClick` listeners.
2. **Balloons rendering pink**: `BalloonLit.shader` lacked GPU instancing support; `CBUFFER` was guarded by `#ifndef UNITY_INSTANCING_ENABLED` making properties undefined; `BalloonWall.EnsureMaterials()` never looked for `Inkshot/BalloonLit` shader; materials set `_Color` but not `_BaseColor` (URP uses `_BaseColor`).
3. **Ricochet perk broken**: Physics collision matrix did not enable Layer 8 (PROJECTILES) ↔ Layer 9 (RICOCHET), preventing darts from bouncing off stuck darts.
4. **VFX warnings**: `VFXFactory` modified `ParticleSystem` duration/lifetime while systems were still playing.
5. **ComboFlashVFX blocking input**: Full-screen flash overlay at sort order 800 never set `blocksRaycasts = false`.
6. **Dart materials missing URP properties**: `DartLauncher` body/tip materials didn't set `_BaseColor`.

## Issue resolution implemented (if there was an issue)

1. **EventSystem**: Added `EventSystem` + `InputSystemUIInputModule` creation to `SceneBuilder.cs` and runtime fallback in `TitleScreen.cs`. Removed all `StandaloneInputModule` references.
2. **UI child cleanup**: Added destroy-existing-children pattern to all 10+ programmatic UI screens (`TitleScreen`, `TutorialOverlay`, `SettingsPanel`, `PauseManager`, `RunEndScreen`, `PerkSelectionScreen`, `RoomIntroScreen`, `InGameHUD`, `RunHUD`, `ComboFlashVFX`). Set `raycastTarget = false` on decorative text elements.
3. **FadeOverlay**: Reuses existing `CanvasGroup` children; always initializes with `blocksRaycasts = false` and `alpha = 0`.
4. **BalloonLit.shader**: Added `#pragma multi_compile_instancing`, `UNITY_VERTEX_INPUT_INSTANCE_ID`, `UNITY_VERTEX_OUTPUT_STEREO` macros. `CBUFFER` always declares all properties; `UNITY_INSTANCING_BUFFER` conditionally overrides `_BaseColor` only when instancing is active. Added `Fallback "Universal Render Pipeline/Lit"`.
5. **BalloonWall.cs**: Shader lookup now prefers `Inkshot/BalloonLit` first. Added `_BaseColor` property setting in `EnsureMaterials()` and `ApplyAtmosphericFade()`. Enabled GPU instancing on materials.
6. **Physics collision matrix** (`DynamicsManager.asset`): Enabled Layer 8 ↔ Layer 9 collision for ricochet perk.
7. **VFXFactory.cs**: Added `system.Stop(true, StopEmittingAndClear)` before modifying particle properties.
8. **ComboFlashVFX.cs**: Set `_group.blocksRaycasts = false` in `EnsureUi()`.
9. **DartLauncher.cs**: Added explicit `_BaseColor` property setting for URP on body and tip materials.
10. **PauseManager.cs**: Added visible pause button for mobile; switched to New Input System keyboard check.
11. **Deleted `GameHUD.cs`**: Dead code replaced by `InGameHUD.cs`.

## File(s) modified:
- `Assets/Shaders/BalloonLit.shader`
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.cs`
- `Assets/Scripts/BalloonGame/Balloons/BalloonJiggle.cs`
- `Assets/Scripts/BalloonGame/Darts/DartLauncher.cs`
- `Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs`
- `Assets/Scripts/BalloonGame/AimAssist.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/TitleScreen.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/TutorialOverlay.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/PauseManager.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/SettingsPanel.cs`
- `Assets/Scripts/BalloonGame/UI/InGameHUD.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/FadeOverlay.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/PerkSelectionScreen.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/RoomIntroScreen.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/RunEndScreen.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/RunHUD.cs`
- `Assets/Scripts/BalloonGame/VFX/VFXFactory.cs`
- `Assets/Scripts/BalloonGame/VFX/ComboFlashVFX.cs`
- `Assets/Scripts/BalloonGame/VFX/BalloonPopVFX.cs`
- `Assets/Scripts/BalloonGame/VFX/ImpactSparkVFX.cs`
- `Assets/Scripts/BalloonGame/VFX/PaintSplatterVFX.cs`
- `Assets/Scripts/BalloonGame/VFX/WallHitVFX.cs`
- `Assets/Scripts/BalloonGame/Visuals/LaunchLaneVisuals.cs`
- `Assets/Scripts/BalloonGame/Visuals/SlingshotVisuals.cs`
- `Assets/Scripts/BalloonGame/UI/GameHUD.cs` (deleted)
- `Assets/Scripts/BalloonGame/UI/GameHUD.cs.meta` (deleted)
- `ProjectSettings/DynamicsManager.asset`
- `Assets/ScriptableObjects/GameConfig.asset`
- `Assets/Scenes/InkshotScene.unity`
