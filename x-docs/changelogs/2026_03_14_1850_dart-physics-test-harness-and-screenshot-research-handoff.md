## 2026-03-14 18:50

## Rewrite dart physics test harness with play-mode screenshot capture + research handoff

## Issues identified (if relevant)
1. Original `DartPhysicsTest.cs` only logged to console — no screenshot capture at all.
2. `ScreenCapture.CaptureScreenshot()` produces no output in `-batchmode` (no game window exists).
3. `Camera.Render()` + RenderTexture produces PNGs but custom URP shader (`BalloonLit.shader`) renders magenta in batch mode.
4. MonoBehaviours in `Editor/` folders cannot be added as components during play mode — `DartPhysicsTestRunner` had to be moved to `Darts/`.
5. Stale Unity lock file sometimes remains after batch exit, causing subsequent runs to fail with "Unity is running" error.

## Issue resolution implemented (if there was an issue)
- Rewrote `DartPhysicsTest.cs` as an `EditorApplication.update` state machine (matching `PlayModeCapture.cs` pattern) that enters play mode, clicks START RUN, and spawns a dart-firing runner.
- Created `DartPhysicsTestRunner.cs` as a runtime MonoBehaviour in `Darts/` with `Camera.Render()` + RenderTexture capture (not `ScreenCapture.CaptureScreenshot()`).
- Added `DartTest()` entry point to `AgentBridgeEntryPoint.cs` and `dart-test` command to `agent-bridge.sh`.
- Magenta rendering remains unresolved — created research handoff (`x-docs/handoffs/2026_03_14_1845_play-mode-screenshot-capture-research.md`) exploring CLI batch mode, MCP-driven, and hybrid approaches.

## File(s) modified:
- `Assets/Scripts/BalloonGame/Editor/DartPhysicsTest.cs` — complete rewrite as state machine
- `Assets/Scripts/BalloonGame/Darts/DartPhysicsTestRunner.cs` — new file, runtime dart firing + RenderTexture capture
- `Assets/Scripts/BalloonGame/Editor/AgentBridge/AgentBridgeEntryPoint.cs` — added DartTest entry point
- `agent-bridge.sh` — added `dart-test` command and help text
- `x-docs/handoffs/2026_03_14_1845_play-mode-screenshot-capture-research.md` — new research handoff
