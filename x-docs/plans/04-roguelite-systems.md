# Plan 04 — Roguelite Systems

> Room progression, perk engine, perk selection UI, room intro, run end, meta-progression, run history.

---

## File Map

| File | Status | Purpose |
|------|--------|---------|
| `Assets/Scripts/BalloonGame/Roguelite/RunManager.cs` | **NEW** | Top-level run orchestrator; owns run state machine |
| `Assets/Scripts/BalloonGame/Roguelite/RunState.cs` | **NEW** | Run state enum and RunData struct |
| `Assets/Scripts/BalloonGame/Roguelite/RoomGenerator.cs` | **NEW** | Produces RoomConfig from room number + difficulty curve |
| `Assets/Scripts/BalloonGame/Roguelite/RoomConfig.cs` | **NEW** | Runtime room configuration data |
| `Assets/Scripts/BalloonGame/Roguelite/RoomNames.cs` | **NEW** | Static carnival/neon room name table |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/PerkManager.cs` | **NEW** | Owns active perks for current run, applies/removes effects |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/PerkEffect.cs` | **NEW** | Abstract base class for all perk effects |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/PerkEffectType.cs` | **NEW** | Enum of all perk effect categories |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/PerkRarity.cs` | **NEW** | Enum: Common, Rare, Legendary |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/DartSpeedEffect.cs` | **NEW** | Modify launch speed multiplier |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/DartPierceEffect.cs` | **NEW** | Add pierce count to darts |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/DartSplitEffect.cs` | **NEW** | Dart splits on first wall bounce |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/ComboExtendEffect.cs` | **NEW** | Extend combo timeout window |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/ComboBonusEffect.cs` | **NEW** | Bonus points per combo level |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/PaintRadiusEffect.cs` | **NEW** | Increase paint balloon AoE |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/GoldMultiplierEffect.cs` | **NEW** | Multiply gold balloon value |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/ExtraDartEffect.cs` | **NEW** | +1 dart per room |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/RicochetEffect.cs` | **NEW** | Increase max ricochet count |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/SlowMotionEffect.cs` | **NEW** | Extend bullet-time duration |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/ScoreMultiplierEffect.cs` | **NEW** | Flat score multiplier |
| `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/HazardShieldEffect.cs` | **NEW** | First hazard pop per room has no penalty |
| `Assets/Scripts/BalloonGame/Roguelite/Meta/MetaProgressionManager.cs` | **NEW** | Manages permanent unlocks between runs |
| `Assets/Scripts/BalloonGame/Roguelite/Meta/MetaUpgrade.cs` | **NEW** | Runtime meta upgrade data |
| `Assets/Scripts/BalloonGame/Roguelite/Meta/RunHistoryTracker.cs` | **NEW** | Records and retrieves run history |
| `Assets/Scripts/BalloonGame/Roguelite/UI/PerkCardUI.cs` | **NEW** | Single perk card prefab component |
| `Assets/Scripts/BalloonGame/Roguelite/UI/PerkSelectionScreen.cs` | **NEW** | Shows 3 perk cards, handles selection |
| `Assets/Scripts/BalloonGame/Roguelite/UI/RoomIntroScreen.cs` | **NEW** | Room number, name, target, perk tray |
| `Assets/Scripts/BalloonGame/Roguelite/UI/RunEndScreen.cs` | **NEW** | Final stats, currency earned, continue button |
| `Assets/Scripts/BalloonGame/Roguelite/UI/RunHUD.cs` | **NEW** | Run-level HUD (room number, ink count, perk icons) |
| `Assets/Scripts/BalloonGame/Roguelite/UI/FadeOverlay.cs` | **NEW** | CanvasGroup alpha lerp utility for transitions |
| `Assets/ScriptableObjects/Perks/PerkSO.cs` | **NEW** | ScriptableObject definition for a single perk |
| `Assets/ScriptableObjects/Rooms/RoomTemplateSO.cs` | **NEW** | ScriptableObject definition for a room template |
| `Assets/ScriptableObjects/GameConfigSO.cs` | **NEW** | Global game config: difficulty curves, room count, currency rates |
| `Assets/ScriptableObjects/MetaUpgradeSO.cs` | **NEW** | ScriptableObject definition for a meta upgrade |
| `Assets/Scripts/BalloonGame/Roguelite/SaveData.cs` | **NEW** | Serializable save data: currency, unlocks, run history |
| `Assets/Scripts/BalloonGame/Roguelite/SaveManager.cs` | **NEW** | JSON save/load to Application.persistentDataPath |
| `Assets/Scripts/BalloonGame/BalloonGameManager.cs` | **MODIFY** | Demote to per-room controller; accept RoomConfig; fire room events |
| `Assets/Scripts/BalloonGame/GameConstants.cs` | **MODIFY** | Add roguelite constants |
| `Assets/Scripts/BalloonGame/BalloonWall.cs` | **MODIFY** | Accept dynamic grid size from RoomConfig |
| `Assets/Scripts/BalloonGame/ScoreManager.cs` | **MODIFY** | Accept perk-driven score multipliers |
| `Assets/Scripts/BalloonGame/GameHUD.cs` | **MODIFY** | Accept room number display, hide run-level info (moved to RunHUD) |

---

## Task 1 — ScriptableObject Data Layer

> Define PerkSO, RoomTemplateSO, GameConfigSO, MetaUpgradeSO, and supporting enums.

- [ ] Create `Assets/ScriptableObjects/Perks/PerkSO.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/Perks/PerkEffectType.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/Perks/PerkRarity.cs`
- [ ] Create `Assets/ScriptableObjects/Rooms/RoomTemplateSO.cs`
- [ ] Create `Assets/ScriptableObjects/GameConfigSO.cs`
- [ ] Create `Assets/ScriptableObjects/MetaUpgradeSO.cs`

```csharp
// === PerkEffectType.cs ===
public enum PerkEffectType
{
    DartSpeed,
    DartPierce,
    DartSplit,
    ComboExtend,
    ComboBonus,
    PaintRadius,
    GoldMultiplier,
    ExtraDart,
    Ricochet,
    SlowMotion,
    ScoreMultiplier,
    HazardShield
}
```

```csharp
// === PerkRarity.cs ===
public enum PerkRarity
{
    Common,
    Rare,
    Legendary
}
```

```csharp
// === PerkSO.cs ===
using UnityEngine;

/// <summary>
/// Data definition for a single perk. All perk behavior is driven from this asset.
/// </summary>
[CreateAssetMenu(fileName = "NewPerk", menuName = "INKSHOT/Perks/Perk Definition")]
public class PerkSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name shown on the perk card.")]
    public string perkName;

    [Tooltip("One-line description of what this perk does.")]
    [TextArea(1, 2)]
    public string description;

    [Tooltip("Icon displayed on the perk card and HUD tray.")]
    public Sprite icon;

    [Tooltip("Unique identifier. Must be unique across all perks.")]
    public string perkId;

    [Header("Classification")]
    public PerkRarity rarity;
    public PerkEffectType effectType;

    [Tooltip("If true, effect is always active. If false, triggered on specific events.")]
    public bool isPassive = true;

    [Header("Effect Parameters")]
    [Tooltip("Primary float value used by the effect (multiplier, duration, count, etc).")]
    public float effectValue = 1f;

    [Tooltip("Secondary float value for effects that need two parameters.")]
    public float effectValueSecondary;

    [Tooltip("Integer parameter (pierce count, split count, dart count, etc).")]
    public int effectIntValue;

    [Header("Meta")]
    [Tooltip("If true, this perk starts locked and must be purchased via meta-progression.")]
    public bool requiresUnlock;

    [Tooltip("Ink cost to unlock this perk in the meta shop.")]
    public int unlockCost;
}
```

```csharp
// === RoomTemplateSO.cs ===
using UnityEngine;

/// <summary>
/// Template defining the static properties of a room type.
/// RoomGenerator combines a template with procedural modifiers to produce a RoomConfig.
/// </summary>
[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "INKSHOT/Rooms/Room Template")]
public class RoomTemplateSO : ScriptableObject
{
    [Header("Identity")]
    public string templateId;
    public string displayName;

    [Header("Grid")]
    [Tooltip("Number of columns in the balloon grid.")]
    [Range(4, 10)]
    public int columns = 8;

    [Tooltip("Number of rows in the balloon grid.")]
    [Range(4, 12)]
    public int rows = 9;

    [Header("Darts")]
    [Range(2, 8)]
    public int baseDarts = 4;

    [Header("Scoring")]
    public int baseTargetScore = 3000;

    [Header("Special Balloons")]
    [Tooltip("Minimum number of special (non-standard) balloons.")]
    public int minSpecials;

    [Tooltip("Maximum number of special (non-standard) balloons.")]
    public int maxSpecials;

    [Tooltip("Minimum number of hazard balloons.")]
    public int minHazards;

    [Tooltip("Maximum number of hazard balloons.")]
    public int maxHazards;

    [Header("Room Type")]
    public RoomType roomType = RoomType.Normal;

    [Tooltip("If true, this template can be selected for bonus rooms.")]
    public bool isBonusEligible;
}

public enum RoomType
{
    Normal,
    Bonus,
    Boss,
    Mystery
}
```

```csharp
// === GameConfigSO.cs ===
using UnityEngine;

/// <summary>
/// Global game configuration. Single source of truth for difficulty curves,
/// room counts, currency rates, and run parameters.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "INKSHOT/Game Config")]
public class GameConfigSO : ScriptableObject
{
    [Header("Run Structure")]
    [Tooltip("Total rooms in a standard run (including boss).")]
    public int totalRooms = 13;

    [Tooltip("Room number interval for bonus rooms (e.g., every 4th room).")]
    public int bonusRoomInterval = 4;

    [Header("Difficulty Curve")]
    [Tooltip("Target score multiplier per room. Applied as: base * (1 + roomNumber * multiplier).")]
    public float targetScoreScaling = 0.15f;

    [Tooltip("Hazard count increase per difficulty tier.")]
    public int hazardEscalation = 1;

    [Header("Currency")]
    [Tooltip("Base Ink earned per room cleared.")]
    public int inkPerRoom = 10;

    [Tooltip("Ink earned per 1000 score points.")]
    public int inkPer1000Score = 5;

    [Tooltip("Bonus Ink multiplier for completing a full run (room 13).")]
    public float fullRunInkMultiplier = 2f;

    [Header("Perk Selection")]
    [Tooltip("Number of perk choices shown between rooms.")]
    public int perkChoiceCount = 3;

