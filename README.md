# Inkfall

A roguelite balloon-dart game built in Unity 6 (URP). Slingshot darts at a balloon wall, pop combos, earn perks, progress through rooms.

## Stack

- **Engine**: Unity 6 / URP 17.3.0
- **Platform**: iOS (portrait 9:16)
- **Input**: Unity Input System + Enhanced Touch
- **Architecture**: EventBus, ObjectPool, ScriptableObject-driven config

## Project Structure

```
Assets/Scripts/BalloonGame/
  Core/        — EventBus, ObjectPool, SaveManager, ComponentUtility
  Data/        — ScriptableObject definitions
  Events/      — Event structs
  Managers/    — BalloonGameManager, ScoreManager
  Roguelite/   — Run/room/perk systems, meta-progression
  Balloons/    — Balloon grid, nodes, types
  Darts/       — Launching, flight, collision
  UI/          — HUD, screens, overlays
  VFX/         — Particle effects
  Visuals/     — Camera, slingshot, launch lane
  Audio/       — AudioManager, SoundLibrary
```

## Agent Bridge

`./agent-bridge.sh <command>` provides Unity Editor interaction for agent-driven development:

- `compile` — Build check (0 errors required)
- `health` — Code quality validation
- `gameplay` — Boot game, capture screenshot
- `dart-test` — Fire scripted darts at 30/60/100% pull, capture results
- `screenshot` — Scene View render
- `validate` — Scene structure checks

## License

See [LICENSE.md](LICENSE.md).
