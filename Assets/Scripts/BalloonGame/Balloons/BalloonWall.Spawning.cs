using System.Collections.Generic;
using UnityEngine;

public partial class BalloonWall
{
    private BalloonTypeSO PickBalloonType(int row, int column, List<Vector2Int> specialSlots, int specialCount, int hazardCount)
    {
        List<BalloonTypeSO> availableTypes = GetAvailableTypes();
        Vector2Int slot = new(column, row);
        bool isSpecialSlot = specialSlots.Contains(slot);

        if (isSpecialSlot)
        {
            int slotIndex = specialSlots.IndexOf(slot);
            BalloonSpecialType requestedType = slotIndex < hazardCount ? BalloonSpecialType.Hazard : BalloonSpecialType.Paint;
            List<BalloonTypeSO> specialTypes = availableTypes.FindAll(type =>
                type.specialType != BalloonSpecialType.Standard &&
                (requestedType == BalloonSpecialType.Hazard
                    ? type.specialType == BalloonSpecialType.Hazard
                    : type.specialType != BalloonSpecialType.Hazard));
            BalloonTypeSO specialType = PickWeightedType(specialTypes);
            if (specialType != null)
            {
                return specialType;
            }
        }

        BalloonColor color = _colors[Random.Range(0, _colors.Length)];
        BalloonTypeSO standardType = availableTypes.Find(type => type.specialType == BalloonSpecialType.Standard && type.balloonColor == color);
        return standardType ?? availableTypes[0];
    }

    private List<BalloonTypeSO> GetAvailableTypes()
    {
        var availableTypes = new List<BalloonTypeSO>();
        foreach (BalloonTypeSO type in GameConfigSO.Instance.balloonTypes)
        {
            if (type == null || !type.canSpawn || type.roomUnlock > (_roomConfig != null ? _roomConfig.roomNumber : 1))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(type.requiredMetaUpgradeId) && !SaveManager.Instance.Data.purchasedMetaUpgradeIds.Contains(type.requiredMetaUpgradeId))
            {
                continue;
            }

            if (_perkManager != null && _perkManager.FewerPrizeBalloons && type.specialType == BalloonSpecialType.Gold)
            {
                continue;
            }

            availableTypes.Add(type);
        }

        return availableTypes;
    }

    private StickerFamily ResolveStickerFamily(BalloonTypeSO type)
    {
        if (type != null && type.forcedStickerFamily != StickerFamily.None)
        {
            return type.forcedStickerFamily;
        }

        if (_runSystemsManager == null)
        {
            return StickerFamily.None;
        }

        float chance = GameConfigSO.Instance.baseStickerSpawnChance + _runSystemsManager.GetPurchasedMetaUpgrades().Count * 0.01f + _perkManager.StickerSpawnBonus;
        if (Random.value > Mathf.Clamp01(chance))
        {
            return StickerFamily.None;
        }

        IReadOnlyList<StickerFamily> families = _runSystemsManager.GetUnlockedStickerFamilies();
        return families.Count == 0 ? StickerFamily.None : families[Random.Range(0, families.Count)];
    }

    private static List<Vector2Int> PickUniqueSlots(int rows, int columns, int count)
    {
        var slots = new List<Vector2Int>(count);
        int safety = rows * columns * 2;
        while (slots.Count < count && safety-- > 0)
        {
            Vector2Int candidate = new(Random.Range(0, columns), Random.Range(0, rows));
            if (!slots.Contains(candidate))
            {
                slots.Add(candidate);
            }
        }

        return slots;
    }

    private static BalloonTypeSO PickWeightedType(List<BalloonTypeSO> types)
    {
        if (types == null || types.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        foreach (BalloonTypeSO type in types)
        {
            totalWeight += Mathf.Max(0.01f, type.spawnWeight);
        }

        float roll = Random.value * totalWeight;
        foreach (BalloonTypeSO type in types)
        {
            roll -= Mathf.Max(0.01f, type.spawnWeight);
            if (roll <= 0f)
            {
                return type;
            }
        }

        return types[types.Count - 1];
    }
}
