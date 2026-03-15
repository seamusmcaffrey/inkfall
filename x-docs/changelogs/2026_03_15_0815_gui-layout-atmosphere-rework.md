# GUI, Layout & Atmosphere Rework

## What Changed
- **GameConstants.cs**: Camera ortho size 10→7.0, camera Y-center at 1.5, lane compressed from 8→4.2 units (LANE_TOP -1.5→-0.8, LANE_BOTTOM -9.5→-5.0), launch position -7.5→-3.5, pull distance 3.0→2.0, activation radius 5→3, HUD margins and font size reduced
- **InGameHUD.cs / InGameHUD.Builder.cs**: Top bar height 68→36px, badge 56→30px, progress bar 12→6px, all font sizes scaled ~40% smaller, tighter element spacing, smaller dart tray
- **BalloonCamera.cs**: Camera now positions at CAMERA_Y_CENTER to center game content in viewport
- **AtmosphereController.cs**: Mist alpha 0.12→0.03, colors dark blue-gray (no purple), rate 5.5→2.0, particles 60→20, size 3.8→2.5
- **EnvironmentBuilder.cs**: Fog alpha 0.55→0.15, layers halved in size, wall renderers disabled entirely, lane guide color darkened
- **NeonLightRig.cs**: Intensities and ranges reduced across all neon/mid/rim lights, AmberBottom/GreenRim repositioned for compressed lane
- **LaunchLaneVisuals.cs**: Guide line alpha reduced from 0.35→0.15
- **GameConfig.asset**: Launch speeds retuned 13-22→10-18 for compressed spatial layout

## Verification
- compile: PASS (0 errors, 0 warnings)
- health: PASS (0 violations, 124 files scanned)
- gameplay screenshot: Board fills ~65% of viewport, dark moody atmosphere, no purple haze, no cyan horizontal line artifact
- dart-test: Distinct trajectories at 30/60/100% pull — 30% hits rows 2-3, 60% hits rows 4-5, 100% hits rows 5-7

## Audit Score (Final — Pass 2)
| Category | Weight | Score |
|----------|--------|-------|
| HUD & GUI Proportions | 25% | 8/10 |
| Atmosphere & Mood | 25% | 9/10 |
| Viewport Fill & Proportions | 20% | 9/10 |
| Spatial Coherence | 15% | 7/10 |
| Integration & No Regressions | 15% | 8/10 |
| **WEIGHTED TOTAL** | | **8.3/10** |

## Process Notes
- Broad single-pass approach addressing all 4 problems worked well since they're interdependent
- Cyan horizontal line was eliminated by disabling wall renderers entirely (not just material override)
- Physics retuned proportionally to lane compression — maintained trajectory differentiation
- Camera Y-centering ensures balanced framing of board + lane content
- HUD scored by code review only (Camera.Render limitation with ScreenSpaceOverlay)
