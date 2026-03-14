# Handoff: Play-Mode Screenshot Capture Research

**Date:** 2026-03-14 18:45
**Status:** Blocked — needs research before further implementation
**Priority:** High — this unblocks the entire visual feedback loop

## Executive Summary

We need automated screenshot capture of Unity play mode for an AI agent feedback loop. The agent fires scripted darts at different pull levels and captures before/after screenshots to verify physics tuning and visual quality. We've built the test harness infrastructure but cannot yet produce correctly-rendered screenshots programmatically. This handoff requests research into the best approach — whether that's CLI batch mode, MCP-driven with Unity open, or something else entirely.

## Problem Statement

We need a workflow where an AI coding agent can:
1. Script game actions (fire darts at specific velocities, click UI buttons)
2. Capture screenshots showing **correctly-rendered visuals** (not magenta)
3. Inspect those screenshots to verify visual quality
4. Iterate on code changes based on what it sees

The approach doesn't need to be headless or CLI-only. If the best path is "Unity runs interactively and the agent talks to it via MCP," that's a valid answer. We're unopinionated about the mechanism — we just need the feedback loop to work.

## What We Have

### MCP Servers (when Unity is open interactively)

Two MCP servers are configured in `.mcp.json`:

- **unityMCP** — HTTP at `localhost:8080/mcp`. Provides scene graph access, script execution, console reading, material inspection.
- **uLoopMCP** — Node server at TCP port 8700. Provides screenshot capture, dynamic C# execution, play mode control.

When Unity is open with MCP running, we can:
- Execute arbitrary C# in the editor via `execute_dynamic_code`
- Take screenshots via MCP screenshot tools (these render correctly — full GPU, real game window)
- Read the console, inspect hierarchy, etc.

**Key fact:** MCP screenshots render correctly because Unity is running interactively with full GPU and shader compilation. This is the one path we know produces non-magenta output.

### CLI Batch Mode (`agent-bridge.sh`)

A shell script that launches Unity in `-batchmode` with `-executeMethod` to run editor code. Works for compilation, health checks, and play-mode automation (entering play mode, clicking buttons, firing darts). Does NOT work for screenshot capture because:

1. **`ScreenCapture.CaptureScreenshot()`** — requires a game view window. In `-batchmode` there is no window. Calls succeed silently but produce no file on disk.

2. **`Camera.Render()` + RenderTexture** — produces PNGs, but the custom `BalloonLit.shader` (URP HLSL with specular, fresnel, SSS) renders magenta. Even with `-nographics` dropped (GPU available), URP shaders don't compile in `-batchmode`.

3. **Editor/runtime script split** — MonoBehaviours in `Editor/` can't be added as play-mode components. We moved `DartPhysicsTestRunner` to `Darts/` to resolve this, but it means the runtime runner can't call editor-only capture APIs.

### Test Harness Infrastructure (built, working mechanically)

| Component | File | Status |
|-----------|------|--------|
| State machine (enter play mode, click START RUN, spawn runner) | `Editor/DartPhysicsTest.cs` (191 lines) | Working |
| Dart firing at 30/60/100% pull with screenshot hooks | `Darts/DartPhysicsTestRunner.cs` (118 lines) | Working — darts fire, balloons pop |
| RenderTexture screenshot capture | `Editor/AgentBridge/ScreenshotCapture.cs` (169 lines) | Produces PNGs, but magenta in batch |
| Agent bridge CLI command | `agent-bridge.sh` (`dart-test` command) | Working |
| Play-mode gameplay capture | `Editor/AgentBridge/PlayModeCapture.cs` (188 lines) | Working, same magenta issue |

## Research Questions

We don't know which approach is right. Here are the questions worth investigating:

### Q1: MCP-driven approach — can we script actions through MCP while Unity renders normally?

