# Visual & Mechanical Parity — Orchestration Plan

Goal: Match the visual reference (visual-refernce.png) and PRD (prd.md) specification.

## Visual Reference Analysis

The reference shows a "graffiti noir carnival" aesthetic:
- Glossy 3D balloons with bright specular highlights, vivid saturated colors
- Metallic emblems on balloons (crown, sun, warning triangle, shield)
- Dark cork/wood board with weathered metal frame
- Neon paint splatters (green, pink, cyan) dripping down the board
- Chrome/silver darts with visible fletching
- Industrial HUD: Room badge (top-left), Score/Target bar (top-center), Darts + Currency (top-right)
- Premium perk selection cards with bold icons, rarity borders
- Room intro screens with balloon type previews and moody fog backdrop

## Implementation Phases

### Phase 1: Color & Material Foundation
- [x] Fix BalloonData.cs colors to be vivid, saturated (matching reference)
- [x] Tune BalloonLit.shader for glossy 3D look with strong specular
- [x] Add additional lights support in BalloonLit.shader (neon point lights)
- [x] Update CLAUDE.md with agent bridge paths

### Phase 2: Environment Overhaul
- [x] EnvironmentBuilder.cs: cork board, metal frame, floor, atmosphere
- [x] Floor stripes for launch area
- [x] Fog/atmosphere layers behind balloons
- [x] NeonLightRig: 6 lights (key, fill, magenta, cyan, gold, green)

### Phase 3: HUD Redesign
- [x] InGameHUD.cs: Match reference layout (Room badge left, Score center, Darts right)
- [x] Industrial styling with dark panels and accent borders
- [x] RunHUD with ink and perk display

### Phase 4: VFX — Paint System
- [x] Improve VFXFactory particle sizes and shapes for paint look
- [x] Add paint drip streaks on board surface (PaintDripEffect)
- [x] Neon glow on paint particles
- [x] PersistentSplatterVFX: neon-boosted paint marks that persist on the board

### Phase 5: Polish
- [x] Perk card premium styling (PerkCardUI)
- [x] Room intro screen with fog backdrop (RoomIntroScreen)
- [x] Dart mesh with fletching (DartMeshGenerator)
- [x] Balloon symbol/emblem overlay system (BalloonEmblem + BalloonEmblem.shader)
- [x] Chrome dart materials (DartLauncher)

### Phase 6: Screen Effects
- [x] ScreenShakeManager: Perlin noise displacement, exponential decay, combo escalation
- [x] SlowMotionController: state machine, combo-scaled depth/duration, smooth easing
- [x] ComboFlashVFX: screen-edge vignette, escalating alpha, attack/decay curves
- [x] ImpactSparkVFX: metallic chrome sparks with size variation

### Phase 7: UI Screens
- [x] RunEndScreen: premium dark styling, structured stats, accent colors
- [x] PerkSelectionScreen: dark backdrop, card panel
- [x] TutorialOverlay: clean tutorial flow

### Phase 8: Distinct Balloon Types
- [x] Gold balloons: bright yellow, high gloss, strong specular
- [x] Hazard balloons: dark body, low gloss, weak specular
- [x] Separate _specialMaterials dictionary to avoid overwriting standard colors

### Phase 9: Performance & Feel
- [x] Fix SlingshotVisuals MaterialPropertyBlock per-frame allocation
- [x] BalloonJiggle with highlight multiplier for aim assist pulse
- [x] AimAssist with angle-based scoring and slerp correction
- [x] DartController with continuous dynamic collision, ricochet system

## Self-Critique

### What's working well:
- Shader model is comprehensive (dual specular, SSS, fresnel, saturation boost, additional lights)
- Color palette is vivid and consistent across UI and gameplay
- All VFX are event-driven via EventBus
- All spawned objects are pooled
- No magic numbers — everything uses GameConstants or ScriptableObject fields
- Zero new compilation errors or health violations

### Remaining gaps (can't verify in batch mode):
- Actual in-game visual quality requires interactive Unity testing
- Shader compilation in batch mode shows magenta — real visual output needs GPU rendering
- Balloon mesh shape vs reference (current is reasonable but can't compare visually)
- Exact layout tuning of UI elements may need adjustment when seen on device
- The persistent splatters use simple colored quads — could be improved with a splatter texture

## Feedback Loop

After each phase:
1. `./agent-bridge.sh health` — verify no new violations
2. `./agent-bridge.sh gameplay` — capture screenshot
3. Compare against visual-refernce.png
4. Identify remaining gaps
5. Iterate

## Key Constraints

- No file over 300 lines (Editor: 200, UI: 250, VFX: 200)
- No Debug.Log without #if UNITY_EDITOR
- No magic numbers — use GameConstants or ScriptableObject fields
- All VFX event-driven via EventBus
- Pool all frequently spawned objects
