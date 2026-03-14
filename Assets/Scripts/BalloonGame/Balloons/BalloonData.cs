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
            BalloonColor.Red => new Color(0.90f, 0.18f, 0.22f),
            BalloonColor.Blue => new Color(0.20f, 0.50f, 0.85f),
            BalloonColor.Yellow => new Color(0.95f, 0.82f, 0.12f),
            BalloonColor.Green => new Color(0.18f, 0.78f, 0.42f),
            BalloonColor.Purple => new Color(0.58f, 0.22f, 0.82f),
            _ => Color.white
        };
    }

    public static string ToDisplayName(this BalloonColor color)
    {
        return color.ToString();
    }
}
