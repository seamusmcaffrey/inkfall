# Handoff: Mobile Viewport Fitting — Game Content Not Filling iPhone Screen

**Date:** 2026-03-15 10:55
**Status:** Needs Expert Input
**Priority:** High

## Executive Summary

Inkshot is a portrait-mode Unity 6 mobile game (balloon-popping with slingshot darts). The UI layer was just fixed to stay within the game viewport (ViewportConstraint pass), but the core problem remains: **the game world only occupies a narrow vertical strip of the iPhone screen, with large black bars on both sides.** The game content needs to fill the full device width while maintaining correct proportions. This handoff seeks expertise on the correct approach to make a Unity orthographic-camera game fill a mobile device screen responsively.

## The Problem (With Screenshots)

**What it looks like in the iPhone simulator:**

The game content (balloon grid + launch lane) renders as a narrow column (~50% of screen width) centered on the device, with thick black bars on both sides. The HUD (score, progress bar, dart tray) is now properly constrained to match the game area width, but the game area itself is too narrow for the screen.

**Root cause:** The game uses a fixed orthographic camera with a hardcoded 9:16 target aspect ratio, enforced via `Camera.rect` pillarboxing. Modern iPhones have taller aspect ratios (9:19.5 for iPhone X+), so on these devices the camera letterboxes vertically. However, the game world is 8 world units wide while the camera's visible width at ortho size 7.0 is only 7.875 units — and the entire thing is pillarboxed to 9:16 regardless of the actual device aspect.

The net effect: the game doesn't adapt to the available screen space. It renders at a fixed 9:16 viewport and pads everything else with black.

## Current Architecture

### Camera System (`Assets/Scripts/BalloonGame/Visuals/BalloonCamera.cs`)

```csharp
private const float TargetAspect = 9f / 16f;  // 0.5625

private void ConfigureCamera()
{
    _camera.orthographic = true;
    _camera.orthographicSize = 7.0f;  // Half-height in world units → 14 units visible vertically
    transform.position = new Vector3(0f, 2.5f, z);  // Centered on board midpoint
}

private void EnforceAspect()
{
    float currentAspect = (float)Screen.width / Screen.height;
    if (currentAspect > TargetAspect)  // Wider than 9:16 → pillarbox
    {
        float width = TargetAspect / currentAspect;
        _camera.rect = new Rect((1f - width) / 2f, 0f, width, 1f);
    }
    else if (currentAspect < TargetAspect)  // Taller than 9:16 → letterbox
    {
        float height = currentAspect / TargetAspect;
        _camera.rect = new Rect(0f, (1f - height) / 2f, 1f, height);
    }
}
```

This runs once in `Awake()` and never updates. It enforces a strict 9:16 viewport via `Camera.rect`, which creates black bars.

### World Space Layout (`Assets/Scripts/BalloonGame/GameConstants.cs`)

| Element | Position / Size | World Units |
|---------|----------------|-------------|
| Board | (-4, 0) to (4, 8) | 8 wide × 8 tall |
| Camera ortho size | 7.0 | 14 tall visible |
| Camera center | Y = 2.5 | |
| Camera visible width | 2 × 7.0 × (9/16) = **7.875** | At 9:16 aspect |
| Board width | **8.0** | **Wider than camera** |
| Launch position | (0, -2.5, 0) | 2.5 below board |
| Lane bounds | Y: -0.8 to -3.5 | 2.7 tall |
| Cork board frame | 8.6 × 8.8 | Board + 0.6/0.8 padding |

**Key observation:** The board (8.0 units) is already wider than the camera's visible width at 9:16 (7.875 units). The rightmost and leftmost balloon columns are slightly clipped.

### UI System

All UI canvases use `ScreenSpaceOverlay` with a `ViewportConstraint` component that mirrors the camera's 9:16 constraint. UI reference resolution is 1080×1920.

