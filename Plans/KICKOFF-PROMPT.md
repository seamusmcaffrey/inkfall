# Kickoff Prompt

Copy everything below the line into a new Claude Code session from this project directory.

---

You are building a **balloon-dart throwing game scene** inside this existing Unity 6 project. Read all plans in `Plans/` (00 through 05) — they are your implementation spec.

## Critical Rules

1. **USE the existing physics.** This project has excellent Rigidbody projectile physics (SUVAT equations, trajectory prediction, ArrowMover). The dart is a Rigidbody. Unity handles gravity, drag, and collision. Do NOT write custom flight controllers or intersection math.

2. **BUILD in this codebase.** New scripts go in `Assets/Scripts/BalloonGame/`. No separate namespaces. Reuse existing patterns (ArrowMover for dart rotation, ArrowPositionPredicter SUVAT math for trajectory preview). Do NOT modify existing scripts.

3. **Input System only.** `activeInputHandler: 1` — use `UnityEngine.InputSystem` (`Pointer.current`, `Keyboard.current`). No legacy `Input` class.

4. **Unity collision.** Balloons have SphereColliders + kinematic Rigidbodies. Darts have colliders + dynamic Rigidbodies. `OnCollisionEnter` handles everything. No custom segment-to-circle math.

## What To Build

A single new scene (`Assets/Scenes/InkshotScene.unity`) with:

- **Balloon wall**: 8×9 grid of colored spheres with perspective scaling (top row smaller/pinched, bottom row full-size)
- **Slingshot input**: Drag to aim, release to throw. Pull direction → opposite launch direction. Pull distance → launch speed (non-linear curve)
- **Dart flight**: Rigidbody with gravity. Rotates to face velocity. Bounces off side walls (bouncy PhysicMaterial). Stops at top wall.
- **Collision**: Dart hits balloon → balloon pops (deactivates), score += 100
- **Game loop**: 4 darts, target 3000 points. Win when score ≥ target. Lose when out of darts. Press R to restart.
- **HUD**: Score, target, darts remaining

## Execution

1. Read ALL existing scripts first — understand what's here before writing anything
2. Read ALL plans (00-05) — they have exact code, constants, and architecture
3. Build incrementally: scene → balloons → dart/slingshot → collision → game loop
4. Test after each step

## do not ask questions
## do not stop short — implement everything end to end
## when done, codebase must compile with zero errors
