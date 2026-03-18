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
        if (!CanPurchase(upgrade, out _))
        {
            return false;
        }

        SaveManager.Instance.SpendInk(upgrade.cost);
        SaveManager.Instance.Data.purchasedMetaUpgradeIds.Add(upgrade.upgradeId);
        SaveManager.Instance.Save();
        return true;
    }

    public bool CanPurchase(MetaUpgradeSO upgrade, out string reason)
    {
        if (upgrade == null)
        {
            reason = "Missing upgrade.";
            return false;
        }

        if (IsUnlocked(upgrade))
        {
            reason = "Already unlocked.";
            return false;
        }

        foreach (string prerequisiteId in upgrade.prerequisiteIds)
        {
            if (!SaveManager.Instance.Data.purchasedMetaUpgradeIds.Contains(prerequisiteId))
            {
                MetaUpgradeSO prerequisite = InkshotContentCatalog.GetMetaById(prerequisiteId);
                reason = prerequisite != null
                    ? $"Requires {prerequisite.displayName}."
                    : "Missing prerequisite.";
                return false;
            }
        }

        if (SaveManager.Instance.Data.totalInk < upgrade.cost)
        {
            reason = $"Need {upgrade.cost} Ink.";
            return false;
        }

        reason = "Purchase unlock.";
        return true;
    }
}