This might be the most natural path since MCP screenshots already render correctly. The question is whether we can drive the full test sequence through MCP:
- Enter play mode via MCP (`execute_dynamic_code` calling `EditorApplication.isPlaying = true`?)
- Click the START RUN button via MCP
- Fire darts at specific velocities via MCP (calling `DartLauncher.SpawnAndLaunch()`)
- Capture screenshots via MCP screenshot tools between each dart
- Exit play mode via MCP

Sub-questions:
- Can MCP execute code that modifies play-mode state? Or is it editor-only?
- Can MCP screenshot tools capture the game view during play mode?
- Is there a way to auto-start the MCP server when Unity opens (so the agent doesn't need manual setup)?
- Can the agent drive this sequence through MCP tool calls in Claude Code's `.mcp.json` integration?

### Q2: Is there an alternative to `-batchmode` for CLI automation?

Something that gives us a real game window (so shaders compile and render correctly) but still allows scripted control:
- `-executeMethod` without `-batchmode` — does Unity open the GUI and run the method?
- A command-line flag that opens the editor interactively but auto-runs a method?
- `[InitializeOnLoad]` that detects a sentinel file and drives automation from within a normally-opened editor?

### Q3: Can URP shaders be forced to compile in `-batchmode`?

If batch mode IS the right path, is there a way to get correct rendering?
- `Shader.WarmupAllShaders()` before capture?
- `UniversalRenderPipeline.RenderSingleCamera()` instead of `Camera.Render()`?
- Pre-building the shader cache interactively, then reusing it in batch?
- Forcing specific shader variant compilation via script?

### Q4: Best practices for play-mode test automation in Unity 6

How do studios and CI pipelines handle "enter play mode, do stuff, capture screenshots, exit"?
- Unity Test Framework play-mode tests with image capture?
- Graphics test framework (Unity uses one internally)?
- Third-party tools or packages?
- Any Unity 6-specific APIs for automated visual testing?

### Q5: Hybrid approach — script via CLI, capture via MCP?

Could we combine approaches?
- Launch Unity interactively from CLI (no `-batchmode`)
- Wait for MCP server to become available
- Drive the test sequence via MCP calls from the shell script
- Capture screenshots via MCP (which renders correctly)
- Exit via MCP or `EditorApplication.Exit()`

This would let us keep the shell-script entry point (`./agent-bridge.sh dart-test`) while getting correct renders.

## Key Files for Context

- `Assets/Shaders/BalloonLit.shader` — custom URP HLSL shader (155 lines), fallback `"Universal Render Pipeline/Lit"`
- `agent-bridge.sh` — CLI entry point, batch/MCP routing (~410 lines)
- `.mcp.json` — MCP server configuration (unityMCP + uLoopMCP)
- `Assets/Scripts/BalloonGame/Editor/DartPhysicsTest.cs` — state machine for play-mode dart test
- `Assets/Scripts/BalloonGame/Darts/DartPhysicsTestRunner.cs` — runtime dart firing + capture
- `Assets/Scripts/BalloonGame/Editor/AgentBridge/PlayModeCapture.cs` — existing gameplay capture state machine
- `Assets/Scripts/BalloonGame/Editor/AgentBridge/ScreenshotCapture.cs` — RenderTexture capture utility

## What Success Looks Like

An automated workflow where the AI agent can:
1. Trigger scripted game actions (fire darts, click buttons)
2. Capture screenshots showing correctly-rendered balloons, darts, and environment
3. Read/inspect those screenshots
4. Make code changes and repeat

The entry point can be a shell command, MCP tool calls, or any other mechanism the agent can invoke. We don't care about the plumbing — we care about getting real, non-magenta screenshots into the agent's feedback loop.

## Environment

- **Unity:** 6000.3.10f1
- **URP:** 17.3.0
- **Platform:** macOS (Darwin 24.6.0)
- **Unity Hub:** `/Applications/Unity/Hub/Editor/`
- **MCP:** unityMCP (HTTP, port 8080), uLoopMCP (Node, TCP port 8700)
- **Agent:** Claude Code with MCP integration via `.mcp.json`
