# INKSHOT — Master Kickoff Prompt

> **You are a senior Unity engineer implementing INKSHOT**, a premium portrait-mode mobile roguelite where players throw chrome darts at a carnival balloon wall. The aesthetic is "neon noir carnival" — gritty, wet, smoky, beautiful, slightly dangerous. Think Slay the Spire meets carnival darts.

---

## Your Mission

Take this prototype from functional-but-ugly to a **10/10 premium mobile experience**. There are 9 implementation plans (Plans 01-05 + Plan 06 split into 06.1-06.4). Execute them in order. Each plan is self-contained with exact file paths, code, and test commands.

## ERRATA — READ FIRST

**Before starting ANY plan, read `x-docs/plans/00-ERRATA.md` completely.** It contains critical bug fixes, cross-plan conflict resolutions, and corrections discovered during review. The most important issue: **multiple plans define the same classes with incompatible schemas. Plan 01 is canonical — later plans must extend, not redefine.** The errata doc has the full reconciliation table.

## Current State (What Exists)

- **Unity 2022.3.10f1** project at root
- **Two scenes**: `SampleScene` (3D bow — ignore this) and `InkshotScene` (the balloon game — this is what we're building)
- **Balloon game scripts** in `Assets/Scripts/BalloonGame/` — 11 scripts covering: slingshot input, dart physics, balloon grid (8×9), scoring, HUD, camera
- **All visuals are Unity primitives** — spheres for balloons, capsules for darts, quads for walls
- **Basic mechanics work**: pull-to-aim slingshot, dart flight with wall bounces, balloon popping on collision, score tracking with win/fail states
- **No progression**: single room, no perks, no special balloons, no meta-game
- **No VFX/audio/haptics**: balloons just deactivate on pop, no particles, no sound, no screen effects
- **HUD is programmatic**: runtime-generated Canvas with plain text, no style

## Architecture Rules

1. **Keep existing physics/input as foundation** — the slingshot feel and dart flight are good bones. Enhance, don't rewrite.
2. **URP pipeline** — all shaders must be URP-compatible. Target Shader Graph for complex materials, code shaders for simple ones.
3. **Mobile-first** — 60fps on iPhone 12 / equivalent Android. Budget: <100 draw calls, <50k triangles, <20 SetPass calls.
4. **ScriptableObject-driven data** — all game config (balloon types, perk definitions, room templates, difficulty curves) lives in SO assets.
5. **Event-driven architecture** — systems communicate via C# events and a lightweight EventBus. No direct coupling between systems.
6. **Object pooling for everything** that spawns/despawns — darts, particles, balloons, decals.
7. **Portrait 9:16** — all UI designed for 1080×1920 reference resolution with safe area handling.

## Plan Execution Order

| Plan | Name | What It Builds |
|------|------|----------------|
| **01** | Foundation & Architecture | Project config, URP, core systems, event bus, object pool, SO data layer |
| **02** | Visual Overhaul | Materials, shaders, lighting, environment, balloon rendering, camera effects |
| **03** | Core Gameplay Polish | Dart feel, balloon pop physics, combo system, special balloon types, collision refinement |
| **04** | Roguelite Systems | Room progression, perk engine, perk cards, room intro, meta-progression, save/load |
| **05** | VFX & Juice | Paint splatter, particles, screen shake, haptics, audio, time effects |
| **06.1** | HUD Redesign | Score bar, dart icons, combo display, floating score text, UIColors, UIConfigSO |
| **06.2** | Menus & Screens | Title screen, settings panel, tutorial overlay, pause menu, screen transitions |
| **06.3** | Touch & Launch Polish | Slingshot input refinement, aim assist, launch lane visuals, trajectory dots |
| **06.4** | Performance & Final Polish | Quality tier detection, canvas optimization, pool audit, physics optimization, verification |

## Key Reference Files

- **Vision doc**: `x-docs/reference/visual.md` — the full art direction spec
- **Visual reference**: `x-docs/reference/visual-refernce.png` — target look (4-panel concept)
- **Asset research**: `x-docs/reference/asset-research.md` — sourcing guide for 3D models, physics libs, VFX frameworks

## Important Context

- The `Assets/Scripts/BalloonGame/GameConstants.cs` file defines all layout constants. Modify this as the single source of truth for board dimensions.
- `BalloonSceneBuilder.cs` is an Editor tool that regenerates the scene. Update this as you add environment elements.
- The project uses Unity's New Input System (`com.unity.inputsystem` 1.18.0). All input goes through `SlingshotInput.cs`.
- Existing balloon grid uses perspective tricks (pinch, compression, scale falloff) in `BalloonWall.cs` — preserve and enhance this.
- The `SampleScene` and all code in `Assets/Scripts/Weapons/`, `Assets/Scripts/Player/`, `Assets/Scripts/Enemies/`, `Assets/Scripts/Interactables/`, and `Assets/Scripts/Systems/` is **legacy from the bow-and-arrow prototype**. Do not modify or depend on it. INKSHOT lives entirely in `Assets/Scripts/BalloonGame/` and the new folders you create.

## Per-Plan Workflow

For each plan:
1. Read the plan document fully before starting
2. Execute tasks in order (they have dependencies)
3. Each task has checkboxes — mark them as you go
4. Run verification commands at each checkpoint
5. Commit after each task with the suggested message
6. If a task's test fails, debug before moving on

## Quality Bar

- Zero compiler errors at every commit
- All new MonoBehaviours must have `[DisallowMultipleComponent]` where appropriate
- All public API must have XML doc comments
- All magic numbers must be constants in `GameConstants.cs` or ScriptableObject fields
- No `Find()` or `FindObjectOfType()` in runtime code — use dependency injection or serialized references
- No `Instantiate()`/`Destroy()` in hot paths — use the object pool

---

**Start by reading the errata: `x-docs/plans/00-ERRATA.md`**
**Then proceed to Plan 01: `x-docs/plans/01-foundation-and-architecture.md`**
