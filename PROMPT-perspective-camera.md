# Task: Switch from Orthographic to Perspective Camera

## Context

The camera is currently orthographic (fixed ortho size, no depth). This is the single biggest reason the scene feels flat — orthographic cameras remove all depth cues by design. Switching to perspective will immediately add natural depth, foreshortening, and the feeling of looking into a space.

Read `CHANGELOG.md` for full context on the recent asset-based visual overhaul. The project just pivoted from 100% procedural visuals to imported 3D assets (Loafbrr balloon pack). Lighting has been simplified to 3 neutral directional lights. Post-processing is disabled.

## Key Files to Modify

- `Assets/Scripts/BalloonGame/Visuals/BalloonCamera.cs` — currently sets `camera.orthographic = true` with width-locked ortho size calculation
- `Assets/Scripts/BalloonGame/GameConstants.cs` — `CAMERA_ORTHO_SIZE`, `MIN/MAX_ORTHO_SIZE`, `TARGET_WORLD_WIDTH` all assume orthographic
- `Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs` — input uses `Camera.main.ScreenToWorldPoint` which works differently with perspective
- `Assets/Scripts/BalloonGame/Visuals/SlingshotVisuals.cs` — trajectory preview rendering
- `Assets/Scripts/BalloonGame/Darts/DartLauncher.cs` — trajectory preview math assumes orthographic projection

## What Needs to Happen

1. Change `BalloonCamera.cs` to use `camera.orthographic = false` with a perspective FOV
2. Position camera at negative Z looking forward toward the board (board is at Z=0)
3. FOV of ~60° with camera ~15-20 units back should frame the board similarly to current view
4. Update all `ScreenToWorldPoint` calls — perspective cameras need a meaningful Z distance parameter
5. Recalculate trajectory preview dots for perspective projection
6. Update or remove ortho-specific constants in `GameConstants.cs`
7. Test that dart launching still works (drag-to-aim → release → dart flies toward board)

## Considerations

- The board spans X: -4 to +4, Y: 0 to 8 (8 units wide, 8 units tall)
- Launch position is at `(0, -2.5, 0)`
- UI is ScreenSpace Overlay so it won't be affected by camera projection changes
- The subtle camera drift (`sin(time)*0.06`) may look different with perspective — evaluate whether to keep it
- Balloon scale constants were recently changed to `BALLOON_MAX_WIDTH=3.15`, `BALLOON_MAX_HEIGHT=3.5` for the imported mesh — may need re-tuning after perspective switch
- Run `./agent-bridge.sh compile` after changes, then visually verify with `./agent-bridge.sh gameplay`
