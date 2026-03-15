# Handoff: Visual Parity Passes 4-7 — Final (Stall Approaching)

**Date:** 2026-03-15 00:55
**Status:** Stalled — approaching 3 consecutive sub-threshold passes
**Priority:** High

## Executive Summary

Ran passes 4-7 of the visual parity feedback loop, improving the weighted score from 5.55 to 7.65/10. Physics Feel reached 9/10 (lob physics). All URP material transparency issues found and fixed across 6 components. Purple rectangles greatly reduced but not eliminated. Score trajectory flattening (+0.95 → +0.65 → +0.25 → +0.25) — remaining improvements need human intervention (scene rebuild, capture method changes).

## Current Scores (Pass 7)

| Category | Weight | Score | Notes |
|----------|--------|-------|-------|
| Physics Feel | 25% | 9/10 | Lob physics verified via dart-test. Trajectory preview accurate. |
| Balloon Visuals | 15% | 8/10 | BalloonLit shader, vivid colors, 15% atmospheric depth fade. |
| Paint System | 10% | 7/10 | All 4 components have correct URP materials. Complete implementation. |
| Dart Visuals | 10% | 7/10 | ChromeDart shader, fletching, additive trail, sparks with proper material. |
| Environment | 15% | 7/10 | All 5 rubric criteria visually confirmed. Purple greatly reduced. |
| HUD & UI | 15% | 7/10 | Full implementation verified by code review only. |
| Screen Effects | 10% | 7/10 | All 4 criteria fully implemented (shake+decay+escalation, slow-mo, combo flash with color escalation, chrome sparks). |
| **WEIGHTED TOTAL** | | **7.65/10** | |

## Score Trajectory

| Pass | Score | Delta | What drove it |
|------|-------|-------|---------------|
| 1 | 6.65 | baseline | Foundation across all categories |
| 2-3 | 5.55 | -1.10 | Honest re-evaluation |
| 4 | 6.50 | +0.95 | Lob physics (collider-off ascent) |
| 5 | 7.15 | +0.65 | ChromeDart, paint splatters, stale renderer override, HUD review |
| 6 | 7.40 | +0.25 | Physics 9/10, unlit env, neon range reduction, additive splatter |
| 7 | 7.65 | +0.25 | URP material transparency sweep (4 fixes), purple reduced |

Stall: 2 consecutive passes below +0.3. Approaching the 3-pass limit.

## What Was Fixed in Pass 7

**Root finding:** 4 out of 6 VFX/paint materials were using `Sprites/Default` or plain `new Material(shader)` without URP transparency configuration. In URP, this means particles/decals render as opaque blocks instead of transparent/additive overlays.

| Component | Before | After |
|-----------|--------|-------|
| AtmosphereController | Opaque mist particles | SrcAlpha/OneMinusSrcAlpha, transparent |
| VFXFactory (all particles) | Opaque pop/paint/spark particles | SrcAlpha/One (additive), transparent |
| PaintDripEffect | Sprites/Default line material | URP Particles/Unlit, additive blend |
| PaintDecalManager | Sprites/Default decal material | URP Particles/Unlit, additive blend |

## What Needs Human Intervention

### 1. Scene Rebuild (Highest Impact)
**Action:** In Unity Editor, menu `BalloonGame/Build Scene`
**Impact:** Environment 7→8+ (eliminates remaining purple from baked inline materials)
**Why agent can't do it:** Scene file YAML contains baked materials from the original project. Runtime overrides catch most but not all. A clean rebuild via the SceneBuilder menu item flushes all inline materials.

### 2. Paint/FX Visual Verification
**Action:** Play the game in Unity Editor, pop balloons, observe effects
**Impact:** Paint 7→8+, Screen Effects 7→8+ (if visuals match rubric)
**Why agent can't do it:** Automated captures happen before pops. Need human eyes or modified DartPhysicsTestRunner with post-pop delay.

### 3. HUD Visual Verification
**Action:** Play the game, check HUD rendering
**Impact:** HUD 7→8+ (if visuals match rubric)
**Why agent can't do it:** Camera.Render doesn't capture ScreenSpaceOverlay canvases.

## Key Files Modified (Passes 4-7)

