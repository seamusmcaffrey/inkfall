# Viewport Constraint — GUI Scoped to 9:16 Game Area

## What Changed

- **New: `ViewportConstraint.cs`** (`Assets/Scripts/BalloonGame/UI/`) — MonoBehaviour that constrains a RectTransform to 9:16 aspect ratio, preventing UI from extending into pillarbox/letterbox bars. Handles both wider-than-9:16 (pillarbox) and taller-than-9:16 (letterbox) screens. Includes re-entrancy guard against recursive `OnRectTransformDimensionsChange` calls.

- **All 13 canvas builders updated:**
  - `matchWidthOrHeight` changed from `0.5f` to `0f` (width-locked scaling) across all canvases
  - HUD canvases (`InGameHUD`, `RunHUD`, `PauseManager` button) get ViewportRoot + SafeArea hierarchy
  - Overlay screens (`TitleScreen`, `PauseManager` overlay, `RoomIntroScreen`, `PerkSelectionScreen`, `RunEndScreen`, `TutorialOverlay`, `SettingsPanel`) get backdrop outside ViewportRoot (full-bleed) + content inside ViewportRoot (constrained)
  - Full-bleed effects (`ComboFlashVFX`, `FadeOverlay`, `ScreenTransition`) only get `matchWidthOrHeight` change — no constraint (intentionally cover entire screen)

- **RunHUD** now has SafeAreaHandler between ViewportRoot and text labels (was missing before)

### Hierarchy pattern (HUD):
```
Canvas (full screen) > ViewportRoot (9:16) > SafeArea (notch inset) > UI elements
```

### Hierarchy pattern (overlay screens):
```
Canvas (full screen) > Backdrop (full screen) + ViewportRoot (9:16) > Content
```

## Files Modified
- `Assets/Scripts/BalloonGame/UI/ViewportConstraint.cs` (NEW — 65 lines)
- `Assets/Scripts/BalloonGame/UI/InGameHUD.Builder.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/RunHUD.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/PauseManager.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/RoomIntroScreen.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/PerkSelectionScreen.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/TitleScreen.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/TutorialOverlay.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/SettingsPanel.Builder.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/RunEndScreen.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/RunEndScreen.Builder.cs`
- `Assets/Scripts/BalloonGame/VFX/ComboFlashVFX.cs`
- `Assets/Scripts/BalloonGame/Roguelite/UI/FadeOverlay.cs`
- `Assets/Scripts/BalloonGame/UI/Screens/ScreenTransition.cs`

## Verification
- compile: PASS (0 errors, 0 warnings)
- health: PASS (0 violations, 125 files scanned)
- gameplay screenshot: Batch-mode scene render shows proper 9:16 pillarboxing. Interactive mode unavailable (SIGILL crash — macOS Metal issue, not code-related).
- All files within line limits

## Audit Score

| Category | Weight | Score | Delta |
|----------|--------|-------|-------|
| HUD & GUI Proportions | 25% | 10/10 | +1 |
| Atmosphere & Mood | 25% | 9/10 | +0 |
| Viewport Fill & Proportions | 20% | 10/10 | +0 |
| Spatial Coherence | 15% | 9/10 | +1 |
| Integration & No Regressions | 15% | 9/10 | +0 |
| **WEIGHTED TOTAL** | | **9.50/10** | **+0.30** |

## Process Notes
- Root cause: All UI canvases used `ScreenSpaceOverlay` which always fills the full physical screen, ignoring `Camera.rect` pillarboxing. UI elements (top bar, progress bar, RunHUD) extended into black bars on non-9:16 screens.
- Fix: `ViewportConstraint` component mirrors the camera's aspect ratio enforcement at the UI layer. No render mode change needed — simpler and more performant than switching to `ScreenSpaceCamera`.
- Design decision: Full-bleed effects (fades, flashes) intentionally bypass the constraint so they cover the entire screen including bars.
- Design decision: Overlay backdrops (dark semi-transparent backgrounds on pause/title/perk/room-intro/run-end screens) also bypass the constraint for full coverage, while interactive content (panels, buttons, text) is constrained.
- `matchWidthOrHeight = 0f` locks UI scaling to width, ensuring consistent horizontal proportions across all devices.
