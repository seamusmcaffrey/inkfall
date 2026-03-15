# Visual & Mechanical Parity — Progress Log

This file is the agent's persistent memory across sessions. Read it first, write to it last.

---

## Pass Summary

| Pass | Category Targeted | Summary | Score | Delta |
|------|-------------------|---------|-------|-------|
| 1 | All (foundation) | Physics tuned (gravity -7.8, speed 14-28), balloon materials enhanced, environment darkened with corner bolts, neon lights boosted, HUD restructured with noir styling, screen effects implemented. | 6.65/10 | baseline |
| 2 | Physics + Environment | Physics retuned (gravity=-12, speed 13-20, exp=1.5). Fixed gameplay capture hanging. Made floor/background unlit. Darkened frame. | pending | — |

---

## Latest Audit Scores

_Last scored: Pass 1. Pass 2 is pending re-audit with consistent capture method._

| Category | Weight | Score | Key Gap |
|----------|--------|-------|---------|
| Physics Feel | 25% | 5/10 | Speed range narrow; needs dart-test verification with new values |
| Balloon Visuals | 15% | 7/10 | Solid materials/mesh; needs visual verify |
| Paint System | 10% | 6/10 | Architecture complete; splatters are plain quads, no paint texture |
| Dart Visuals | 10% | 7/10 | Chrome + fletching good; fins very thin |
| Environment | 15% | 7/10 | Full neon rig + cork board; no normal maps yet |
| HUD & UI | 15% | 8/10 | Comprehensive noir layout; dart icons still rectangles |
| Screen Effects | 10% | 8/10 | All spec requirements implemented |
| **WEIGHTED TOTAL** | | **6.65/10** | |

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

## Lessons Learned (accumulated)

These apply to ALL future passes:

1. **Never change capture method mid-session.** Use `./agent-bridge.sh gameplay` consistently.
2. **One visual change → one screenshot.** Don't batch visual changes without intermediate verification.
3. **Read exit codes before retrying.** Failed commands tell you why in stdout/stderr.
4. **Iterate, don't analyze.** Trying a value and screenshotting is faster than calculating by hand.
5. **Don't change measurement while changing the measured thing.** Don't move the camera AND change physics in the same pass.
6. **Read scene hierarchy before debugging visuals.** Stale objects from prior passes cause confusion.
7. **Console log trajectory peaks** are useful for rough physics tuning but don't replace `dart-test` screenshots.
