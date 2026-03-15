# Width-Locked Adaptive Viewport

## What Changed

- **BalloonCamera.cs** — Removed `EnforceAspect()` pillarboxing. Camera.rect is now always `(0,0,1,1)`. Added `ApplyAdaptiveSize()` which calculates `orthographicSize = (TARGET_WORLD_WIDTH / 2) / screenAspect`, clamped to `[MIN_ORTHO_SIZE, MAX_ORTHO_SIZE]`. Recalculates on aspect ratio change in LateUpdate for editor responsiveness.
- **GameConstants.cs** — Added `TARGET_WORLD_WIDTH` (8.6f), `MIN_ORTHO_SIZE` (7.0f), `MAX_ORTHO_SIZE` (12.0f) for adaptive viewport.
- **ViewportConstraint.cs** — Converted from 9:16 constraint to full-stretch pass-through. The 9 UI builder files that `AddComponent<ViewportConstraint>()` continue to work without modification.
- **EnvironmentBuilder.cs** — Extended BackWall and LaneFloor to cover the expanded viewport on tall screens. Fog layers and vignettes sized using MAX_ORTHO_SIZE to ensure coverage at any aspect ratio.
- **SceneValidator.cs** — Updated camera checks: ortho size validates against [MIN, MAX] range instead of fixed value; position check validates Y against CAMERA_Y_CENTER.
- **PauseManager.cs, RoomIntroScreen.cs, RunEndScreen.cs** — Updated stale comments referencing "9:16" and "pillarbox".

## Verification

- compile: pass (0 errors)
- health: pass (0 violations)
- All files within CLAUDE.md line limits

## Architecture

The camera system is now **width-locked, height-flexible**:
- Board width (8.0 units) + frame margin = 8.6 units always fills the screen width
- On taller devices (iPhone 14/15, 9:19.5 aspect), more vertical space is revealed
- On wider devices (iPad, 3:4 aspect), ortho size clamps to 7.0, showing side padding
- UI uses `matchWidthOrHeight = 0` (width-locked CanvasScaler) synchronized with camera
- SafeAreaHandler handles notch/dynamic island insets

### Device behavior examples

| Device | Aspect | Ortho Size | Visible Width | Visible Height |
|--------|--------|-----------|---------------|----------------|
| iPhone SE | 9:16 (0.5625) | 7.64 | 8.6 | 15.3 |
| iPhone 15 | 9:19.5 (0.4615) | 9.31 | 8.6 | 18.6 |
| iPad | 3:4 (0.75) | 7.0 (clamped) | 10.5 | 14.0 |

## Process Notes

- ViewportConstraint kept as no-op rather than removing from 9 files — pragmatic, no functional difference
- Runtime aspect recalculation added in LateUpdate (one float comparison per frame) for editor usability
- CAMERA_ORTHO_SIZE constant retained as fallback default; MIN_ORTHO_SIZE happens to match its value