    [Tooltip("Weight for Common rarity perks (out of 100).")]
    [Range(0, 100)]
    public int commonWeight = 60;

    [Tooltip("Weight for Rare rarity perks (out of 100).")]
    [Range(0, 100)]
    public int rareWeight = 30;

    [Tooltip("Weight for Legendary rarity perks (out of 100).")]
    [Range(0, 100)]
    public int legendaryWeight = 10;

    [Header("Room Templates")]
    [Tooltip("All available room templates. RoomGenerator selects from these.")]
    public RoomTemplateSO[] roomTemplates;

    [Header("Perk Pool")]
    [Tooltip("All perks that can appear in the selection pool.")]
    public PerkSO[] allPerks;

    [Header("Meta Upgrades")]
    [Tooltip("All available meta upgrades.")]
    public MetaUpgradeSO[] allMetaUpgrades;

    [Header("Timing")]
    [Tooltip("Duration of the room intro screen in seconds.")]
    public float roomIntroDuration = 2f;

    [Tooltip("Delay after perk selection before transitioning to next room.")]
    public float perkSelectionDelay = 0.8f;

    [Tooltip("Duration of fade transitions in seconds.")]
    public float fadeDuration = 0.4f;
}
```

```csharp
// === MetaUpgradeSO.cs ===
using UnityEngine;

/// <summary>
/// Definition for a permanent meta-progression upgrade purchased between runs.
/// </summary>
[CreateAssetMenu(fileName = "NewMetaUpgrade", menuName = "INKSHOT/Meta/Meta Upgrade")]
public class MetaUpgradeSO : ScriptableObject
{
    [Header("Identity")]
    public string upgradeId;
    public string displayName;

    [TextArea(1, 2)]
    public string description;
    public Sprite icon;

    [Header("Cost")]
    [Tooltip("Ink cost to purchase this upgrade.")]
    public int inkCost;

    [Header("Effect")]
    public MetaUpgradeType upgradeType;
    public float effectValue;
    public int effectIntValue;

    [Tooltip("PerkSO to unlock, if upgradeType is UnlockPerk.")]
    public PerkSO perkToUnlock;
}

public enum MetaUpgradeType
{
    /// <summary>Add permanent starting darts.</summary>
    StartingDarts,
    /// <summary>Permanent base score percentage bonus.</summary>
    BaseScoreBonus,
    /// <summary>Unlock a new perk into the selection pool.</summary>
    UnlockPerk,
    /// <summary>Cosmetic dart skin.</summary>
    DartSkin
}
```

**Commit:** `feat(roguelite): add ScriptableObject data layer — PerkSO, RoomTemplateSO, GameConfigSO, MetaUpgradeSO`

---

## Task 2 — Save System

> Serializable save data and JSON persistence.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/SaveData.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/SaveManager.cs`

```csharp
// === SaveData.cs ===
using System;
using System.Collections.Generic;

/// <summary>
/// Serializable save data persisted between sessions.
/// </summary>
[Serializable]
public class SaveData
{
    public int totalInk;
    public List<string> unlockedPerkIds = new();
    public List<string> purchasedUpgradeIds = new();
    public List<string> unlockedDartSkins = new();
    public string equippedDartSkin = "default";

    // Permanent stat bonuses from meta upgrades
    public int bonusStartingDarts;
    public float bonusBaseScorePercent;

    // Run history (last 10)
    public List<RunRecord> runHistory = new();

    // Lifetime stats
    public int totalRunsPlayed;
    public int totalRoomsCleared;
    public int totalBalloonsPopped;
    public int bestRunScore;
    public int bestRoomStreak;
}

[Serializable]
public class RunRecord
{
    public string date;
    public int roomsCleared;
    public int finalScore;
    public int inkEarned;
    public List<string> perksChosen = new();
    public int balloonsPopped;
    public int dartsThrown;
    public int maxCombo;
}
```

```csharp
// === SaveManager.cs ===
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles saving and loading game data to persistent storage as JSON.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "inkshot_save.json";
    private const int MaxRunHistory = 10;

    /// <summary>Fires after any save operation completes.</summary>
    public event Action OnSaveCompleted;

    /// <summary>Fires after load completes with the loaded data.</summary>
    public event Action<SaveData> OnLoadCompleted;

    public SaveData Data { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        Load();
    }

    /// <summary>
    /// Load save data from disk. Creates default data if no save exists.
    /// </summary>
    public void Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                Data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveManager] Loaded save: {Data.totalInk} ink, {Data.totalRunsPlayed} runs");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to load save, creating new: {e.Message}");
                Data = new SaveData();
            }
        }
        else
        {
            Data = new SaveData();
            Debug.Log("[SaveManager] No save found, created new save data");
        }

        OnLoadCompleted?.Invoke(Data);
    }

    /// <summary>
    /// Write current save data to disk.
    /// </summary>
    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveManager] Saved: {Data.totalInk} ink");
            OnSaveCompleted?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
        }
    }

    /// <summary>
    /// Add a completed run record. Trims history to MaxRunHistory entries.
    /// </summary>
    public void RecordRun(RunRecord record)
    {
        Data.runHistory.Insert(0, record);
        while (Data.runHistory.Count > MaxRunHistory)
        {
            Data.runHistory.RemoveAt(Data.runHistory.Count - 1);
        }

        Data.totalRunsPlayed++;
        if (record.finalScore > Data.bestRunScore)
        {
            Data.bestRunScore = record.finalScore;
        }

        if (record.roomsCleared > Data.bestRoomStreak)
        {
            Data.bestRoomStreak = record.roomsCleared;
        }

        Data.totalRoomsCleared += record.roomsCleared;
        Data.totalBalloonsPopped += record.balloonsPopped;
    }

    /// <summary>
    /// Add Ink currency and save immediately.
    /// </summary>
    public void AddInk(int amount)
    {
        Data.totalInk += amount;
        Save();
    }

    /// <summary>
    /// Spend Ink currency. Returns false if insufficient funds.
    /// </summary>
    public bool SpendInk(int amount)
    {
        if (Data.totalInk < amount)
        {
            return false;
        }

        Data.totalInk -= amount;
        Save();
        return true;
    }

    /// <summary>
    /// Check if a perk has been unlocked via meta-progression.
    /// </summary>
    public bool IsPerkUnlocked(string perkId)
    {
        return Data.unlockedPerkIds.Contains(perkId);
    }

    /// <summary>
    /// Check if a meta upgrade has been purchased.
    /// </summary>
    public bool IsUpgradePurchased(string upgradeId)
    {
        return Data.purchasedUpgradeIds.Contains(upgradeId);
    }

    /// <summary>
    /// Delete save data. Mainly for debug/testing.
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        Data = new SaveData();
        Debug.Log("[SaveManager] Save data deleted");
    }
}
```

**Commit:** `feat(roguelite): add SaveManager and SaveData for JSON persistence`

---

## Task 3 — Perk Effect System

> Abstract PerkEffect base class and all 12 concrete effect implementations.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/Perks/PerkEffect.cs`
- [ ] Create all 12 effect classes in `Assets/Scripts/BalloonGame/Roguelite/Perks/Effects/`

```csharp
// === PerkEffect.cs ===
using UnityEngine;

/// <summary>
/// Abstract base class for all perk effects. Subclasses implement Apply/Remove
/// to modify game systems when a perk is acquired or lost.
/// Perk effects are instantiated at runtime from PerkSO data — no hardcoded
/// perk logic exists in game systems.
/// </summary>
public abstract class PerkEffect
{
    /// <summary>The source PerkSO that created this effect.</summary>
    public PerkSO Source { get; private set; }

    /// <summary>Whether this effect is currently active.</summary>
    public bool IsActive { get; private set; }

    public void Initialize(PerkSO source)
    {
        Source = source;
    }

    /// <summary>
    /// Apply this effect to the game systems. Called when the perk is acquired.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        OnApply();
        Debug.Log($"[PerkEffect] Applied: {Source.perkName} ({GetType().Name})");
    }

    /// <summary>
    /// Remove this effect from game systems. Called when the run ends.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        OnRemove();
        Debug.Log($"[PerkEffect] Removed: {Source.perkName} ({GetType().Name})");
    }

    /// <summary>Override to apply the perk's effect to game systems.</summary>
    protected abstract void OnApply();

    /// <summary>Override to clean up the perk's effect from game systems.</summary>
    protected abstract void OnRemove();

    /// <summary>
    /// Factory method: creates the correct PerkEffect subclass for a given PerkSO.
    /// </summary>
    public static PerkEffect CreateFromSO(PerkSO perkSO)
    {
        PerkEffect effect = perkSO.effectType switch
        {
            PerkEffectType.DartSpeed => new DartSpeedEffect(),
            PerkEffectType.DartPierce => new DartPierceEffect(),
            PerkEffectType.DartSplit => new DartSplitEffect(),
            PerkEffectType.ComboExtend => new ComboExtendEffect(),
            PerkEffectType.ComboBonus => new ComboBonusEffect(),
            PerkEffectType.PaintRadius => new PaintRadiusEffect(),
            PerkEffectType.GoldMultiplier => new GoldMultiplierEffect(),
            PerkEffectType.ExtraDart => new ExtraDartEffect(),
            PerkEffectType.Ricochet => new RicochetEffect(),
            PerkEffectType.SlowMotion => new SlowMotionEffect(),
            PerkEffectType.ScoreMultiplier => new ScoreMultiplierEffect(),
            PerkEffectType.HazardShield => new HazardShieldEffect(),
            _ => null
        };

        if (effect != null)
        {
            effect.Initialize(perkSO);
        }

        return effect;
    }
}
```

```csharp
// === DartSpeedEffect.cs ===
/// <summary>Modifies dart launch speed multiplier.</summary>
public class DartSpeedEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.DartSpeedMultiplier *= Source.effectValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.DartSpeedMultiplier /= Source.effectValue;
    }
}
```

```csharp
// === DartPierceEffect.cs ===
/// <summary>Adds pierce count so darts pass through balloons.</summary>
public class DartPierceEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.DartPierceCount += Source.effectIntValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.DartPierceCount -= Source.effectIntValue;
    }
}
```

```csharp
// === DartSplitEffect.cs ===
/// <summary>Dart splits into N darts on first wall bounce.</summary>
public class DartSplitEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.DartSplitCount += Source.effectIntValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.DartSplitCount -= Source.effectIntValue;
    }
}
```

```csharp
// === ComboExtendEffect.cs ===
/// <summary>Extends the combo timeout window.</summary>
public class ComboExtendEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.ComboTimeoutExtension += Source.effectValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.ComboTimeoutExtension -= Source.effectValue;
    }
}
```

```csharp
// === ComboBonusEffect.cs ===
/// <summary>Adds bonus points per combo level.</summary>
public class ComboBonusEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.ComboBonusPerLevel += (int)Source.effectValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.ComboBonusPerLevel -= (int)Source.effectValue;
    }
}
```

```csharp
// === PaintRadiusEffect.cs ===
/// <summary>Increases paint balloon area-of-effect radius.</summary>
public class PaintRadiusEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.PaintRadiusMultiplier *= Source.effectValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.PaintRadiusMultiplier /= Source.effectValue;
    }
}
```

```csharp
// === GoldMultiplierEffect.cs ===
/// <summary>Multiplies gold balloon score value.</summary>
public class GoldMultiplierEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.GoldValueMultiplier *= Source.effectValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.GoldValueMultiplier /= Source.effectValue;
    }
}
```

```csharp
// === ExtraDartEffect.cs ===
/// <summary>Grants additional darts per room.</summary>
public class ExtraDartEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.BonusDartsPerRoom += Source.effectIntValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.BonusDartsPerRoom -= Source.effectIntValue;
    }
}
```

```csharp
// === RicochetEffect.cs ===
/// <summary>Increases maximum wall ricochet count for darts.</summary>
public class RicochetEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.BonusRicochetCount += Source.effectIntValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.BonusRicochetCount -= Source.effectIntValue;
    }
}
```

```csharp
// === SlowMotionEffect.cs ===
/// <summary>Extends bullet-time duration.</summary>
public class SlowMotionEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.SlowMotionDurationBonus += Source.effectValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.SlowMotionDurationBonus -= Source.effectValue;
    }
}
```

```csharp
// === ScoreMultiplierEffect.cs ===
/// <summary>Applies a flat score multiplier to all scoring.</summary>
public class ScoreMultiplierEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.ScoreMultiplier *= Source.effectValue;
    }

    protected override void OnRemove()
    {
        PerkModifiers.ScoreMultiplier /= Source.effectValue;
    }
}
```

```csharp
// === HazardShieldEffect.cs ===
/// <summary>Negates the penalty of the first hazard balloon popped per room.</summary>
public class HazardShieldEffect : PerkEffect
{
    protected override void OnApply()
    {
        PerkModifiers.HazardShieldCharges += 1;
    }

