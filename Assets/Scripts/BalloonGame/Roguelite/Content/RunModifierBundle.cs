using System;

[Serializable]
public class RunModifierBundle
{
    public int extraPierce;
    public int extraRicochets;
    public int extraDarts;
    public int extraRerolls;
    public int startingTickets;
    public int adjacentBurstCount;
    public int miniHitCount;
    public int extraSpecialSpawns;
    public int extraHazardSpawns;
    public int recipeUnlockRoomDelta;
    public int scoreFlatBonus;
    public int ticketFlatBonus;
    public int finisherFlatCharge;

    public float launchSpeedBonus;
    public float paintRadiusBonus;
    public float scoreMultiplierBonus;
    public float goldMultiplierBonus;
    public float overflowTicketRateBonus;
    public float overflowFinisherRateBonus;
    public float finisherChargeMultiplierBonus;
    public float hazardPenaltyScaleBonus;
    public float shopDiscountBonus;
    public float refundChanceBonus;
    public float missReturnChanceBonus;
    public float anchorRadiusBonus;
    public float bounceScoreBonus;
    public float colorFocusBonus;
    public float stickerFocusBonus;
    public float stickerSpawnBonus;
    public float chaosSpawnBonus;

    public bool recipesAlwaysOn;
    public bool reversePaintOrder;
    public bool firstStuckDartAnchor;
    public bool anchorPaintPulse;
    public bool yellowCountsAsGold;
    public bool skullsAreJackpots;
    public bool missesReturnInsteadOfStick;
    public bool doubleFinisherCharge;
    public bool fewerPrizeBalloons;

    public void Add(RunModifierBundle other)
    {
        if (other == null)
        {
            return;
        }

        extraPierce += other.extraPierce;
        extraRicochets += other.extraRicochets;
        extraDarts += other.extraDarts;
        extraRerolls += other.extraRerolls;
        startingTickets += other.startingTickets;
        adjacentBurstCount += other.adjacentBurstCount;
        miniHitCount += other.miniHitCount;
        extraSpecialSpawns += other.extraSpecialSpawns;
        extraHazardSpawns += other.extraHazardSpawns;
        recipeUnlockRoomDelta += other.recipeUnlockRoomDelta;
        scoreFlatBonus += other.scoreFlatBonus;
        ticketFlatBonus += other.ticketFlatBonus;
        finisherFlatCharge += other.finisherFlatCharge;

        launchSpeedBonus += other.launchSpeedBonus;
        paintRadiusBonus += other.paintRadiusBonus;
        scoreMultiplierBonus += other.scoreMultiplierBonus;
        goldMultiplierBonus += other.goldMultiplierBonus;
        overflowTicketRateBonus += other.overflowTicketRateBonus;
        overflowFinisherRateBonus += other.overflowFinisherRateBonus;
        finisherChargeMultiplierBonus += other.finisherChargeMultiplierBonus;
        hazardPenaltyScaleBonus += other.hazardPenaltyScaleBonus;
        shopDiscountBonus += other.shopDiscountBonus;
        refundChanceBonus += other.refundChanceBonus;
        missReturnChanceBonus += other.missReturnChanceBonus;
        anchorRadiusBonus += other.anchorRadiusBonus;
        bounceScoreBonus += other.bounceScoreBonus;
        colorFocusBonus += other.colorFocusBonus;
        stickerFocusBonus += other.stickerFocusBonus;
        stickerSpawnBonus += other.stickerSpawnBonus;
        chaosSpawnBonus += other.chaosSpawnBonus;

        recipesAlwaysOn |= other.recipesAlwaysOn;
        reversePaintOrder |= other.reversePaintOrder;
        firstStuckDartAnchor |= other.firstStuckDartAnchor;
        anchorPaintPulse |= other.anchorPaintPulse;
        yellowCountsAsGold |= other.yellowCountsAsGold;
        skullsAreJackpots |= other.skullsAreJackpots;
        missesReturnInsteadOfStick |= other.missesReturnInsteadOfStick;
        doubleFinisherCharge |= other.doubleFinisherCharge;
        fewerPrizeBalloons |= other.fewerPrizeBalloons;
    }
}
