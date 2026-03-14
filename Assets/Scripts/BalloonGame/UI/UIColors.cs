using UnityEngine;

/// <summary>
/// Centralized UI palette for INKSHOT.
/// </summary>
public static class UIColors
{
    public static readonly Color ScoreWhite = new(1f, 1f, 1f, 1f);
    public static readonly Color TargetGray = new(0.62f, 0.62f, 0.68f, 1f);
    public static readonly Color DartBlue = new(0.58f, 0.84f, 1f, 1f);
    public static readonly Color ComboGold = new(1f, 0.85f, 0.2f, 1f);
    public static readonly Color RoomPink = new(1f, 0.23f, 0.62f, 1f);
    public static readonly Color HazardRed = new(0.95f, 0.24f, 0.26f, 1f);
    public static readonly Color InkCyan = new(0.08f, 0.92f, 0.92f, 1f);

    public static readonly Color ProgressBarTrack = new(0.09f, 0.09f, 0.12f, 0.9f);
    public static readonly Color ProgressBarFillStart = new(0.04f, 0.85f, 0.38f, 1f);
    public static readonly Color ProgressBarFillEnd = new(1f, 0.88f, 0.15f, 1f);
    public static readonly Color ProgressBarGlow = new(0.15f, 1f, 0.5f, 0.5f);

    public static readonly Color ComboTier1 = new(1f, 1f, 1f, 1f);
    public static readonly Color ComboTier2 = new(1f, 0.95f, 0.35f, 1f);
    public static readonly Color ComboTier3 = new(1f, 0.62f, 0.14f, 1f);
    public static readonly Color ComboTier4 = new(1f, 0.2f, 0.65f, 1f);

    public static readonly Color CommonWhite = new(0.82f, 0.84f, 0.88f, 1f);
    public static readonly Color RareBlueBorder = new(0.22f, 0.52f, 1f, 1f);
    public static readonly Color EpicPurpleBorder = new(0.68f, 0.24f, 1f, 1f);
    public static readonly Color LegendaryGoldBorder = new(1f, 0.82f, 0.14f, 1f);

    public static readonly Color PanelBackground = new(0.04f, 0.05f, 0.08f, 0.9f);
    public static readonly Color ShadowBlack = new(0f, 0f, 0f, 0.85f);
    public static readonly Color ClearedGreen = new(0.32f, 0.94f, 0.44f, 1f);
    public static readonly Color FailedRed = new(1f, 0.35f, 0.35f, 1f);

    public static Color GetBalloonTextColor(BalloonColor balloonColor)
    {
        return balloonColor switch
        {
            BalloonColor.Red => new Color(1f, 0.35f, 0.35f, 1f),
            BalloonColor.Blue => new Color(0.35f, 0.6f, 1f, 1f),
            BalloonColor.Yellow => new Color(1f, 0.95f, 0.35f, 1f),
            BalloonColor.Green => new Color(0.35f, 0.95f, 0.46f, 1f),
            BalloonColor.Purple => new Color(0.75f, 0.4f, 1f, 1f),
            _ => ScoreWhite,
        };
    }

    public static Color GetComboColor(int comboCount)
    {
        return comboCount switch
        {
            <= 2 => ComboTier1,
            <= 4 => ComboTier2,
            <= 6 => ComboTier3,
            _ => ComboTier4,
        };
    }

    public static Color GetRarityColor(PerkRarity rarity)
    {
        return rarity switch
        {
            PerkRarity.Common => CommonWhite,
            PerkRarity.Uncommon => DartBlue,
            PerkRarity.Rare => RareBlueBorder,
            PerkRarity.Legendary => LegendaryGoldBorder,
            _ => CommonWhite,
        };
    }
}
