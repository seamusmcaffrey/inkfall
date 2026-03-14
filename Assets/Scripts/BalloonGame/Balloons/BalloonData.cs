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
            BalloonColor.Red => new Color(0.902f, 0.224f, 0.275f),
            BalloonColor.Blue => new Color(0.271f, 0.482f, 0.616f),
            BalloonColor.Yellow => new Color(0.945f, 0.980f, 0.933f),
            BalloonColor.Green => new Color(0.165f, 0.616f, 0.561f),
            BalloonColor.Purple => new Color(0.608f, 0.349f, 0.714f),
            _ => Color.white
        };
    }

    public static string ToDisplayName(this BalloonColor color)
    {
        return color.ToString();
    }
}
