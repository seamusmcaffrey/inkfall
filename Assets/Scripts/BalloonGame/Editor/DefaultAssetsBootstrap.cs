#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates the default INKSHOT ScriptableObject assets the first time the editor imports the project.
/// </summary>
[InitializeOnLoad]
public static class DefaultAssetsBootstrap
{
    private const string RootFolder = "Assets/ScriptableObjects";
    private const string BalloonFolder = RootFolder + "/BalloonTypes";
    private const string PerkFolder = RootFolder + "/Perks";
    private const string RoomFolder = RootFolder + "/Rooms";
    private const string MetaFolder = RootFolder + "/Meta";
    private const string GameConfigPath = RootFolder + "/GameConfig.asset";
    private const string UIConfigPath = RootFolder + "/UIConfig.asset";
    private const string JuiceConfigPath = RootFolder + "/JuiceConfig.asset";
    private const string SoundLibraryPath = RootFolder + "/SoundLibrary.asset";

    static DefaultAssetsBootstrap()
    {
        EditorApplication.delayCall += EnsureDefaults;
    }

    [MenuItem("INKSHOT/Create Default Assets")]
    public static void EnsureDefaults()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        EnsureFolder(RootFolder);
        EnsureFolder(BalloonFolder);
        EnsureFolder(PerkFolder);
        EnsureFolder(RoomFolder);
        EnsureFolder(MetaFolder);

        var balloonTypes = new List<BalloonTypeSO>
        {
            CreateBalloon("StandardRed", "standard-red", "Red Balloon", BalloonColor.Red, BalloonSpecialType.Standard, GameConstants.SCORE_PER_BALLOON),
            CreateBalloon("StandardBlue", "standard-blue", "Blue Balloon", BalloonColor.Blue, BalloonSpecialType.Standard, GameConstants.SCORE_PER_BALLOON),
            CreateBalloon("StandardYellow", "standard-yellow", "Yellow Balloon", BalloonColor.Yellow, BalloonSpecialType.Standard, GameConstants.SCORE_PER_BALLOON),
            CreateBalloon("StandardGreen", "standard-green", "Green Balloon", BalloonColor.Green, BalloonSpecialType.Standard, GameConstants.SCORE_PER_BALLOON),
            CreateBalloon("StandardPurple", "standard-purple", "Purple Balloon", BalloonColor.Purple, BalloonSpecialType.Standard, GameConstants.SCORE_PER_BALLOON),
            CreateBalloon("PaintBalloon", "paint", "Paint Balloon", BalloonColor.Blue, BalloonSpecialType.Paint, GameConstants.SCORE_PER_BALLOON + 25, asset =>
            {
                asset.effectRadius = GameConstants.DEFAULT_PAINT_RADIUS;
            }),
            CreateBalloon("GoldBalloon", "gold", "Gold Balloon", BalloonColor.Yellow, BalloonSpecialType.Gold, GameConstants.SCORE_PER_BALLOON + 50, asset =>
            {
                asset.currencyReward = 3;
            }),
            CreateBalloon("HazardBalloon", "hazard", "Hazard Balloon", BalloonColor.Purple, BalloonSpecialType.Hazard, -150, asset =>
            {
                asset.hazardPenalty = 150;
                asset.endsComboOnPop = true;
            }),
        };

        var perks = new List<PerkSO>
        {
            CreatePerk("SwiftHands", "swift-hands", "Swift Hands", "Launch darts faster.", PerkRarity.Common, PerkEffectType.DartSpeed, accentColor: UIColors.InkCyan, effectValue: 0.15f),
            CreatePerk("NeedleThread", "needle-thread", "Needle Thread", "+1 dart pierce.", PerkRarity.Uncommon, PerkEffectType.DartPierce, accentColor: UIColors.ComboGold, effectIntValue: 1),
            CreatePerk("WetWall", "wet-wall", "Wet Wall", "Paint explosions reach farther.", PerkRarity.Uncommon, PerkEffectType.PaintRadius, accentColor: UIColors.DartBlue, effectValue: 0.35f),
            CreatePerk("DoubleDown", "double-down", "Double Down", "Flat score multiplier up.", PerkRarity.Rare, PerkEffectType.ScoreMultiplier, accentColor: UIColors.RoomPink, effectValue: 0.2f),
            CreatePerk("Insurance", "insurance", "Insurance", "Arm a hazard shield each room.", PerkRarity.Rare, PerkEffectType.HazardShield, accentColor: UIColors.ClearedGreen, effectValue: 1f),
        };

