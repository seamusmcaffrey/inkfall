using UnityEngine;

public enum BalloonColor
{
    Red,
    Blue,
    Yellow,
    Green,
    Purple
}

public static class BalloonColorExtensions
{
    public static Color ToUnityColor(this BalloonColor color)
    {
        return color switch
        {
            BalloonColor.Red => new Color(0.95f, 0.12f, 0.18f),
            BalloonColor.Blue => new Color(0.15f, 0.45f, 0.95f),
            BalloonColor.Yellow => new Color(1.0f, 0.88f, 0.05f),
            BalloonColor.Green => new Color(0.12f, 0.85f, 0.35f),
            BalloonColor.Purple => new Color(0.65f, 0.15f, 0.90f),
            _ => Color.white
        };
    }

    public static string ToDisplayName(this BalloonColor color)
    {
        return color.ToString();
    }
}
