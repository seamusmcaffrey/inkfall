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
    public PerkRarity rarity = PerkRarity.Common;
    public PerkEffectType effectType = PerkEffectType.None;
    public bool isPassive = true;

    [Header("Effect Parameters")]
    public float effectValue = 1f;
    public float effectValueSecondary;
    public int effectIntValue;

    [Header("Unlocking")]
    public bool requiresUnlock;
    public int unlockCost;

    [Header("Presentation")]
    public Color accentColor = Color.white;
}
