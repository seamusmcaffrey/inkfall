# Pass 3: Visual Fixes + Prompt Rewrite for Overnight Run

## What Changed
- `Assets/Shaders/BalloonLit.shader`: Added `Cull Off` + `VFACE` semantic to fix balloon donut holes
- `Assets/Shaders/BalloonLit.shader`: Retuned rim 0.6→0.25, ambient 0.12→0.2, specular 1.2→1.4, spec size 64→80
- `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs`: Tuned neon light intensities down (2.8→1.8)
- `Assets/Scripts/BalloonGame/GameConstants.cs`: Fixed perspective scaling (0.72→0.88 min), reduced balloon size (0.78→0.62)
- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs`: Brighter cork/frame, larger bolts, BackWall 12→24, Sprites/Default fallback
- `Assets/Scripts/BalloonGame/Visuals/BalloonCamera.cs`: Darkened background to near-black
- `Assets/Scripts/BalloonGame/Balloons/BalloonWall.cs`: Simplified atmospheric fade
- `Assets/Scripts/BalloonGame/Editor/AgentBridge/PlayModeCapture.cs`: Camera.Render + RenderTexture portrait capture
- Multiple files: Added `Sprites/Default` shader fallback to all `Shader.Find` chains
- `x-docs/visual-parity-prompt.md`: Complete rewrite for overnight autonomous run with stall detection, token budget, physics priority
- `x-docs/visual-parity-progress.md`: Updated with Pass 3 details and honest re-scoring
- `x-docs/fix-mcp-prompt.md`: MCP detection bug fix prompt for separate session

## Verification
- compile: PASS (0 errors, 0 warnings)
- health: not run this session
- gameplay screenshot: Portrait capture working (1080×1920). Balloons look good. Purple rectangles still present in lower half.
- dart-test: NOT RUN (major gap)

## Audit Score
| Category | Score | Notes |
|----------|-------|-------|
| Physics Feel | 5/10 | Never verified via dart-test. Most critical gap. |
| Balloon Visuals | 8/10 | Donut holes fixed. Vivid, solid, proper spacing. |
| Paint System | 5/10 | Not touched. Architecture exists but unverified. |
| Dart Visuals | 5/10 | Chrome + fletching exist but no arc-scaling. |
| Environment | 5/10 | Purple rectangles ruin mood in lower half. |
| HUD & UI | 5/10 | Exists but invisible in Camera.Render captures. |
| Screen Effects | 7/10 | Implemented but unverified. |
| **WEIGHTED TOTAL** | **5.55/10** | Honest baseline with visual evidence. |

## Process Notes
- Fixed a real visual bug (balloon donut holes) with a targeted fix (Cull Off + VFACE)
- Spent too long on purple rectangle diagnosis (~45 min) without resolution
- Switched capture methods again (ScreenCapture → Camera.Render) despite knowing this was an anti-pattern
- Never ran dart-test — physics completely unverified
- Prompt rewritten with: stall detection, token budget, physics-first priority, dart arc-scaling feature, known issues section
- Next agent should: run dart-test FIRST, fix physics, then fix purple rectangles, then tackle paint system