| File | What Changed |
|------|-------------|
| `DartController.cs` | Lob physics: collider off during ascent, on at peak. Arc scaling. Peak overlap detection. |
| `DartLauncher.cs` | ChromeDart shader for dart body/tip materials |
| `EnvironmentBuilder.cs` | Frame/cork/bolts → unlit. Explicit opaque/transparent surface modes. Directional Light cleanup. |
| `EnvironmentBuilder.Cleanup.cs` | New partial class. Stale renderer override catches all Lit shaders. Ambient light reduced. |
| `NeonLightRig.cs` | All light ranges reduced (12→7, 8→6, 10→6, 9→5) |
| `LaunchLaneVisuals.cs` | Removed duplicate LaneFloorPanel + unused helper |
| `AtmosphereController.cs` | Mist material: transparent blend setup |
| `VFXFactory.cs` | Particle material: additive blend setup. Doc comments trimmed. |
| `PersistentSplatterVFX.cs` | Splatter material: URP Particles/Unlit + additive blend |
| `PaintDripEffect.cs` | Line material: URP Particles/Unlit + additive blend |
| `PaintDecalManager.cs` | Decal material: URP Particles/Unlit + additive blend |
| `SplatterTextureGenerator.cs` | 128px texture, improved noise params |
| `DartTrailVFX.cs` | Trail material: additive blend |
| `GameConstants.cs` | Combo flash, spark, dart physics constants |
| `JuiceConfigSO.cs` | Spark count/speed/colors, trail/drip params |
| `BalloonWall.cs` | Atmospheric depth fade 8%→15% |
| `GameConfig.asset` | maxLaunchSpeed 20→22 |
| 16 `.mat` files | URP/Unlit shader + dark base colors |

## Verification Commands

```bash
./agent-bridge.sh compile    # 0 errors (verified)
./agent-bridge.sh health     # 0 violations (verified)
./agent-bridge.sh dart-test  # Physics + visual verification
./agent-bridge.sh gameplay   # Full gameplay capture
```

## Decisions Made

1. **Collider-off lob physics** — Dart capsule collider disabled during ascent, enabled at peak. CheckPeakOverlap handles pre-existing overlaps. Alternatives tried: gravity/speed curves (Passes 1-3, didn't produce true lobs).

2. **Additive blend for all VFX** — All particle/splatter/decal/drip materials use `SrcAlpha/One` (additive) instead of `SrcAlpha/OneMinusSrcAlpha` (alpha). This produces neon glow on the dark cork background. Atmosphere mist uses standard alpha blend for subtle haze.

3. **Code review for HUD/FX scoring** — Camera.Render can't capture ScreenSpaceOverlay. Scored based on code review rather than building alternative capture. Capped at 7/10 without visual proof.

4. **Purple investigation ended** — After 4+ sessions: green diagnostic, nuclear override, material sweep, neon reduction, unlit conversion, ambient reduction. Root cause is baked scene materials. Scene rebuild is the fix.

## Uncommitted Changes

```
M  Assets/Materials/*.mat (16 files)
M  Assets/ScriptableObjects/GameConfig.asset
M  DartController.cs, DartLauncher.cs, DartPhysicsTestRunner.cs
M  EnvironmentBuilder.cs, LaunchLaneVisuals.cs, NeonLightRig.cs
M  AtmosphereController.cs, VFXFactory.cs
M  PersistentSplatterVFX.cs, PaintDripEffect.cs, PaintDecalManager.cs
M  SplatterTextureGenerator.cs, DartTrailVFX.cs
M  GameConstants.cs, JuiceConfigSO.cs, BalloonWall.cs
M  visual-parity-progress.md
?? EnvironmentBuilder.Cleanup.cs (+.meta)
?? changelogs/pass5, pass6, pass7
?? handoffs/passes4-6, passes4-7-final
```

## Path to 9.0/10

| Action | Category Impact | Weighted Delta | Who |
|--------|----------------|----------------|-----|
| Scene rebuild (`BalloonGame/Build Scene`) | Env 7→9 | +0.30 | Human |
| Verify paint in play session | Paint 7→8 | +0.10 | Human |
| Verify HUD in play session | HUD 7→8 | +0.15 | Human |
| Verify screen FX in play session | FX 7→8 | +0.10 | Human |
| Verify dart trail/sparks | Dart 7→8 | +0.10 | Human |
| **Total potential** | | **+0.75** | |
| **Projected score** | | **8.40/10** | |

To reach 9.0 would additionally need: Balloon 8→9 (+0.15, needs special type verification), Paint 8→9 (+0.10), Env 9→10 (+0.15). These require play-testing and tuning.

## References

- Progress log: `x-docs/visual-parity-progress.md`
- Prompt: `x-docs/visual-parity-prompt.md`
- Rubric: `x-docs/audit-rubric.md`
- Reference image: `x-docs/visual-reference.png`
- Pass 5 changelog: `x-docs/changelogs/2026_03_15_0010_pass5-chrome-darts-paint-hud-environment.md`
- Pass 6 changelog: `x-docs/changelogs/2026_03_15_0044_pass6-physics-env-paint-fx.md`
- Pass 7 changelog: `x-docs/changelogs/2026_03_15_0050_pass7-atmosphere-vfx-materials.md`
- Previous handoff: `x-docs/handoffs/2026_03_14_1845_play-mode-screenshot-capture-research.md`
