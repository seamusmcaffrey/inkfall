# Prompt to give the agent

Copy and paste the block below into a new Claude Code conversation:

---

Read and follow `x-docs/gui-layout-pass/layout-rework-prompt.md`. It extends `x-docs/visual-parity-prompt.md` which defines the feedback loop, Agent Bridge, MCP tooling, safety rails, and iteration protocol — read that too. You are an ORCHESTRATOR. Use subagents to manage your context. Critique your own work as you go — then cycle on the work again if needed. Ultrathink. Think hard. Take your time.

When done, codebase must be left in a clean state with zero new code errors, lint violations, test failures, or formatting changes. Do not ask questions. Do not stop short of any aspect of implementation — everything should be completed. Do not shy away from doing the hard work → just go go go, implement end to end.

Start by reading the visual parity progress log (`x-docs/visual-parity-progress.md`) AND the reference screenshots in `x-docs/reference-screenshots/` to understand the current state. Your baseline score is 0/10 on the new layout rubric (HUD proportions, atmosphere, viewport fill, spatial coherence, integration).

The game is PORTRAIT (9:16) and stays that way. Do NOT change it to landscape. The problems are: (1) the HUD/GUI bar is massively oversized relative to the gameboard — shrink it so the board is the hero, (2) the atmosphere is purple haze instead of dark noir carnival — fix or remove the mist/fog, (3) there's a stray cyan line across the balloons and objects scattered far from the game area, (4) the launch lane is as tall as the board but mostly empty — compress it so the board gets more viewport real estate.

The prompt has stall detection built in: 3 consecutive passes without +0.3 improvement → STOP and write a detailed handoff. But as long as audit scores keep improving, keep looping. Run all night if needed.

IMPORTANT CONTEXT: The prior visual parity work (7 passes, score 7.65/10) focused on physics, balloon rendering, paint VFX, and dart visuals. All of that work is solid — do NOT regress it. Physics must still work (dart-test must show distinct trajectories at 30/60/100% pull).

NOTE: There are ~34 uncommitted modified files from the prior visual parity work. Do NOT discard these changes. Build on top of them. Commit them first if you want a clean baseline, or just continue working.
