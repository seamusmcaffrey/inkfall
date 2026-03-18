# Balloon Dart Game: Master Overview

Core gameplay loop: **Drag slingshot → launch dart → dart arcs via Rigidbody physics → hits balloon on wall → balloon pops → score points → repeat until darts run out or target met.**

## What We're Building

A new scene (`InkshotScene`) inside the Inkfall Unity 6 project. We reuse and extend the project's existing physics, input, and projectile systems — not replace them.

## Design Inspiration

Game design comes from **Inkshot** (a Swift/SpriteKit iOS game): 8×9 balloon grid, slingshot throw mechanic, perspective-scaled wall, point scoring. But all implementation uses this project's existing Unity patterns.

## Scope

**IN SCOPE:**
- New scene: `Assets/Scenes/InkshotScene.unity`
- Balloon wall: 8 columns × 9 rows with perspective scaling
- Slingshot drag-to-aim input (replacing auto-aim)
- Dart launched as Rigidbody projectile using existing launcher math
- Unity physics collision (OnCollisionEnter) for balloon pops
- Scoring: 100 pts per balloon, target 3000, 4 darts per round
- Basic HUD (score, darts remaining, target)
- Game state machine: start → throw → win/lose → restart

**OUT OF SCOPE:**
- Paint/splash effects, perks, upgrades, currencies
- Multiple balloon types, hazards
- Stuck darts, ricochets off darts
- Mid-flight curve input
- Sound effects, haptics, visual effects, animations

## Architecture: Extend, Don't Replace

```
Assets/
├── Scenes/
│   ├── SampleScene.unity          ← Existing (untouched)
│   └── InkshotScene.unity         ← NEW
├── Scripts/
│   ├── Player/                    ← Existing (untouched)
│   ├── Systems/                   ← Existing (untouched)
│   ├── Enemies/                   ← Existing (untouched)
│   ├── Interactables/             ← Existing (untouched)
│   ├── Weapons/                   ← Existing (untouched, but referenced)
│   │   ├── ProjectileLaunchers/   ← REUSE: launch velocity math
│   │   ├── Projectiles/           ← REUSE: ArrowMover, ArrowLaunchData
│   │   └── Targets/               ← REUSE: ITargetable pattern
│   └── BalloonGame/               ← NEW: all new scripts here
│       ├── BalloonWall.cs
│       ├── BalloonNode.cs
│       ├── BalloonData.cs
│       ├── SlingshotInput.cs
│       ├── SlingshotVisuals.cs
│       ├── DartLauncher.cs
│       ├── DartController.cs
│       ├── GameManager.cs
│       ├── ScoreManager.cs
│       ├── GameHUD.cs
│       └── GameConstants.cs
├── Prefabs/
│   ├── ArrowPrefab.prefab        ← Existing (reference for dart)
│   └── DartPrefab.prefab         ← NEW (adapted from arrow)
├── Materials/
│   ├── (existing materials)       ← Existing (untouched)
│   ├── BalloonRed.mat            ← NEW
│   ├── BalloonBlue.mat           ← NEW
│   ├── BalloonYellow.mat         ← NEW
│   ├── BalloonGreen.mat          ← NEW
│   ├── BalloonPurple.mat         ← NEW
│   └── WallFrame.mat             ← NEW
└── Data/
    └── Weapons/                   ← Existing ScriptableObjects (reference)
```

## What We Reuse From This Project

| Existing Code | How We Use It |
|---|---|
| `ArrowMover.cs` | Pattern for dart Rigidbody movement — set initial velocity, rotate to face velocity in FixedUpdate |
| `ArrowLaunchData.cs` | Data struct for launch parameters |
| `ArrowPositionPredicter.cs` | Trajectory preview visualization — adapt to show slingshot aim guide |
| `HeightBasedProjectileLauncher.cs` | Reference for SUVAT math if needed for trajectory prediction |
| `ArrowPrefab.prefab` | Template for creating DartPrefab (Rigidbody + collider + mover) |
| `InputReader.cs` / `GameInput.inputactions` | Input System patterns (we create new actions or use Pointer.current directly) |
| Physics layers (Projectiles=8, Enemies=7) | Add a Balloons layer, configure collision matrix |

## What We Take From Inkshot (Design Only)

| Inkshot Design | Unity Implementation |
|---|---|
| 8×9 balloon grid | Sphere GameObjects with SphereColliders |
| Perspective scaling (0.72–1.0) | Transform.localScale per row |
| Slingshot pull-to-launch | SlingshotInput reads Pointer.current drag |
| Pull→velocity mapping (pow 1.45) | DartLauncher calculates velocity from pull |
| Gravity-driven arc | Rigidbody.useGravity = true (Unity physics) |
| Segment-to-circle collision | **REPLACED**: Unity OnCollisionEnter |
| Custom flight time scaling | **REMOVED**: real-time physics feel better |
| Score: 100/balloon, target 3000 | ScoreManager tracks identical values |

## Key Technical Decisions

1. **Rigidbody Physics**: Dart is a Rigidbody. Gravity, drag, and collision are handled by Unity's physics engine. No custom flight controller.

2. **Unity Collision**: Balloons have SphereColliders. Dart has a collider. `OnCollisionEnter` triggers the pop. No custom intersection math.

3. **Orthographic Camera**: Fixed portrait view (9:16), orthographic size 10. Balloons are 3D spheres that look like a flat grid from the camera's perspective.

4. **Side Wall Bounce**: Frame walls have colliders with a bouncy PhysicMaterial (bounciness ~0.8). Dart bounces naturally via Unity physics.

5. **Input System**: Uses `UnityEngine.InputSystem` — `Pointer.current` for drag input, `Keyboard.current` for restart. No legacy `Input` class.

6. **Isolated New Code**: All new scripts in `Assets/Scripts/BalloonGame/`. No modifications to existing scripts. No `Inkshot` namespace — just normal classes.

## Implementation Order

Execute plans in this order:
1. **Plan 01**: Scene setup — camera, walls, lighting, constants
2. **Plan 02**: Balloon wall — grid generation, perspective layout, colliders
3. **Plan 03**: Dart + slingshot — input, launch, Rigidbody flight, trajectory preview
4. **Plan 04**: Collision + scoring — OnCollisionEnter, balloon pop, score tracking
5. **Plan 05**: Game loop — state machine, HUD, round flow, restart

Each plan is self-contained and testable. Build and verify after each.
