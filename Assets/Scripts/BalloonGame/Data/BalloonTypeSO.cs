using UnityEngine;

/// <summary>
/// ScriptableObject definition for a balloon type.
/// </summary>
[CreateAssetMenu(fileName = "BalloonType", menuName = "INKSHOT/Balloons/Balloon Type")]
public class BalloonTypeSO : ScriptableObject
{
    [Header("Identity")]
    public string typeId = "standard-red";
    public string displayName = "Standard";
    public BalloonColor balloonColor = BalloonColor.Red;
    public BalloonSpecialType specialType = BalloonSpecialType.Standard;
    public Sprite icon;

    [Header("Scoring")]
    public int basePoints = GameConstants.SCORE_PER_BALLOON;
    public float scoreMultiplier = 1f;
    public int currencyReward;
    public int hazardPenalty;

    [Header("Gameplay")]
    public bool canSpawn = true;
    [Range(0f, 1f)] public float spawnWeight = 1f;
    public float effectRadius = GameConstants.DEFAULT_PAINT_RADIUS;
    public int durability = 1;
    public bool endsComboOnPop;
    public bool isChaosBalloon;
    public int roomUnlock = 1;
    public string requiredMetaUpgradeId;
    public StickerFamily forcedStickerFamily = StickerFamily.None;

    [Header("Rendering")]
    public Material materialOverride;
    public Color tintOverride = Color.clear;

    public int ResolvedPoints => Mathf.RoundToInt(basePoints * Mathf.Max(0.1f, scoreMultiplier));
}

public enum BalloonSpecialType
{
    Standard,
    Paint,
    Gold,
    Hazard,
    Shield,
    Mixer,
    Invert,
    Wash,
    Clone,
    Rainbow,
}
