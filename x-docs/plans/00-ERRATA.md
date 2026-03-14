# INKSHOT Plans — Errata & Known Issues

> **READ THIS BEFORE EXECUTING ANY PLAN.** This document lists bugs, conflicts, and corrections found during review. Apply these fixes as you encounter them during implementation.

---

## CRITICAL: Duplicate Class Definitions (Plans 01 vs 03 vs 04 vs 05)

Multiple plans independently define the same classes with incompatible schemas. **Plan 01 is the CANONICAL source** for all shared infrastructure. Later plans must EXTEND, not redefine.

| Class | Plan 01 (canonical) | Conflicting Plan | Resolution |
|-------|-------------------|-----------------|------------|
| `PerkSO` | `Data/PerkSO.cs` | Plan 04 redefines at `ScriptableObjects/Perks/PerkSO.cs` | **Use Plan 01's path and base fields. Plan 04 should ADD fields to Plan 01's definition, not create a new file.** |
| `PerkRarity` | 4 values: Common, Uncommon, Rare, Legendary | Plan 04 has 3 values (no Uncommon) | **Use Plan 01's 4-value enum. Plan 04's `GetRarityWeight()` needs an Uncommon case.** |
| `PerkEffectType` | 10 values | Plan 04 has 12 different values | **Merge both sets into a single enum. Plan 04's values are more gameplay-specific — use Plan 04's list but keep any unique Plan 01 entries.** |
| `RoomTemplateSO` | `Data/RoomTemplateSO.cs` | Plan 04 redefines at `ScriptableObjects/Rooms/RoomTemplateSO.cs` | **Use Plan 01's path. Merge Plan 04's additional fields into Plan 01's definition.** |
| `GameConfigSO` | `Data/GameConfigSO.cs` (singleton) | Plan 04 redefines at `ScriptableObjects/GameConfigSO.cs` (non-singleton) | **Use Plan 01's singleton pattern and path. Add Plan 04's run-structure fields to it.** |
| `SaveData` | `Core/SaveData.cs` | Plan 04 redefines at `Roguelite/SaveData.cs` | **Use Plan 01's path. Merge Plan 04's fields (totalInk, runHistory, etc.) into it.** |
| `SaveManager` | `Core/SaveManager.cs` (PlayerPrefs singleton) | Plan 04 redefines at `Roguelite/SaveManager.cs` (file-based) | **Use Plan 01's singleton pattern. Upgrade storage to Plan 04's file-based approach (Application.persistentDataPath). Add Plan 04's methods (RecordRun, AddInk, SpendInk).** |
| `AudioManager` | `Core/AudioManager.cs` | Plan 05 redefines at `Audio/AudioManager.cs` | **Use Plan 01's path and singleton pattern. Add Plan 05's pooled AudioSource and SoundLibrary features to it.** |
| `BalloonTypeSO` | `Data/BalloonTypeSO.cs` | Plan 03 recreates at `ScriptableObjects/BalloonTypes/BalloonTypeSO.cs` | **Use Plan 01's path. Plan 03 should modify it, not recreate it.** |

**Rule: All `.cs` class definitions go under `Assets/Scripts/BalloonGame/`. The `Assets/ScriptableObjects/` folder is for `.asset` instances only.**

---

## Plan 01: Foundation

### 1. URP Setup Script Uses Non-Public APIs
`UniversalRenderPipelineAsset.Create()` does not exist as a public API in URP 14.x. Properties like `mainLightRenderingMode`, `supportsMainLightShadows`, `shadowDistance` are read-only.

**Fix:** Rewrite `URPSetup.cs` to use `SerializedObject` / `SerializedProperty`:
```csharp
var pipelineAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
var so = new SerializedObject(pipelineAsset);
so.FindProperty("m_MainLightRenderingMode").intValue = 1; // PerPixel
so.FindProperty("m_MainLightShadowsSupported").boolValue = true;
so.FindProperty("m_MainLightShadowmapResolution").intValue = 1024;
so.FindProperty("m_ShadowDistance").floatValue = 15f;
// ... etc
so.ApplyModifiedPropertiesWithoutUndo();
```

### 2. File Name Mismatch
File map says `URPRenderer.asset` but code creates `URPAsset_Renderer.asset`. **Use `URPRenderer.asset` consistently.**

### 3. GameConfigSO vs GameConstants Duplication
Both define combo windows, dart speeds, etc. **Rule: `GameConstants` holds LAYOUT constants (positions, sizes, layers). `GameConfigSO` holds TUNABLE gameplay values (speeds, timings, multipliers). Move all tuning values from GameConstants to GameConfigSO.**

### 4. Singleton Init Order
`AudioManager.Awake()` accesses `SaveManager.Instance`. **Fix:** Use lazy initialization — don't call `ApplySavedVolumes()` in `Awake()`. Instead, call it in `Start()` or on first use.

### 5. BalloonPoppedEvent.Color Field Name
Rename to `BalloonPoppedEvent.BalloonColor` to avoid shadowing `UnityEngine.Color`.

---

## Plan 02: Visual Overhaul

### 1. Shader _BaseColor Instancing Conflict
`_BaseColor` is declared in both CBUFFER and UNITY_INSTANCING_BUFFER. **Fix:** Wrap CBUFFER declaration:
```hlsl
#ifndef UNITY_INSTANCING_ENABLED
    half4 _BaseColor;
#endif
```

### 2. Balloon Positioning Bug
`localPosition` is set before `SetParent()`, so positions are treated as world-space. **Fix:** Call `SetParent(transform)` FIRST, then set `localPosition`.

### 3. Wrong Particle Shader for URP
`"Particles/Standard Unlit"` doesn't exist in URP. **Fix:** Use `"Universal Render Pipeline/Particles/Unlit"`.

