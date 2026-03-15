## 2026-03-15 10:55

## ViewportConstraint pass + mobile fitting handoff

## Work completed this session
- Created `ViewportConstraint.cs` — constrains UI RectTransforms to 9:16 aspect ratio, matching camera pillarbox
- Updated all 13 canvas builders with ViewportRoot hierarchy and `matchWidthOrHeight = 0f`
- Split overlay screen hierarchies: backdrop full-bleed, interactive content constrained
- Added SafeAreaHandler to RunHUD (was missing)
- Fixed re-entrancy guard bug in ViewportConstraint and early-return _applying deadlock
- Compile: 0 errors, Health: 0 violations

## Issue identified but not resolved
- Game world only fills ~50% of iPhone screen width — renders as narrow column with black bars
- Root cause: `BalloonCamera.EnforceAspect()` hardcodes 9:16 via `Camera.rect`, creating pillarbox on all non-9:16 devices
- Board is 8 world units wide but camera shows only 7.875 units at 9:16 — edge columns clipped
- Handoff written requesting expertise on responsive orthographic camera fitting for mobile

## Files modified
- `Assets/Scripts/BalloonGame/UI/ViewportConstraint.cs` — NEW (67 lines)
- `Assets/Scripts/BalloonGame/UI/InGameHUD.Builder.cs` — ViewportRoot added
- `Assets/Scripts/BalloonGame/Roguelite/UI/RunHUD.cs` — ViewportRoot + SafeArea added
- `Assets/Scripts/BalloonGame/UI/Screens/PauseManager.cs` — both canvases updated
- `Assets/Scripts/BalloonGame/Roguelite/UI/RoomIntroScreen.cs` — backdrop/content split
- `Assets/Scripts/BalloonGame/Roguelite/UI/PerkSelectionScreen.cs` — backdrop/content split
- `Assets/Scripts/BalloonGame/UI/Screens/TitleScreen.cs` — backdrop/content split
- `Assets/Scripts/BalloonGame/UI/Screens/SettingsPanel.Builder.cs` — backdrop/content split
- `Assets/Scripts/BalloonGame/Roguelite/UI/RunEndScreen.cs` + `.Builder.cs` — viewport root
- `Assets/Scripts/BalloonGame/VFX/ComboFlashVFX.cs` — matchWidthOrHeight only
- `Assets/Scripts/BalloonGame/Roguelite/UI/FadeOverlay.cs` — matchWidthOrHeight only
- `Assets/Scripts/BalloonGame/UI/Screens/ScreenTransition.cs` — matchWidthOrHeight only
- `Assets/Scripts/BalloonGame/UI/Screens/TutorialOverlay.cs` — ViewportRoot added
- `x-docs/gui-layout-pass/progress.md` — pass 5 entry

## Next steps (from handoff)
1. Determine correct camera approach: remove pillarboxing and use dynamic ortho size, or adapt board to fit
2. Make game world fill full device width on all iPhones in portrait mode
3. Handle extra vertical space on taller devices (9:19.5) gracefully
4. Update or remove ViewportConstraint to match new camera behavior

## Status
Blocked — needs architectural decision on camera/viewport approach

## See also
- Handoff: `x-docs/handoffs/2026_03_15_1055_mobile-viewport-fitting.md`
- GUI progress: `x-docs/gui-layout-pass/progress.md`
