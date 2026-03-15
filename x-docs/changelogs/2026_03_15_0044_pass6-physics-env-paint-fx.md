# Pass 6 — Physics 9/10, Environment Cleanup, Paint Neon, FX Boost

## What Changed
- EnvironmentBuilder.cs: Frame, cork, bolts switched from Lit to Unlit materials
- EnvironmentBuilder.cs: CreateMaterial now sets explicit _Surface opaque/transparent mode with proper blend settings
- EnvironmentBuilder.cs: "Directional Light" added to StaleVisualRoots cleanup list
- EnvironmentBuilder.Cleanup.cs: OverrideStaleRenderers removes IsChildOf skip, catches all Lit materials; reduced ambient light to (0.05, 0.04, 0.04)
- NeonLightRig.cs: All light ranges reduced (NeonRange 12→7, RimRange 8→6, MidRange 10→6, AmberRange 9→5)
- LaunchLaneVisuals.cs: Removed duplicate LaneFloorPanel and unused CreateUnlitMaterial (EnvironmentBuilder handles floor)
- PersistentSplatterVFX.cs: Splatter material upgraded from Sprites/Default to URP Particles/Unlit with additive blend for neon glow
- GameConstants.cs: Combo flash (thickness 0.12→0.18, alpha/combo 0.03→0.06, cap 0.4→0.55), spark (size bigger, stretch 0.15→0.25)
- JuiceConfigSO.cs: Spark count 18→24, speed 16→20, chrome colors boosted
- BalloonWall.cs: Atmospheric depth fade 8%→15%

## Verification
- compile: PASS (0 errors)
- health: PASS (0 violations)
- gameplay screenshot: Atmospheric fade visible. Purple rectangles persist (confirmed NOT from lights).
- dart-test: 30/60/100% hit correct rows. Impact sparks visible. Trajectory preview line visible.

## Audit Score
| Category | Score | Delta |
|----------|-------|-------|
| Physics Feel | 9/10 | +1 |
| Balloon Visuals | 8/10 | +0 |
| Paint System | 6/10 | +0 |
| Dart Visuals | 7/10 | +0 |
| Environment | 6/10 | +0 |
| HUD & UI | 7/10 | +0 |
| Screen Effects | 7/10 | +0 |
| **WEIGHTED TOTAL** | **7.40/10** | **+0.25** |

## Process Notes
- Green light diagnostic test definitively proved purple rectangles are NOT from neon lights — critical finding for future debugging
- Nuclear material override test proved purple is from BEHIND code-created objects, not from their materials
- Physics scored 9/10 because trajectory preview was confirmed to use identical physics equations as dart flight (same gravity, same kinematic equation)
- Paint/FX improvements can't be visually verified because gameplay capture happens before pops
- Scene file rebuild (BalloonGame/Build Scene) is the most likely fix for purple — would flush all baked inline Lit materials
