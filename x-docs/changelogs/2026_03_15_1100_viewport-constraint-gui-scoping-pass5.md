## 2026-03-15 11:00

## GUI viewport constraint — scope all UI to 9:16 camera area (Pass 5)

## Issues identified (if relevant)
- All 13 UI canvases used `ScreenSpaceOverlay` which fills the full physical screen, causing HUD elements (top bar, progress bar, RunHUD, dart tray) to extend into pillarbox/letterbox black bars on non-9:16 screens
- `matchWidthOrHeight = 0.5f` gave inconsistent scaling across devices
- Overlay screen backdrops (title, pause, perk select, room intro, run end, settings) would show gaps in pillarbox areas if naively constrained
- RunHUD was missing SafeAreaHandler — text could be obscured by notch/dynamic island
- ViewportConstraint had a re-entrancy risk from `OnRectTransformDimensionsChange` and an early-return bug that could permanently lock `_applying = true`

## Issue resolution implemented (if there was an issue)
- Created `ViewportConstraint.cs` — constrains RectTransform to 9:16 aspect ratio, mirroring `BalloonCamera.EnforceAspect()` at the UI layer. Includes re-entrancy guard with `_applying` flag set only after early-return checks pass.
- Updated all 13 canvas builders with two patterns:
  - **HUD canvases** (InGameHUD, RunHUD, PauseManager button): `Canvas > ViewportRoot > SafeArea > content`
  - **Overlay screens** (Title, Pause overlay, RoomIntro, PerkSelection, RunEnd, Tutorial, Settings): backdrop parented directly to canvas (full-bleed), interactive content inside ViewportRoot (constrained)
  - **Full-bleed effects** (ComboFlashVFX, FadeOverlay, ScreenTransition): only `matchWidthOrHeight` changed — no viewport constraint (intentionally cover entire screen)
- Changed all `matchWidthOrHeight` from `0.5f` to `0f` (width-locked scaling)
- Added SafeAreaHandler to RunHUD between ViewportRoot and text labels
- Code review agent caught backdrop-inside-viewport and missing SafeArea bugs before shipping

## File(s) modified:
- `Assets/Scripts/BalloonGame/UI/ViewportConstraint.cs` (NEW — 67 lines)
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
- `x-docs/gui-layout-pass/progress.md`
