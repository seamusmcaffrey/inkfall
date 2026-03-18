using System.Collections.Generic;
using UnityEngine;

public static partial class InkshotContentCatalog
{
    private static bool _isBuilt;
    private static readonly List<BalloonTypeSO> BalloonTypes = new();
    private static readonly List<PerkSO> Perks = new();
    private static readonly List<PerkSO> Relics = new();
    private static readonly List<StarterLoadoutDefinition> Loadouts = new();
    private static readonly List<PaintRecipeDefinition> Recipes = new();
    private static readonly List<ComboFinisherDefinition> Finishers = new();
    private static readonly List<MetaUpgradeSO> MetaUpgrades = new();
    private static readonly Dictionary<string, PerkSO> PerksById = new();
    private static readonly Dictionary<string, MetaUpgradeSO> MetaById = new();

    public static void EnsureHydrated(GameConfigSO config)
    {
        if (config == null)
        {
            return;
        }

        if (!_isBuilt)
        {
            BuildCatalog();
        }

        config.balloonTypes = BalloonTypes;
        config.perkPool = Perks;
        config.keystoneRelics = Relics;
        config.starterLoadouts = Loadouts;
        config.paintRecipes = Recipes;
        config.comboFinishers = Finishers;
        config.metaUpgrades = MetaUpgrades;
    }

    public static PerkSO GetPerkById(string perkId)
    {
        if (string.IsNullOrEmpty(perkId))
        {
            return null;
        }

        if (!_isBuilt)
        {
            BuildCatalog();
        }

        return PerksById.TryGetValue(perkId, out PerkSO perk) ? perk : null;
    }

    public static MetaUpgradeSO GetMetaById(string upgradeId)
    {
        if (string.IsNullOrEmpty(upgradeId))
        {
            return null;
        }

        if (!_isBuilt)
        {
            BuildCatalog();
        }

        return MetaById.TryGetValue(upgradeId, out MetaUpgradeSO upgrade) ? upgrade : null;
    }

    private static void BuildCatalog()
    {
        BalloonTypes.Clear();
        Perks.Clear();
        Relics.Clear();
        Loadouts.Clear();
        Recipes.Clear();
        Finishers.Clear();
        MetaUpgrades.Clear();
        PerksById.Clear();
        MetaById.Clear();

        BuildBalloonTypes();
        BuildPerks();
        BuildRelics();
        BuildLoadouts();
        BuildRecipes();
        BuildFinishers();
        BuildMetaUpgrades();
        _isBuilt = true;
    }

