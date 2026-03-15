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
            BalloonColor.Red => new Color(1.0f, 0.08f, 0.15f),
            BalloonColor.Blue => new Color(0.08f, 0.35f, 1.0f),
            BalloonColor.Yellow => new Color(1.0f, 0.92f, 0.0f),
            BalloonColor.Green => new Color(0.0f, 0.92f, 0.30f),
            BalloonColor.Purple => new Color(0.72f, 0.08f, 1.0f),
            _ => Color.white
        };
    }

    public static string ToDisplayName(this BalloonColor color)
    {
        return color.ToString();
    }
}
