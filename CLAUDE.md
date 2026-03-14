# Inkshot (Unity) — Project Standards

## Hard Limits

- **No file over 300 lines.** If approaching 250, split proactively into focused partial classes or new files.
- **No magic numbers.** Use named constants in `GameConstants.cs` or ScriptableObject fields for all tuning values.
- **No `// ReSharper disable` or `#pragma warning disable`.** Fix the root cause.
- **No `Debug.Log()` in committed code.** Use conditional compilation (`#if UNITY_EDITOR`) or a logging utility for diagnostics.
- **No stringly-typed keys.** Use enums or constants for layer names, tags, PlayerPrefs keys, event names, etc.
- **No god files.** If a MonoBehaviour is doing input + logic + rendering + UI, split it.
- **No external packages beyond what's in manifest.json.** Check with the project owner before adding dependencies.
- **No `FindObjectOfType` in Update loops.** Cache references in `Awake`/`Start` or use `ComponentUtility.ResolveSceneReference<T>()`.
- **No `GetComponent` in hot paths.** Cache in `Awake` or use `[SerializeField]`.
- **No snapshot/screenshot tests.** Use behavioral assertions on game state.

## File Size Guide

| File type | Max lines |
|-----------|-----------|
| MonoBehaviour (gameplay) | 300 |
| MonoBehaviour (UI screen) | 250 |
| ScriptableObject definition | 200 |
| Manager / service | 250 |
| Data / model | 150 |
| Editor script | 200 |
| Partial class extension | 150 |
| Test file | 300 |
| VFX / visual component | 200 |
| Static utility / constants | 150 |

If a file exceeds its limit, extract into partial classes (`BalloonWall.Generation.cs`, `DartController.Collision.cs`), child components, or focused helper types.

## Architecture

### Layer Separation

```
Scripts/BalloonGame/Core/        # EventBus, ObjectPool, SaveManager, ComponentUtility — no gameplay logic
Scripts/BalloonGame/Data/        # ScriptableObject definitions — pure data, no behavior
Scripts/BalloonGame/Events/      # Event struct definitions — plain C# structs
Scripts/BalloonGame/Managers/    # Game state orchestration — BalloonGameManager, ScoreManager
Scripts/BalloonGame/Roguelite/   # Run/room/perk systems — meta-progression layer
Scripts/BalloonGame/Balloons/    # Balloon grid, nodes, types — rendering + collision
Scripts/BalloonGame/Darts/       # Dart launching, flight, collision — physics + input
Scripts/BalloonGame/UI/          # HUD, screens, overlays — UnityUI / TextMeshPro
Scripts/BalloonGame/VFX/         # Particle effects, pop/splash/spark — event-driven visuals
Scripts/BalloonGame/ScreenFX/    # Screen shake, slow-motion, post-processing
Scripts/BalloonGame/Visuals/     # Slingshot band, launch lane, camera — non-particle rendering
Scripts/BalloonGame/Audio/       # AudioManager, SoundLibrary — pooled audio playback
Scripts/BalloonGame/Combo/       # Combo tracking and multiplier calculation
Scripts/BalloonGame/Haptics/     # Device vibration feedback
Scripts/BalloonGame/Editor/      # Editor-only tooling (SceneBuilder, URPSetup)
```

- **Core and Data have zero MonoBehaviour rendering logic.** They are infrastructure and pure data.
- **Managers orchestrate state transitions.** They do not render or handle input directly.
- **UI scripts read state from managers** — they don't compute game logic.
- **VFX and Visuals subscribe to EventBus events** — they react, they don't drive gameplay.

### One Responsibility Per File

- One MonoBehaviour per file (small private helpers co-located are fine).
- One ScriptableObject definition per file.
- One event struct per logical group (related events can share a file in `Events/`).
- Shared utilities go in `Core/`.

### State — Single Source of Truth

- `BalloonGameManager` owns room-level state (game phase, darts remaining, current score).
- `RunManager` owns run-level state (room number, total score, perk draft flow).
- UI components read from these managers — they never maintain parallel state.
- ScriptableObjects (`GameConfigSO`, `BalloonTypeSO`, etc.) are the single source for tuning values.
- Never store the same state in both a manager and a component. One source of truth.

## Unity / C# Standards

### Component Management

- **Cache all component references.** Use `[SerializeField]` or `Awake()` — never `GetComponent` per frame.
- **Use `ComponentUtility.EnsureComponent<T>()`** for runtime-created components.
- **Use `ComponentUtility.ResolveSceneReference<T>()`** for cross-object dependency resolution.
- **Prefer `[SerializeField] private`** over `public` fields for Inspector-exposed values.
- **SetDependencies() pattern** for programmatic injection (used by SceneBuilder for scene setup).

### EventBus Pattern

- All cross-system communication goes through `EventBus<T>`.
- Subscribe in `OnEnable`, unsubscribe in `OnDisable`. No exceptions.
- Event structs live in `Events/` — keep them small, immutable data carriers.
- Never use `SendMessage` or `BroadcastMessage`. Use EventBus.

### Object Pooling

- **All frequently spawned objects must use `ObjectPool`.** Balloons, darts, audio sources, VFX particles.
- Register pools in `PoolManager` for centralized warm-up.
- `Return()` pooled objects — never `Destroy()` them in gameplay.
- Set pool initial capacity to expected max concurrent usage.

### Physics

