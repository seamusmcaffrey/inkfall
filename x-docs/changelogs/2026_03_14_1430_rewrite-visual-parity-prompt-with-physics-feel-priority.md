## 2026-03-14 14:30

## Rewrite visual parity agent prompt to prioritize physics feel over visuals

## Issues identified (if relevant)
The previous agent prompt focused exclusively on visual parity. Darts feel like they're thrown at a flat table — no matter the pull strength, they hit the bottom row of balloons. Root cause: only 4.5 units between launch position (y=-5.5) and board bottom (y=-1.0), combined with launch speeds of 10-40 that cross the gap instantly with no visible arc. The original bow-and-arrow physics feel from git history was lost.

## Issue resolution implemented (if there was an issue)
Rewrote `x-docs/visual-parity-prompt.md` with physics feel as the top priority (Section 0, "DO THIS FIRST"). Added: root cause analysis with exact file paths and values, "table vs wall" feel target framing, scripted input testing methodology (30%/60%/100% pull with before/after screenshots), git history study directive, and two new done criteria gates requiring distinct trajectories and visible arcs.

## File(s) modified:
- `x-docs/visual-parity-prompt.md`
