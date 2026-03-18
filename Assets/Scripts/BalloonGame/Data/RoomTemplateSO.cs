using UnityEngine;

/// <summary>
/// ScriptableObject definition for a room archetype.
/// </summary>
[CreateAssetMenu(fileName = "RoomTemplate", menuName = "INKSHOT/Rooms/Room Template")]
public class RoomTemplateSO : ScriptableObject
{
    [Header("Identity")]
    public string templateId = "room-template";
    public string displayName = "Neon Gallery";
    public RoomType roomType = RoomType.Normal;
    public bool isBonusEligible;

    [Header("Grid")]
    [Range(3, 10)] public int columns = GameConstants.BOARD_COLUMNS;
    [Range(3, 12)] public int rows = GameConstants.BOARD_ROWS;

    [Header("Targets")]
    public int baseTargetScore = GameConstants.BASE_TARGET_SCORE;
    public int baseDarts = GameConstants.STARTING_DARTS;
    [Range(0.2f, 1f)] public float targetPressure = 0.5f;
    [Range(0f, 0.2f)] public float depthPressureBonus = 0.1f;
    public int flatTargetBonus;

    [Header("Special Balloons")]
    public int minSpecials;
    public int maxSpecials = 2;
    public int minHazards;
    public int maxHazards = 1;

    [Header("Theme")]
    public Color roomAccent = new(0.11f, 0.82f, 0.89f, 1f);
}

public enum RoomType
{
    Normal,
    Bonus,
    Boss,
    Mystery,
}
