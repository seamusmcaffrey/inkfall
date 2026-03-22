## 2026-03-22 19:30

## Visual parity pass — match graffiti noir carnival reference aesthetic

## Issues identified (if relevant)
- HUD was invisible in gameplay screenshots due to ScreenSpaceOverlay canvas not being captured by Camera.Render()
- HUD text was too small after switching to ScreenSpaceCamera (matchWidthOrHeight was 0 on portrait)
- Emblem quads had inverted triangle winding, making them invisible with backface culling
- Balloon mesh had procedural "tie" stubs at the bottom that didn't match the reference aesthetic
- Neon lighting, specular highlights, and atmosphere density were all far below reference levels

## Issue resolution implemented (if there was an issue)
- Switched InGameHUD canvas to ScreenSpaceCamera with matchWidthOrHeight=0.5, tripled font sizes
- Fixed emblem quad winding order from {0,2,1,0,3,2} to {0,1,2,0,2,3}
- Removed tie geometry from BalloonMeshGenerator, replaced with smooth bottom cap
- Boosted neon light intensity (NeonPointIntensity 1.8→4.5), added lower neon accent lights
- Increased specular (SpecularIntensity 6→10, SpecularSize 200→250), rim (0.18→0.30), emission (0.02→0.06)
- Enhanced post-processing (Bloom 6→10, threshold 0.35→0.20, added FilmGrain and ColorAdjustments)
- Thickened atmosphere overlays (FogAlpha 0.08→0.14, EdgeGlowAlpha 0.12→0.22, added RoomHaze)
- Enlarged balloons (SLOT_FILL_X 0.792→0.88, SLOT_FILL_Y 0.891→0.95)
- Boosted emblem scale (0.62→0.80) and glow multiplier formula (1+Glow → 1+Glow*2)

## File(s) modified:
- `Assets/Scripts/BalloonGame/Visuals/BalloonMeshGenerator.cs`
- `Assets/Scripts/BalloonGame/Balloons/BalloonEmblem.cs`
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.Materials.cs`
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.Comparison.cs`
- `Assets/Scripts/BalloonGame/GameConstants.cs`
- `Assets/Scripts/BalloonGame/Juice/JuiceConfigSO.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/RoomIntroScreen.cs`
- `Assets/Scripts/BalloonGame/UI/InGameHUD.cs`
- `Assets/Scripts/BalloonGame/UI/InGameHUD.Builder.cs`
- `Assets/Scripts/BalloonGame/UI/UIColors.cs`
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs`
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Atmosphere.cs`
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Frame.cs`
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Room.cs`
- `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs`
- `Assets/Scripts/BalloonGame/Visuals/PostProcessingSetup.cs`
- `Assets/Scripts/BalloonGame/Visuals/ProceduralTextures.cs`
- `Assets/Shaders/BalloonEmblem.shader`
- `Assets/Shaders/BalloonLit.shader`
- `Assets/Scenes/InkshotScene.unity`
