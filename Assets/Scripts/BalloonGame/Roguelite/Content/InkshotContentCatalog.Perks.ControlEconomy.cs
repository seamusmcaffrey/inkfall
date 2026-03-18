using UnityEngine;

public static partial class InkshotContentCatalog
{
    private static void BuildControlAndEconomyPerks()
    {
        CreatePerk("sewer-rats", "Sewer Rats", "Green pops loose a rat that chases down another balloon.", PerkFamily.SeedingFutureWall, PerkRarity.Uncommon, UIColors.RoomPink, perk =>
        {
            perk.roomUnlock = 3;
            perk.effectType = PerkEffectType.ComboExtend;
            perk.effectValue = 0.12f;
            perk.requiresUnlock = true;
            perk.requiredMetaUpgradeId = "skull-pack";
        });
        CreatePerk("caustic-flood", "Caustic Flood", "Red pops at x4 combo melt the entire row below.", PerkFamily.PaintEngine, PerkRarity.Rare, UIColors.InkCyan, perk =>
        {
            perk.roomUnlock = 5;
            perk.effectType = PerkEffectType.ComboExtend;
            perk.effectValue = 0.22f;
            perk.requiresUnlock = true;
            perk.requiredMetaUpgradeId = "paint-lab";
        });
        CreatePerk("jackpot-lane", "Jackpot Lane", "Yellow pops at x5 combo cash out a ticket and wipe the lane.", PerkFamily.ReactiveFlow, PerkRarity.Rare, UIColors.ComboGold, perk =>
        {
            perk.roomUnlock = 5;
            perk.effectType = PerkEffectType.ScoreMultiplier;
            perk.effectValue = 0.08f;
            perk.modifiers.scoreMultiplierBonus = 0.08f;
            perk.requiresUnlock = true;
            perk.requiredMetaUpgradeId = "economy-ledger";
        });
    }
}
