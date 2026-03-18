using UnityEngine;

/// <summary>
/// ScriptableObject definition for a perk.
/// </summary>
[CreateAssetMenu(fileName = "Perk", menuName = "INKSHOT/Perks/Perk")]
public class PerkSO : ScriptableObject
{
    [Header("Identity")]
    public string perkId = "perk-id";
    public string perkName = "Perk Name";
    [TextArea(1, 3)] public string description = "Perk description";
    public Sprite icon;

    [Header("Classification")]
    public PerkFamily family = PerkFamily.ThrowMods;
    public PerkRarity rarity = PerkRarity.Common;
    public PerkEffectType effectType = PerkEffectType.None;
    public bool isPassive = true;
    public bool isKeystone;
    public bool isStarterExclusive;
    public int shopCost = 3;
    public int roomUnlock = 1;

    [Header("Effect Parameters")]
    public float effectValue = 1f;
    public float effectValueSecondary;
    public int effectIntValue;

    [Header("Run Focus")]
    public bool hasColorFocus;
    public BalloonColor colorFocus = BalloonColor.Red;
    public bool hasStickerFocus;
    public StickerFamily stickerFocus = StickerFamily.None;
    public RunModifierBundle modifiers = new();

    [Header("Unlocking")]
    public bool requiresUnlock;
    public string requiredMetaUpgradeId;
    public int unlockCost;

    [Header("Presentation")]
    public Color accentColor = Color.white;
}
