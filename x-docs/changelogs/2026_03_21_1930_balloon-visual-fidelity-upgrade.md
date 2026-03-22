## 2026-03-21 19:30

## Balloon visual fidelity upgrade — glossy lacquered look with normal maps and enhanced specular

## Issues identified (if relevant)
- Standard balloons were using Loafbrr ORM Shader Graph materials (TransparentCutout mode, incompatible alpha=0 colors) instead of the game's custom BalloonLit shader, producing a flat, matte appearance that didn't match the intended glossy/lacquered art direction.
- BalloonLit shader had no texture sampling — no normal map support meant zero surface detail.
- Runtime material creation used overly conservative glossiness (0.25) and specular (0.15) defaults, far below what's needed for the reference look.
- Specular highlights were being compressed by the tone curve, preventing the bright white "window reflection" glare seen in the reference.
- Pre-existing compile errors in the third-party URPToonShader package (RTHandle API change in Unity 6, JTRP importer ParallelWriter rename) blocked batch-mode compilation.

## Issue resolution implemented (if there was an issue)
- **Shader upgrade**: Added tangent-space normal map sampling to BalloonLit.shader. Restructured the lighting pipeline to apply specular AFTER the tone curve so highlights stay bright and white instead of being compressed. Boosted specular intensity (0.15→1.0), size (80→160), rim (0.08→0.25), glossiness (0.45→0.78), and saturation (1.30→1.45). Switched from Cull Off to Cull Back for closed-mesh performance.
- **Material pipeline**: Redirected all 5 standard balloon ScriptableObjects from Loafbrr Shader Graph materials to BalloonLit materials. Assigned the Loafbrr normal map (Balloons_Normal_1K.png) to all .mat files. Enabled GPU instancing. Cleared materialOverride for special types (Gold, Hazard, Paint) so the runtime system handles them with BalloonLit shader.
- **Runtime materials**: Added `balloonNormalMap` field to GameConfigSO for runtime normal map injection. Extracted 6 named constants for shader defaults in BalloonWall.Materials.cs (no magic numbers). Runtime-created materials now receive all BalloonLit properties including normal map.
- **Mesh generator**: Added `RecalculateTangents()` call for normal map support.
- **Third-party fixes**: Migrated RenderFrontHairShadowMaskFeature.cs from deprecated RenderTargetIdentifier to RTHandle API. Wrapped JTRP ModelOutlineImporter in `#if JTRP_OUTLINE_IMPORTER` to exclude incompatible Collections API usage.

## File(s) modified:
- `Assets/Shaders/BalloonLit.shader`
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.Materials.cs`
- `Assets/Scripts/BalloonGame/Data/GameConfigSO.cs`
- `Assets/Scripts/BalloonGame/Visuals/BalloonMeshGenerator.cs`
- `Assets/Materials/BalloonRed.mat`
- `Assets/Materials/BalloonBlue.mat`
- `Assets/Materials/BalloonGreen.mat`
- `Assets/Materials/BalloonYellow.mat`
- `Assets/Materials/BalloonPurple.mat`
- `Assets/ScriptableObjects/BalloonTypes/StandardRed.asset`
- `Assets/ScriptableObjects/BalloonTypes/StandardBlue.asset`
- `Assets/ScriptableObjects/BalloonTypes/StandardGreen.asset`
- `Assets/ScriptableObjects/BalloonTypes/StandardYellow.asset`
- `Assets/ScriptableObjects/BalloonTypes/StandardPurple.asset`
- `Assets/ScriptableObjects/BalloonTypes/GoldBalloon.asset`
- `Assets/ScriptableObjects/BalloonTypes/HazardBalloon.asset`
- `Assets/ScriptableObjects/BalloonTypes/PaintBalloon.asset`
- `Assets/ScriptableObjects/GameConfig.asset`
- `Assets/ThirdParty/URPToonShader/Assets/ChiliMilkToonShader/RenderFeature/RenderFrontHairShadowMaskFeature.cs`
- `Assets/ThirdParty/URPToonShader/Assets/ChiliMilkToonShader/Tool/JTRP/Editor/Import/ModelOutlineImporter.cs`
