# Visual & Mechanical Parity — Autonomous Agent Prompt

## Mission

Achieve 10/10 visual AND mechanical parity with `x-docs/reference/visual-refernce.png`. Work autonomously — do not ask questions, do not stop short. You are a long-running agent that self-corrects through structured feedback loops.

## Safety Rails

### Stall Detection — HARD STOP

Track your pass-over-pass score delta. If you hit ANY of these conditions, **STOP IMMEDIATELY**:

- **3 consecutive passes with delta < +0.3** → You are spinning. Stop. Write a detailed handoff to `x-docs/visual-parity-progress.md` explaining what you tried, what blocked you, and what a human should investigate.
- **Same bug debugged for > 20 minutes without resolution** → Try the brute-force approach (disable things, add debug colors, comment out systems). If that doesn't work in 10 more minutes, document the bug as a known issue and move to the next category.
- **Agent-bridge command fails 3 times in a row** → Check `pgrep -x Unity`, kill if stuck, `rm -f Temp/UnityLockfile`, retry once. If still failing, document and move on.

### Token Budget Awareness

You are expected to run for many hours. Pace yourself:
- **Max 2 consecutive passes on the same category** before rotating to the next lowest-scoring one
- **No hard pass limit** — as long as the audit score shows consistent improvement, keep going
- **Each pass should take 20-40 minutes** — if a pass takes over 60 minutes, you're over-analyzing. Ship what you have, score, move on.

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
- **Known Issues** — bugs documented by previous passes

Also read the 2-3 most recent changelogs in `x-docs/changelogs/` to understand what was recently tried.

### Step 2: PLAN — Target the Biggest Gap

Pick the **lowest-scoring category** from the last audit (weighted by rubric importance). If two categories tie, prefer the higher-weighted one.

Write a short plan (5-10 bullets) of what you will change this pass. Include:
- Which files you'll modify
- What the visual/mechanical delta should be
- How you'll verify (which agent-bridge command)
- Success criteria: "Score should go from X to Y because Z"

Do not plan more than one pass ahead. Finish this pass first.

**Priority order:**
1. **Physics Feel (25% weight) — ALWAYS work on this first if below 8.** The game is unplayable without correct dart physics. Do not work on anything else until darts can reach all rows with a visible arc.
2. Environment & Framing (15%) — sets the mood for everything else. Fix purple rectangles early.
3. Balloon Visuals (15%) — the core visual element
4. HUD & UI (15%) — player-facing polish
5. Dart Visuals (10%) — includes the new arc-scaling feature
6. Paint System (10%) — atmospheric detail
7. Screen Effects (10%) — juice layer (only polish once fundamentals are solid)

### Step 3: EXECUTE — Implement in Focused Phases

Work in small phases (1-3 files per phase). After each phase:

1. `./agent-bridge.sh compile` — must pass with 0 errors
2. `./agent-bridge.sh health` — must introduce no new violations

**Use subagents for independent work.** If you're fixing balloon materials AND dart trails, those are independent — run them in parallel subagents. Protect your context window.

**Do not batch visual changes.** One visual change → one verification screenshot. If you change 5 things and the screenshot looks wrong, you won't know which change caused it.

**Kill stuck Unity before running agent-bridge.** Always run this before any agent-bridge command:
```bash
pgrep -x Unity | head -1 | xargs -r kill 2>/dev/null; rm -f Temp/UnityLockfile
```

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

**Capture method note:** `./agent-bridge.sh gameplay` uses PlayModeCapture which renders via Camera.Render + RenderTexture at 1080×1920 portrait. This captures the game world accurately but does NOT capture ScreenSpaceOverlay UI (HUD). This is a known limitation. Score the HUD by reading the code, not by seeing it in screenshots.

### Step 5: SCORE — Audit Yourself

Using the rubric in `x-docs/audit-rubric.md`, score each category 1-10. **Be brutally honest — only score what you've SEEN working in a screenshot.** If you haven't verified something visually, it gets the same score as last pass (no credit for "should work").

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
- **Hit stall detection threshold** → STOP. Write handoff. Do not continue.

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

### Physics Feel (target: 10/10) — HIGHEST PRIORITY

**The mental model:** You are standing at a carnival, 10 feet from a VERTICAL balloon wall mounted perpendicular to the ground. You are throwing darts UP and FORWARD in a parabolic arc. The darts rise, peak, then descend INTO the wall. You are NOT shooting horizontally like a gun — you are LOBBING like tossing a ball at a tall target.

