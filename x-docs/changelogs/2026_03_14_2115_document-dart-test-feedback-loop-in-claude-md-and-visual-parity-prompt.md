## 2026-03-14 21:15

## Document dart-test feedback loop in CLAUDE.md and visual-parity-prompt.md

## Issues identified (if relevant)
1. `CLAUDE.md` did not mention `dart-test` command or explain interactive vs batch rendering modes — agents had no guidance to use the physics feedback loop.
2. `visual-parity-prompt.md` described scripted input testing as a hypothetical ("write a test harness") rather than referencing the working `./agent-bridge.sh dart-test` tool. No concrete output locations, iteration workflow, or tuning levers were documented.
3. Done criteria referenced generic "scripted dart launches" without pointing to the specific command.

## Issue resolution implemented (if there was an issue)
- **CLAUDE.md**: Added full "Dart Physics Test" subsection under Agent Bridge documenting what the command does, its output location, how to interpret results, and when to use it. Added "Rendering Modes" subsection explaining interactive mode vs batch mode routing. Updated command list to include `dart-test`.
- **visual-parity-prompt.md**: Replaced the generic "Scripted input testing" section with a concrete "Dart physics feedback loop" section covering the exact command, what it does step-by-step, output file naming, how to iterate on results, and the tuning levers available (`GameConfigSO` fields, `GameConstants` values). Updated Done Criteria to reference `./agent-bridge.sh dart-test` explicitly. Updated physics fix step 6 to point to the dart-test command.

## File(s) modified:
- `CLAUDE.md` — Added Dart Physics Test docs, Rendering Modes section, updated command list
- `x-docs/visual-parity-prompt.md` — Replaced scripted input testing with concrete dart-test workflow, updated Done Criteria
