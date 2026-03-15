using UnityEngine;

/// <summary>
/// Centralized UI palette for INKSHOT — industrial noir carnival aesthetic.
/// Single source of truth for all UI colors across HUD, screens, and overlays.
/// </summary>
public static class UIColors
{
    // Core palette
    public static readonly Color ScoreWhite = new(0.96f, 0.97f, 1f, 1f);
    public static readonly Color TargetGray = new(0.55f, 0.56f, 0.64f, 1f);
    public static readonly Color DartBlue = new(0.45f, 0.82f, 1f, 1f);
    public static readonly Color ComboGold = new(1f, 0.82f, 0.12f, 1f);
    public static readonly Color RoomPink = new(1f, 0.18f, 0.58f, 1f);
    public static readonly Color HazardRed = new(0.95f, 0.18f, 0.22f, 1f);
    public static readonly Color InkCyan = new(0f, 0.95f, 0.95f, 1f);

    // Progress bar gradient: cool cyan -> warm gold
    public static readonly Color ProgressBarTrack = new(0.06f, 0.06f, 0.1f, 0.92f);
    public static readonly Color ProgressBarFillStart = new(0f, 0.78f, 0.88f, 1f);
    public static readonly Color ProgressBarFillEnd = new(1f, 0.82f, 0.12f, 1f);
    public static readonly Color ProgressBarGlow = new(0.1f, 0.95f, 0.6f, 0.55f);

    // Combo escalation tiers
    public static readonly Color ComboTier1 = new(1f, 1f, 1f, 1f);
    public static readonly Color ComboTier2 = new(1f, 0.92f, 0.22f, 1f);
    public static readonly Color ComboTier3 = new(1f, 0.55f, 0.08f, 1f);
    public static readonly Color ComboTier4 = new(1f, 0.08f, 0.58f, 1f);

    // Combo glow/shadow behind text (per tier)
    public static readonly Color ComboGlow1 = new(0.5f, 0.5f, 0.6f, 0.3f);
    public static readonly Color ComboGlow2 = new(0.6f, 0.55f, 0f, 0.4f);
    public static readonly Color ComboGlow3 = new(0.6f, 0.3f, 0f, 0.5f);
    public static readonly Color ComboGlow4 = new(0.6f, 0f, 0.3f, 0.6f);

    // Perk rarity borders
    public static readonly Color CommonWhite = new(0.72f, 0.74f, 0.78f, 1f);
    public static readonly Color RareBlueBorder = new(0.18f, 0.48f, 1f, 1f);
    public static readonly Color EpicPurpleBorder = new(0.65f, 0.18f, 1f, 1f);
    public static readonly Color LegendaryGoldBorder = new(1f, 0.78f, 0.08f, 1f);

    // Panel / overlay
    public static readonly Color PanelBackground = new(0.03f, 0.03f, 0.06f, 0.92f);
    public static readonly Color TopBarBackground = new(0.02f, 0.02f, 0.04f, 0.88f);
    public static readonly Color BadgeBackground = new(0.08f, 0.08f, 0.14f, 0.95f);
    public static readonly Color BadgeBorder = new(0.25f, 0.25f, 0.35f, 0.6f);
    public static readonly Color ShadowBlack = new(0f, 0f, 0f, 0.88f);

    // Status
    public static readonly Color ClearedGreen = new(0.22f, 0.96f, 0.42f, 1f);
    public static readonly Color FailedRed = new(1f, 0.3f, 0.3f, 1f);

    // Floating score tiers
    public static readonly Color FloatingStandard = new(1f, 1f, 1f, 1f);
    public static readonly Color FloatingGold = new(1f, 0.85f, 0.15f, 1f);
    public static readonly Color FloatingPremium = new(1f, 0.55f, 0.1f, 1f);

    // Currency icon tint
    public static readonly Color CurrencyIconTint = new(0.1f, 0.92f, 0.85f, 1f);

    // Dart icon active vs spent
    public static readonly Color DartIconActive = new(0.45f, 0.82f, 1f, 1f);
    public static readonly Color DartIconSpent = new(0.3f, 0.3f, 0.38f, 0.25f);

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

    public static Color GetComboGlowColor(int comboCount)
    {
        return comboCount switch
        {
            <= 2 => ComboGlow1,
            <= 4 => ComboGlow2,
            <= 6 => ComboGlow3,
            _ => ComboGlow4,
        };
    }

    /// <summary>
    /// Lerps the progress bar fill between cyan (start) and gold (end).
    /// </summary>
    public static Color GetProgressFillColor(float progress)
    {
        return Color.Lerp(ProgressBarFillStart, ProgressBarFillEnd, progress);
    }

    /// <summary>
    /// Returns color for floating score text based on point value.
    /// </summary>
    public static Color GetFloatingScoreColor(int points)
    {
        const int goldThreshold = 300;
        const int premiumThreshold = 600;
        if (points >= premiumThreshold) return FloatingPremium;
        if (points >= goldThreshold) return FloatingGold;
        return FloatingStandard;
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