    protected override void OnRemove()
    {
        PerkModifiers.HazardShieldCharges -= 1;
    }
}
```

**Commit:** `feat(roguelite): add PerkEffect base class and 12 concrete effect implementations`

---

## Task 4 — PerkModifiers Static Registry

> Central static class that game systems read to get perk-modified values. Keeps perk logic out of game systems.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/Perks/PerkModifiers.cs`

```csharp
// === PerkModifiers.cs ===
/// <summary>
/// Static registry of perk-modified values. Game systems read these fields
/// instead of hardcoded constants. PerkEffect subclasses write to these fields.
/// Call Reset() at the start of each run.
/// </summary>
public static class PerkModifiers
{
    // Dart modifiers
    public static float DartSpeedMultiplier = 1f;
    public static int DartPierceCount = 0;
    public static int DartSplitCount = 0;
    public static int BonusDartsPerRoom = 0;
    public static int BonusRicochetCount = 0;

    // Combo modifiers
    public static float ComboTimeoutExtension = 0f;
    public static int ComboBonusPerLevel = 0;

    // Balloon modifiers
    public static float PaintRadiusMultiplier = 1f;
    public static float GoldValueMultiplier = 1f;

    // Score modifiers
    public static float ScoreMultiplier = 1f;

    // Utility modifiers
    public static float SlowMotionDurationBonus = 0f;
    public static int HazardShieldCharges = 0;

    /// <summary>
    /// Reset all modifiers to default values. Called at the start of each run.
    /// </summary>
    public static void Reset()
    {
        DartSpeedMultiplier = 1f;
        DartPierceCount = 0;
        DartSplitCount = 0;
        BonusDartsPerRoom = 0;
        BonusRicochetCount = 0;
        ComboTimeoutExtension = 0f;
        ComboBonusPerLevel = 0;
        PaintRadiusMultiplier = 1f;
        GoldValueMultiplier = 1f;
        ScoreMultiplier = 1f;
        SlowMotionDurationBonus = 0f;
        HazardShieldCharges = 0;
    }

    /// <summary>
    /// Consume one hazard shield charge. Returns true if a charge was available.
    /// </summary>
    public static bool ConsumeHazardShield()
    {
        if (HazardShieldCharges <= 0) return false;
        HazardShieldCharges--;
        return true;
    }

    /// <summary>
    /// Restore per-room charges (e.g., hazard shield resets each room).
    /// Call at the start of each room.
    /// </summary>
    public static void ResetPerRoomCharges()
    {
        // HazardShield charges are re-granted by the effect being active;
        // we just need to track the "used this room" state.
        // This is handled by PerkManager.OnRoomStart().
    }
}
```

**Commit:** `feat(roguelite): add PerkModifiers static registry for perk-driven game values`

---

## Task 5 — PerkManager

> Manages active perks for the current run. Handles perk selection logic.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/Perks/PerkManager.cs`

```csharp
// === PerkManager.cs ===
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's active perks during a run.
/// Handles weighted-random perk selection, application, and cleanup.
/// </summary>
public class PerkManager : MonoBehaviour
{
    [SerializeField] private GameConfigSO _gameConfig;

    private readonly List<PerkSO> _ownedPerks = new();
    private readonly List<PerkEffect> _activeEffects = new();

    /// <summary>All perks currently owned this run.</summary>
    public IReadOnlyList<PerkSO> OwnedPerks => _ownedPerks;

    /// <summary>
    /// Initialize for a new run. Clears all perks and resets modifiers.
    /// </summary>
    public void InitializeRun()
    {
        RemoveAllPerks();
        PerkModifiers.Reset();
    }

    /// <summary>
    /// Acquire a perk: add to owned list and activate its effect.
    /// </summary>
    public void AcquirePerk(PerkSO perk)
    {
        if (_ownedPerks.Contains(perk))
        {
            Debug.LogWarning($"[PerkManager] Already own perk: {perk.perkName}");
            return;
        }

        _ownedPerks.Add(perk);

        PerkEffect effect = PerkEffect.CreateFromSO(perk);
        if (effect != null)
        {
            effect.Activate();
            _activeEffects.Add(effect);
        }

        Debug.Log($"[PerkManager] Acquired perk: {perk.perkName} (total: {_ownedPerks.Count})");
    }

    /// <summary>
    /// Called at the start of each room to reset per-room perk charges.
    /// </summary>
    public void OnRoomStart()
    {
        // Reset hazard shield charges based on active HazardShield effects
        int shieldCount = 0;
        foreach (PerkEffect effect in _activeEffects)
        {
            if (effect is HazardShieldEffect && effect.IsActive)
            {
                shieldCount++;
            }
        }
        PerkModifiers.HazardShieldCharges = shieldCount;
    }

    /// <summary>
    /// Generate a selection of N perks for the player to choose from.
    /// Respects rarity weights, excludes already-owned perks, and checks unlock status.
    /// </summary>
    public List<PerkSO> GenerateSelection(int count, SaveData saveData)
    {
        var candidates = new List<PerkSO>();
        foreach (PerkSO perk in _gameConfig.allPerks)
        {
            if (_ownedPerks.Contains(perk)) continue;
            if (perk.requiresUnlock && !saveData.unlockedPerkIds.Contains(perk.perkId)) continue;
            candidates.Add(perk);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[PerkManager] No perk candidates available");
            return new List<PerkSO>();
        }

        var selection = new List<PerkSO>();
        int attempts = 0;
        const int maxAttempts = 100;

        while (selection.Count < count && selection.Count < candidates.Count && attempts < maxAttempts)
        {
            attempts++;
            PerkSO picked = WeightedRandomPick(candidates);
            if (picked != null && !selection.Contains(picked))
            {
                selection.Add(picked);
            }
        }

        return selection;
    }

    /// <summary>
    /// Remove all perks and deactivate effects. Called at end of run.
    /// </summary>
    public void RemoveAllPerks()
    {
        foreach (PerkEffect effect in _activeEffects)
        {
            effect.Deactivate();
        }
        _activeEffects.Clear();
        _ownedPerks.Clear();
    }