### 4. Balloon Mesh Seam
No triangles connect sphere bottom ring to tie ring. **Fix:** Add a ring of triangles connecting the last sphere ring vertices to the tie ring vertices.

### 5. Scene Dark in Editor
NeonLightRig only creates lights at runtime. **Fix:** Have BalloonSceneBuilder also create a basic directional light in the scene hierarchy (non-runtime) so the scene is visible in editor.

### 6. SRP Batcher Claim
MaterialPropertyBlock disables SRP Batching. **Correction:** Balloons will batch via GPU Instancing, not SRP Batcher. This is still performant — just update the documentation to be accurate.

### 7. Point Light Render Mode
`ForcePixel` is expensive on low-end. **Fix:** Use `LightRenderMode.Auto`.

---

## Plan 03: Core Gameplay Polish

### 1. Task Ordering Bug
Task 1 (ScoreManager) references `balloon.PointValue` and `balloon.TypeId` which don't exist until Task 2 (BalloonNode). **Fix:** Implement Task 2 before Task 1, or implement them simultaneously.

### 2. ComboTracker Never Wired to ScoreManager
`ScoreManager._comboTracker` is `[SerializeField]` but never assigned. **Fix:** In `ScoreManager.Awake()`, add: `_comboTracker = GetComponent<ComboTracker>() ?? FindAnyObjectByType<ComboTracker>();`

### 3. Ricochet Won't Trigger
Parent CapsuleCollider on LAYER_PROJECTILES intercepts collision before child SphereCollider on LAYER_RICOCHET. **Fix:** When dart enters Stopped state, disable or remove the parent CapsuleCollider. Only the ricochet SphereCollider should remain active.

### 4. BULLET_TIME_PROXIMITY Dead Code
Defined but never used. **Fix:** Remove it, or wire it into LaunchFeel to only trigger slow-mo when dart passes within proximity of a balloon.

### 5. Combo Design Note
Combos are near-useless without pierce or paint — single darts can't chain within 1.5s across throws. **This is intentional design** — combos reward Paint balloon chain reactions and piercing darts. Document this in the combo system comments.

---

## Plan 04: Roguelite Systems

### 1. BalloonWall.ActiveCount() Not Defined
Called but never created. **Fix:** Add to BalloonWall:
```csharp
public int ActiveCount() => _balloons.Count(b => b != null && !b.IsPopped);
```

### 2. BalloonWall.ClearWall() Not Defined
Called but never created. **Fix:** Add to BalloonWall (or verify it exists as `Clear()` in existing code and rename the call).

### 3. Task 7→8 Ordering
RunManager (Task 7) depends on `BalloonGameManager.OnRoomCleared`/`OnRoomFailed` events added in Task 8. **Fix:** Implement Task 8 before Task 7, or stub the events in Task 7.

### 4. RunHUD Rebuilds Perk Icons Every Update
Destroys and re-instantiates every perk icon on each `UpdateDisplay()`. **Fix:** Cache icons, only add/remove when the perk list actually changes.

### 5. No Guard on StartNewRun()
Can be called while a run is in progress. **Fix:** Add `if (CurrentState != RunState.Idle) return;` at the top.

---

## Plan 05: VFX & Juice

### 1. CINEMACHINE_AVAILABLE Not Defined
This symbol doesn't exist — all Cinemachine code is silently compiled out. **Fix:** Use `#if UNITY_CINEMACHINE` or check for the package via Version Defines in an .asmdef. For Cinemachine 2.x, the define is typically not auto-set — safest approach is to always include Cinemachine code (it's in the manifest) and remove the `#if` guard entirely.

### 2. Cinemachine API Typo
`CinemachineImpulseDefinition.ImpulseTypes` (plural) should be `CinemachineImpulseDefinition.ImpulseType` (singular) for Cinemachine 2.x.

### 3. WaitForSeconds in Slow-Motion
`ReturnToPoolAfter` uses `WaitForSeconds` which scales with `Time.timeScale`. During slow-mo, VFX stay alive longer than intended. **Fix:** Use `WaitForSecondsRealtime`.

### 4. Particle Budget Math Wrong
Plan claims worst case ~200 but actual worst case is ~504 (8 pops × 25 + 4 paint × 50 + sparks + wall hits). **Fix:** Either reduce per-effect counts (pop: 15, paint: 30) or accept ~300 as the real budget with a note that effects auto-expire within 0.5s.

### 5. Wall Bounce VFX Missing
`DartController` doesn't fire events on side wall bounces, only on stop. **Fix:** Add `OnWallBounce` event to DartController, fire it in the bounce handling code. JuiceManager subscribes to play WallHitVFX on bounce.

### 6. Auto-Added Components Have Null Config
`JuiceManager.Awake()` adds sibling components via `AddComponent<>()` but can't pass `JuiceConfigSO` to them. **Fix:** After `AddComponent`, immediately set the config: `var shake = gameObject.AddComponent<ScreenShakeManager>(); shake.SetConfig(_config);` — add a public `SetConfig()` method to each component.

### 7. ParticleSystem.Burst Deprecation
`new Burst(float, short)` is obsolete. **Fix:** Use `new ParticleSystem.Burst(0f, (int)count)`.

---

## Cross-Plan Event Architecture Note

Plans 01 and 03 introduce EventBus while Plan 05 subscribes to existing C# Action events directly. **Resolution:** Keep BOTH systems during development. Existing events (`BalloonNode.OnAnyBalloonPopped`, `DartController.OnDartFinished`, `SlingshotInput.OnLaunch`) remain as-is. EventBus is used for NEW events only (ComboEvent, RoomStartedEvent, PerkSelectedEvent, etc.). This avoids a risky migration of working event wiring.
