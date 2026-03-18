using UnityEngine;

public static partial class InkshotContentCatalog
{
    private static void BuildThrowAndPaintPerks()
    {
        CreatePerk("static-surge", "Static Surge", "Blue pops arc lightning into the nearest balloon. At x5 combo, the whole row discharges.", PerkFamily.ThrowMods, PerkRarity.Common, UIColors.InkCyan, perk =>
        {
            perk.roomUnlock = 2;
            perk.effectType = PerkEffectType.DartSpeed;
            perk.effectValue = 0.08f;
            perk.modifiers.launchSpeedBonus = 0.08f;
        });
        CreatePerk("acid-drip", "Acid Drip", "Red pops drip acid straight down. At x4 combo, the whole row below dissolves.", PerkFamily.PaintEngine, PerkRarity.Common, UIColors.ClearedGreen, perk =>
        {
            perk.roomUnlock = 2;
            perk.effectType = PerkEffectType.ComboExtend;
            perk.effectValue = 0.12f;
            perk.hasColorFocus = true;
            perk.colorFocus = BalloonColor.Red;
            perk.modifiers.colorFocusBonus = 0.12f;
        });
        CreatePerk("gold-glimmer", "Gold Glimmer", "Yellow pops flicker sideways and clip the rest of the lane.", PerkFamily.ReactiveFlow, PerkRarity.Common, UIColors.ComboGold, perk =>
        {
            perk.roomUnlock = 3;
            perk.effectType = PerkEffectType.GoldMultiplier;
            perk.effectValue = 0.12f;
            perk.modifiers.goldMultiplierBonus = 0.12f;
            perk.requiresUnlock = true;
            perk.requiredMetaUpgradeId = "scouted-prizes";
        });
        CreatePerk("violet-relay", "Violet Relay", "Purple pops echo across the wall and mirror a matching balloon.", PerkFamily.ThrowMods, PerkRarity.Uncommon, UIColors.EpicPurpleBorder, perk =>
        {
            perk.roomUnlock = 3;
            perk.effectType = PerkEffectType.ComboExtend;
            perk.effectValue = 0.1f;
            perk.hasColorFocus = true;
            perk.colorFocus = BalloonColor.Purple;
            perk.modifiers.colorFocusBonus = 0.14f;
            perk.requiresUnlock = true;
            perk.requiredMetaUpgradeId = "throw-mods-ledger";
        });
        CreatePerk("stormfront", "Stormfront", "Blue pops at x4 combo call a full-row lightning sweep.", PerkFamily.GeometryControl, PerkRarity.Uncommon, UIColors.DartBlue, perk =>
        {
            perk.roomUnlock = 4;
            perk.effectType = PerkEffectType.ComboExtend;
            perk.effectValue = 0.18f;
            perk.requiresUnlock = true;
            perk.requiredMetaUpgradeId = "throw-mods-ledger";
        });
    }
}
