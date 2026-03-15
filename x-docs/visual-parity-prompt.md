# Visual & Mechanical Parity — Autonomous Agent Prompt

## Mission

Achieve 10/10 visual AND mechanical parity with `x-docs/reference/visual-refernce.png`. Work autonomously — do not ask questions, do not stop short. You are a long-running agent that self-corrects through structured feedback loops.

## Before You Write Any Code

Read these in order. Do not skip any.

| # | What | Path | Why |
|---|------|------|-----|
| 1 | **Progress log** | `x-docs/visual-parity-progress.md` | Your memory. Read the latest pass, scores, gaps, and "next priorities". This is where you pick up. |
| 2 | **Visual reference** | `x-docs/reference/visual-refernce.png` | The target. Every decision should move toward this image. |
| 3 | **Audit rubric** | `x-docs/audit-rubric.md` | How you score yourself. 7 categories, weighted. |
| 4 | **PRD** | `prd.md` | Mechanical spec — scoring, physics, controls, balloon types, perks. |
| 5 | **Asset research** | `x-docs/reference/asset-research.md` | Programmatic methods for procedural meshes, shaders, particles. |
| 6 | **Project standards** | `CLAUDE.md` | Hard limits, architecture, Agent Bridge commands. |
| 7 | **Changelogs** | `x-docs/changelogs/` | Read the 2-3 most recent. Learn what was tried, what worked, what failed. |

## The Loop

Every pass follows the same cycle. No exceptions.

```
┌─────────────────────────────────────────────────────┐
│                   THE FEEDBACK LOOP                  │
│                                                      │
│  1. READ    — progress log, reference, last scores   │
│  2. PLAN    — pick lowest-score category, set goals  │
│  3. EXECUTE — implement in focused phases            │
│  4. VERIFY  — compile, health, screenshot, dart-test │
│  5. SCORE   — audit against rubric, compare to last  │
│  6. WRITE   — update progress log with results       │
│  7. DECIDE  — improved? continue. stalled? pivot.    │
│              └───────── loop back to 1 ──────────────┘
└─────────────────────────────────────────────────────┘
```

### Step 1: READ — Load Context

Read `x-docs/visual-parity-progress.md`. Find:
- **Last pass number** — you are pass N+1
- **Last weighted score** — your floor (you must beat this)
- **Per-category scores** — identify the weakest
- **"Next priorities"** — the previous agent's recommendations
- **"What went wrong"** — mistakes to avoid repeating

Also read the 2-3 most recent changelogs in `x-docs/changelogs/` to understand what was recently tried.

### Step 2: PLAN — Target the Biggest Gap

Pick the **lowest-scoring category** from the last audit (weighted by rubric importance). If two categories tie, prefer the higher-weighted one.

Write a short plan (5-10 bullets) of what you will change this pass. Include:
- Which files you'll modify
- What the visual/mechanical delta should be
- How you'll verify (which agent-bridge command)
- Success criteria: "Score should go from X to Y because Z"

Do not plan more than one pass ahead. Finish this pass first.

**Priority order when scores are close:**
1. Physics Feel (25% weight) — if below 8, always work on this first
2. Environment & Framing (15%) — sets the mood for everything else
3. Balloon Visuals (15%) — the core visual element
4. HUD & UI (15%) — player-facing polish
5. Paint System (10%) — atmospheric detail
6. Dart Visuals (10%) — secondary visual element
7. Screen Effects (10%) — juice layer (only polish once fundamentals are solid)

### Step 3: EXECUTE — Implement in Focused Phases

Work in small phases (1-3 files per phase). After each phase:

1. `./agent-bridge.sh compile` — must pass with 0 errors
2. `./agent-bridge.sh health` — must introduce no new violations

**Use subagents for independent work.** If you're fixing balloon materials AND dart trails, those are independent — run them in parallel subagents. Protect your context window.

**Do not batch visual changes.** One visual change → one verification screenshot. If you change 5 things and the screenshot looks wrong, you won't know which change caused it.

### Step 4: VERIFY — Evidence Before Claims

After all phases in this pass are complete, run verification:

**For any visual change:**
```bash
./agent-bridge.sh gameplay
```
Read the output PNG at `Logs/agent-feedback/screenshots/gameplay_*.png`. Compare side-by-side with `x-docs/reference/visual-refernce.png`.

**For any physics/trajectory change:**
```bash
./agent-bridge.sh dart-test
```
Read each PNG at `Logs/agent-feedback/screenshots/dart_test_*.png`. Verify:
- 30% pull → hits lower-mid rows (rows 1-3)
- 60% pull → hits mid-upper rows (rows 4-6)
- 100% pull → hits upper rows (rows 7-9)
- Visible parabolic arc in all shots

**These screenshot steps are not optional.** They are your eyes. Without them you are coding blind.

