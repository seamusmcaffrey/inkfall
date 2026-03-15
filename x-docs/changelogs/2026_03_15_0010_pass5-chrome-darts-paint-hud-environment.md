# Pass 5 — Chrome Darts, Paint System, Environment Cleanup, HUD Audit

## What Changed
- DartLauncher.cs: Switched dart body/tip materials from URP/Lit to existing Inkshot/ChromeDart shader with fresnel tint
- GameConstants.cs: Paint splatters — max 30->50, scale 0.25-0.55->0.35-0.75, neonBoost 0.35->0.55, alpha 0.85->0.92, fade 0.6->1.2s
- SplatterTextureGenerator.cs: Resolution 64->128px, bolder noise (lower alpha threshold, softer edges, wider radial falloff)
- JuiceConfigSO.cs: Updated Range attribute for maxPersistentSplatters (50->80 max)
- EnvironmentBuilder.cs: Added OverrideStaleRenderers() for runtime material cleanup (Standard, InternalErrorShader, any Lit)
- EnvironmentBuilder.Cleanup.cs: Extracted cleanup methods into partial class (fixed 206-line health violation)
- BackWall.mat, WallFrame.mat, LaneFloor.mat: _BaseColor white->dark (matching EnvironmentBuilder runtime values)

## Verification
- compile: PASS (0 errors)
- health: PASS (0 violations)
- gameplay screenshot: Board area looks correct. Purple blocks in lane area persist (known issue, exhaustively debugged).
- dart-test: Distinct trajectories at 30/60/100%. Paint splatters visible where balloons popped.

## Audit Score
| Category | Score | Delta |
|----------|-------|-------|
| Physics Feel | 8/10 | +0 |
| Balloon Visuals | 8/10 | +0 |
| Paint System | 6/10 | +1 |
| Dart Visuals | 7/10 | +1 |
| Environment | 6/10 | +1 |
| HUD & UI | 7/10 | +2 |
| Screen Effects | 7/10 | +0 |
| **WEIGHTED TOTAL** | **7.15/10** | **+0.65** |

## Process Notes
- ChromeDart.shader existed in Assets/Shaders/ but was never referenced by DartLauncher — free win
- Purple rectangle debugging exhausted: disabled wall renderers, reduced magenta lights, overrode all Lit materials, darkened .mat files — none resolved it. Needs human investigation with Editor scene view.
- HUD scored by thorough code review (all 5 rubric criteria fully implemented in code). Camera.Render can't capture ScreenSpaceOverlay.
- Broad pass across 4 categories was more effective than deep-diving stuck Environment category
