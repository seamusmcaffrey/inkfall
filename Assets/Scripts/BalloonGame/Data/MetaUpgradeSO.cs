using UnityEngine;

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
    public PerkEffectType effectType = PerkEffectType.None;
    public float effectValue = 1f;
    public int effectIntValue;
}
