# Visual & Mechanical Parity — Progress Log

This file is the agent's persistent memory across sessions. Read it first, write to it last.

---

## Pass Summary

| Pass | Category Targeted | Summary | Score | Delta |
|------|-------------------|---------|-------|-------|
| 1 | All (foundation) | Physics tuned (gravity -7.8, speed 14-28), balloon materials enhanced, environment darkened with corner bolts, neon lights boosted, HUD restructured with noir styling, screen effects implemented. | 6.65/10 | baseline |
| 2 | Physics + Environment | Physics retuned (gravity=-12, speed 13-20, exp=1.5). Fixed gameplay capture hanging. Made floor/background unlit. Darkened frame. | 5.55/10 | -1.10 |
| 3 | Environment + Capture | Fixed balloon donut holes (Cull Off + VFACE). Reduced balloon sizing/spacing. Fixed perspective scaling. Added shader fallbacks. Camera.Render portrait capture. Purple rectangles still unresolved. | 5.55/10 | +0.00 |

---

## Latest Audit Scores

_Last scored: Pass 3 (honest scoring with visual verification)._

| Category | Weight | Score | Key Gap |
|----------|--------|-------|---------|
| Physics Feel | 25% | 5/10 | Values exist but NEVER verified via dart-test. No evidence darts reach all rows. No arc verification. |
| Balloon Visuals | 15% | 8/10 | Custom shader with wrap lighting, dual specular, SSS, rim. Donut holes fixed. Vivid colors. |
| Paint System | 10% | 5/10 | Architecture exists but plain quads. Not visually verified. |
| Dart Visuals | 10% | 5/10 | Chrome material + fletching exist. No arc-scaling (grow/shrink). Not verified. |
| Environment | 15% | 5/10 | Cork board + frame + neon rig exist. Purple rectangles in lower half ruin mood. |
| HUD & UI | 15% | 5/10 | All elements exist but Camera.Render misses ScreenSpaceOverlay. Can't verify. |
| Screen Effects | 10% | 7/10 | All implemented per spec. Not visually verified during gameplay. |
| **WEIGHTED TOTAL** | | **5.55/10** | Previous 6.65 was scored without visual evidence — this is honest baseline. |

---

## Pass 1 Details

### What Shipped
- Physics: gravity set to -7.8, speed 14-28
- Balloon materials: emission, glossiness, specular highlights per color
- Environment: dark cork board, weathered metal frame, corner bolts, neon light rig (magenta/cyan/gold/green)
- Paint VFX: neon-boosted splatters, drip system, persistent marks
- HUD: industrial noir styling, room badge, score/target, darts counter, progress bar
- Screen effects: shake (Perlin + decay), slow-mo (3+ combo), combo flash (vignette), impact sparks

### What Worked
- Broad foundation pass covering all categories established the baseline
- EventBus-driven VFX architecture made adding effects clean
- ScriptableObject config made tuning values easy to adjust

### What Didn't Work
- Tried to do everything in one pass — too broad, hard to verify individual changes
- Physics feel underserved because visual work consumed most of the effort

### Next Priorities (written by Pass 1)
1. Physics feel needs major rework — darts don't arc, speed range too narrow
2. Paint splatters need texture (currently plain quads)
3. Balloon depth/glossiness needs more work

---

## Pass 2 Details

### What Shipped
- Physics: gravity -7.8→-12, minSpeed 10→13, maxSpeed 36→20, exponent 1.8→1.5
- Console logs confirm trajectory spread: 30%→Y=0.8, 60%→Y=3.5, 100%→Y=9.2
- PlayModeCapture: added auto-exit detection (no longer hangs)
- Gameplay capture: switched to portrait RenderTexture (1080x1920)
- EnvironmentBuilder: runtime rebuild + stale object cleanup
- NeonLightRig: runtime rebuild
- Floor/background/fog/stripes: switched to unlit materials
- Frame: reduced metallic 0.85→0.5, smoothness 0.72→0.45, darkened color
- Walls: overridden to dark unlit at runtime
- DartPhysicsTestRunner: UNITY_EDITOR guards, X-offset per dart
- Progress bar fill: initialized to 0

### What Worked
- Physics retuning via console log trajectory peaks was effective for rough tuning
- Auto-exit detection pattern (matching DartPhysicsTest) fixed the capture hang
- Unlit materials for floor/walls immediately improved the moody atmosphere

### What Didn't Work
- **Changed capture method mid-session** (landscape ScreenCapture → portrait Camera.Render) — before/after became incomparable. Never do this.
- **~30 min debugging capture timeouts** before reading the exit code. Always read exit codes first.
- **Excessive analysis of "cyan line" bug** instead of just iterating. Try it, screenshot, evaluate.
- **Didn't discover stale SceneBuilder objects** until 3 capture cycles in. Read scene hierarchy before debugging visuals.

### Next Priorities (written by Pass 2)
1. **Re-audit with consistent capture method** — use `./agent-bridge.sh gameplay` for all comparisons going forward
2. **Paint system** — splatters are visually weak (biggest gap). Need procedural splatter textures, not plain quads
3. **Balloon depth/glossiness** — more specular punch to match reference
4. **HUD verification** — Camera.Render misses ScreenSpaceOverlay; need gameplay capture to verify HUD

---

## Pass 3 Details

