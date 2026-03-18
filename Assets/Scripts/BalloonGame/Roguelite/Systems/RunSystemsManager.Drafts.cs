using System.Collections.Generic;
using UnityEngine;

public partial class RunSystemsManager
{
    public int GetPerkRerollCost()
    {
        return _freeRerollsRemaining > 0
            ? 0
            : GameConfigSO.Instance.perkRerollBaseCost + _rerollsUsedThisRoom * GameConfigSO.Instance.perkRerollCostStep;
    }

    public bool ConsumePerkReroll()
    {
        if (_freeRerollsRemaining > 0)
        {
            _freeRerollsRemaining--;
            _rerollsUsedThisRoom++;
            return true;
        }

        if (!TrySpendTickets(GetPerkRerollCost()))
        {
            return false;
        }

        _rerollsUsedThisRoom++;
        return true;
    }

    public List<PerkSO> GetPerkChoices(int roomNumber)
    {
        var pool = new List<PerkSO>();
        foreach (PerkSO perk in GameConfigSO.Instance.perkPool)
        {
            if (perk == null || perk.roomUnlock > roomNumber)
            {
                continue;
            }

            if (perk.requiresUnlock && !IsUnlocked(perk.requiredMetaUpgradeId))
            {
                continue;
            }

            bool alreadyOwned = false;
            foreach (PerkSO activePerk in _perkManager.ActivePerks)
            {
                if (activePerk == perk)
                {
                    alreadyOwned = true;
                    break;
                }
            }

            if (!alreadyOwned)
            {
                pool.Add(perk);
            }
        }

        return DrawPerks(pool, GameConfigSO.Instance.perkChoicesPerDraft);
    }

    public List<PerkSO> GetRelicChoices(int roomNumber)
    {
        var pool = new List<PerkSO>();
        foreach (PerkSO relic in GameConfigSO.Instance.keystoneRelics)
        {
            if (relic == null || relic.roomUnlock > roomNumber)
            {
                continue;
            }

            if ((!relic.requiresUnlock || IsUnlocked(relic.requiredMetaUpgradeId)) && !_activeRelics.Contains(relic))
            {
                pool.Add(relic);
            }
        }

        return DrawPerks(pool, 3);
    }

    public bool ShouldOfferRelic(int roomNumber)
    {
        return roomNumber > 1 && roomNumber % 4 == 0;
    }

    private static List<PerkSO> DrawPerks(List<PerkSO> pool, int count)
    {
        var picks = new List<PerkSO>();
        int safety = 64;
        while (picks.Count < count && pool.Count > 0 && safety-- > 0)
        {
            float totalWeight = 0f;
            foreach (PerkSO perk in pool)
            {
                totalWeight += GameConfigSO.Instance.GetRarityWeight(perk.rarity);
            }

            float roll = Random.value * Mathf.Max(0.01f, totalWeight);
            float cursor = 0f;
            for (int index = 0; index < pool.Count; index++)
            {
                cursor += GameConfigSO.Instance.GetRarityWeight(pool[index].rarity);
                if (cursor < roll && index < pool.Count - 1)
                {
                    continue;
                }

                picks.Add(pool[index]);
                pool.RemoveAt(index);
                break;
            }
        }

        return picks;
    }
}
