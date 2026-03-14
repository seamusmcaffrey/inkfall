/// <summary>
/// Flavor name table for generated rooms.
/// </summary>
public static class RoomNames
{
    private static readonly string[] Names =
    {
        "Velvet Midway",
        "Chrome Arcade",
        "Blue Static Hall",
        "Sour Lights Gallery",
        "Glass Lantern Run",
        "Rain-Slick Alley",
        "Afterhours Booth",
        "Golden Carousel",
    };

    public static string GetName(int roomNumber)
    {
        if (Names.Length == 0)
        {
            return $"Room {roomNumber}";
        }

        return Names[(roomNumber - 1) % Names.Length];
    }
}