        var rooms = new List<RoomTemplateSO>
        {
            CreateRoom("OpeningBooth", "opening-booth", "Opening Booth", RoomType.Normal, 8, 9, GameConstants.BASE_TARGET_SCORE, GameConstants.STARTING_DARTS, 1, 2, 0, 1, UIColors.InkCyan),
            CreateRoom("ChromeMidway", "chrome-midway", "Chrome Midway", RoomType.Normal, 8, 9, 3600, GameConstants.STARTING_DARTS, 2, 3, 1, 1, UIColors.RoomPink),
            CreateRoom("PrizeChamber", "prize-chamber", "Prize Chamber", RoomType.Bonus, 7, 8, 3200, GameConstants.STARTING_DARTS + 1, 2, 4, 0, 0, UIColors.ComboGold, isBonusEligible: true),
        };

        CreateAssetIfMissing<MetaUpgradeSO>(MetaFolder + "/StarterKit.asset", asset =>
        {
            asset.upgradeId = "starter-kit";
            asset.displayName = "Starter Kit";
            asset.description = "Start each run with +1 dart.";
            asset.effectType = PerkEffectType.ExtraDart;
            asset.effectIntValue = 1;
            asset.cost = 75;
        });

        GameConfigSO gameConfig = CreateAssetIfMissing<GameConfigSO>(GameConfigPath, _ => { });
        gameConfig.balloonTypes = balloonTypes;
        gameConfig.perkPool = perks;
        gameConfig.roomTemplates = rooms;
        EditorUtility.SetDirty(gameConfig);

        CreateAssetIfMissing<UIConfigSO>(UIConfigPath, _ => { });
        CreateAssetIfMissing<JuiceConfigSO>(JuiceConfigPath, _ => { });

        SoundLibrarySO soundLibrary = CreateAssetIfMissing<SoundLibrarySO>(SoundLibraryPath, _ => { });
        SoundLibraryBootstrap.EnsurePopulated(soundLibrary);
        EditorUtility.SetDirty(soundLibrary);

        AssetDatabase.SaveAssets();
    }

    private static BalloonTypeSO CreateBalloon(string fileName, string typeId, string displayName, BalloonColor color, BalloonSpecialType specialType, int basePoints, System.Action<BalloonTypeSO> configure = null)
    {
        return CreateAssetIfMissing<BalloonTypeSO>($"{BalloonFolder}/{fileName}.asset", asset =>
        {
            asset.typeId = typeId;
            asset.displayName = displayName;
            asset.balloonColor = color;
            asset.specialType = specialType;
            asset.basePoints = basePoints;
            asset.spawnWeight = 1f;
            asset.canSpawn = true;
            configure?.Invoke(asset);
        });
    }

    private static PerkSO CreatePerk(string fileName, string perkId, string perkName, string description, PerkRarity rarity, PerkEffectType effectType, Color accentColor, float effectValue = 0f, int effectIntValue = 0)
    {
        return CreateAssetIfMissing<PerkSO>($"{PerkFolder}/{fileName}.asset", asset =>
        {
            asset.perkId = perkId;
            asset.perkName = perkName;
            asset.description = description;
            asset.rarity = rarity;
            asset.effectType = effectType;
            asset.effectValue = effectValue;
            asset.effectIntValue = effectIntValue;
            asset.accentColor = accentColor;
        });
    }

    private static RoomTemplateSO CreateRoom(string fileName, string templateId, string displayName, RoomType roomType, int columns, int rows, int targetScore, int baseDarts, int minSpecials, int maxSpecials, int minHazards, int maxHazards, Color accent, bool isBonusEligible = false)
    {
        return CreateAssetIfMissing<RoomTemplateSO>($"{RoomFolder}/{fileName}.asset", asset =>
        {
            asset.templateId = templateId;
            asset.displayName = displayName;
            asset.roomType = roomType;
            asset.columns = columns;
            asset.rows = rows;
            asset.baseTargetScore = targetScore;
            asset.baseDarts = baseDarts;
            asset.minSpecials = minSpecials;
            asset.maxSpecials = maxSpecials;
            asset.minHazards = minHazards;
            asset.maxHazards = maxHazards;
            asset.roomAccent = accent;
            asset.isBonusEligible = isBonusEligible;
        });
    }

    private static T CreateAssetIfMissing<T>(string path, System.Action<T> initialize) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            initialize?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, path);
        }
        else
        {
            initialize?.Invoke(asset);
        }

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int index = 1; index < parts.Length; index++)
        {
            string next = $"{current}/{parts[index]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[index]);
            }

            current = next;
        }
    }
}
#endif
