# Agent Bridge — AI Agent Integration for Inkshot

## Quick Start

```bash
./agent-bridge.sh status       # Check what's available right now
./agent-bridge.sh report       # Run all checks (compile + validate + health)
./agent-bridge.sh screenshot   # Capture 1080x1920 PNG of the rendered scene
```

The bridge auto-detects whether Unity is open and routes accordingly:
- **Unity closed** → batch mode (launches Unity, runs task, exits — ~30-60s)
- **Unity open + MCP** → instant MCP calls to the running Editor (~1-2s)
- **Unity open, no MCP** → `health` still works (file scanning, no Unity needed)

## MCP Setup (Recommended)

MCP lets agents talk to a running Unity Editor instantly — no batch mode boot cycles.

### Prerequisites
- Python 3.10+ (`python3 --version`)
- Node.js 20+ (`node --version`)
- `uvx` (`pip install uv` if missing)
- `uloop-cli` (`npm install -g uloop-cli`)

### Packages (already added to manifest.json)
- `com.coplaydev.unity-mcp` — 36 tools: scene, scripts, materials, shaders, animation, tests, console
- `io.github.hatayama.uloopmcp` — 15 tools: screenshots, dynamic C# execution, play mode control

### First-Time Setup
1. Open the Unity project
2. Unity will import the MCP packages (first time may take a few minutes)
3. Start CoplayDev MCP: **Window > MCP for Unity > Start Server**
4. Start uLoopMCP: **Window > uLoop** (auto-starts)
5. Verify: `./agent-bridge.sh status` should show "MCP Server: AVAILABLE"

### Claude Code Configuration
The `.mcp.json` in the project root auto-configures Claude Code:
```json
{
  "mcpServers": {
    "unityMCP": { "url": "http://localhost:8080/mcp" },
    "uLoopMCP": { "command": "uvx", "args": ["--from", "uloop-cli", "uloop", "mcp"] }
  }
}
```

### Available MCP Tools

**CoplayDev (unityMCP) — 36 tools:**
| Tool | What it does |
|------|-------------|
| `manage_gameobject` | Create, modify, delete GameObjects |
| `manage_components` | Add/remove/configure components |
| `manage_scene` | Load, save, create scenes |
| `manage_prefabs` | Prefab creation and editing |
| `create_script` / `manage_script` | Create and navigate C# scripts |
| `manage_material` / `manage_shader` | Materials and shader management |
| `manage_animation` | Animation clips and controllers |
| `manage_camera` | Camera setup, Cinemachine |
| `manage_graphics` | Volumes, post-processing, render stats |
| `manage_vfx` | Visual effects |
| `manage_ui` | UI elements |
| `manage_scriptable_object` | ScriptableObject CRUD |
| `find_gameobjects` | Search for GameObjects |
| `read_console` | Read Unity console output (structured) |
| `refresh_unity` | Force asset database refresh (triggers compile) |
| `run_tests` / `get_test_job` | Execute and check unit test results |
| `execute_menu_item` | Execute any Unity menu command |
| `batch_execute` | Batch multiple operations (10-100x faster) |

**uLoopMCP — 15 tools:**
| Tool | What it does |
|------|-------------|
| `compile` | Trigger compilation, get structured errors |
| `screenshot` | Capture Editor window screenshots (Game View, Scene View) |
| `execute-dynamic-code` | Run arbitrary C# code in Unity context |
| `control-play-mode` | Start/stop Play Mode |
| `get-logs` | Console logs with filtering and stack traces |
| `run-tests` | Run EditMode/PlayMode tests |
| `get-hierarchy` | Scene hierarchy as structured data |
| `find-game-objects` | Search GameObjects by criteria |

## Feedback Loop Pattern

After every code change, agents should follow this cycle:

1. **Edit code** — make the change
2. **Compile** — `./agent-bridge.sh compile` or MCP `refresh_unity`
3. **If errors**: read JSON output, fix, goto 2
4. **Health check** — `./agent-bridge.sh health` (always works, no Unity needed)
5. **If violations**: fix, goto 2
6. **Screenshot** — `./agent-bridge.sh screenshot` or MCP `screenshot`
7. **Validate** — `./agent-bridge.sh validate` for scene integrity
8. **Stop after 20 iterations** — if still failing, explain the blocker

## Commands Reference

### Works in any mode
| Command | What it does |
|---------|-------------|
| `./agent-bridge.sh health` | Code standards violations (file scanning — no Unity) |
| `./agent-bridge.sh status` | Check Unity/MCP state |

### Auto-routes (batch or MCP)
| Command | What it does |
|---------|-------------|
| `./agent-bridge.sh compile` | Trigger recompilation, get structured errors |
| `./agent-bridge.sh validate` | Validate scene (camera, layers, components, physics) |
| `./agent-bridge.sh report` | Combined: compile + validate + health |
| `./agent-bridge.sh screenshot` | Capture 1080x1920 PNG of rendered scene |

### MCP-only (requires Unity open + MCP server)
| Command | What it does |
|---------|-------------|
| `./agent-bridge.sh console` | Read Unity console output |
| `./agent-bridge.sh hierarchy` | Inspect scene hierarchy |
| `./agent-bridge.sh run-code '<code>'` | Execute C# in Editor context |

## Output

All results written to `Logs/agent-feedback/` (gitignored):
- `compilation.json` — compiler errors with file, line, column, assembly
- `validation.json` — scene checks (camera, layers, required components)
- `health.json` — code violations (file size, Debug.Log, GetComponent in Update)
- `report.json` — combined report from all checks
- `screenshots/` — captured PNGs (1080x1920 portrait)
- `editor.log` — full Unity Editor log

## Architecture

```
agent-bridge.sh                         # CLI entry point (auto-routes batch/MCP)
.mcp.json                               # Claude Code MCP server configuration
Editor/AgentBridge/
├── AgentReport.cs                      # JSON-serializable data structures
├── AgentBridgeEntryPoint.cs            # -executeMethod targets + menu items
├── CompilationReporter.cs              # CompilationPipeline hooks
├── SceneValidator.cs                   # Scene setup validation
├── CodeHealthAnalyzer.cs               # Code standards enforcement
└── ScreenshotCapture.cs                # RenderTexture-based scene capture
```

## Rules for Agents

- Read CLAUDE.md for all project coding standards
- Never edit `.unity` scene files directly — use SceneBuilder or MCP tools
- Never add packages to manifest.json without approval
- All cross-system communication uses EventBus
- All tuning values go in ScriptableObjects or GameConstants
- File size limits are enforced (see CLAUDE.md)
- Cache component references in Awake — never GetComponent in Update
- Stop after 20 failed iterations and explain the blocker
