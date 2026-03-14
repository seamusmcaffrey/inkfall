using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles permanent unlocks purchased with Ink.
/// </summary>
[DisallowMultipleComponent]
public class MetaProgressionManager : MonoBehaviour
{
    public IReadOnlyList<MetaUpgradeSO> GetAvailableUpgrades()
    {
        return GameConfigSO.Instance.metaUpgrades;
    }

    public bool IsUnlocked(MetaUpgradeSO upgrade)
    {
        return upgrade != null && SaveManager.Instance.Data.purchasedMetaUpgradeIds.Contains(upgrade.upgradeId);
    }

    public bool Purchase(MetaUpgradeSO upgrade)
    {
        if (upgrade == null || IsUnlocked(upgrade))
        {
            return false;
        }

        if (!SaveManager.Instance.SpendInk(upgrade.cost))
        {
            return false;
        }

        SaveManager.Instance.Data.purchasedMetaUpgradeIds.Add(upgrade.upgradeId);
        SaveManager.Instance.Save();
        return true;
    }
}
