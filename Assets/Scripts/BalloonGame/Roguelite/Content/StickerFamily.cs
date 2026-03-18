using UnityEngine;

public enum StickerFamily
{
    None,
    Crown,
    Skull,
    Star,
    Bolt,
    Clover,
    Target,
}

public static class StickerFamilyExtensions
{
    public static string ToDisplayName(this StickerFamily family)
    {
        return family switch
        {
            StickerFamily.Crown => "Crown",
            StickerFamily.Skull => "Skull",
            StickerFamily.Star => "Star",
            StickerFamily.Bolt => "Bolt",
            StickerFamily.Clover => "Clover",
            StickerFamily.Target => "Target",
            _ => "None",
        };
    }

    public static Color ToColor(this StickerFamily family)
    {
        return family switch
        {
            StickerFamily.Crown => new Color(1f, 0.88f, 0.3f, 1f),
            StickerFamily.Skull => new Color(1f, 0.4f, 0.45f, 1f),
            StickerFamily.Star => new Color(0.45f, 0.95f, 1f, 1f),
            StickerFamily.Bolt => new Color(1f, 0.95f, 0.15f, 1f),
            StickerFamily.Clover => new Color(0.35f, 1f, 0.62f, 1f),
            StickerFamily.Target => new Color(1f, 0.58f, 0.18f, 1f),
            _ => new Color(1f, 1f, 1f, 0f),
        };
    }
}