    private PerkSO WeightedRandomPick(List<PerkSO> candidates)
    {
        float totalWeight = 0f;
        foreach (PerkSO perk in candidates)
        {
            totalWeight += GetRarityWeight(perk.rarity);
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (PerkSO perk in candidates)
        {
            cumulative += GetRarityWeight(perk.rarity);
            if (roll <= cumulative)
            {
                return perk;
            }
        }

        return candidates[candidates.Count - 1];
    }

    private float GetRarityWeight(PerkRarity rarity)
    {
        return rarity switch
        {
            PerkRarity.Common => _gameConfig.commonWeight,
            PerkRarity.Rare => _gameConfig.rareWeight,
            PerkRarity.Legendary => _gameConfig.legendaryWeight,
            _ => _gameConfig.commonWeight
        };
    }
}
```

**Commit:** `feat(roguelite): add PerkManager for run-time perk acquisition and selection`

---

## Task 6 — Room Generation

> RoomConfig data, RoomGenerator, and room name table.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/RoomConfig.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/RoomNames.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/RoomGenerator.cs`

```csharp
// === RoomConfig.cs ===
/// <summary>
/// Runtime configuration for a single room, produced by RoomGenerator.
/// Passed to BalloonGameManager to set up a room.
/// </summary>
public class RoomConfig
{
    public int RoomNumber;
    public string RoomName;
    public int Columns;
    public int Rows;
    public int Darts;
    public int TargetScore;
    public int SpecialCount;
    public int HazardCount;
    public RoomType RoomType;
    public int Seed;
}
```

```csharp
// === RoomNames.cs ===
/// <summary>
/// Static table of creative room names themed to carnival and neon noir.
/// </summary>
public static class RoomNames
{
    private static readonly string[] Names =
    {
        "THE NEON MAZE",
        "SMOKE AND MIRRORS",
        "MIDNIGHT MIDWAY",
        "THE VELVET DARK",
        "FUNHOUSE ALLEY",
        "ELECTRIC RAIN",
        "THE CROOKED LANE",
        "INK AND FIRE",
        "THE GLASS ARCADE",
        "SHADOW CARNIVAL",
        "THE LAST WALTZ",
        "STATIC BLOOM",
        "THE HOLLOW TENT",
        "CHROME TWILIGHT",
        "THE DRIPPING SIGN",
        "WET NEON",
        "THE BACK LOT",
        "BROKEN CALLIOPE",
        "THE PAINTED DOOR",
        "GHOST LIGHTS",
        "THE BARKER'S BOOTH",
        "DEAD FERRIS",
        "THE RIGGED GAME",
        "MERCURY LANE",
        "THE FLICKERING MARQUEE",
        "SIDESHOW ROW",
        "THE SPINNING CAGE",
        "BURIED TICKET",
        "THE WAXWORK HALL",
        "AFTERGLOW ALLEY",
        "THE RUSTED CROWN",
        "PHOSPHOR WALK",
        "THE FORTUNE TELLER'S LIE",
        "TRAPDOOR STAGE",
        "THE RINGMASTER'S GHOST"
    };

    /// <summary>
    /// Get a room name by index. Wraps around if index exceeds array length.
    /// Uses a deterministic shuffle seeded by run seed for variety.
    /// </summary>
    public static string GetName(int roomNumber, int runSeed)
    {
        // Simple deterministic selection: hash room number + run seed
        int hash = (roomNumber * 7919 + runSeed * 104729) & 0x7FFFFFFF;
        return Names[hash % Names.Length];
    }

    /// <summary>
    /// Boss room always has a unique name.
    /// </summary>
    public static string GetBossName()
    {
        return "THE FINAL ACT";
    }
}
```

```csharp
// === RoomGenerator.cs ===
using UnityEngine;

/// <summary>
/// Produces a RoomConfig from a room number, run seed, and game config.
/// Handles difficulty escalation, bonus room insertion, and boss room setup.
/// </summary>
public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private GameConfigSO _gameConfig;

    /// <summary>
    /// Generate a RoomConfig for the given room number within a run.
    /// </summary>
    /// <param name="roomNumber">1-based room number.</param>
    /// <param name="runSeed">Seed for this run, ensures reproducible layouts.</param>
    /// <param name="saveData">Save data for meta bonuses.</param>
    public RoomConfig Generate(int roomNumber, int runSeed, SaveData saveData)
    {
        RoomConfig config = new RoomConfig
        {
            RoomNumber = roomNumber,
            Seed = runSeed + roomNumber
        };

        // Determine room type
        if (roomNumber == _gameConfig.totalRooms)
        {
            config.RoomType = RoomType.Boss;
            config.RoomName = RoomNames.GetBossName();
        }
        else if (roomNumber > 1 && roomNumber % _gameConfig.bonusRoomInterval == 0)
        {
            config.RoomType = RoomType.Bonus;
            config.RoomName = "BONUS ROUND";
        }
        else
        {
            config.RoomType = RoomType.Normal;
            config.RoomName = RoomNames.GetName(roomNumber, runSeed);
        }

        // Select base template from difficulty tier
        DifficultyTier tier = GetTier(roomNumber);
        ApplyTierDefaults(config, tier);

        // Apply bonus room overrides
        if (config.RoomType == RoomType.Bonus)
        {
            config.Darts = 6;
            config.HazardCount = 0;
            config.SpecialCount = config.Columns * config.Rows / 3; // Lots of gold/paint
            config.TargetScore = 0; // No target — just collect
        }

        // Apply boss room overrides
        if (config.RoomType == RoomType.Boss)
        {
            config.Columns = 9;
            config.Rows = 10;
            config.Darts = 3;
            config.TargetScore = (int)(GameConstants.BASE_TARGET_SCORE * 3.5f);
            config.SpecialCount = 6;
            config.HazardCount = 5;
        }

        // Apply perk bonuses
        config.Darts += PerkModifiers.BonusDartsPerRoom;

        // Apply meta bonuses
        config.Darts += saveData.bonusStartingDarts;

        return config;
    }

    private DifficultyTier GetTier(int roomNumber)
    {
        if (roomNumber <= 3) return DifficultyTier.Easy;
        if (roomNumber <= 7) return DifficultyTier.Medium;
        if (roomNumber <= 10) return DifficultyTier.Hard;
        return DifficultyTier.Expert;
    }

    private void ApplyTierDefaults(RoomConfig config, DifficultyTier tier)
    {
        switch (tier)
        {
            case DifficultyTier.Easy:
                config.Columns = 6;
                config.Rows = 5;
                config.Darts = 5;
                config.TargetScore = (int)(GameConstants.BASE_TARGET_SCORE *
                    (1f + config.RoomNumber * _gameConfig.targetScoreScaling * 0.5f));
                config.SpecialCount = Random.Range(0, 2);
                config.HazardCount = 0;
                break;

            case DifficultyTier.Medium:
                config.Columns = 7;
                config.Rows = 7;
                config.Darts = 4;
                config.TargetScore = (int)(GameConstants.BASE_TARGET_SCORE *
                    (1f + config.RoomNumber * _gameConfig.targetScoreScaling));
                config.SpecialCount = Random.Range(2, 4);
                config.HazardCount = 1;
                break;

            case DifficultyTier.Hard:
                config.Columns = 8;
                config.Rows = 8;
                config.Darts = 4;
                config.TargetScore = (int)(GameConstants.BASE_TARGET_SCORE *
                    (1f + config.RoomNumber * _gameConfig.targetScoreScaling * 1.3f));
                config.SpecialCount = Random.Range(3, 5);
                config.HazardCount = Random.Range(2, 4);
                break;

            case DifficultyTier.Expert:
                config.Columns = 8;
                config.Rows = 9;
                config.Darts = 3;
                config.TargetScore = (int)(GameConstants.BASE_TARGET_SCORE *
                    (1f + config.RoomNumber * _gameConfig.targetScoreScaling * 1.6f));
                config.SpecialCount = Random.Range(4, 7);
                config.HazardCount = Random.Range(3, 6);
                break;
        }
    }

    private enum DifficultyTier
    {
        Easy,
        Medium,
        Hard,
        Expert
    }
}
```

**Commit:** `feat(roguelite): add RoomGenerator, RoomConfig, and RoomNames for procedural room setup`

---

## Task 7 — Run State and RunManager

> Central run orchestrator. Replaces BalloonGameManager as top-level controller.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/RunState.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/RunManager.cs`
- [ ] Modify `Assets/Scripts/BalloonGame/BalloonGameManager.cs` to accept RoomConfig

```csharp
// === RunState.cs ===
/// <summary>
/// States of the run state machine.
/// </summary>
public enum RunState
{
    /// <summary>Run is starting — show initial UI, reset state.</summary>
    RunStart,
    /// <summary>Setting up the next room (generating config, building balloon wall).</summary>
    RoomSetup,
    /// <summary>Player is actively playing the current room.</summary>
    Playing,
    /// <summary>Room has been completed — transitioning to perk selection or next phase.</summary>
    RoomComplete,
    /// <summary>Perk selection screen is active.</summary>
    PerkSelection,
    /// <summary>Player failed the current room — run is ending.</summary>
    RunFailed,
    /// <summary>Player completed all rooms — run is ending with victory.</summary>
    RunVictory,
    /// <summary>Run end screen is showing stats and currency.</summary>
    RunEnd
}
```

```csharp
// === RunData.cs ===
using System.Collections.Generic;

/// <summary>
/// Transient data for the current run. Not persisted — lives only during a run.
/// </summary>
public class RunData
{
    public int RunSeed;
    public int CurrentRoomNumber;
    public int TotalScore;
    public int TotalInkEarned;
    public int TotalBalloonsPopped;
    public int TotalDartsThrown;
    public int MaxCombo;
    public List<string> PerksChosen = new();

    public void Reset(int seed)
    {
        RunSeed = seed;
        CurrentRoomNumber = 0;
        TotalScore = 0;
        TotalInkEarned = 0;
        TotalBalloonsPopped = 0;
        TotalDartsThrown = 0;
        MaxCombo = 0;
        PerksChosen.Clear();
    }
}
```

```csharp
// === RunManager.cs ===
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Top-level orchestrator for a complete roguelite run.
/// Manages the run state machine, room transitions, perk selection, and run end.
/// Replaces BalloonGameManager as the scene-level controller.
/// BalloonGameManager becomes a per-room controller that RunManager configures.
/// </summary>
[DisallowMultipleComponent]
public class RunManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameConfigSO _gameConfig;
    [SerializeField] private BalloonGameManager _roomController;
    [SerializeField] private RoomGenerator _roomGenerator;
    [SerializeField] private PerkManager _perkManager;
    [SerializeField] private SaveManager _saveManager;

    [Header("UI")]
    [SerializeField] private PerkSelectionScreen _perkSelectionScreen;
    [SerializeField] private RoomIntroScreen _roomIntroScreen;
    [SerializeField] private RunEndScreen _runEndScreen;
    [SerializeField] private RunHUD _runHUD;
    [SerializeField] private FadeOverlay _fadeOverlay;

    /// <summary>Current state of the run state machine.</summary>
    public RunState CurrentState { get; private set; }

    /// <summary>Current run data (score, room number, etc).</summary>
    public RunData CurrentRun { get; private set; } = new RunData();

    /// <summary>Fires when the run state changes.</summary>
    public event Action<RunState> OnRunStateChanged;

    private RoomConfig _currentRoomConfig;

    /// <summary>
    /// Begin a new run. Called from the title screen or lobby.
    /// </summary>
    public void StartNewRun()
    {
        int seed = Environment.TickCount;
        CurrentRun.Reset(seed);
        _perkManager.InitializeRun();
        SetState(RunState.RunStart);
        StartCoroutine(RunStartSequence());
    }

    private void SetState(RunState newState)
    {
        CurrentState = newState;
        OnRunStateChanged?.Invoke(newState);
        Debug.Log($"[RunManager] State → {newState}");
    }

    private IEnumerator RunStartSequence()
    {
        // Brief fade in
        if (_fadeOverlay != null)
        {
            yield return _fadeOverlay.FadeIn(_gameConfig.fadeDuration);
        }

        AdvanceToNextRoom();
    }

    private void AdvanceToNextRoom()
    {
        CurrentRun.CurrentRoomNumber++;

        if (CurrentRun.CurrentRoomNumber > _gameConfig.totalRooms)
        {
            SetState(RunState.RunVictory);
            StartCoroutine(RunEndSequence(true));
            return;
        }

        SetState(RunState.RoomSetup);
        _currentRoomConfig = _roomGenerator.Generate(
            CurrentRun.CurrentRoomNumber,
            CurrentRun.RunSeed,
            _saveManager.Data);

        StartCoroutine(RoomSetupSequence());
    }

