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
    public float comboMultiplierPerStep = GameConstants.COMBO_MULTIPLIER_PER_STEP;
    public int comboMaxStack = GameConstants.COMBO_MAX_STACK;
    public float scoreMultiplier = 1f;

    [Header("Run Structure")]
    public int totalRooms = 13;
    public int bonusRoomInterval = 4;
    public float targetScoreScaling = 0.15f;
    public int inkPerRoom = 10;
    public int inkPer1000Score = 5;
    public float fullRunInkMultiplier = 2f;
    public int perkChoicesPerDraft = 3;

    [Header("Balloon Visuals")]
    [Tooltip("Imported 3D prefab to use for balloons. If null, falls back to procedural mesh.")]
    public GameObject balloonPrefabOverride;
    [Tooltip("Imported 3D mesh to use for balloons (without prefab). If null, falls back to procedural mesh.")]
    public Mesh balloonMeshOverride;

    [Header("Special Balloons")]
    public float defaultPaintRadius = GameConstants.DEFAULT_PAINT_RADIUS;
    public int goldBalloonBonus = 250;
    public int hazardBalloonPenalty = 150;
    public List<BalloonTypeSO> balloonTypes = new();

    [Header("Roguelite Content")]
    public List<RoomTemplateSO> roomTemplates = new();
    public List<PerkSO> perkPool = new();
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
