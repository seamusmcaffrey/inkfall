using System;

[Serializable]
public class ComboFinisherDefinition
{
    public string finisherId = "ink-wave";
    public string displayName = "Ink Wave";
    public string description = "Floods a lane with paint.";
    public FinisherEffectType effectType = FinisherEffectType.RadialPop;
    public int minPopCount = 4;
    public int minScoreBurst = 500;
    public int minRecipeCount = 0;
    public int minStickerCount = 0;
    public float chargeCost = 1f;
    public int cooldownShots = 2;
    public int power = 3;
    public float radius = GameConstants.DEFAULT_PAINT_RADIUS * 1.6f;
}

public enum FinisherEffectType
{
    RadialPop,
    DartRain,
    GoldRush,
    CloneStorm,
    EchoBurst,
    CleanupSweep,
}