**Hierarchy:** `Canvas (full screen) > ViewportRoot (constrained to 9:16) > SafeArea > UI elements`

The `ViewportConstraint` component (`Assets/Scripts/BalloonGame/UI/ViewportConstraint.cs`) computes insets to match 9:16, identical math to `BalloonCamera.EnforceAspect()`.

### Target Devices

- iPhone SE (9:16 — 750×1334)
- iPhone 14/15 (9:19.5 — 1179×2556)
- iPhone 15 Pro Max (9:19.5 — 1290×2796)
- iPad (3:4 — 2048×2732)

## What Needs to Be Solved

1. **The game world should fill the device screen width** — no black side bars on any iPhone in portrait mode. On iPhones (which are all narrower than or equal to 9:16 in portrait), the camera should use the full screen width.

2. **Taller devices should see more content, not letterbox** — on a 9:19.5 phone, the extra vertical space could show more of the launch lane, more atmosphere above the board, or simply more dark background. The board and lane should not be squeezed.

3. **Board width vs camera width mismatch** — the board is 8 units wide but the camera only shows 7.875 units at 9:16. Either the board needs to be narrower, the camera wider, or the ortho size adjusted so the full board is visible.

4. **UI must track whatever the camera does** — the `ViewportConstraint` and `CanvasScaler` settings need to work in harmony with whatever camera/viewport approach is chosen.

5. **Safe area handling** — iPhone notches, dynamic islands, and home indicators must be respected. A `SafeAreaHandler` component already exists and is used by HUD canvases.

## Approaches to Evaluate

The following approaches have been identified but NOT evaluated or implemented. The incoming agent should assess which (or which combination) is appropriate:

### A. Remove pillarboxing, let camera fill screen width
- Set `Camera.rect = (0, 0, 1, 1)` always
- Adjust `orthographicSize` dynamically based on actual aspect ratio to ensure the board fits width-wise
- On taller screens, more vertical space is visible (extra atmosphere/background)
- On wider screens (iPads), more horizontal space visible (extra dark background)

### B. Keep fixed ortho size, adjust board to fit
- Reduce board width from 8 to ~7.5 units so it fits within camera at any reasonable aspect
- Or reduce column count, increase balloon size

### C. Dynamic ortho size based on board width
- Calculate `orthographicSize = (BOARD_WIDTH / 2) / camera.aspect` to guarantee the board always fills the width exactly
- Vertical content adjusts automatically

### D. Use a different rendering approach
- Camera targets a specific world rect rather than using fixed ortho size
- Content-aware camera that guarantees all gameplay elements are visible

### E. Mixed approach
- World-space game content fills width dynamically
- UI uses CanvasScaler with `matchWidthOrHeight = 0` (already done) to match

## Key Files

| File | Purpose | Lines |
|------|---------|-------|
| `Assets/Scripts/BalloonGame/Visuals/BalloonCamera.cs` | Camera setup + aspect enforcement | 73 |
| `Assets/Scripts/BalloonGame/GameConstants.cs` | All spatial constants | 148 |
| `Assets/Scripts/BalloonGame/UI/ViewportConstraint.cs` | UI viewport constraint (new) | 67 |
| `Assets/Scripts/BalloonGame/UI/InGameHUD.Builder.cs` | Main HUD layout | 211 |
| `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs` | Cork board, frame, fog, lane floor | ~200 |
| `Assets/Scripts/BalloonGame/Visuals/LaunchLaneVisuals.cs` | Lane guide lines and dart pips | ~130 |
| `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs` | Neon lighting positions | ~130 |
| `Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs` | Touch input, aiming | ~150 |

## What Was Just Completed (This Session)

- Created `ViewportConstraint.cs` — constrains UI RectTransforms to 9:16 to match camera viewport
- Updated all 13 canvas builders to use ViewportConstraint
- Changed `matchWidthOrHeight` from 0.5 to 0 across all canvases (width-locked UI scaling)
- Overlay screens use backdrop outside ViewportRoot (full-bleed dark backgrounds) + content inside (constrained)
- Added SafeAreaHandler to RunHUD (was missing)
- Compile: 0 errors, Health: 0 violations