**Capture method consistency:** Always use `./agent-bridge.sh gameplay` for visual comparison. Never switch capture methods between passes — this invalidates before/after comparison. The gameplay command uses interactive mode with full Metal GPU, so shaders render correctly (no magenta).

### Step 5: SCORE — Audit Yourself

Using the rubric in `x-docs/audit-rubric.md`, score each category 1-10. Be honest — generous self-scoring wastes future passes.

Compute the weighted total:
```
total = physics×0.25 + balloons×0.15 + paint×0.10 + darts×0.10
      + environment×0.15 + hud×0.15 + screenFX×0.10
```

Compare against the last pass's score. Three outcomes:

| Delta | Meaning | Action |
|-------|---------|--------|
| +0.5 or more | Real progress | Continue targeting next-weakest category |
| +0.1 to +0.4 | Marginal | Acceptable, but reconsider approach if this repeats |
| 0 or negative | Stalled or regressed | **Stop. Re-read reference. Change your approach entirely.** |

### Step 6: WRITE — Update the Progress Log

Append a new pass entry to `x-docs/visual-parity-progress.md`. Use this exact format:

**In the summary table at the top:**
```
| N | [category targeted] | [2-sentence summary of what shipped] | X.XX/10 |
```

**Then add a details section:**
```markdown
## Pass N Details

### Audit Breakdown
| Category | Score | Delta | Key Gap |
|----------|-------|-------|---------|
| Physics Feel | X/10 | +/-Y | ... |
| Balloon Visuals | X/10 | +/-Y | ... |
| ... | | | |
| **WEIGHTED TOTAL** | **X.XX/10** | **+/-Y.YY** | |

### What Shipped
- [bullet list of concrete changes with file names]

### What Worked
- [approaches that moved the score up — future passes should repeat these]

### What Didn't Work
- [approaches that failed or wasted time — future passes should avoid these]

### Next Priorities
1. [specific action for next pass]
2. [specific action for next pass]
3. [specific action for next pass]
```

**This section is critical.** It is the memory that future passes (and future conversations) read to avoid repeating mistakes and to continue making progress. Write it like you're handing off to a colleague who has never seen this codebase.

### Step 7: DECIDE — Continue or Pivot

- **Score ≥ 9.0** → Run a final `./agent-bridge.sh gameplay`, read the screenshot, and honestly compare to `visual-refernce.png`. If it matches, you're done. Write a changelog entry.
- **Score improved** → Loop back to Step 1. Target the next-weakest category.
- **Score stalled (two consecutive passes with < +0.3)** → You are likely polishing the wrong thing. Re-read the reference image. What is the **single biggest visual difference** between your screenshot and the reference? Target that, even if it's not the lowest-scoring rubric category.
- **Score regressed** → Revert your changes (`git checkout -- .`), re-read the progress log's "What Didn't Work", and try a different approach.

## The Target

Study `x-docs/reference/visual-refernce.png` carefully. The reference shows four views:

**Top-left — Active gameplay:**
- Dense 8-wide balloon grid with vivid, glossy, 3D balloons (red, blue, green, purple, yellow, gold)
- Special balloons with readable emblems (crown on gold, biohazard on green, paint splash, shield)
- Thick neon paint splatters dripping down a dark cork board
- Weathered metal frame with corner bolts
- HUD: room badge top-left, score/target center, darts + currency top-right
- Neon glow from off-screen light sources (magenta, cyan tints on balloon edges)

**Top-right — Perk selection:**
- Three premium cards with rarity-colored borders (green/blue/purple gradient)
- Dark translucent overlay behind cards
- Industrial noir card styling with icons, names, descriptions
- "CHOOSE A PERK" header

**Bottom-left — Combo/pop moment:**
- Explosive paint burst from popped balloons
- Neon paint streaks running down the board
- Visible dart lodged in board
- Combo text overlay
- Board looks "vandalized" — accumulated paint from multiple pops

**Bottom-right — Room intro:**
- Large room number ("ROOM 13") in bold display type
- Room name ("THE NEON MAZE") below
- Target score displayed
- Multi-color neon floor stripes in the launch area
- Atmospheric fog/mist
- Dark moody environment framing

## Category-Specific Guidance

### Physics Feel (target: 10/10)

**The feel:** Imagine standing 10 feet from a carnival balloon wall. You cock your arm back, aim up slightly, and lob a dart. It arcs through the air, rises above your eye level, then descends into the balloon grid. That's the feel — not a flat horizontal throw.

**Current spatial layout:**
- Launch: `LAUNCH_POSITION = (0, -7.5, 0)`
- Board: `BOARD_BOTTOM = 0.0`, `BOARD_TOP = 8.0` (8 units tall, 9 rows)
- Gravity: `DART_GRAVITY = -12`
- Gap from launch to board bottom: 7.5 units