- All physics constants (restitution, friction, damping, force) must be named constants or ScriptableObject fields.
- `Rigidbody.collisionDetectionMode = ContinuousDynamic` on all fast-moving bodies (darts).
- Physics layers defined via Unity's layer system — reference by constant, never raw int.
- Collision matrix configured in `ProjectSettings/DynamicsManager.asset`.

**Collision Layers:**
```
3  = Environment (walls, floor)
7  = Balloons
8  = Projectiles (darts)
9  = Ricochet (dart-to-dart bouncing anchors)
10 = UIWorld (floating text, world-space UI)
```

### ScriptableObject Configuration

- **All tuning values go in ScriptableObjects**, not hardcoded in MonoBehaviours.
- `GameConfigSO` — input, scoring, dart lifetime, perk weights.
- `BalloonTypeSO` — per-type balloon definitions (color, score, special behavior).
- `JuiceConfigSO` — VFX intensity, shake magnitude, slow-motion parameters.
- `UIConfigSO` — animation durations, fade speeds, layout constants.
- Access global config via `GameConfigSO.Instance`.

## Rendering & Visual Standards

### URP (Universal Render Pipeline)

- Project uses **URP 17.3.0** on **Unity 6**.
- Custom shaders go in `Assets/Shaders/` — use Shader Graph or URP-compatible hand-written shaders.
- `BalloonLit.shader` is the primary custom shader for balloon rendering.

### Particle Systems

- **Always set `maxParticles`.** Never allow unbounded emission.
- **VFX are event-driven.** Subscribe to EventBus events, spawn on demand, auto-cleanup.
- **VFXFactory creates runtime particle systems** — don't use prefab instantiation for one-shot effects.
- **Quality-tier scaling.** Use `GameConstants.ParticleQualityMultiplier` to reduce particles on low-end devices.

### Camera

- Fixed orthographic camera, portrait orientation (9:16).
- Orthographic size: 10.
- Managed by `BalloonCamera` component.

## UI Standards

### TextMeshPro

- All text uses TextMeshPro — no legacy Unity Text.
- Font sizes and colors defined in `UIConfigSO` or shared style constants.
- Score displays, combo text, and floating labels all use TMP.

### Screen Flow

- Screens managed by enable/disable pattern with fade transitions.
- `FadeOverlay` for scene-level transitions.
- `ScreenTransition` for individual screen swaps.
- All UI animation durations come from `UIConfigSO`.

### Layout

- Reference resolution: 1080×1920.
- Safe area padding: 24px (from `GameConstants`).
- Test on multiple aspect ratios — layout must not break on wider or narrower screens.

## Input

- **Unity Input System** (1.18.0) — no legacy `Input` class usage.
- Enhanced Touch Support enabled for mobile.
- `SlingshotInput` handles drag-to-aim via `Pointer.current`.
- Dead zone, cancel radius, and pull range defined in `GameConfigSO`.

## Naming Conventions

- **PascalCase**: Types, enums, methods, properties (`BalloonNode`, `GameState`, `PopBalloon()`)
- **camelCase**: Local variables, parameters, private fields (`dartsRemaining`, `comboMultiplier`)
- **_camelCase**: Private backing fields with serialization (`_balloonWall`, `_scoreManager`)
- **SCREAMING_SNAKE**: Never. Use `static readonly` or `const` properties.
- **Files match types**: `BalloonNode.cs` contains `BalloonNode`. Period.
- **Partial classes**: `TypeName.Category.cs` (e.g., `BalloonWall.Generation.cs`)
- **SO suffix**: ScriptableObject definitions use `SO` suffix (`GameConfigSO`, `BalloonTypeSO`)
- **No Hungarian notation.** No `m_`, `s_`, `k_` prefixes.

## Testing

- Test game logic (Managers/, Core/, Roguelite/), not rendering (VFX/, Visuals/).
- Unity Test Framework (1.6.0) is available — use it for edit-mode and play-mode tests.
- Tests go in a `Tests/` assembly with naming: `<ClassName>Tests.cs`.
- Use deterministic seeds for reproducible scenarios.
- Test behavior and outcomes, not internal implementation details.
- Mock at boundaries only (e.g., substitute `SaveManager` with test-specific storage).

## Git

- Commit after each completed phase.
- Conventional commit messages: `feat:`, `fix:`, `refactor:`, `chore:`, `polish:`.
- No committing `.env`, secrets, `Library/`, `Temp/`, `Logs/`, or build artifacts.
- Never commit `.meta` files without their corresponding asset.
- Phase commits should be atomic — the project must compile at every commit.

## Performance Checklist

Before considering any phase complete:
1. No `GetComponent` or `Find*` calls in `Update`/`FixedUpdate`/`LateUpdate`.
2. All pooled objects returned properly — no leaked instances.
3. Particle systems have `maxParticles` set and auto-stop/cleanup.
4. No unbounded collections growing without cleanup.
5. EventBus subscriptions balanced — every `OnEnable` subscribe has an `OnDisable` unsubscribe.
6. ScriptableObject references are not null at runtime (validate in `OnValidate` where appropriate).

## When In Doubt

1. Smaller files > clever abstractions
2. Split by responsibility > split by arbitrary line count
3. Read the file before editing it
4. EventBus > direct references between unrelated systems
5. ScriptableObject config > hardcoded values
6. Delete code > comment it out
7. One thing done well > two things done halfway
8. Pool and reuse > instantiate and destroy