**This fixed the GUI-extending-beyond-game-area problem but did NOT fix the game-not-filling-the-screen problem.**

## Uncommitted Changes

```
M  Assets/Scripts/BalloonGame/Roguelite/UI/FadeOverlay.cs         # matchWidthOrHeight 0.5→0
M  Assets/Scripts/BalloonGame/Roguelite/UI/PerkSelectionScreen.cs  # ViewportRoot + backdrop split
M  Assets/Scripts/BalloonGame/Roguelite/UI/RoomIntroScreen.cs      # ViewportRoot + backdrop split
M  Assets/Scripts/BalloonGame/Roguelite/UI/RunEndScreen.Builder.cs # Parent to _vpRoot
M  Assets/Scripts/BalloonGame/Roguelite/UI/RunEndScreen.cs         # _vpRoot field + backdrop split
M  Assets/Scripts/BalloonGame/Roguelite/UI/RunHUD.cs               # ViewportRoot + SafeArea
M  Assets/Scripts/BalloonGame/UI/InGameHUD.Builder.cs              # ViewportRoot between canvas/safe
M  Assets/Scripts/BalloonGame/UI/Screens/PauseManager.cs           # Both canvases get ViewportRoot
M  Assets/Scripts/BalloonGame/UI/Screens/ScreenTransition.cs       # matchWidthOrHeight 0.5→0
M  Assets/Scripts/BalloonGame/UI/Screens/SettingsPanel.Builder.cs  # ViewportRoot + backdrop split
M  Assets/Scripts/BalloonGame/UI/Screens/TitleScreen.cs            # ViewportRoot + backdrop split
M  Assets/Scripts/BalloonGame/UI/Screens/TutorialOverlay.cs        # ViewportRoot
M  Assets/Scripts/BalloonGame/VFX/ComboFlashVFX.cs                 # matchWidthOrHeight 0.5→0
?? Assets/Scripts/BalloonGame/UI/ViewportConstraint.cs             # NEW — viewport constraint component
M  x-docs/gui-layout-pass/progress.md                              # Pass 5 entry
```

## Build & Verify

```bash
./agent-bridge.sh compile    # Must pass with 0 errors
./agent-bridge.sh health     # Must pass with 0 violations
./agent-bridge.sh gameplay   # Captures gameplay screenshot (requires interactive Unity — currently crashes with SIGILL on this machine)
./agent-bridge.sh screenshot # Batch-mode scene render (works)
```

## Project Standards

See `CLAUDE.md` for full standards. Key constraints:
- No file over 300 lines (UI: 250, Visuals: 200)
- No magic numbers — use `GameConstants.cs` or ScriptableObject fields
- All cross-system communication via `EventBus<T>`
- Camera: fixed orthographic, portrait (9:16 target)
- UI reference resolution: 1080×1920
- URP 17.3.0 on Unity 6

## Questions for Incoming Agent

- What is the correct approach to make an orthographic Unity game fill the full width of any iPhone in portrait while gracefully handling variable height?
- Should `Camera.rect` enforcement be removed entirely, or should the camera adapt its ortho size to the actual device aspect?
- How should the `ViewportConstraint` be updated (or removed) if the camera no longer pillarboxes?
- Is there a standard Unity pattern for "width-fixed, height-flexible" orthographic games?
- Should the board width (8.0 units) be treated as the canonical width that the camera must always show, or should it adapt?

## Related Documentation

- `x-docs/gui-layout-pass/progress.md` — 5 passes of GUI/layout iteration history
- `x-docs/visual-parity-prompt.md` — full visual parity process and rubric
- `x-docs/changelogs/` — all recent changes with verification results
- `CLAUDE.md` — project standards and agent bridge commands