    private IEnumerator RoomSetupSequence()
    {
        // Fade to black
        if (_fadeOverlay != null)
        {
            yield return _fadeOverlay.FadeOut(_gameConfig.fadeDuration);
        }

        // Show room intro
        if (_roomIntroScreen != null)
        {
            _roomIntroScreen.Show(
                _currentRoomConfig.RoomNumber,
                _currentRoomConfig.RoomName,
                _currentRoomConfig.TargetScore,
                _currentRoomConfig.RoomType,
                _perkManager.OwnedPerks);
        }

        // Update run HUD
        if (_runHUD != null)
        {
            _runHUD.UpdateDisplay(
                CurrentRun.CurrentRoomNumber,
                _gameConfig.totalRooms,
                CurrentRun.TotalInkEarned,
                _perkManager.OwnedPerks);
        }

        // Fade in with room intro visible
        if (_fadeOverlay != null)
        {
            yield return _fadeOverlay.FadeIn(_gameConfig.fadeDuration);
        }

        // Wait for intro duration
        yield return new WaitForSeconds(_gameConfig.roomIntroDuration);

        // Hide room intro
        if (_roomIntroScreen != null)
        {
            _roomIntroScreen.Hide();
        }

        // Configure and start the room
        _perkManager.OnRoomStart();
        _roomController.ConfigureRoom(_currentRoomConfig);
        _roomController.StartRoom();

        SetState(RunState.Playing);
    }

    private void OnEnable()
    {
        BalloonGameManager.OnRoomCleared += HandleRoomCleared;
        BalloonGameManager.OnRoomFailed += HandleRoomFailed;
    }

    private void OnDisable()
    {
        BalloonGameManager.OnRoomCleared -= HandleRoomCleared;
        BalloonGameManager.OnRoomFailed -= HandleRoomFailed;
    }

    private void HandleRoomCleared(int roomScore, int balloonsPopped, int dartsUsed)
    {
        if (CurrentState != RunState.Playing) return;

        // Apply score multiplier from perks
        int modifiedScore = (int)(roomScore * PerkModifiers.ScoreMultiplier);
        CurrentRun.TotalScore += modifiedScore;
        CurrentRun.TotalBalloonsPopped += balloonsPopped;
        CurrentRun.TotalDartsThrown += dartsUsed;

        // Calculate ink earned for this room
        int roomInk = _gameConfig.inkPerRoom +
                      (modifiedScore / 1000) * _gameConfig.inkPer1000Score;
        CurrentRun.TotalInkEarned += roomInk;

        SetState(RunState.RoomComplete);

        // Decide: perk selection or advance
        if (CurrentRun.CurrentRoomNumber < _gameConfig.totalRooms &&
            _currentRoomConfig.RoomType != RoomType.Bonus)
        {
            StartCoroutine(PerkSelectionSequence());
        }
        else
        {
            AdvanceToNextRoom();
        }
    }

    private void HandleRoomFailed(int roomScore, int balloonsPopped, int dartsUsed)
    {
        if (CurrentState != RunState.Playing) return;

        int modifiedScore = (int)(roomScore * PerkModifiers.ScoreMultiplier);
        CurrentRun.TotalScore += modifiedScore;
        CurrentRun.TotalBalloonsPopped += balloonsPopped;
        CurrentRun.TotalDartsThrown += dartsUsed;

        SetState(RunState.RunFailed);
        StartCoroutine(RunEndSequence(false));
    }

    private IEnumerator PerkSelectionSequence()
    {
        SetState(RunState.PerkSelection);

        var choices = _perkManager.GenerateSelection(
            _gameConfig.perkChoiceCount,
            _saveManager.Data);

        if (choices.Count == 0)
        {
            // No perks available — skip selection
            AdvanceToNextRoom();
            yield break;
        }

        // Show perk selection screen
        PerkSO selectedPerk = null;
        if (_perkSelectionScreen != null)
        {
            _perkSelectionScreen.Show(choices, (perk) =>
            {
                selectedPerk = perk;
            });
        }

        // Wait for player to select a perk
        while (selectedPerk == null)
        {
            yield return null;
        }

        // Acquire the perk
        _perkManager.AcquirePerk(selectedPerk);
        CurrentRun.PerksChosen.Add(selectedPerk.perkId);

        // Brief delay for animation
        yield return new WaitForSeconds(_gameConfig.perkSelectionDelay);

        // Hide perk selection
        if (_perkSelectionScreen != null)
        {
            _perkSelectionScreen.Hide();
        }

        AdvanceToNextRoom();
    }

    private IEnumerator RunEndSequence(bool isVictory)
    {
        SetState(RunState.RunEnd);

        // Apply full-run ink multiplier if victorious
        if (isVictory)
        {
            CurrentRun.TotalInkEarned =
                (int)(CurrentRun.TotalInkEarned * _gameConfig.fullRunInkMultiplier);
        }

        // Record the run
        RunRecord record = new RunRecord
        {
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            roomsCleared = CurrentRun.CurrentRoomNumber - (isVictory ? 0 : 1),
            finalScore = CurrentRun.TotalScore,
            inkEarned = CurrentRun.TotalInkEarned,
            perksChosen = new System.Collections.Generic.List<string>(CurrentRun.PerksChosen),
            balloonsPopped = CurrentRun.TotalBalloonsPopped,
            dartsThrown = CurrentRun.TotalDartsThrown,
            maxCombo = CurrentRun.MaxCombo
        };

        _saveManager.RecordRun(record);
        _saveManager.AddInk(CurrentRun.TotalInkEarned);

        // Clean up perks
        _perkManager.RemoveAllPerks();

        // Fade to run end screen
        if (_fadeOverlay != null)
        {
            yield return _fadeOverlay.FadeOut(_gameConfig.fadeDuration);
        }

        if (_runEndScreen != null)
        {
            _runEndScreen.Show(
                isVictory,
                CurrentRun.TotalScore,
                record.roomsCleared,
                CurrentRun.TotalInkEarned,
                CurrentRun.TotalBalloonsPopped,
                CurrentRun.TotalDartsThrown,
                CurrentRun.MaxCombo);
        }

        if (_fadeOverlay != null)
        {
            yield return _fadeOverlay.FadeIn(_gameConfig.fadeDuration);
        }
    }

    /// <summary>
    /// Called by RunEndScreen's Continue button. Returns to lobby/title.
    /// </summary>
    public void OnContinueFromRunEnd()
    {
        if (_runEndScreen != null)
        {
            _runEndScreen.Hide();
        }

        // For now, just start a new run. Title/lobby screen comes in Plan 06.
        Debug.Log("[RunManager] Run complete. Returning to lobby (not yet implemented).");
    }
}
```

**Commit:** `feat(roguelite): add RunManager as top-level run orchestrator with state machine`

---

## Task 8 — Modify BalloonGameManager for Room Integration

> Demote BalloonGameManager to a per-room controller. Accept RoomConfig. Fire room-level events.

- [ ] Modify `Assets/Scripts/BalloonGame/BalloonGameManager.cs`
- [ ] Modify `Assets/Scripts/BalloonGame/BalloonWall.cs` to accept dynamic grid size
- [ ] Modify `Assets/Scripts/BalloonGame/ScoreManager.cs` to support perk multipliers
- [ ] Update `Assets/Scripts/BalloonGame/GameConstants.cs` with roguelite constants

```csharp
// === BalloonGameManager.cs (MODIFIED) ===
using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Per-room game controller. Manages a single room's lifecycle.
/// Configured by RunManager with a RoomConfig before each room begins.
/// </summary>
public class BalloonGameManager : MonoBehaviour
{
    public enum GameState
    {
        RoomIntro,
        Ready,
        DartInFlight,
        RoomCleared,
        RoomFailed
    }

    /// <summary>Fires when the room is cleared. Args: score, balloons popped, darts used.</summary>
    public static event Action<int, int, int> OnRoomCleared;

    /// <summary>Fires when the room is failed. Args: score, balloons popped, darts used.</summary>
    public static event Action<int, int, int> OnRoomFailed;

    private const float IntroDuration = 1f;

    public GameState CurrentState { get; private set; } = GameState.RoomIntro;
    public int DartsRemaining { get; private set; }

    private BalloonWall _balloonWall;
    private SlingshotInput _slingshotInput;
    private ScoreManager _scoreManager;
    private GameHUD _hud;
    private DartLauncher _dartLauncher;
    private DartController _activeDart;
    private float _introTimer;

    private RoomConfig _roomConfig;
    private int _dartsUsedThisRoom;
    private int _balloonsAtStart;

    private void Awake()
    {
        _balloonWall = FindAnyObjectByType<BalloonWall>();
        _slingshotInput = FindAnyObjectByType<SlingshotInput>();
        _scoreManager = GetComponent<ScoreManager>();
        _hud = GetComponent<GameHUD>();
        ConfigureSceneCollisionLayers();

        var launcherObject = new GameObject("DartLauncher");
        launcherObject.transform.SetParent(transform, false);
        _dartLauncher = launcherObject.AddComponent<DartLauncher>();
    }

    private void Start()
    {
        if (_slingshotInput != null)
        {
            _slingshotInput.OnLaunch += HandleLaunch;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged += HandleScoreChanged;
            _scoreManager.OnTargetReached += HandleTargetReached;
        }

        DartController.OnDartFinished += HandleDartFinished;

        // Only auto-start if no RunManager is present (standalone testing)
        if (FindAnyObjectByType<RunManager>() == null)
        {
            StartRoom();
        }
    }

    private void OnDestroy()
    {
        if (_slingshotInput != null)
        {
            _slingshotInput.OnLaunch -= HandleLaunch;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged -= HandleScoreChanged;
            _scoreManager.OnTargetReached -= HandleTargetReached;
        }

        DartController.OnDartFinished -= HandleDartFinished;
    }

    /// <summary>
    /// Configure this room with a RoomConfig. Called by RunManager before StartRoom.
    /// </summary>
    public void ConfigureRoom(RoomConfig config)
    {
        _roomConfig = config;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.RoomIntro:
                _introTimer -= Time.deltaTime;
                if (_introTimer <= 0f)
                {
                    CurrentState = GameState.Ready;
                    _slingshotInput?.SetCanFire(true);
                    UpdateHud();
                }
                break;

            case GameState.RoomCleared:
            case GameState.RoomFailed:
                // Only allow restart via keyboard in standalone mode (no RunManager)
                if (FindAnyObjectByType<RunManager>() == null &&
                    Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                {
                    RestartRoom();
                }
                break;
        }
    }

