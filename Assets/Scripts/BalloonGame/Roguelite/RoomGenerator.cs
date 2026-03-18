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

        return new RoomConfig
        {
            roomNumber = roomNumber,
            roomName = RoomNames.GetName(roomNumber),
            columns = template != null ? template.columns : GameConstants.BOARD_COLUMNS,
            rows = template != null ? template.rows : GameConstants.BOARD_ROWS,
            targetScore = RunScoreMath.EstimateRoomTarget(template, roomNumber, config),
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

        RoomTemplateSO openingTemplate = null;
        RoomTemplateSO bonusTemplate = null;
        RoomTemplateSO primaryNormalTemplate = null;
        foreach (RoomTemplateSO template in config.roomTemplates)
        {
            if (template == null)
            {
                continue;
            }

            if (openingTemplate == null && template.templateId == "opening-booth")
            {
                openingTemplate = template;
            }

            if (bonusTemplate == null && template.roomType == RoomType.Bonus)
            {
                bonusTemplate = template;
            }

            if (primaryNormalTemplate == null && template.roomType == RoomType.Normal && template.templateId != "opening-booth")
            {
                primaryNormalTemplate = template;
            }
        }

        if (roomNumber <= 1 && openingTemplate != null)
        {
            return openingTemplate;
        }

        bool isBonusRoom = bonusTemplate != null &&
            config.bonusRoomInterval > 0 &&
            roomNumber % config.bonusRoomInterval == 0;
        if (isBonusRoom || (roomNumber == config.totalRooms && bonusTemplate != null))
        {
            return bonusTemplate;
        }

        return primaryNormalTemplate ?? openingTemplate ?? config.roomTemplates[0];
    }
}
