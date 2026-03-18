using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Permanent meta progression upgrade definition.
/// </summary>
[CreateAssetMenu(fileName = "MetaUpgrade", menuName = "INKSHOT/Meta/Upgrade")]
public class MetaUpgradeSO : ScriptableObject
{
    public string upgradeId = "meta-upgrade";
    public string displayName = "Meta Upgrade";
    [TextArea(1, 3)] public string description = "Permanent run improvement";
    public Sprite icon;
    public int cost = 50;
    public string branchLabel = "Core";
    public List<string> prerequisiteIds = new();
    public List<string> unlockedLoadoutIds = new();
    public List<string> unlockedPerkIds = new();
    public List<string> unlockedRelicIds = new();
    public List<StickerFamily> unlockedStickerFamilies = new();
    public PerkEffectType effectType = PerkEffectType.None;
    public float effectValue = 1f;
    public int effectIntValue;
    public RunModifierBundle modifiers = new();
}
