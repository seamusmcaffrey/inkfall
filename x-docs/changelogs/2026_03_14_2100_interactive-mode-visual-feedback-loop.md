## 2026-03-14 21:00

## Implement interactive-mode visual feedback loop for non-magenta screenshot capture

## Issues identified
1. `agent-bridge.sh` launched Unity in `-batchmode` for dart-test and gameplay commands, which suppresses Metal GPU initialization on macOS, causing all URP/custom shaders (BalloonLit.shader) to render magenta in screenshots.
2. `ShaderInclusionGuard` used `AssetDatabase.LoadAssetAtPath` for `ProjectSettings/GraphicsSettings.asset`, which always returns null since ProjectSettings/ is not inside Assets/. Fixed to use `LoadAllAssetsAtPath`.
3. MCP menu paths in agent-bridge.sh used `BalloonGame/Agent Bridge/...` but actual MenuItem paths are `Inkshot/Agent Bridge/...`.
4. `DartPhysicsTest.cs` only called `EditorApplication.Exit(0)` in batch mode, meaning interactive launches via `-executeMethod` would leave the Editor window open.
5. `DartPhysicsTest.cs` exceeded the 200-line Editor script limit after changes (205 lines).

## Issue resolution implemented
- **Interactive launch mode**: Added `run_interactive()` function to `agent-bridge.sh` that launches Unity WITHOUT `-batchmode`, enabling full Metal GPU context and correct URP shader compilation.
- **Visual smart routing**: Added `run_visual_smart()` that routes to MCP when Unity is open, or interactive mode when Unity is closed. Applied to `dart-test` and `gameplay` commands.
- **Shader warmup**: Added `Shader.WarmupAllShaders()` in both `DartPhysicsTestRunner` (runtime) and `DartPhysicsTest` (editor) before first screenshot capture — defense-in-depth against variant stripping.
- **ShaderInclusionGuard**: New `[InitializeOnLoad]` editor script that ensures `BalloonLit.shader` is in Unity's "Always Included Shaders" list via `SerializedObject` on `GraphicsSettings`, preventing URP variant stripping.
- **Auto-exit detection**: `DartPhysicsTest` now detects `-executeMethod` in command-line args to auto-exit after test completion, regardless of batch vs interactive mode.
- **Menu path fix**: Corrected MCP menu paths to `Inkshot/Agent Bridge/...`.
- **Line count**: Compacted `IsExecuteMethodLaunch()` to a one-liner using `Array.Exists()`, trimmed docstring — file is now 197 lines.

## File(s) modified:
- `agent-bridge.sh` — added `run_interactive()`, `run_visual_smart()`, updated dart-test/gameplay routing, updated help text
- `Assets/Scripts/BalloonGame/Darts/DartPhysicsTestRunner.cs` — added `Shader.WarmupAllShaders()` + frame wait before captures
- `Assets/Scripts/BalloonGame/Editor/DartPhysicsTest.cs` — auto-exit flag, shader warmup in CaptureBeforeShot, compacted
- `Assets/Scripts/BalloonGame/Editor/AgentBridge/ShaderInclusionGuard.cs` — **new file**, Always Included Shaders guard

## Research implemented from:
- `x-docs/research/autonomous agents/frontend-fb-loop.md`
- Key recommendations applied: interactive mode launch, shader warmup, Always Included Shaders, MCP-first routing

## Verification:
- Shell syntax: `bash -n agent-bridge.sh` passes
- Health check: 5 pre-existing violations, 0 new violations introduced
- DartPhysicsTest.cs: 197 lines (under 200 limit)
- Code review: 2 critical issues found and fixed (GraphicsSettings load, menu paths)
