# Agent Bridge System — Technical Reference

## What It Is

A feedback loop system that lets AI agents autonomously compile, validate, screenshot, and health-check a Unity project. Agents can work without a human needing to open/close Unity.

## Architecture

```
agent-bridge.sh              CLI entry point — auto-routes batch mode vs MCP
.mcp.json                    Claude Code MCP server config (auto-discovered)
Packages/manifest.json       Two MCP packages added (CoplayDev + uLoopMCP)
Editor/AgentBridge/
├── AgentReport.cs            JSON data structures (122 lines)
├── AgentBridgeEntryPoint.cs  -executeMethod CLI targets + menu items (148 lines)
├── CompilationReporter.cs    CompilationPipeline hooks → JSON errors (131 lines)
├── SceneValidator.cs         Scene checks: camera, layers, components (187 lines)
├── CodeHealthAnalyzer.cs     Code standards: file size, Debug.Log, patterns (200 lines)
├── ScreenshotCapture.cs      Camera.Render → RenderTexture → PNG (Scene View) (169 lines)
└── PlayModeCapture.cs        SessionState state machine: boot → click → capture (200 lines)
```

## Two Execution Modes

### Batch Mode (Unity closed)
- `agent-bridge.sh` launches Unity with `-batchmode -executeMethod ...`
- Unity boots (~30-60s), runs the task, writes JSON to `Logs/agent-feedback/`, exits
- Used for: compile, validate, health, report, screenshot, gameplay, smoke-test
- Screenshot uses `-batchmode` WITHOUT `-nographics` (GPU needed for rendering)

### MCP Mode (Unity open)
- Agent talks to running Editor via HTTP (`http://localhost:8080/mcp`)
- Instant responses (~1-2s), no boot cycle
- Two MCP servers installed:
  - **CoplayDev unity-mcp** (36 tools): scene, scripts, materials, animation, tests, console
  - **uLoopMCP** (15 tools): screenshots, dynamic C# execution, play mode control
- Start in Unity: Window > MCP for Unity > Start Server

### Auto-Routing
`agent-bridge.sh` detects Unity state via `Temp/UnityLockfile` and MCP via HTTP health check:
- Unity closed → batch mode
- Unity open + MCP responding → MCP calls
- Unity open, no MCP → file-only checks (health works, others blocked)

## Screenshot Capabilities

### Scene View (Camera.Render)
- `ScreenshotCapture.CaptureScene()` — renders camera directly to RenderTexture
- Shows geometry + lighting but BYPASSES URP pipeline
- Result: you see shapes/objects but with broken shaders (magenta)
- Useful for: verifying geometry exists, layout, object placement

### Game View (ScreenCapture.CaptureScreenshot)
- `PlayModeCapture` with `ScreenCapture.CaptureScreenshot()` during Play Mode
- Captures the ACTUAL framebuffer — exactly what the player sees
- Shows the real rendering pipeline output (including bugs like white screen)
- Useful for: verifying what users actually see, finding rendering bugs

### Play Mode Capture Flow
PlayModeCapture.cs uses a SessionState-based state machine (survives domain reload):
1. Open InkshotScene
2. Enter Play Mode
3. Wait 1.0s for bootstrap
4. Click "START RUN" button on TitleScreen
5. Wait 4.0s for room/balloons to load
6. Capture screenshot
7. Exit Play Mode → Exit Unity (batch mode)

Pattern borrowed from BalloonSmokeRunner.cs which does the same flow for smoke testing.

## Commands Reference

```bash
# Works without Unity
./agent-bridge.sh health          # File-only code standards scan
./agent-bridge.sh status          # Check Unity/MCP state

# Batch mode (Unity must be closed)
./agent-bridge.sh compile         # Compile → JSON errors
./agent-bridge.sh validate        # Scene checks → JSON results
./agent-bridge.sh report          # All checks combined → JSON
./agent-bridge.sh screenshot      # Scene View render → PNG
./agent-bridge.sh gameplay        # Play Mode → navigate → Game View PNG
./agent-bridge.sh smoke-test      # Play Mode bootstrap test

# MCP only (Unity must be open + MCP server running)
./agent-bridge.sh console         # Read Unity console
./agent-bridge.sh hierarchy       # Scene hierarchy as JSON
./agent-bridge.sh run-code '...'  # Execute C# in Editor
```

## Output Location

All output goes to `Logs/agent-feedback/` (gitignored via `[Ll]ogs/` rule):
- `compilation.json` — compiler errors with file, line, column, assembly
- `validation.json` — scene validation checks (19 checks)
- `health.json` — code health violations (file size, Debug.Log, GetComponent)
- `report.json` — combined report
- `screenshots/scene_*.png` — Scene View captures (1080x1920)
- `screenshots/gameplay_*.png` — Game View captures (1080x1920)
- `editor.log` — full Unity Editor log

## Key Findings from First Scan

### Compilation: PASS (0 errors)

### Validation: 18/19 passed
- Layer 10 (UIWorld) is undefined
- Layer names don't match code expectations: "Enviroment" (typo), "Enemies" (not "Balloons"), "Interactables" (not "Ricochet")

### Health: 12 violations
- 6 files over size limits (BalloonWall 417, BalloonSmokeRunner 389, SceneBuilder 366, SlingshotVisuals 313, SettingsPanel 278, BalloonGameManager 271)
- 6 unguarded Debug.Log calls (Haptics, ObjectPool, BalloonWall, RunManager, TitleScreen)

### Game View: White screen (rendering pipeline issue — URP not rendering to Game camera)

## MCP Setup Details

### Prerequisites
- Python 3.10+ (found at `/opt/homebrew/bin/python3`)
- `uv` package manager (symlinked to `/opt/homebrew/bin/uv` from Python framework)
- Node.js 20+ (via nvm)
- `uloop-cli` (`npm install -g uloop-cli`)

### Unity Packages (in manifest.json)
```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#beta",
"io.github.hatayama.uloopmcp": "https://github.com/hatayama/uLoopMCP.git?path=/Packages/src"
```

### Claude Code Config (.mcp.json in project root)
```json
{
  "mcpServers": {
    "unityMCP": { "url": "http://localhost:8080/mcp" },
    "uLoopMCP": { "command": "node", "args": ["Library/PackageCache/io.github.hatayama.uloopmcp@.../TypeScriptServer~/dist/server.bundle.js"], "env": { "UNITY_TCP_PORT": "8700" } }
  }
}
```

## Design Decisions

1. **SessionState for Play Mode** — Unity reloads the domain when entering Play Mode, destroying all static state. SessionState persists across domain reloads within an Editor session.
2. **No Debug.Log in AgentBridge code** — project rule. Use `Console.WriteLine` for batch mode output, `File.WriteAllText` for JSON.
3. **AgentBridge self-exclusion** — CodeHealthAnalyzer skips `/AgentBridge/` to avoid self-referential violations.
4. **Duplicate write removal** — individual analyzers return data only; AgentBridgeEntryPoint handles all JSON file writes.
5. **Two-lane rendering** — `-nographics` for logic-only tasks (compile, health), GPU enabled for screenshots.
6. **File size limits enforced** — all Editor scripts under 200 lines, data files under 150 lines per CLAUDE.md.
