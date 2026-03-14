# Visual & Mechanical Parity — Agent Prompt

## Mission

Achieve 10/10 visual AND mechanical parity with the reference materials. Visual polish means nothing if the game doesn't feel right. You are not done until both aspects match. Work autonomously — do not ask questions, do not stop short.

**Priority order: Physics Feel & Visual Quality → Screen Effects → GUI Polish**

The game is TRENDING toward the look of the reference images but still has a long way to go. The physics feel is getting there but still needs A LOT of work. 

## Reference Materials

Read these before writing any code:

| What | Path | Why |
|------|------|-----|
| Visual reference | `visual-refernce.png` (project root) | The target look — every pixel decision should move toward this |
| PRD | `prd.md` (project root) | Mechanical spec — scoring, physics feel, controls, balloon types, perk system |
| Asset research | `x-docs/reference/asset-research.md` | Programmatic methods for procedural meshes, shaders, particles |
| Git history | `git log --all --oneline` | The codebase was originally a bow-and-arrow sim — study what the physics felt like before |
| Project standards | `CLAUDE.md` (project root) | Hard limits, architecture, Agent Bridge commands |

## How to Work

### You are an orchestrator

Use subagents for independent tasks. Protect your context window — delegate, don't do everything inline. Run subagents in parallel when their work doesn't overlap.

### Work incrementally with feedback loops

Do not attempt everything at once. Break work into focused phases. After each phase:

1. `./agent-bridge.sh compile` — must pass with 0 errors
2. `./agent-bridge.sh health` — must introduce no new violations
3. **`./agent-bridge.sh gameplay` — capture a screenshot, read the PNG, compare against `visual-refernce.png`**
4. Identify the gap between current state and reference
5. If you got 5% closer, that's a successful pass — keep going in that direction
6. If you didn't get closer, stop and inspect your methods before continuing

**The screenshot step is not optional.** It is your primary feedback mechanism. If batch mode renders magenta, note it and use MCP if Unity is open. But always attempt the capture.

### Scripted input testing

You must verify physics feel with actual dart launches, not just by reading code. Script slingshot pulls at varying distances:

1. Write a test harness or editor script that programmatically calls `SlingshotInput`'s launch path with controlled pull vectors
2. Test at minimum 3 pull lengths: short (30%), medium (60%), full (100%) of `MAX_PULL_DISTANCE`
3. For each pull length, capture a screenshot at launch and another ~0.5s after release
4. The resulting dart trajectories should show **visible arcs** — not flat lines into the bottom row
5. If all pull lengths produce darts that hit the bottom row, the physics are broken — keep tuning

### Self-critique

After each phase, score your work honestly. Ask yourself:
- Does this **feel** like throwing a dart at a vertical wall of balloons?
- Can I see the dart arc through the air with readable flight?
- Does pull distance meaningfully change which balloons I can reach?
- Does this look like the reference image? Where does it diverge?
- Would a player describe this as "juicy" and "satisfying"?

If any answer is no, iterate before moving on.

## What to Build (decomposed)

### 0. Physics Feel (DO THIS FIRST)

**The problem**: Darts currently feel like they're being thrown at a flat table of balloons directly ahead, not tossed at a vertical wall. No matter how hard you pull the slingshot, darts just hit the bottom row. This is the #1 priority.

