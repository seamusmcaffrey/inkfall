# Pass 7 — URP Material Transparency Sweep, Purple Reduced

## What Changed
- AtmosphereController.cs: Mist particle material now properly configured as transparent (SrcAlpha/OneMinusSrcAlpha blend, ZWrite off, _SURFACE_TYPE_TRANSPARENT)
- VFXFactory.cs: Shared particle material configured with additive blend (SrcAlpha/One) for all particle effects (pop, paint, sparks, wall hit)
- VFXFactory.cs: Doc comments trimmed to fit 200-line VFX file limit
- PaintDripEffect.cs: Line material upgraded from Sprites/Default to URP Particles/Unlit with additive blend for neon drip visibility
- PaintDecalManager.cs: Decal material upgraded from Sprites/Default to URP Particles/Unlit with additive blend for neon decal visibility

## Verification
- compile: PASS (0 errors)
- health: PASS (0 violations)
- dart-test: Physics verified. Purple rectangles significantly reduced in lane area.
- gameplay: All 5 environment rubric criteria visually confirmed. Atmospheric haze faintly visible.

## Audit Score
| Category | Score | Delta |
|----------|-------|-------|
| Physics Feel | 9/10 | +0 |
| Balloon Visuals | 8/10 | +0 |
| Paint System | 7/10 | +1 |
| Dart Visuals | 7/10 | +0 |
| Environment | 7/10 | +1 |
| HUD & UI | 7/10 | +0 |
| Screen Effects | 7/10 | +0 |
| **WEIGHTED TOTAL** | **7.65/10** | **+0.25** |

## Process Notes
- Stall detection: 2 consecutive passes below +0.3 (Pass 6: +0.25, Pass 7: +0.25). One more triggers STOP.
- Root finding: 4 out of 6 paint/VFX materials were using Sprites/Default without URP transparency setup — particles and decals were likely rendering as opaque blocks. This is a systemic fix that improves ALL visual effects at once.
- The VFXFactory fix is architecturally important — without transparent blend setup, ALL particle effects (pop burst, paint splatter, impact sparks) were rendering incorrectly in URP.