    /// <summary>
    /// Start (or restart) the current room.
    /// </summary>
    public void StartRoom()
    {
        CurrentState = GameState.RoomIntro;
        _introTimer = IntroDuration;
        _dartsUsedThisRoom = 0;
        _activeDart = null;

        int darts;
        int targetScore;
        int columns;
        int rows;

        if (_roomConfig != null)
        {
            darts = _roomConfig.Darts;
            targetScore = _roomConfig.TargetScore;
            columns = _roomConfig.Columns;
            rows = _roomConfig.Rows;
        }
        else
        {
            darts = GameConstants.STARTING_DARTS;
            targetScore = GameConstants.BASE_TARGET_SCORE;
            columns = GameConstants.BOARD_COLUMNS;
            rows = GameConstants.BOARD_ROWS;
        }

        DartsRemaining = darts;
        _scoreManager?.Initialize(targetScore);
        _balloonWall?.GenerateWall(columns, rows);
        _balloonsAtStart = _balloonWall != null ? _balloonWall.ActiveCount() : 0;
        _slingshotInput?.SetCanFire(false);
        UpdateHud();
    }

    private void RestartRoom()
    {
        _activeDart = null;

        foreach (DartController dart in FindObjectsByType<DartController>(FindObjectsSortMode.None))
        {
            Destroy(dart.gameObject);
        }

        StartRoom();
    }

    private void HandleLaunch(Vector3 velocity)
    {
        if (CurrentState != GameState.Ready || DartsRemaining <= 0)
        {
            return;
        }

        // Apply perk speed modifier
        velocity *= PerkModifiers.DartSpeedMultiplier;

        _slingshotInput?.SetCanFire(false);
        _activeDart = _dartLauncher.SpawnAndLaunch(velocity);
        _dartsUsedThisRoom++;
        CurrentState = GameState.DartInFlight;
        UpdateHud();
    }

    private void HandleDartFinished(DartController dart)
    {
        if (dart != _activeDart)
        {
            return;
        }

        _activeDart = null;

        if (CurrentState == GameState.RoomCleared)
        {
            return;
        }

        DartsRemaining--;
        if (DartsRemaining <= 0)
        {
            CurrentState = GameState.RoomFailed;
            _slingshotInput?.SetCanFire(false);
            UpdateHud();

            int score = _scoreManager != null ? _scoreManager.CurrentScore : 0;
            int popped = _balloonsAtStart - (_balloonWall != null ? _balloonWall.ActiveCount() : 0);
            OnRoomFailed?.Invoke(score, popped, _dartsUsedThisRoom);
            Debug.Log($"ROOM FAILED. Score: {_scoreManager?.CurrentScore}/{_scoreManager?.TargetScore}");
            return;
        }

        CurrentState = GameState.Ready;
        _slingshotInput?.SetCanFire(true);
        UpdateHud();
    }

    private void HandleScoreChanged(int score)
    {
        UpdateHud();
    }

    private void HandleTargetReached()
    {
        CurrentState = GameState.RoomCleared;
        _slingshotInput?.SetCanFire(false);
        UpdateHud();

        int score = _scoreManager != null ? _scoreManager.CurrentScore : 0;
        int popped = _balloonsAtStart - (_balloonWall != null ? _balloonWall.ActiveCount() : 0);
        OnRoomCleared?.Invoke(score, popped, _dartsUsedThisRoom);
        Debug.Log($"ROOM CLEARED! Score: {_scoreManager?.CurrentScore}/{_scoreManager?.TargetScore}");
    }

    private void UpdateHud()
    {
        int roomNumber = _roomConfig?.RoomNumber ?? 0;
        _hud?.UpdateDisplay(
            _scoreManager != null ? _scoreManager.CurrentScore : 0,
            _scoreManager != null ? _scoreManager.TargetScore : GameConstants.BASE_TARGET_SCORE,
            DartsRemaining,
            CurrentState);
    }

    private static void ConfigureSceneCollisionLayers()
    {
        SetLayerIfFound("BackWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("LeftWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("RightWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("TopWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("LaneFloor", GameConstants.LAYER_ENVIRONMENT);
    }

    private static void SetLayerIfFound(string objectName, int layer)
    {
        GameObject sceneObject = GameObject.Find(objectName);
        if (sceneObject != null)
        {
            sceneObject.layer = layer;
        }
    }
}
```

```csharp
// === BalloonWall.cs — MODIFIED GenerateWall signature ===
// Add overload that accepts dynamic grid size:

    /// <summary>
    /// Generate a balloon wall with the specified grid dimensions.
    /// </summary>
    public void GenerateWall(int columns, int rows)
    {
        ClearWall();
        EnsureMaterials();

        float slotX = GameConstants.BOARD_WIDTH / columns;
        float slotY = GameConstants.BOARD_HEIGHT / rows;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                BalloonColor color = _colors[Random.Range(0, _colors.Length)];
                float scale = PerspectiveScale(row, rows);
                Vector3 position = PerspectivePosition(row, column, rows, columns, slotX, slotY);

                var balloon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                balloon.name = $"Balloon_{row}_{column}";
                balloon.transform.SetParent(transform);
                balloon.transform.localPosition = position;

                float width = GameConstants.BALLOON_MAX_WIDTH * GameConstants.BALLOON_SLOT_RATIO_X * scale;
                float height = GameConstants.BALLOON_MAX_HEIGHT * GameConstants.BALLOON_SLOT_RATIO_Y * scale;
                balloon.transform.localScale = new Vector3(width, height, width);
                balloon.layer = GameConstants.LAYER_BALLOONS;

                var renderer = balloon.GetComponent<Renderer>();
                renderer.material = _materials[color];
                ApplyAtmosphericFade(renderer, row, rows);

                var rigidbody = balloon.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;

                var node = balloon.AddComponent<BalloonNode>();
                node.Initialize(row, column, color);
                _balloons.Add(node);
            }
        }
    }

    /// <summary>
    /// Generate a balloon wall with default dimensions from GameConstants.
    /// </summary>
    [ContextMenu("Generate Wall")]
    public void GenerateWall()
    {
        GenerateWall(GameConstants.BOARD_COLUMNS, GameConstants.BOARD_ROWS);
    }
```

```csharp
// === GameConstants.cs — ADD roguelite constants ===
// Append to the existing static class:

    // Roguelite constants
    public const int DEFAULT_TOTAL_ROOMS = 13;
    public const int BONUS_ROOM_INTERVAL = 4;
    public const int INK_PER_ROOM = 10;
    public const int INK_PER_1000_SCORE = 5;
    public const float FULL_RUN_INK_MULTIPLIER = 2f;
    public const int PERK_CHOICE_COUNT = 3;
    public const float ROOM_INTRO_DURATION = 2f;
    public const float PERK_SELECTION_DELAY = 0.8f;
    public const float FADE_DURATION = 0.4f;
```

**Commit:** `refactor(roguelite): demote BalloonGameManager to per-room controller, add RoomConfig integration`

---

## Task 9 — Fade Overlay Utility

> Simple CanvasGroup-based fade for all transitions.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/UI/FadeOverlay.cs`

```csharp
// === FadeOverlay.cs ===
using System.Collections;
using UnityEngine;

/// <summary>
/// Full-screen fade overlay using CanvasGroup alpha lerp.
/// Attach to a full-screen Image (black) with a CanvasGroup component.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class FadeOverlay : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        // Start transparent
        _canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Fade from black to transparent (reveal).
    /// </summary>
    public Coroutine FadeIn(float duration)
    {
        return StartCoroutine(Fade(1f, 0f, duration));
    }

    /// <summary>
    /// Fade from transparent to black (conceal).
    /// </summary>
    public Coroutine FadeOut(float duration)
    {
        return StartCoroutine(Fade(0f, 1f, duration));
    }

    /// <summary>
    /// Instantly set alpha.
    /// </summary>
    public void SetAlpha(float alpha)
    {
        _canvasGroup.alpha = alpha;
        _canvasGroup.blocksRaycasts = alpha > 0.5f;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        _canvasGroup.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = to;
        _canvasGroup.blocksRaycasts = to > 0.5f;
    }
}
```

**Commit:** `feat(roguelite): add FadeOverlay CanvasGroup utility for screen transitions`

---

## Task 10 — Perk Selection Screen UI

> Three perk cards rise from bottom. Player taps to select. Carnival tarot feel.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/UI/PerkCardUI.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/UI/PerkSelectionScreen.cs`

```csharp
// === PerkCardUI.cs ===
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for a single perk card in the selection screen.
/// Intended to be a prefab with icon, name, description, and rarity border.
/// </summary>
public class PerkCardUI : MonoBehaviour
{
    [Header("Card Elements")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Image _borderImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Button _selectButton;
    [SerializeField] private CanvasGroup _canvasGroup;

    /// <summary>The PerkSO this card represents.</summary>
    public PerkSO Perk { get; private set; }

    private RectTransform _rectTransform;

    private static readonly Color CommonBorderColor = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color RareBorderColor = new Color(0.3f, 0.5f, 0.9f);
    private static readonly Color LegendaryBorderColor = new Color(1f, 0.8f, 0.2f);

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// Configure the card to display a specific perk.
    /// </summary>
    public void Configure(PerkSO perk)
    {
        Perk = perk;

        if (_nameText != null)
        {
            _nameText.text = perk.perkName;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = perk.description;
        }

        if (_iconImage != null && perk.icon != null)
        {
            _iconImage.sprite = perk.icon;
            _iconImage.enabled = true;
        }
        else if (_iconImage != null)
        {
            _iconImage.enabled = false;
        }

        if (_borderImage != null)
        {
            _borderImage.color = GetRarityColor(perk.rarity);
        }
    }

    /// <summary>
    /// Register a callback for when this card is tapped.
    /// </summary>
    public void SetSelectCallback(System.Action<PerkSO> callback)
    {
        if (_selectButton != null)
        {
            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(() => callback?.Invoke(Perk));
        }
    }

    /// <summary>
    /// Play the "selected" visual state: scale up and glow.
    /// </summary>
    public void PlaySelectedState()
    {
        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one * 1.1f;
        }

        if (_borderImage != null)
        {
            Color glowColor = _borderImage.color;
            glowColor.a = 1f;
            _borderImage.color = glowColor;
        }
    }

