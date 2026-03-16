## 2026-03-16 18:00

## iOS build pipeline, balloon scaling normalization, and shader tuning for device parity

## Issues identified (if relevant)
1. No iOS build pipeline existed — project had never been built or deployed to a physical device.
2. Custom shaders (BalloonEmblem, ChromeDart) were stripped from iOS builds because they weren't in AlwaysIncludedShaders.
3. BalloonEmblem mesh used deprecated Unity mesh API (property setters); DartMeshGenerator cone had no UVs at all — both caused "Mesh uv is out of bounds" errors on device.
4. Balloons were massively oversized on device — old BALLOON_MAX_WIDTH/HEIGHT constants (3.15/3.5) produced ~2.9 unit balloons in ~1.6 unit slots, so only ~3 balloons fit on screen in portrait.
5. When using the Loafbrr prefab override, BalloonWall kept the prefab's embedded material instead of applying the game's BalloonLit shader — causing a white sheen and different appearance on Metal/iOS vs editor.
6. BalloonLit shader had aggressive specular (2.5 intensity), white rim light (1.2 intensity), and broad sheen that rendered far more prominently on Metal than in the editor.

## Issue resolution implemented (if there was an issue)
1. Created full iOS build pipeline: `iOSBuilder.cs` (editor script with menu items + CLI), `iOSPostProcess.cs` (Xcode project post-processing for signing/bitcode/deployment target), and `build-ios.sh` (one-command build + deploy script). Team ID 28864BA964, bundle ID com.seamuslawless.inkshot, IL2CPP backend, auto signing.
2. Added BalloonEmblem and ChromeDart shader GUIDs to GraphicsSettings.asset AlwaysIncludedShaders list.
3. Migrated BalloonEmblem to modern mesh API (SetVertices/SetUVs/SetTriangles). Added UV generation to DartMeshGenerator cone.
4. Replaced fixed balloon sizing with slot-relative scaling: new ApplyBalloonScale() method normalizes against mesh bounds, using BALLOON_SLOT_FILL_X/Y constants. Current values: 0.792/0.891.
5. Changed BalloonWall material logic to always apply BalloonLit materials via ResolveMaterial(), regardless of whether a prefab override is active.
6. Tuned BalloonLit shader defaults: SpecularIntensity 2.5→0.15, SpecularSize 160→80, RimIntensity 1.2→0.08, RimPower 2.0→3.0, Glossiness 0.85→0.45, broad sheen multiplier 0.20→0.01, SSS 0.40→0.12. Material DefaultSmoothness 0.88→0.25, DefaultEmissionIntensity 0.06→0.

## File(s) modified:
- `Assets/Scripts/BalloonGame/Editor/iOSBuilder.cs` (new)
- `Assets/Scripts/BalloonGame/Editor/iOSPostProcess.cs` (new)
- `build-ios.sh` (new)
- `.gitignore`
- `Assets/Scripts/BalloonGame/GameConstants.cs`
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.cs`
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.Materials.cs`
- `Assets/Scripts/BalloonGame/Balloons/BalloonEmblem.cs`
- `Assets/Scripts/BalloonGame/Visuals/DartMeshGenerator.cs`
- `Assets/Shaders/BalloonLit.shader`
- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`
- `ProjectSettings/GraphicsSettings.asset`