### Audit Breakdown
| Category | Score | Delta | Key Gap |
|----------|-------|-------|---------|
| Physics Feel | 5/10 | +0 | Never ran dart-test. Values exist but no visual proof. |
| Balloon Visuals | 8/10 | +1 | Donut holes fixed (Cull Off + VFACE). Sizing/spacing corrected. |
| Paint System | 5/10 | -1 | Not touched. Previous score was generous. |
| Dart Visuals | 5/10 | -2 | Not touched. Needs arc-scaling feature (grow on rise, shrink on fall). |
| Environment | 5/10 | -2 | Purple rectangles in lower half. Shader fallbacks added but didn't resolve. |
| HUD & UI | 5/10 | -3 | Camera.Render doesn't capture ScreenSpaceOverlay. Can't verify HUD. |
| Screen Effects | 7/10 | -1 | Implemented but unverified. |
| **WEIGHTED TOTAL** | **5.55/10** | **-1.10** | Score drop is from honest re-evaluation, not regression. |

### What Shipped
- BalloonLit.shader: Added `Cull Off` + `VFACE` normal flip (fixed donut holes)
- BalloonLit.shader: Retuned rim intensity 0.6→0.25, ambient 0.12→0.2, specular 1.2→1.4
- NeonLightRig: Tuned light intensities (neon 2.8→1.8, key 0.65→0.72)
- GameConstants: Fixed perspective scaling (min scale 0.72→0.88, pinch 0.12→0.06)
- GameConstants: Reduced balloon size (width 0.78→0.62, height 0.94→0.74)
- EnvironmentBuilder: Brightened cork (0.14→0.22), larger bolts, wider stripes, BackWall 12→24 wide
- EnvironmentBuilder: Added Sprites/Default shader fallback chain
- All Shader.Find chains: Added Sprites/Default as final fallback
- BalloonCamera: Background darkened to (0.02, 0.02, 0.03)
- PlayModeCapture: Switched to Camera.Render with RenderTexture at 1080×1920 portrait
- BalloonWall: Simplified atmospheric fade (removed aggressive 0.6× darkening)
- x-docs/fix-mcp-prompt.md: Wrote MCP detection bug fix prompt

### What Worked
- Cull Off + VFACE immediately fixed the donut holes — simple, targeted fix
- Reducing balloon size from 0.78 to 0.62 made cork board visible between balloons
- Camera.Render + RenderTexture gives clean portrait captures without editor chrome

### What Didn't Work
- **~45 min diagnosing purple rectangles** without resolution. Root cause still unclear.
- **Switched capture methods AGAIN** (ScreenCapture → Camera.Render) despite knowing this was anti-pattern from Pass 2.
- **Never ran dart-test** — physics completely unverified.
- **Camera.Render misses ScreenSpaceOverlay** — HUD is invisible in captures. Need dual-capture or ScreenCapture crop.
- **Scope creep into tooling** — spent too much time on capture mechanics instead of game visuals.

### Known Issues (for next pass)
1. **Purple rectangles in lower half** — Large purple/magenta areas visible below the balloon board in the launch lane area (y=-1.5 to -9.5). Hypotheses: (a) SceneBuilder's LIT materials receiving neon light before EnvironmentBuilder overrides them, (b) shader fallback producing unexpected colors, (c) light bleeding. Need to debug by: disabling NeonLightRig temporarily, checking if purple disappears.
2. **HUD not captured** — Camera.Render doesn't render ScreenSpaceOverlay canvases. Either switch back to ScreenCapture with viewport-rect cropping, or keep Camera.Render and verify HUD separately.
3. **Dart arc-scaling not implemented** — PRD/reference implies darts should visually grow as they rise and shrink as they fall. No code exists for this yet. Needs implementation in DartController.cs.

### Next Priorities (written by Pass 3)
1. **Fix purple rectangles** — Try disabling NeonLightRig, see if they vanish. If so, add culling masks or reduce light range. If not, trace exact objects with a debug pass.
2. **Run dart-test** — First time ever. Verify trajectories at 30/60/100%.
3. **Implement dart arc-scaling** — Darts grow on rise, shrink on fall. New feature in DartController.
4. **Fix HUD capture** — Either crop ScreenCapture to viewport rect, or use dual-capture approach.
5. **Paint system** — Biggest visual gap from reference. Need procedural splatter textures.

---

## Lessons Learned (accumulated)

These apply to ALL future passes:

1. **Never change capture method mid-session.** Use `./agent-bridge.sh gameplay` consistently.
2. **One visual change → one screenshot.** Don't batch visual changes without intermediate verification.
3. **Read exit codes before retrying.** Failed commands tell you why in stdout/stderr.
4. **Iterate, don't analyze.** Trying a value and screenshotting is faster than calculating by hand.
5. **Don't change measurement while changing the measured thing.** Don't move the camera AND change physics in the same pass.
6. **Read scene hierarchy before debugging visuals.** Stale objects from prior passes cause confusion.
7. **Console log trajectory peaks** are useful for rough physics tuning but don't replace `dart-test` screenshots.
8. **Time-box debugging.** If a visual bug isn't resolved in 15 minutes, try a brute-force approach (disable things, add debug colors) instead of analyzing code.
9. **Run dart-test FIRST** when physics is the lowest score. Don't get distracted by visual polish.
10. **Score HONESTLY.** Previous scores were optimistic (rated "implemented" features 7-8 without visual verification). Only score what you've SEEN working in a screenshot.
11. **Kill stuck Unity processes.** Check `pgrep -x Unity` and kill + rm lockfile before retrying agent-bridge commands.
12. **PlayModeCapture uses Camera.Render** — this does NOT capture ScreenSpaceOverlay UI. HUD verification requires a separate approach.