**THIS IS THE MOST IMPORTANT CATEGORY.** If the physics don't feel right, nothing else matters. The dart throw MUST have a physical lobbing feel with a visible parabolic arc. Currently it does NOT feel right — darts collide with the bottom row of balloons because the trajectory is too flat.

**The perpendicular wall problem:** The balloon board is a vertical wall (BOARD_BOTTOM=0 to BOARD_TOP=8). The launch point is at y=-7.5, far below the board. A dart must arc UPWARD to reach the board. Think of it like throwing a ball at a second-story window — you have to lob it UP, not throw it straight.

**Critical requirements:**
1. **Darts must be throwable to ALL spots on the board with skill.** This means:
   - Gentle pull (30%) → arc peaks just above board bottom → hits rows 1-3
   - Medium pull (60%) → arc peaks at mid-board height → hits rows 4-6
   - Strong pull (100%) → arc peaks above board top → hits rows 7-9
2. **The arc must be visually obvious.** The dart should rise noticeably above the launch point, reach an apex, then descend into the board. Not a flat line.
3. **Bottom row should NOT be trivially hit.** Currently darts smash into the bottom row because the trajectory is too flat. A proper lobbing arc means even hitting the bottom row requires some aim — the dart arcs up and comes down to row 1, it doesn't fly straight into it.
4. **Different pull strengths should produce VERY different arcs.** 30% should be a gentle lob; 100% should be a dramatic high arc.

**Current spatial layout:**
- Launch: `LAUNCH_POSITION = (0, -7.5, 0)`
- Board: `BOARD_BOTTOM = 0.0`, `BOARD_TOP = 8.0` (8 units tall, 9 rows)
- Gravity: `DART_GRAVITY = -12`
- Gap from launch to board bottom: 7.5 units

**Why the current physics might be wrong:** The launch velocity vector is likely too horizontal. For a lobbing feel, the initial velocity should be MORE VERTICAL than horizontal. The dart should shoot mostly upward, with gravity curving it forward into the board. Examine `SlingshotInput.cs` to see how the velocity vector is computed from pull direction — the angle mapping may need adjustment so that pulling down-and-back launches the dart up-and-forward at a steep angle.

**Tuning levers** (all in `GameConfigSO` or `GameConstants`):
- `minLaunchSpeed` / `maxLaunchSpeed` — speed range
- `pullSpeedExponent` — nonlinear pull response
- `LAUNCH_POSITION` — dart spawn point
- `DART_GRAVITY` — downward acceleration
- `BOARD_BOTTOM` / `BOARD_TOP` — grid position
- The velocity direction computation in `SlingshotInput` — this determines the launch angle

**Verification protocol for physics:**
1. Run `./agent-bridge.sh dart-test` — examine each PNG
2. **30% pull PNG:** Dart should be visible mid-arc, hitting rows 1-3. If it hits the floor or misses the board, min speed is too low.
3. **60% pull PNG:** Dart should hit rows 4-6. If it hits same rows as 30%, speed range is too narrow.
4. **100% pull PNG:** Dart should hit rows 7-9. If it hits same rows as 60%, max speed is too low or gravity too strong.
5. **If all PNGs show darts at bottom of board:** The launch angle is too flat. Increase the vertical component of launch velocity.

**Advanced verification (slow-motion):**
To visualize the arc in detail, temporarily add `Time.timeScale = 0.15f` at dart launch (in the test runner only), capture 3-5 screenshots at intervals during flight, then restore. This lets you see:
- Does the dart rise visibly above the launch point?
- Is the apex clearly above the target row?
- Does it descend with a clear curve?