    /// <summary>
    /// Play the "not selected" visual state: fade and shrink.
    /// </summary>
    public void PlayDismissedState()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0.3f;
        }

        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one * 0.85f;
        }
    }

    /// <summary>
    /// Reset card to default visual state.
    /// </summary>
    public void ResetVisuals()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Enable or disable the button interaction.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (_selectButton != null)
        {
            _selectButton.interactable = interactable;
        }
    }

    private static Color GetRarityColor(PerkRarity rarity)
    {
        return rarity switch
        {
            PerkRarity.Common => CommonBorderColor,
            PerkRarity.Rare => RareBorderColor,
            PerkRarity.Legendary => LegendaryBorderColor,
            _ => CommonBorderColor
        };
    }
}
```

```csharp
// === PerkSelectionScreen.cs ===
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Perk selection screen shown between rooms. Displays 3 perk cards for the player to choose from.
/// Dark overlay on the post-battle balloon wall, neon edge accents, carnival tarot feel.
/// </summary>
public class PerkSelectionScreen : MonoBehaviour
{
    [Header("Card Slots")]
    [SerializeField] private PerkCardUI _cardLeft;
    [SerializeField] private PerkCardUI _cardCenter;
    [SerializeField] private PerkCardUI _cardRight;

    [Header("UI")]
    [SerializeField] private CanvasGroup _screenCanvasGroup;
    [SerializeField] private GameObject _screenRoot;

    private Action<PerkSO> _onPerkSelected;
    private PerkCardUI[] _cards;
    private bool _selectionMade;

    private void Awake()
    {
        _cards = new[] { _cardLeft, _cardCenter, _cardRight };
        if (_screenRoot != null)
        {
            _screenRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Show the perk selection screen with the given choices.
    /// </summary>
    /// <param name="choices">List of PerkSO options (up to 3).</param>
    /// <param name="onSelected">Callback when the player selects a perk.</param>
    public void Show(List<PerkSO> choices, Action<PerkSO> onSelected)
    {
        _onPerkSelected = onSelected;
        _selectionMade = false;

        if (_screenRoot != null)
        {
            _screenRoot.SetActive(true);
        }

        // Configure available cards
        for (int i = 0; i < _cards.Length; i++)
        {
            if (i < choices.Count && _cards[i] != null)
            {
                _cards[i].gameObject.SetActive(true);
                _cards[i].Configure(choices[i]);
                _cards[i].ResetVisuals();
                _cards[i].SetInteractable(true);
                _cards[i].SetSelectCallback(HandleCardSelected);
            }
            else if (_cards[i] != null)
            {
                _cards[i].gameObject.SetActive(false);
            }
        }

        // Fade in
        if (_screenCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(_screenCanvasGroup, 0f, 1f, 0.3f));
        }
    }

    /// <summary>
    /// Hide the perk selection screen.
    /// </summary>
    public void Hide()
    {
        if (_screenRoot != null)
        {
            _screenRoot.SetActive(false);
        }
    }

    private void HandleCardSelected(PerkSO perk)
    {
        if (_selectionMade) return;
        _selectionMade = true;

        // Visual feedback: highlight selected, dim others
        foreach (PerkCardUI card in _cards)
        {
            if (card == null || !card.gameObject.activeSelf) continue;

            card.SetInteractable(false);

            if (card.Perk == perk)
            {
                card.PlaySelectedState();
            }
            else
            {
                card.PlayDismissedState();
            }
        }

        _onPerkSelected?.Invoke(perk);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }
}
```

**Commit:** `feat(roguelite): add PerkCardUI prefab component and PerkSelectionScreen`

---

## Task 11 — Room Intro Screen and Run HUD

> Room intro with dramatic name display. Run-level HUD with room counter, ink, perk icons.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/UI/RoomIntroScreen.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/UI/RunHUD.cs`

```csharp
// === RoomIntroScreen.cs ===
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Dramatic room intro screen. Shows room number, challenge name, target score,
/// special balloon preview, and owned perk icons.
/// Displayed for ~2 seconds before each room begins.
/// </summary>
public class RoomIntroScreen : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _roomNumberText;
    [SerializeField] private TextMeshProUGUI _roomNameText;
    [SerializeField] private TextMeshProUGUI _targetScoreText;

    [Header("Perk Tray")]
    [SerializeField] private Transform _perkIconContainer;
    [SerializeField] private GameObject _perkIconPrefab;

    [Header("Root")]
    [SerializeField] private GameObject _screenRoot;
    [SerializeField] private CanvasGroup _canvasGroup;

    /// <summary>
    /// Show the room intro screen with room details and owned perks.
    /// </summary>
    public void Show(int roomNumber, string roomName, int targetScore,
                     RoomType roomType, IReadOnlyList<PerkSO> ownedPerks)
    {
        if (_screenRoot != null)
        {
            _screenRoot.SetActive(true);
        }

        if (_roomNumberText != null)
        {
            _roomNumberText.text = $"ROOM {roomNumber}";
        }

        if (_roomNameText != null)
        {
            _roomNameText.text = roomName;
        }

        if (_targetScoreText != null)
        {
            if (roomType == RoomType.Bonus)
            {
                _targetScoreText.text = "BONUS ROUND — COLLECT EVERYTHING!";
            }
            else
            {
                _targetScoreText.text = $"TARGET: {targetScore:N0}";
            }
        }

        // Populate perk icons
        if (_perkIconContainer != null)
        {
            // Clear existing icons
            for (int i = _perkIconContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_perkIconContainer.GetChild(i).gameObject);
            }

            // Add owned perk icons
            if (_perkIconPrefab != null && ownedPerks != null)
            {
                foreach (PerkSO perk in ownedPerks)
                {
                    GameObject iconObj = Instantiate(_perkIconPrefab, _perkIconContainer);
                    Image iconImage = iconObj.GetComponent<Image>();
                    if (iconImage != null && perk.icon != null)
                    {
                        iconImage.sprite = perk.icon;
                    }
                }
            }
        }

        // Fade in
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Hide the room intro screen.
    /// </summary>
    public void Hide()
    {
        if (_screenRoot != null)
        {
            _screenRoot.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        const float duration = 0.3f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }
}
```

```csharp
// === RunHUD.cs ===
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Run-level HUD overlay. Displays room progress, Ink earned, and owned perk icons.
/// Sits above the room-level GameHUD.
/// </summary>
public class RunHUD : MonoBehaviour
{
    [Header("Room Progress")]
    [SerializeField] private TextMeshProUGUI _roomProgressText;

    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI _inkText;
    [SerializeField] private Image _inkIcon;

    [Header("Perk Tray")]
    [SerializeField] private Transform _perkIconContainer;
    [SerializeField] private GameObject _perkIconPrefab;

    [Header("Root")]
    [SerializeField] private GameObject _hudRoot;

    /// <summary>
    /// Update the run HUD with current run state.
    /// </summary>
    public void UpdateDisplay(int currentRoom, int totalRooms, int inkEarned,
                              IReadOnlyList<PerkSO> ownedPerks)
    {
        if (_hudRoot != null && !_hudRoot.activeSelf)
        {
            _hudRoot.SetActive(true);
        }

        if (_roomProgressText != null)
        {
            _roomProgressText.text = $"{currentRoom}/{totalRooms}";
        }

        if (_inkText != null)
        {
            _inkText.text = inkEarned.ToString();
        }

        // Refresh perk icons
        if (_perkIconContainer != null)
        {
            for (int i = _perkIconContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_perkIconContainer.GetChild(i).gameObject);
            }

            if (_perkIconPrefab != null && ownedPerks != null)
            {
                foreach (PerkSO perk in ownedPerks)
                {
                    GameObject iconObj = Instantiate(_perkIconPrefab, _perkIconContainer);
                    Image iconImage = iconObj.GetComponent<Image>();
                    if (iconImage != null && perk.icon != null)
                    {
                        iconImage.sprite = perk.icon;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Hide the run HUD (e.g., during run end screen).
    /// </summary>
    public void Hide()
    {
        if (_hudRoot != null)
        {
            _hudRoot.SetActive(false);
        }
    }
}
```

**Commit:** `feat(roguelite): add RoomIntroScreen and RunHUD for in-run UI`

---

## Task 12 — Run End Screen

> Shows final stats, currency earned, and continue button.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/UI/RunEndScreen.cs`

```csharp
// === RunEndScreen.cs ===
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Run end screen showing final stats and currency earned.
/// Displayed after room failure or completing all 13 rooms.
/// </summary>
public class RunEndScreen : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TextMeshProUGUI _headerText;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _roomsClearedText;
    [SerializeField] private TextMeshProUGUI _inkEarnedText;
    [SerializeField] private TextMeshProUGUI _balloonsPoppedText;
    [SerializeField] private TextMeshProUGUI _dartsThrownText;
    [SerializeField] private TextMeshProUGUI _maxComboText;

    [Header("Button")]
    [SerializeField] private Button _continueButton;

    [Header("Root")]
    [SerializeField] private GameObject _screenRoot;
    [SerializeField] private CanvasGroup _canvasGroup;

    private RunManager _runManager;

