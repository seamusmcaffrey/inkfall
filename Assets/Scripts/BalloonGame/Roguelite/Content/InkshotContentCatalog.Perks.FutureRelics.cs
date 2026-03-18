using UnityEngine;

public static partial class InkshotContentCatalog
{
    private static void BuildFutureAndRecoveryPerks()
    {
        CreatePerk("rat-king", "Rat King", "Green pops at x5 combo call a swarm that clears the lowest active row.", PerkFamily.SeedingFutureWall, PerkRarity.Rare, UIColors.RoomPink, perk =>
        {
            perk.roomUnlock = 5;
            perk.effectType = PerkEffectType.ComboExtend;
            perk.effectValue = 0.22f;
            perk.requiresUnlock = true;
            perk.requiredMetaUpgradeId = "skull-pack";
        });
        CreatePerk("curtain-call", "Curtain Call", "Purple pops at x6 combo pull the top row into one last sweep.", PerkFamily.PaintEngine, PerkRarity.Rare, UIColors.ComboGold, perk =>
        {
            perk.roomUnlock = 6;
            perk.effectType = PerkEffectType.ScoreMultiplier;
            perk.effectValue = 0.06f;
            perk.modifiers.scoreMultiplierBonus = 0.06f;
            perk.requiresUnlock = true;
            perk.requiredMetaUpgradeId = "recipe-atlas";
        });
    }

    private static void BuildRelics()
    {
        CreateRelic("chain-parade", "Chain Parade", "Every reaction hands off to one extra target.", UIColors.ComboGold, relic =>
        {
            relic.roomUnlock = 4;
            relic.effectType = PerkEffectType.ComboExtend;
            relic.effectValue = 0.2f;
            relic.requiresUnlock = true;
            relic.requiredMetaUpgradeId = "relic-shelves";
        });
        CreateRelic("monsoon-reel", "Monsoon Reel", "Every row clear keeps rolling and also washes the row beside it.", UIColors.InkCyan, relic =>
        {
            relic.roomUnlock = 4;
            relic.effectType = PerkEffectType.ComboExtend;
            relic.effectValue = 0.18f;
            relic.requiresUnlock = true;
            relic.requiredMetaUpgradeId = "relic-shelves";
        });
        CreateRelic("burrow-crown", "Burrow Crown", "Rats split in two and chew through double targets.", UIColors.RoomPink, relic =>
        {
            relic.roomUnlock = 8;
            relic.effectType = PerkEffectType.ComboExtend;
            relic.effectValue = 0.18f;
            relic.requiresUnlock = true;
            relic.requiredMetaUpgradeId = "keystone-annex";
        });
        CreateRelic("prism-sweep", "Prism Sweep", "At x6 combo with three colors in one shot, top and bottom rows vanish together.", UIColors.EpicPurpleBorder, relic =>
        {
            relic.roomUnlock = 8;
            relic.effectType = PerkEffectType.ComboExtend;
            relic.effectValue = 0.25f;
            relic.requiresUnlock = true;
            relic.requiredMetaUpgradeId = "keystone-annex";
        });
    }
}
