using UnityEngine;

/// <summary>
/// Builds room configs from templates and difficulty scaling.
/// </summary>
[DisallowMultipleComponent]
public class RoomGenerator : MonoBehaviour
{
    public RoomConfig Generate(int roomNumber)
    {
        GameConfigSO config = GameConfigSO.Instance;
        RoomTemplateSO template = SelectTemplate(config, roomNumber);
        float scaling = 1f + Mathf.Max(0, roomNumber - 1) * config.targetScoreScaling;

        return new RoomConfig
        {
            roomNumber = roomNumber,
            roomName = RoomNames.GetName(roomNumber),
            columns = template != null ? template.columns : GameConstants.BOARD_COLUMNS,
            rows = template != null ? template.rows : GameConstants.BOARD_ROWS,
            targetScore = Mathf.RoundToInt((template != null ? template.baseTargetScore : GameConstants.BASE_TARGET_SCORE) * scaling),
            startingDarts = template != null ? template.baseDarts : GameConstants.STARTING_DARTS,
            minSpecials = template != null ? template.minSpecials : 0,
            maxSpecials = template != null ? template.maxSpecials : Mathf.Clamp(roomNumber / 2, 1, 4),
            minHazards = template != null ? template.minHazards : 0,
            maxHazards = template != null ? template.maxHazards + roomNumber / 4 : Mathf.Clamp(roomNumber / 3, 0, 4),
            roomType = template != null ? template.roomType : RoomType.Normal,
            accentColor = template != null ? template.roomAccent : new Color(0.14f, 0.72f, 0.84f, 1f),
            template = template,
        };
    }

    private static RoomTemplateSO SelectTemplate(GameConfigSO config, int roomNumber)
    {
        if (config.roomTemplates == null || config.roomTemplates.Count == 0)
        {
            return null;
        }

        return config.roomTemplates[(roomNumber - 1) % config.roomTemplates.Count];
    }
}