    private void Awake()
    {
        _runManager = FindAnyObjectByType<RunManager>();
        if (_continueButton != null)
        {
            _continueButton.onClick.AddListener(HandleContinue);
        }

        if (_screenRoot != null)
        {
            _screenRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Show the run end screen with final stats.
    /// </summary>
    public void Show(bool isVictory, int totalScore, int roomsCleared,
                     int inkEarned, int balloonsPopped, int dartsThrown, int maxCombo)
    {
        if (_screenRoot != null)
        {
            _screenRoot.SetActive(true);
        }

        if (_headerText != null)
        {
            _headerText.text = isVictory ? "RUN COMPLETE!" : "GAME OVER";
            _headerText.color = isVictory
                ? new Color(1f, 0.8f, 0.2f)
                : new Color(0.9f, 0.3f, 0.3f);
        }

        SetStatText(_scoreText, "SCORE", totalScore.ToString("N0"));
        SetStatText(_roomsClearedText, "ROOMS", roomsCleared.ToString());
        SetStatText(_inkEarnedText, "INK EARNED", inkEarned.ToString());
        SetStatText(_balloonsPoppedText, "BALLOONS", balloonsPopped.ToString());
        SetStatText(_dartsThrownText, "DARTS THROWN", dartsThrown.ToString());
        SetStatText(_maxComboText, "BEST COMBO", $"x{maxCombo}");

        // Fade in
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Hide the run end screen.
    /// </summary>
    public void Hide()
    {
        if (_screenRoot != null)
        {
            _screenRoot.SetActive(false);
        }
    }

    private void HandleContinue()
    {
        if (_runManager != null)
        {
            _runManager.OnContinueFromRunEnd();
        }
    }

    private void SetStatText(TextMeshProUGUI textComponent, string label, string value)
    {
        if (textComponent != null)
        {
            textComponent.text = $"{label}: {value}";
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        const float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }
}
```

**Commit:** `feat(roguelite): add RunEndScreen for game over and victory display`

---

## Task 13 — Meta-Progression Manager

> Handles permanent upgrades between runs.

- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/Meta/MetaProgressionManager.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Roguelite/Meta/RunHistoryTracker.cs`

```csharp
// === MetaProgressionManager.cs ===
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages permanent meta-progression upgrades purchased between runs.
/// Reads from and writes to SaveManager.
/// </summary>
public class MetaProgressionManager : MonoBehaviour
{
    [SerializeField] private GameConfigSO _gameConfig;
    [SerializeField] private SaveManager _saveManager;

    /// <summary>
    /// Attempt to purchase a meta upgrade. Returns true if successful.
    /// </summary>
    public bool PurchaseUpgrade(MetaUpgradeSO upgrade)
    {
        if (_saveManager.IsUpgradePurchased(upgrade.upgradeId))
        {
            Debug.LogWarning($"[Meta] Already purchased: {upgrade.displayName}");
            return false;
        }

        if (!_saveManager.SpendInk(upgrade.inkCost))
        {
            Debug.Log($"[Meta] Not enough Ink for: {upgrade.displayName} (need {upgrade.inkCost}, have {_saveManager.Data.totalInk})");
            return false;
        }

        _saveManager.Data.purchasedUpgradeIds.Add(upgrade.upgradeId);

        // Apply persistent effect
        switch (upgrade.upgradeType)
        {
            case MetaUpgradeType.StartingDarts:
                _saveManager.Data.bonusStartingDarts += upgrade.effectIntValue;
                break;

            case MetaUpgradeType.BaseScoreBonus:
                _saveManager.Data.bonusBaseScorePercent += upgrade.effectValue;
                break;

            case MetaUpgradeType.UnlockPerk:
                if (upgrade.perkToUnlock != null &&
                    !_saveManager.Data.unlockedPerkIds.Contains(upgrade.perkToUnlock.perkId))
                {
                    _saveManager.Data.unlockedPerkIds.Add(upgrade.perkToUnlock.perkId);
                }
                break;

            case MetaUpgradeType.DartSkin:
                if (!_saveManager.Data.unlockedDartSkins.Contains(upgrade.upgradeId))
                {
                    _saveManager.Data.unlockedDartSkins.Add(upgrade.upgradeId);
                }
                break;
        }

        _saveManager.Save();
        Debug.Log($"[Meta] Purchased: {upgrade.displayName}");
        return true;
    }

    /// <summary>
    /// Get all upgrades that are available for purchase (not yet bought, player has enough ink).
    /// </summary>
    public List<MetaUpgradeSO> GetAvailableUpgrades()
    {
        var available = new List<MetaUpgradeSO>();
        foreach (MetaUpgradeSO upgrade in _gameConfig.allMetaUpgrades)
        {
            if (!_saveManager.IsUpgradePurchased(upgrade.upgradeId))
            {
                available.Add(upgrade);
            }
        }
        return available;
    }

    /// <summary>
    /// Get all upgrades that have been purchased.
    /// </summary>
    public List<MetaUpgradeSO> GetPurchasedUpgrades()
    {
        var purchased = new List<MetaUpgradeSO>();
        foreach (MetaUpgradeSO upgrade in _gameConfig.allMetaUpgrades)
        {
            if (_saveManager.IsUpgradePurchased(upgrade.upgradeId))
            {
                purchased.Add(upgrade);
            }
        }
        return purchased;
    }
}
```

```csharp
// === RunHistoryTracker.cs ===
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides read access to run history from SaveData.
/// Used by UI to display past runs.
/// </summary>
public class RunHistoryTracker : MonoBehaviour
{
    [SerializeField] private SaveManager _saveManager;

    /// <summary>
    /// Get the last N run records.
    /// </summary>
    public List<RunRecord> GetRecentRuns(int count = 10)
    {
        if (_saveManager?.Data?.runHistory == null)
        {
            return new List<RunRecord>();
        }

        int take = Mathf.Min(count, _saveManager.Data.runHistory.Count);
        return _saveManager.Data.runHistory.GetRange(0, take);
    }

    /// <summary>
    /// Get lifetime statistics.
    /// </summary>
    public (int runs, int rooms, int balloons, int bestScore, int bestStreak) GetLifetimeStats()
    {
        SaveData data = _saveManager?.Data;
        if (data == null)
        {
            return (0, 0, 0, 0, 0);
        }

        return (data.totalRunsPlayed, data.totalRoomsCleared, data.totalBalloonsPopped,
                data.bestRunScore, data.bestRoomStreak);
    }
}
```

**Commit:** `feat(roguelite): add MetaProgressionManager and RunHistoryTracker`

---

## Task 14 — Integration Wiring and Verification

> Wire everything together in the scene hierarchy. Verify compilation.

- [ ] Ensure all scripts compile with zero errors
- [ ] Document the required scene hierarchy for RunManager and all UI

**Scene Hierarchy (to be set up in InkshotScene):**

```
RunManager (GameObject)
├── RunManager (component)
├── RoomGenerator (component)
├── PerkManager (component)
├── SaveManager (component)
├── MetaProgressionManager (component)
├── RunHistoryTracker (component)
│
├── RunUICanvas (Canvas — ScreenSpaceOverlay, sort order 200)
│   ├── CanvasScaler (ScaleWithScreenSize, 1080×1920)
│   ├── GraphicRaycaster
│   │
│   ├── FadeOverlay (Image — black, stretch all, CanvasGroup)
│   │   └── FadeOverlay (component)
│   │
│   ├── RunHUD (GameObject)
│   │   ├── RunHUD (component)
│   │   ├── RoomProgressText (TMP)
│   │   ├── InkIcon (Image)
│   │   ├── InkText (TMP)
│   │   └── PerkIconContainer (HorizontalLayoutGroup)
│   │
│   ├── RoomIntroScreen (GameObject)
│   │   ├── RoomIntroScreen (component)
│   │   ├── CanvasGroup
│   │   ├── RoomNumberText (TMP — size 72, bold)
│   │   ├── RoomNameText (TMP — size 48)
│   │   ├── TargetScoreText (TMP — size 36)
│   │   └── PerkIconContainer (HorizontalLayoutGroup)
│   │
│   ├── PerkSelectionScreen (GameObject)
│   │   ├── PerkSelectionScreen (component)
│   │   ├── CanvasGroup
│   │   ├── DarkOverlay (Image — black 80% alpha, stretch all)
│   │   ├── CardLeft (PerkCardUI prefab instance)
│   │   ├── CardCenter (PerkCardUI prefab instance)
│   │   └── CardRight (PerkCardUI prefab instance)
│   │
│   └── RunEndScreen (GameObject)
│       ├── RunEndScreen (component)
│       ├── CanvasGroup
│       ├── HeaderText (TMP — size 64)
│       ├── ScoreText (TMP)
│       ├── RoomsClearedText (TMP)
│       ├── InkEarnedText (TMP)
│       ├── BalloonsPoppedText (TMP)
│       ├── DartsThrownText (TMP)
│       ├── MaxComboText (TMP)
│       └── ContinueButton (Button + TMP "CONTINUE")
│
BalloonGameManager (existing — now per-room controller)
├── ScoreManager (existing)
├── GameHUD (existing)
└── DartLauncher (created at runtime)

BalloonWall (existing)
SlingshotInput (existing)
```

**PerkCard Prefab** (`Assets/Prefabs/UI/PerkCard.prefab`):
```
PerkCard (RectTransform 280×420)
├── PerkCardUI (component)
├── CanvasGroup
├── Button
├── Background (Image — dark, rounded)
├── Border (Image — outline, color set by rarity)
├── Icon (Image — 120×120, centered upper area)
├── NameText (TMP — size 28, bold, centered)
└── DescriptionText (TMP — size 20, centered, below name)
```

**PerkIcon Prefab** (`Assets/Prefabs/UI/PerkIcon.prefab`):
```
PerkIcon (RectTransform 48×48)
├── Image (sprite set at runtime)
```

**Verification checklist:**
- [ ] All scripts in `Assets/Scripts/BalloonGame/Roguelite/` compile without errors
- [ ] BalloonGameManager still works in standalone mode (no RunManager in scene)
- [ ] BalloonWall.GenerateWall() without arguments still produces the default 8×9 grid
- [ ] All SO types appear in the Create Asset menu under `INKSHOT/`
- [ ] RunManager can be added to the scene without null reference errors on Awake
- [ ] PerkModifiers.Reset() returns all values to defaults

**Commit:** `feat(roguelite): wire scene hierarchy and verify full compilation`

---

## Dependency Graph

```
Task 1  (SO Data Layer)
  ↓
Task 2  (Save System)
  ↓
Task 3  (Perk Effects) ──→ Task 4 (PerkModifiers)
  ↓                           ↓
Task 5  (PerkManager) ←──────┘
  ↓
Task 6  (Room Generation)
  ↓
Task 7  (RunManager + RunState)
  ↓
Task 8  (BalloonGameManager modifications)
  ↓
Task 9  (FadeOverlay)
  ↓
Task 10 (Perk Selection Screen)
  ↓
Task 11 (Room Intro + Run HUD)
  ↓
Task 12 (Run End Screen)
  ↓
Task 13 (Meta-Progression)
  ↓
Task 14 (Integration + Verification)
```

---

## Notes

- **RunManager replaces BalloonGameManager** as the top-level scene controller. BalloonGameManager is demoted to a per-room controller that receives a RoomConfig and fires room-level events (`OnRoomCleared`, `OnRoomFailed`).
- **Backward compatibility:** BalloonGameManager detects whether a RunManager exists. If not, it self-starts with default constants — preserving the ability to test a single room in isolation.
- **PerkModifiers is the bridge** between the perk system and game systems. Game code reads `PerkModifiers.DartSpeedMultiplier` instead of hardcoded values. Perk effects write to PerkModifiers. This keeps coupling out of game systems entirely.
- **Currency is "Ink"** throughout the codebase. UI displays a droplet icon next to the number.
- **All transitions use FadeOverlay** (CanvasGroup alpha lerp). No DOTween or complex animations in this plan.
- **RoomNames contains 35 entries** — enough for variety across many runs. Boss room always uses "THE FINAL ACT".
- **Perk selection never shows duplicates** within a single selection, and never shows perks the player already owns. If the pool is exhausted, selection is skipped.
- **Meta-progression UI** (the shop screen) is deferred to Plan 06. MetaProgressionManager provides the API; the shop screen will call it.