**Tuning levers** (all in `GameConfigSO` or `GameConstants`):
- `minLaunchSpeed` / `maxLaunchSpeed` — speed range
- `pullSpeedExponent` — nonlinear pull response
- `LAUNCH_POSITION` — dart spawn point
- `DART_GRAVITY` — downward acceleration
- `BOARD_BOTTOM` / `BOARD_TOP` — grid position

**Verification:** `./agent-bridge.sh dart-test` — read each PNG. If all pull levels hit the same rows, the velocity curve is too flat.

**Key files:**
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs`
- `Assets/Scripts/BalloonGame/Darts/DartController.cs`
- `Assets/Scripts/BalloonGame/GameConstants.cs`
- `Assets/ScriptableObjects/GameConfig.asset`

### Balloon Visuals (target: 10/10)
- Glossy 3D with strong specular highlights, vivid saturated colors
- Distinct materials per type (standard, gold, hazard, paint, shield)
- Readable symbols/emblems on special balloons
- Perspective scaling (smaller at top, larger at bottom)

### Paint System (target: 10/10)
- Thick neon paint splatter particles on pop
- Paint drip streaks running down the board
- Persistent paint marks on cork board (neon-boosted, capped, oldest fade)
- Board looks increasingly "vandalized" as the room progresses

### Dart Visuals (target: 10/10)
- Chrome/silver metallic with high smoothness
- Visible fletching geometry
- Trails during flight
- Impact sparks on collision

### Environment & Framing (target: 10/10)
- Dark cork board with weathered metal frame and corner bolts
- Neon lighting rig (magenta, cyan, gold, green point lights)
- Atmospheric fog/mist with slow color oscillation
- Multi-color floor stripes in launch area
- Dark moody backdrop

### HUD & UI (target: 10/10)
- Industrial noir aesthetic
- Top bar: room badge (left), score/target (center), darts + currency (right)
- Premium perk cards with rarity-colored borders
- Room intro: large room number, name, target score
- Combo text escalates in size and color

### Screen Effects (target: 10/10)
- Screen shake: Perlin noise, exponential decay, combo escalation
- Slow motion: triggers on 3+ combo, scales with combo, smooth easing
- Combo flash: screen-edge vignette, color escalation (white → yellow → orange → magenta)
- Impact sparks: small, fast, chrome-colored

## Constraints

All constraints from `CLAUDE.md` apply. The critical ones:

- No file over its category line limit (gameplay: 300, UI: 250, VFX/Visuals: 200)
- No magic numbers — use `GameConstants` or ScriptableObject fields
- No `Debug.Log` without `#if UNITY_EDITOR`
- All VFX event-driven via EventBus, all spawned objects pooled
- Zero new compilation errors
- Zero new health violations
- **Visual changes require visual verification** — `./agent-bridge.sh gameplay` and read the screenshot

## Done Criteria

You are done when ALL of these are true:

1. `./agent-bridge.sh compile` → 0 errors
2. `./agent-bridge.sh health` → no new violations
3. `./agent-bridge.sh dart-test` → distinct trajectories at 30/60/100%, visible arc, different rows reached
4. `./agent-bridge.sh gameplay` screenshot closely matches `x-docs/reference/visual-refernce.png`
5. Self-audit weighted score ≥ 9.0/10
6. `x-docs/visual-parity-progress.md` updated with your final pass results
7. Codebase is clean — no half-finished work, no commented-out code
8. A changelog entry written to `x-docs/changelogs/` documenting what you did

## Anti-Patterns (learned from previous passes)

These are mistakes from prior sessions. Do not repeat them.

1. **Never change capture method mid-session.** Pass 1 used landscape `ScreenCapture`, Pass 2 switched to portrait `Camera.Render`. Before/after became incomparable. Always use `./agent-bridge.sh gameplay`.
2. **Never batch visual changes without intermediate verification.** Change one thing, screenshot, confirm, move on.
3. **Don't spend >15 minutes debugging tooling.** If `agent-bridge.sh` hangs or produces unexpected output, read the exit code, check `Logs/agent-feedback/editor.log`, and move on.
4. **Don't analyze when you can iterate.** If you're unsure whether a value is right, try it and run the test. Reading screenshots is faster than calculating trajectories by hand.
5. **Don't change measurement while changing the thing being measured.** If you're tuning physics, don't also change the camera or capture resolution in the same pass.
6. **Read the exit code before retrying.** A failed `agent-bridge.sh` command tells you why in stdout/stderr. Don't blindly re-run.

## Changelog Protocol

After completing a pass (or if stopping mid-pass), write a changelog:

**File:** `x-docs/changelogs/YYYY_MM_DD_HHMM_short-description.md`

**Format:**
```markdown
# [Short Description]

## What Changed
- [bullet list of changes with file names]

## Verification
- compile: [pass/fail]
- health: [violations count]
- gameplay screenshot: [what it shows]
- dart-test: [trajectory observations]

## Audit Score
[paste your audit table]

## Process Notes
- [what worked, what didn't, what the next agent should know]
```
