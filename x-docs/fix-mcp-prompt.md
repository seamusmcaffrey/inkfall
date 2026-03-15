# Fix agent-bridge.sh MCP Detection Bug

## The Bug

In `agent-bridge.sh`, the `is_mcp_available()` function has a bug that causes it to ALWAYS report MCP as available, even when the server is down. This prevents fallback to interactive mode.

```bash
is_mcp_available() {
    local response
    response=$(curl -s -o /dev/null -w "%{http_code}" --max-time 2 "$MCP_URL" 2>/dev/null || echo "000")
    [[ "$response" != "000" ]]
}
```

**Root cause:** When curl fails with exit code 7 (connection refused), it still writes "000" to stdout via `-w "%{http_code}"`. Then `|| echo "000"` triggers (because curl returned non-zero), appending another "000". The variable becomes "000000", which passes `!= "000"`, so the function incorrectly returns true.

## What To Fix

1. **Fix `is_mcp_available()`** — Make it correctly detect when the MCP HTTP endpoint is unreachable. The fix should:
   - Return false (exit 1) when curl can't connect at all
   - Return false when curl gets a non-2xx/non-4xx response
   - Return true only when the server actually responds
   - Handle the curl exit code separately from the HTTP status code

2. **Test the fix** — After fixing, verify:
   - With no server on port 8080: `is_mcp_available` returns false
   - With a server on port 8080: `is_mcp_available` returns true
   - `./agent-bridge.sh gameplay` correctly falls back to interactive mode when MCP is down
   - `./agent-bridge.sh status` correctly reports MCP state

3. **Also add auto-cleanup of stale Unity lockfiles** — When `is_unity_running()` returns true (lockfile exists at `Temp/UnityLockfile`) but no Unity Editor process is actually running (`pgrep -x Unity` returns nothing), the lockfile is stale. Add cleanup logic so that interactive mode can launch without manual `rm -f Temp/UnityLockfile`.

## Files

- `agent-bridge.sh` — the only file that needs changes

## Constraints

- Follow existing code style in the script
- Don't change any command behavior, only fix the detection/routing logic
- Run `bash -n agent-bridge.sh` after changes to verify syntax
- Run `./agent-bridge.sh status` to verify the output is correct