**Root cause analysis** — read these files and understand the spatial layout:
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs` — pull-to-velocity conversion
- `Assets/Scripts/BalloonGame/Darts/DartController.cs` — physics after launch (gravity, rotation)
- `Assets/Scripts/BalloonGame/GameConstants.cs` — `LAUNCH_POSITION = (0, -5.5, 0)`, `BOARD_BOTTOM = -1.0`
- `Assets/ScriptableObjects/GameConfig.asset` — `minLaunchSpeed: 10`, `maxLaunchSpeed: 40`, `pullSpeedExponent: 1.45`

**The spatial problem**: Launch point to board bottom is only 4.5 units. At speed 10-40 with gravity, darts arrive at the bottom row almost instantly with no visible arc. There's no room for the dart to fly.

**What "right" feels like** (from PRD §7):
- Heroic, readable, slightly forgiving physics — not realistic dart simulation
- Shots feel snappy and responsive
- Dart travel speed is fast enough to stay exciting but slow enough to read outcomes
- Slight auto-stabilization on release
- Very small aim forgiveness on direct balloon collisions
- Sticky misses lodge firmly into the board

**What to fix**:
1. **Study git history** (`git log --all --diff-filter=M -- Assets/Scripts/`) to understand the original bow-and-arrow arc physics. The game originally had satisfying projectile arcs.
2. **Increase the launch-to-board gap**. The dart needs room to fly. Consider: lowering `LAUNCH_POSITION`, raising `BOARD_BOTTOM`, or both. The player should feel distance between themselves and the balloon wall.
3. **Tune the velocity curve**. A full pull should reach the top rows. A light pull should reach the middle. The bottom rows should be hit by angled shots, not every shot by default.
4. **Add visible arc**. Gravity should create a readable parabolic trajectory. The player should see the dart rise, peak, and descend into the balloon wall. This is the "throwing at a wall" feeling.
5. **Trajectory preview must match**. The dotted aim line in `SlingshotVisuals` must accurately reflect where the dart will actually go, including the arc.
6. **Verify with scripted pulls** (see "Scripted input testing" above). Do not consider physics work done until you can show screenshots of darts reaching different rows at different pull strengths.

**The feel target**: Imagine standing 10 feet from a carnival balloon wall. You cock your arm back, aim up slightly, and lob a dart. It arcs through the air, rises above your eye level, then descends into the balloon grid. That's the feel. Not a flat horizontal throw.

### 1. Balloon Visuals
- Glossy 3D look with strong specular highlights and vivid saturated colors
- Distinct materials for each type (standard, gold, hazard, paint)
- Readable symbols/emblems on special balloons (crown, star, triangle, shield)
- Proper perspective scaling (smaller at top, larger at bottom)

### 2. Paint System
- Thick neon paint splatter particles on balloon pop
- Paint drip streaks that run down the board
- Persistent paint marks on the cork board surface (neon-boosted, capped, oldest fade out)
- Paint should make the board look increasingly "vandalized" as the room progresses

### 3. Dart Visuals
- Chrome/silver metallic materials with high smoothness
- Visible fletching on the dart mesh
- Trails behind darts during flight
- Clear impact feedback (sparks, sound, screen response)

### 4. Environment & Framing
- Dark cork board with weathered metal frame and corner bolts
- Neon lighting rig (magenta, cyan, gold, green point lights)
- Atmospheric fog/mist with slow color oscillation
- Multi-color floor stripes in the launch area
- Dark moody backdrop

### 5. HUD & UI
- Industrial noir aesthetic across all screens
- Top bar: room badge (left), score/target (center), darts + currency (right)
- Premium perk selection cards with rarity-colored borders
- Room intro with large room number, name, target score
- Run end screen with structured stats
- Combo text that escalates in size and color

### 6. Screen Effects (juice)
- Screen shake: Perlin noise displacement, exponential decay, escalates with combo
- Slow motion: triggers on 3+ combos, scales depth with combo size, smooth easing
- Combo flash: screen-edge vignette, color escalation (white > yellow > orange > magenta)
- Impact sparks: small, fast, chrome-colored

## Constraints

All constraints from `CLAUDE.md` apply. The critical ones:

- No file over its category line limit (gameplay: 300, UI: 250, VFX/Visuals: 200, Editor: 200)
- No magic numbers — `GameConstants` or ScriptableObject fields
- No `Debug.Log` without `#if UNITY_EDITOR`
- All VFX event-driven via EventBus, all spawned objects pooled
- Zero new compilation errors when done
- Zero new health violations when done
- **Visual changes require visual verification** — run `./agent-bridge.sh gameplay` and read the screenshot

## Done Criteria

You are done when ALL of these are true:

1. `./agent-bridge.sh compile` → 0 errors, 0 warnings
2. `./agent-bridge.sh health` → no new violations beyond pre-existing
3. Scripted dart launches at 30%, 60%, 100% pull show **distinct trajectories reaching different board rows**
4. Darts visibly arc through the air — no flat-line trajectories
5. `./agent-bridge.sh gameplay` screenshot closely matches `visual-refernce.png`
6. Every section in "What to Build" above is addressed
7. The codebase is clean — no half-finished work, no commented-out code, no TODO placeholders
8. You would score **both** the visual appearance AND the mechanical feel as 10/10
