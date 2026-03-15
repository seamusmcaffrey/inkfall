#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$SCRIPT_DIR"
OUTPUT_DIR="$PROJECT_PATH/Logs/agent-feedback"
LOG_FILE="$OUTPUT_DIR/editor.log"
MCP_URL="${UNITY_MCP_URL:-http://localhost:8080/mcp}"
UNITY_LOCKFILE="$PROJECT_PATH/Temp/UnityLockfile"

# ── Unity discovery ──────────────────────────────────────────────
find_unity() {
    if [[ -n "${UNITY_EDITOR_PATH:-}" ]]; then
        echo "$UNITY_EDITOR_PATH"
        return
    fi

    local hub_base="/Applications/Unity/Hub/Editor"
    if [[ -d "$hub_base" ]]; then
        local newest
        newest=$(ls -1d "$hub_base"/*/Unity.app/Contents/MacOS/Unity 2>/dev/null | sort -V | tail -1)
        if [[ -n "$newest" ]]; then
            echo "$newest"
            return
        fi
    fi

    if [[ -x "/Applications/Unity/Unity.app/Contents/MacOS/Unity" ]]; then
        echo "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
        return
    fi

    if command -v unity-editor &>/dev/null; then
        echo "unity-editor"
        return
    fi

    echo ""
}

# ── State detection ──────────────────────────────────────────────
unity_process_running() {
    pgrep -x Unity >/dev/null 2>&1
}

cleanup_stale_unity_lockfile() {
    if [[ -f "$UNITY_LOCKFILE" ]] && ! unity_process_running; then
        rm -f "$UNITY_LOCKFILE"
    fi
}

is_unity_running() {
    if [[ ! -f "$UNITY_LOCKFILE" ]]; then
        return 1
    fi

    if unity_process_running; then
        return 0
    fi

    cleanup_stale_unity_lockfile
    return 1
}

is_mcp_available() {
    # Quick health check on the MCP HTTP endpoint
    local response

    if ! response=$(curl -s -o /dev/null -w "%{http_code}" --max-time 2 "$MCP_URL" 2>/dev/null); then
        return 1
    fi

    case "$response" in
        2*|4*)
            return 0
            ;;
        *)
            return 1
            ;;
    esac
}

UNITY="$(find_unity)"

if [[ -z "$UNITY" ]]; then
    echo "ERROR: Unity Editor not found."
    echo "Set UNITY_EDITOR_PATH to your Unity executable."
    exit 1
fi

mkdir -p "$OUTPUT_DIR"

# ── Batch mode runner (Unity must NOT be open) ───────────────────
run_batch() {
    local method="$1"
    local label="$2"
    local use_graphics="${3:-false}"

    if is_unity_running; then
        echo "ERROR: Unity Editor is running with this project."
        echo ""
        echo "Options:"
        echo "  1. Use MCP: start the MCP server in Unity (Window > MCP for Unity > Start Server)"
        echo "  2. Close Unity Editor, then retry this command"
        echo "  3. Use './agent-bridge.sh health' (works without Unity)"
        return 1
    fi

    echo "[$label] Starting (batch mode)..."
    echo "  Unity: $UNITY"
    echo "  Project: $PROJECT_PATH"
    echo "  Method: $method"
    echo ""

    local graphics_flag="-nographics"
    if [[ "$use_graphics" == "true" ]]; then
        graphics_flag=""
        echo "  Graphics: enabled (GPU rendering)"
    fi

    local exit_code=0
    "$UNITY" \
        -batchmode \
        $graphics_flag \
        -projectPath "$PROJECT_PATH" \
        -executeMethod "$method" \
        -logFile "$LOG_FILE" \
        -timestamps \
        || exit_code=$?

    if [[ $exit_code -eq 0 ]]; then
        echo "[$label] PASSED"
    else
        echo "[$label] FAILED (exit code: $exit_code)"
        echo "  Check logs: $LOG_FILE"
    fi

    local json_file
    case "$label" in
        "Compile") json_file="$OUTPUT_DIR/compilation.json" ;;
        "Validate") json_file="$OUTPUT_DIR/validation.json" ;;
        "Health") json_file="$OUTPUT_DIR/health.json" ;;
        "Report") json_file="$OUTPUT_DIR/report.json" ;;
        *) json_file="" ;;
    esac

    if [[ -n "$json_file" && -f "$json_file" ]]; then
        echo ""
        echo "--- Results ---"
        cat "$json_file"
        echo ""
    fi

    return $exit_code
}

# ── Interactive mode runner (full GPU, no -batchmode) ─────────────
# Launches Unity with the full Editor GUI so Metal/GPU shaders
# compile correctly. Used for visual tests that need non-magenta renders.
run_interactive() {
    local method="$1"
    local label="$2"

    if is_unity_running; then
        echo "ERROR: Unity Editor is already running with this project."
        echo ""
        echo "Options:"
        echo "  1. Use MCP: start the MCP server in Unity (Window > MCP for Unity > Start Server)"
        echo "  2. Close Unity Editor, then retry this command"
        echo "  3. Use './agent-bridge.sh health' (works without Unity)"
        return 1
    fi

    echo "[$label] Starting (interactive mode — full GPU rendering)..."
    echo "  Unity: $UNITY"
    echo "  Project: $PROJECT_PATH"
    echo "  Method: $method"
    echo "  Graphics: Metal (interactive — shaders will compile correctly)"
    echo ""

    local exit_code=0
    "$UNITY" \
        -projectPath "$PROJECT_PATH" \
        -executeMethod "$method" \
        -logFile "$LOG_FILE" \
        -timestamps \
        || exit_code=$?

    if [[ $exit_code -eq 0 ]]; then
        echo "[$label] PASSED"
    else
        echo "[$label] FAILED (exit code: $exit_code)"
        echo "  Check logs: $LOG_FILE"
    fi

    return $exit_code
}

# ── Smart routing for visual tests ────────────────────────────────
# Uses MCP if Unity is open, otherwise launches in interactive mode.
run_visual_smart() {
    local batch_method="$1"
    local mcp_tool="$2"
    local mcp_args="${3:-\{\}}"
    local label="$4"

    if is_unity_running; then
        if is_mcp_available; then
            echo "  Mode: MCP (Unity is open, MCP server responding)"
            call_mcp "$mcp_tool" "$mcp_args" "$label"
        else
            echo "ERROR: Unity is open but MCP server is not running."
            echo "  Start it: Window > MCP for Unity > Start Server"
            echo "  Or close Unity and retry (will use interactive mode)."
            return 1
        fi
    else
        run_interactive "$batch_method" "$label"
    fi
}

# ── MCP caller (Unity must be open with MCP server running) ──────
call_mcp() {
    local tool_name="$1"
    local args="${2:-{\}}"
    local label="${3:-MCP}"

    if ! is_mcp_available; then
        echo "[$label] MCP server not responding at $MCP_URL"
        echo "  Start it in Unity: Window > MCP for Unity > Start Server"
        return 1
    fi

    echo "[$label] Calling MCP tool: $tool_name"

    local payload
    payload=$(cat <<MCPEOF
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "$tool_name",
    "arguments": $args
  }
}
MCPEOF
)

    local response
    response=$(curl -s --max-time 30 \
        -X POST \
        -H "Content-Type: application/json" \
        -d "$payload" \
        "$MCP_URL" 2>/dev/null)

    echo "$response"
    return 0
}

# ── File-only health check (no Unity needed) ────────────────────
run_health_standalone() {
    echo "[Health] Running standalone code analysis..."
    echo "  (No Unity needed — scanning source files directly)"
    echo ""

    local violations=0
    local files_scanned=0
    local violation_list=""

    while IFS= read -r -d '' cs_file; do
        [[ "$cs_file" == *"/AgentBridge/"* ]] && continue
        files_scanned=$((files_scanned + 1))

        local line_count
        line_count=$(wc -l < "$cs_file")
        local rel_path="${cs_file#"$PROJECT_PATH/"}"

        local limit=300
        case "$rel_path" in
            */Editor/*)   limit=200 ;;
            */Data/*)     limit=150 ;;
            */VFX/*)      limit=200 ;;
            */Visuals/*)  limit=200 ;;
            */UI/*)       limit=250 ;;
            */Managers/*) limit=250 ;;
        esac

        if [[ $line_count -gt $limit ]]; then
            violations=$((violations + 1))
            violation_list="${violation_list}  OVER LIMIT: $rel_path ($line_count lines, limit $limit)\n"
        fi

        if head -1 "$cs_file" | grep -q '#if UNITY_EDITOR'; then
            continue
        fi
        local debug_hits
        debug_hits=$(grep -n 'Debug\.\(Log\|LogWarning\|LogError\)' "$cs_file" 2>/dev/null | grep -v '^\s*//' || true)
        if [[ -n "$debug_hits" ]]; then
            while IFS= read -r hit; do
                violations=$((violations + 1))
                local hit_line
                hit_line=$(echo "$hit" | cut -d: -f1)
                violation_list="${violation_list}  DEBUG.LOG: $rel_path:$hit_line\n"
            done <<< "$debug_hits"
        fi
    done < <(find "$PROJECT_PATH/Assets/Scripts/BalloonGame" -name '*.cs' -print0 2>/dev/null)

    echo "Files scanned: $files_scanned"
    echo "Violations: $violations"
    if [[ $violations -gt 0 ]]; then
        echo ""
        echo "--- Violations ---"
        echo -e "$violation_list"
        return 1
    else
        echo "All clean."
        return 0
    fi
}

# ── Smart routing: pick best available execution method ──────────
run_smart() {
    local batch_method="$1"
    local mcp_tool="$2"
    local mcp_args="${3:-{\}}"
    local label="$4"
    local use_graphics="${5:-false}"

    if is_unity_running; then
        if is_mcp_available; then
            echo "  Mode: MCP (Unity is open, MCP server responding)"
            call_mcp "$mcp_tool" "$mcp_args" "$label"
        else
            echo "ERROR: Unity is open but MCP server is not running."
            echo "  Start it: Window > MCP for Unity > Start Server"
            echo "  Or close Unity and retry (will use batch mode)."
            return 1
        fi
    else
        run_batch "$batch_method" "$label" "$use_graphics"
    fi
}

# ── Status ───────────────────────────────────────────────────────
show_status() {
    echo "Inkshot Agent Bridge — Status"
    echo ""
    echo "Unity Editor: $UNITY"
    echo "Project:      $PROJECT_PATH"

    if is_unity_running; then
        echo "Unity State:  RUNNING (Editor has project open)"
        if is_mcp_available; then
            echo "MCP Server:   AVAILABLE at $MCP_URL"
            echo ""
            echo "All commands available via MCP (no batch mode needed)."
        else
            echo "MCP Server:   NOT RUNNING"
            echo ""
            echo "Start MCP: In Unity, go to Window > MCP for Unity > Start Server"
            echo "Until then, only 'health' and 'status' work."
        fi
    else
        echo "Unity State:  NOT RUNNING"
        echo "MCP Server:   N/A (Unity not open)"
        echo ""
        echo "All commands available via batch mode."
    fi
}

# ── Main dispatch ────────────────────────────────────────────────
case "${1:-help}" in
    compile)
        run_smart \
            "Inkshot.Editor.AgentBridge.AgentBridgeEntryPoint.Compile" \
            "refresh_unity" '{}' "Compile"
        ;;
    validate)
        run_smart \
            "Inkshot.Editor.AgentBridge.AgentBridgeEntryPoint.Validate" \
            "execute_menu_item" '{"menu_path":"Inkshot/Agent Bridge/Validate Scene"}' "Validate"
        ;;
    health)
        if is_unity_running; then
            if is_mcp_available; then
                run_smart \
                    "Inkshot.Editor.AgentBridge.AgentBridgeEntryPoint.Health" \
                    "execute_menu_item" '{"menu_path":"Inkshot/Agent Bridge/Health Check"}' "Health"
            else
                run_health_standalone
            fi
        else
            run_batch "Inkshot.Editor.AgentBridge.AgentBridgeEntryPoint.Health" "Health"
        fi
        ;;
    report)
        run_smart \
            "Inkshot.Editor.AgentBridge.AgentBridgeEntryPoint.FullReport" \
            "execute_menu_item" '{"menu_path":"Inkshot/Agent Bridge/Full Report"}' "Report"
        ;;
    screenshot)
        run_smart \
            "Inkshot.Editor.AgentBridge.AgentBridgeEntryPoint.Screenshot" \
            "screenshot" '{"window":"Game"}' "Screenshot" "true"
        if ls "$OUTPUT_DIR"/screenshots/*.png 1>/dev/null 2>&1; then
            echo "Screenshots saved to: $OUTPUT_DIR/screenshots/"
            ls -lt "$OUTPUT_DIR"/screenshots/*.png | head -5
        fi
        ;;
    gameplay)
        # Play Mode capture: boots game, clicks Start Run, screenshots the game board
        # Uses interactive mode (not batch) so URP shaders render correctly via Metal
        run_visual_smart \
            "Inkshot.Editor.AgentBridge.AgentBridgeEntryPoint.CaptureGameplay" \
            "execute_menu_item" '{"menu_path":"Inkshot/Agent Bridge/Capture Gameplay"}' "Gameplay"
        if ls "$OUTPUT_DIR"/screenshots/gameplay_*.png 1>/dev/null 2>&1; then
            echo "Gameplay screenshots saved to: $OUTPUT_DIR/screenshots/"
            ls -lt "$OUTPUT_DIR"/screenshots/gameplay_*.png | head -5
        fi
        ;;
    dart-test)
        # Play Mode: fires darts at 30/60/100% pull with before/after screenshots
        # Uses interactive mode (not batch) so URP shaders render correctly via Metal
        run_visual_smart \
            "Inkshot.Editor.AgentBridge.AgentBridgeEntryPoint.DartTest" \
            "execute_menu_item" '{"menu_path":"Inkshot/Agent Bridge/Dart Physics Test"}' "DartTest"
        if ls "$OUTPUT_DIR"/screenshots/dart_test_*.png 1>/dev/null 2>&1; then
            echo "Dart test screenshots saved to: $OUTPUT_DIR/screenshots/"
            ls -lt "$OUTPUT_DIR"/screenshots/dart_test_*.png | head -10
        fi
        ;;
    smoke-test)
        run_batch "Inkshot.Editor.AgentBridge.AgentBridgeEntryPoint.SmokeTest" "SmokeTest"
        ;;
    console)
        if is_mcp_available; then
            call_mcp "read_console" '{}' "Console"
        else
            echo "Console reading requires MCP. Start Unity and the MCP server."
            exit 1
        fi
        ;;
    hierarchy)
        if is_mcp_available; then
            call_mcp "find_gameobjects" '{"search_type":"all"}' "Hierarchy"
        else
            echo "Hierarchy inspection requires MCP. Start Unity and the MCP server."
            exit 1
        fi
        ;;
    run-code)
        if [[ -z "${2:-}" ]]; then
            echo "Usage: ./agent-bridge.sh run-code '<C# code>'"
            exit 1
        fi
        if is_mcp_available; then
            local escaped_code
            escaped_code=$(echo "$2" | sed 's/"/\\"/g')
            call_mcp "execute_dynamic_code" "{\"code\":\"$escaped_code\"}" "RunCode"
        else
            echo "Dynamic code execution requires MCP. Start Unity and the MCP server."
            exit 1
        fi
        ;;
    status)
        show_status
        ;;
    help|--help|-h)
        echo "Inkshot Agent Bridge — Unity feedback loop for AI agents"
        echo ""
        echo "Usage: ./agent-bridge.sh <command>"
        echo ""
        echo "Commands (work in batch mode OR via MCP):"
        echo "  compile      Trigger recompilation and report errors"
        echo "  validate     Validate scene setup (camera, layers, components)"
        echo "  health       Analyze code for standards violations"
        echo "  report       Run all checks and produce combined report"
        echo "  screenshot   Capture scene screenshot (1080x1920 PNG)"
        echo "  dart-test    Fire darts at 30/60/100% pull with before/after screenshots (interactive GPU)"
        echo "  smoke-test   Run play-mode smoke test (batch only)"
        echo ""
        echo "Commands (MCP only — requires Unity open + MCP server):"
        echo "  console      Read Unity console output"
        echo "  hierarchy    Inspect scene hierarchy"
        echo "  run-code     Execute arbitrary C# in the Editor"
        echo ""
        echo "Utility:"
        echo "  status       Check Unity/MCP state and available commands"
        echo "  help         Show this help"
        echo ""
        echo "Routing:"
        echo "  Unity closed → batch mode (compile, health) or interactive mode (visual tests)"
        echo "  Unity open + MCP → instant MCP calls (no relaunch)"
        echo "  Unity open, no MCP → 'health' works; others need MCP started"
        echo ""
        echo "Visual tests (dart-test, gameplay, screenshot) use interactive mode"
        echo "for correct Metal GPU shader compilation (avoids magenta renders)."
        echo ""
        echo "Environment:"
        echo "  UNITY_EDITOR_PATH   Path to Unity Editor executable (auto-detected)"
        echo "  UNITY_MCP_URL       MCP server URL (default: http://localhost:8080/mcp)"
        echo ""
        echo "Output:"
        echo "  JSON results: Logs/agent-feedback/"
        echo "  Screenshots:  Logs/agent-feedback/screenshots/"
        echo "  Editor log:   Logs/agent-feedback/editor.log"
        ;;
    *)
        echo "Unknown command: $1"
        echo "Run './agent-bridge.sh help' for usage."
        exit 1
        ;;
esac
