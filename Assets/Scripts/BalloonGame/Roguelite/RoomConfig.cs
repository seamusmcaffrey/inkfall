using UnityEngine;

/// <summary>
/// Runtime configuration for a generated room.
/// </summary>
public class RoomConfig
{
    public int roomNumber;
    public string roomName;
    public int columns;
    public int rows;
    public int targetScore;
    public int startingDarts;
    public int minSpecials;
    public int maxSpecials;
    public int minHazards;
    public int maxHazards;
    public RoomType roomType;
    public Color accentColor;
    public RoomTemplateSO template;
}