    private static T CreateRuntimeAsset<T>(string name) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        asset.name = name;
        asset.hideFlags = HideFlags.DontUnloadUnusedAsset;
        return asset;
    }

    private static BalloonTypeSO CreateBalloonType(string id, string name, BalloonColor color,
        BalloonSpecialType specialType, int basePoints, float weight, System.Action<BalloonTypeSO> configure = null)
    {
        BalloonTypeSO balloonType = CreateRuntimeAsset<BalloonTypeSO>(name);
        balloonType.typeId = id;
        balloonType.displayName = name;
        balloonType.balloonColor = color;
        balloonType.specialType = specialType;
        balloonType.basePoints = basePoints;
        balloonType.spawnWeight = weight;
        balloonType.canSpawn = true;
        configure?.Invoke(balloonType);
        BalloonTypes.Add(balloonType);
        return balloonType;
    }

    private static PerkSO CreatePerk(string id, string name, string description, PerkFamily family,
        PerkRarity rarity, Color accentColor, System.Action<PerkSO> configure = null)
    {
        PerkSO perk = CreateRuntimeAsset<PerkSO>(name);
        perk.perkId = id;
        perk.perkName = name;
        perk.description = description;
        perk.family = family;
        perk.rarity = rarity;
        perk.accentColor = accentColor;
        perk.shopCost = rarity switch
        {
            PerkRarity.Common => 3,
            PerkRarity.Uncommon => 4,
            PerkRarity.Rare => 5,
            _ => 6,
        };
        configure?.Invoke(perk);
        Perks.Add(perk);
        PerksById[id] = perk;
        return perk;
    }

    private static PerkSO CreateRelic(string id, string name, string description, Color accentColor,
        System.Action<PerkSO> configure = null)
    {
        PerkSO relic = CreatePerk(id, name, description, PerkFamily.Keystone, PerkRarity.Legendary, accentColor, perk =>
        {
            perk.isKeystone = true;
            perk.shopCost = 9;
            configure?.Invoke(perk);
        });
        Relics.Add(relic);
        Perks.Remove(relic);
        return relic;
    }

    private static MetaUpgradeSO CreateMetaUpgrade(string id, string name, string description, int cost,
        string branchLabel, System.Action<MetaUpgradeSO> configure = null)
    {
        MetaUpgradeSO upgrade = CreateRuntimeAsset<MetaUpgradeSO>(name);
        upgrade.upgradeId = id;
        upgrade.displayName = name;
        upgrade.description = description;
        upgrade.cost = cost;
        upgrade.branchLabel = branchLabel;
        configure?.Invoke(upgrade);
        MetaUpgrades.Add(upgrade);
        MetaById[id] = upgrade;
        return upgrade;
    }

    private static StarterLoadoutDefinition CreateLoadout(string id, string name, string description,
        string tradeoff, Color accentColor, System.Action<StarterLoadoutDefinition> configure = null)
    {
        var loadout = new StarterLoadoutDefinition
        {
            loadoutId = id,
            displayName = name,
            description = description,
            tradeoff = tradeoff,
            accentColor = accentColor,
        };
        configure?.Invoke(loadout);
        Loadouts.Add(loadout);
        return loadout;
    }

    private static PaintRecipeDefinition CreateRecipe(string id, string name, string description,
        BalloonColor primary, BalloonColor secondary, PaintRecipeEffectType effectType, int power, float radius = 0f)
    {
        var recipe = new PaintRecipeDefinition
        {
            recipeId = id,
            displayName = name,
            description = description,
            primaryColor = primary,
            secondaryColor = secondary,
            effectType = effectType,
            power = power,
            radius = radius <= 0f ? GameConstants.DEFAULT_PAINT_RADIUS : radius,
        };
        Recipes.Add(recipe);
        return recipe;
    }

    private static ComboFinisherDefinition CreateFinisher(string id, string name, string description,
        FinisherEffectType effectType, int minPopCount, int minScoreBurst, int minRecipeCount, int minStickerCount,
        float chargeCost, int cooldownShots, int power, float radius)
    {
        var finisher = new ComboFinisherDefinition
        {
            finisherId = id,
            displayName = name,
            description = description,
            effectType = effectType,
            minPopCount = minPopCount,
            minScoreBurst = minScoreBurst,
            minRecipeCount = minRecipeCount,
            minStickerCount = minStickerCount,
            chargeCost = chargeCost,
            cooldownShots = cooldownShots,
            power = power,
            radius = radius,
        };
        Finishers.Add(finisher);
        return finisher;
    }

    private static void BuildBalloonTypes()
    {
        foreach (BalloonColor color in new[]
        {
            BalloonColor.Red, BalloonColor.Blue, BalloonColor.Yellow, BalloonColor.Green, BalloonColor.Purple
        })
        {
            CreateBalloonType(
                $"standard-{color.ToString().ToLowerInvariant()}",
                $"{color.ToDisplayName()} Balloon",
                color,
                BalloonSpecialType.Standard,
                GameConstants.SCORE_PER_BALLOON,
                1f);
        }

        CreateBalloonType("paint", "Paint Balloon", BalloonColor.Blue, BalloonSpecialType.Paint, 135, 0.48f, type =>
        {
            type.effectRadius = GameConstants.DEFAULT_PAINT_RADIUS;
        });
        CreateBalloonType("gold", "Gold Balloon", BalloonColor.Yellow, BalloonSpecialType.Gold, 160, 0.26f, type =>
        {
            type.currencyReward = 2;
        });
        CreateBalloonType("hazard", "Hazard Balloon", BalloonColor.Purple, BalloonSpecialType.Hazard, -150, 0.2f, type =>
        {
            type.hazardPenalty = 160;
            type.endsComboOnPop = true;
        });
        CreateBalloonType("mixer", "Mixer Balloon", BalloonColor.Blue, BalloonSpecialType.Mixer, 140, 0.1f, type =>
        {
            type.isChaosBalloon = true;
            type.roomUnlock = 3;
            type.requiredMetaUpgradeId = "chaos-permit";
        });
        CreateBalloonType("invert", "Invert Balloon", BalloonColor.Purple, BalloonSpecialType.Invert, 155, 0.08f, type =>
        {
            type.isChaosBalloon = true;
            type.roomUnlock = 5;
            type.requiredMetaUpgradeId = "paint-lab";
        });
        CreateBalloonType("wash", "Wash Balloon", BalloonColor.Green, BalloonSpecialType.Wash, 130, 0.08f, type =>
        {
            type.isChaosBalloon = true;
            type.roomUnlock = 3;
            type.requiredMetaUpgradeId = "chaos-permit";
        });
        CreateBalloonType("clone", "Clone Balloon", BalloonColor.Red, BalloonSpecialType.Clone, 150, 0.08f, type =>
        {
            type.isChaosBalloon = true;
            type.roomUnlock = 4;
            type.requiredMetaUpgradeId = "chaos-permit";
        });
        CreateBalloonType("rainbow", "Rainbow Balloon", BalloonColor.Yellow, BalloonSpecialType.Rainbow, 180, 0.06f, type =>
        {
            type.isChaosBalloon = true;
            type.roomUnlock = 6;
            type.requiredMetaUpgradeId = "paint-lab";
        });
    }
}
