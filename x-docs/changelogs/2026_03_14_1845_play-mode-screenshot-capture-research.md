## 2026-03-14 18:45

## Play-mode dart physics test harness + research handoff for headless screenshot capture

## Work completed this session

- Rewrote `DartPhysicsTest.cs` from a console-logging-only tool into a full `EditorApplication.update` state machine that enters play mode, clicks START RUN, waits for game board, captures a "before" screenshot, then spawns a runner
- Created `DartPhysicsTestRunner.cs` as a runtime MonoBehaviour (in `Darts/`, not `Editor/`) that fires darts at 30%, 60%, 100% pull and captures screenshots after each using `Camera.Render()` + RenderTexture
- Added `DartTest()` entry point to `AgentBridgeEntryPoint.cs`
- Added `dart-test` command to `agent-bridge.sh` with help text
- Discovered and resolved: editor scripts can't be added as components in play mode (moved runner out of `Editor/`)
- Discovered: `ScreenCapture.CaptureScreenshot()` produces no output in `-batchmode` (no game window)
- Confirmed: darts fire correctly, balloons pop, but screenshots render magenta due to custom URP shader in batch mode

## Files modified
- `Assets/Scripts/BalloonGame/Editor/DartPhysicsTest.cs` — complete rewrite, state machine pattern
- `Assets/Scripts/BalloonGame/Darts/DartPhysicsTestRunner.cs` — new file, runtime dart firing + screenshot capture
- `Assets/Scripts/BalloonGame/Editor/AgentBridge/AgentBridgeEntryPoint.cs` — added DartTest entry point
- `agent-bridge.sh` — added `dart-test` command

## Next steps (from handoff)
- External research needed on headless rendering options (see handoff document)
- Once research resolves the magenta issue, run `./agent-bridge.sh dart-test` and inspect output PNGs
- Continue visual parity work using the screenshot feedback loop

## Status
Blocked — dart test infrastructure works mechanically but screenshots render magenta

## See also
- Handoff document: `x-docs/handoffs/2026_03_14_1845_play-mode-screenshot-capture-research.md`