**Key files:**
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs` — velocity direction computation, THE MOST LIKELY PLACE TO FIX
- `Assets/Scripts/BalloonGame/Darts/DartController.cs` — gravity application, flight physics
- `Assets/Scripts/BalloonGame/GameConstants.cs` — DART_GRAVITY, LAUNCH_POSITION
- `Assets/ScriptableObjects/GameConfig.asset` — speed range, exponent

### Dart Arc Visuals (NEW FEATURE — target: 8/10)

**Concept:** As a dart rises through its parabolic arc, it should visually "grow" — getting slightly larger as it rises toward the apex. After the apex, as it descends toward the balloon board, it should "shrink" back to normal size. This creates a compelling sense of depth and perspective, as if the dart is traveling toward the viewer then away.

**Implementation approach:**
1. In `DartController.FixedUpdate()`, track the dart's vertical velocity direction
2. When `velocity.y > 0` (rising), lerp scale from 1.0 toward a max (e.g., 1.3×)
3. When `velocity.y <= 0` (falling/at apex), lerp scale back toward 1.0
4. The scale curve should be smooth — use velocity.y magnitude as the driver
5. Add config values to `GameConstants` or `GameConfigSO`: `DART_ARC_SCALE_MIN`, `DART_ARC_SCALE_MAX`

**Verification approach:** To see the arc scaling in action:
1. Modify `DartPhysicsTestRunner` (or create a variant) to fire a single dart at 60% pull
2. Set `Time.timeScale = 0.1f` temporarily during the dart flight
3. Take screenshots at regular intervals along the trajectory (3-5 shots)
4. Verify: dart visually grows during rise phase, shrinks during fall phase
5. Restore time scale after screenshots

Alternatively, use `./agent-bridge.sh dart-test` normally and look at the dart size in each capture. The 30% pull dart should be smaller (mostly descending), the 100% pull dart should be larger at peak (high arc).

**Key files:**
- `Assets/Scripts/BalloonGame/Darts/DartController.cs` — add scale logic in FixedUpdate
- `Assets/Scripts/BalloonGame/GameConstants.cs` — add scale range constants

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

## Known Issues (from prior passes)

These are bugs documented by previous agents. Fix them or work around them.

### 1. Purple Rectangles in Lower Half
**Symptom:** Large purple/magenta areas visible below the balloon board in the launch lane area (y=-1.5 to -9.5).
**Hypotheses:**
- (a) SceneBuilder creates BackWall/LaneFloor with LIT materials → NeonLightRig's magenta/cyan lights illuminate them before EnvironmentBuilder overrides the materials with UNLIT dark versions
- (b) EnvironmentBuilder.Awake() calls `Destroy()` (deferred) on stale objects, but during the first frame both the LIT and UNLIT versions coexist
- (c) Light bleeding from NeonLightRig point lights (range 8-12) reaching into the lane area
**Debugging approach:**
1. Temporarily comment out `NeonLightRig.BuildRig()` call, run gameplay, see if purple disappears → confirms light bleeding
2. If purple persists without lights, the materials themselves are wrong → check `Shader.Find` return values
3. If light-related: reduce neon light ranges, or add culling to exclude layer 3 (Environment)
**Status:** Unresolved after 45 min of investigation in Pass 3.

### 2. HUD Not Visible in Gameplay Captures
**Symptom:** PlayModeCapture uses `Camera.Render()` + RenderTexture which doesn't capture ScreenSpaceOverlay canvases.
**Options:**
- (a) Switch back to `ScreenCapture.CaptureScreenshotAsTexture()` and crop to camera viewport rect
- (b) Keep Camera.Render for game world captures, verify HUD by code inspection
- (c) Temporarily switch all canvases to ScreenSpaceCamera mode, render, switch back
**Status:** Using Camera.Render. HUD scored by code, not visual evidence.

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

1. **Never change capture method mid-session.** Use `./agent-bridge.sh gameplay` consistently. Camera.Render is the current method. Don't switch to ScreenCapture.
2. **Never batch visual changes without intermediate verification.** Change one thing, screenshot, confirm, move on.
3. **Don't spend >20 minutes debugging a single issue.** Try brute-force (disable systems, add debug colors). If stuck, document and move on.
4. **Don't analyze when you can iterate.** If you're unsure whether a value is right, try it and run the test. Reading screenshots is faster than calculating trajectories by hand.
5. **Don't change measurement while changing the thing being measured.** If you're tuning physics, don't also change the camera or capture resolution in the same pass.
6. **Read the exit code before retrying.** A failed `agent-bridge.sh` command tells you why in stdout/stderr. Don't blindly re-run.
7. **Score honestly.** Only score what you've SEEN in a screenshot. "Implemented but not verified" = same score as last pass.
8. **Kill stuck Unity before retrying.** `pgrep -x Unity | xargs kill; rm -f Temp/UnityLockfile`
9. **Don't score yourself generously to feel progress.** Generous self-scoring wastes future passes chasing phantom improvements.
10. **Run dart-test when physics is lowest.** Don't get distracted by visual polish when mechanics are broken.

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
