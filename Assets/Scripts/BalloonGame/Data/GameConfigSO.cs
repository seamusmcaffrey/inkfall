using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Global tuning config for INKSHOT.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "INKSHOT/Game Config")]
public class GameConfigSO : ScriptableObject
{
    private const string AssetPath = "Assets/ScriptableObjects/GameConfig.asset";
    private static GameConfigSO _instance;

    [Header("Input / Launch")]
    public float pullSpeedExponent = 3f;
    [Tooltip("Legacy: not used by aim-point model. Kept for test runner backward compat.")]
    public float minLaunchSpeed = 10f;
    [Tooltip("Legacy: not used by aim-point model. Kept for test runner backward compat.")]
    public float maxLaunchSpeed = 40f;
    public float minPreviewVelocity = 1f;
    public float aimAssistStrength = 0.55f;
    public bool aimAssistEnabled = true;

    [Header("Darts")]
    public float dartLifetimeSeconds = GameConstants.DEFAULT_DART_LIFETIME;
    public float defaultLaunchCooldown = 0.25f;
    public int defaultMaxRicochets = GameConstants.DEFAULT_MAX_RICOCHETS;
    public int defaultMaxPierce = GameConstants.DEFAULT_MAX_PIERCE;

    [Header("Scoring / Combo")]
    public int baseScorePerBalloon = GameConstants.SCORE_PER_BALLOON;
    public float comboWindowSeconds = GameConstants.COMBO_WINDOW_SECONDS;
    public float comboMultiplierPerStep = 0.28f;
    public int comboSoftCap = 6;
    public float comboMultiplierLateStep = 0.12f;
    public int comboMaxStack = GameConstants.COMBO_MAX_STACK;
    public float scoreMultiplier = 1f;

    [Header("Run Structure")]
    public int totalRooms = 12;
    public int bonusRoomInterval = 4;
    public float targetScoreScaling = 0.1f;
    public float roomTargetMultiplier = 0.5f;
    public float roomDepthScoreBonusPerRoom = 0.025f;
    public int inkPerRoom = 10;
    public int inkPer1000Score = 5;
    public float fullRunInkMultiplier = 2f;
    public int perkChoicesPerDraft = 3;
    public int recipeUnlockRoom = 4;

    [Header("Run Economy")]
    public int startingPrizeTickets = 2;
    public int perkRerollBaseCost = 2;
    public int perkRerollCostStep = 1;
    public int shopIntervalRooms = 3;
    public int shopOfferCount = 4;
    public int shopRefreshCost = 2;
    public int overflowScorePerTicket = 200;
    public int overflowScorePerFinisherCharge = 400;

    [Header("Reactive Flow")]
    public float finisherChargePerPop = 0.08f;
    public float finisherChargePerRecipe = 0.35f;
    public int finisherScoreBurstThreshold = 600;
    public float staticSurgeRange = 2.75f;
    public float sewerRatRange = 3.8f;
    public int chainParadeBonusChains = 1;
    public int burrowCrownRatCount = 2;
    public int stormfrontComboThreshold = 4;
    public int staticSurgeSweepComboThreshold = 5;
    public int causticFloodComboThreshold = 4;
    public int ratKingComboThreshold = 5;
    public int jackpotLaneComboThreshold = 5;
    public int jackpotLaneTicketBonus = 1;
    public int curtainCallComboThreshold = 6;
    public int prismSweepComboThreshold = 6;
    public int prismSweepUniqueColorsRequired = 3;

    [Header("Balloon Visuals")]
    [Tooltip("Imported 3D prefab to use for balloons. If null, falls back to procedural mesh.")]
    public GameObject balloonPrefabOverride;
    [Tooltip("Imported 3D mesh to use for balloons (without prefab). If null, falls back to procedural mesh.")]
    public Mesh balloonMeshOverride;

    [Header("Special Balloons")]
    public float defaultPaintRadius = GameConstants.DEFAULT_PAINT_RADIUS;
    public int goldBalloonBonus = 250;
    public int hazardBalloonPenalty = 150;
    [Range(0f, 1f)] public float baseStickerSpawnChance = 0.38f;
    [Range(0f, 1f)] public float baseChaosBalloonChance = 0.08f;
    public List<BalloonTypeSO> balloonTypes = new();

    [Header("Roguelite Content")]
    public List<RoomTemplateSO> roomTemplates = new();
    public List<PerkSO> perkPool = new();
    public List<PerkSO> keystoneRelics = new();
    public List<StarterLoadoutDefinition> starterLoadouts = new();
    public List<PaintRecipeDefinition> paintRecipes = new();
    public List<ComboFinisherDefinition> comboFinishers = new();
    public List<MetaUpgradeSO> metaUpgrades = new();

    public static GameConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = LoadAsset() ?? CreateInstance<GameConfigSO>();
                _instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }

            InkshotContentCatalog.EnsureHydrated(_instance);
            return _instance;
        }
    }

    public float GetRarityWeight(PerkRarity rarity)
    {
        return rarity switch
        {
            PerkRarity.Common => 1f,
            PerkRarity.Uncommon => 0.7f,
            PerkRarity.Rare => 0.35f,
            PerkRarity.Legendary => 0.12f,
            _ => 1f,
        };
    }

    private static GameConfigSO LoadAsset()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameConfigSO>(AssetPath);
#else
        return null;
#endif
    }
}
