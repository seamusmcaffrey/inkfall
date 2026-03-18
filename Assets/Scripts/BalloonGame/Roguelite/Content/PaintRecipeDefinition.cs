using System;

[Serializable]
public class PaintRecipeDefinition
{
    public string recipeId = "afterburner";
    public string displayName = "Afterburner";
    public string description = "Chain burst from a hot ricochet.";
    public BalloonColor primaryColor = BalloonColor.Red;
    public BalloonColor secondaryColor = BalloonColor.Blue;
    public int unlockRoom = 4;
    public PaintRecipeEffectType effectType = PaintRecipeEffectType.AdjacentBurst;
    public int power = 1;
    public int secondaryPower;
    public float radius = GameConstants.DEFAULT_PAINT_RADIUS;
}

public enum PaintRecipeEffectType
{
    AdjacentBurst,
    TicketBurst,
    SafetyDividend,
    CloneBurst,
    ScoreSpike,
    AnchorPulse,
    JackpotShift,
    PrimerMark,
    RicochetEcho,
}
