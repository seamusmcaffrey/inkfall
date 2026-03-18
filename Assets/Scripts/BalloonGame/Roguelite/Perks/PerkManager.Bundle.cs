public partial class PerkManager
{
    private void ResetBundle()
    {
        _bundle.extraPierce = 0;
        _bundle.extraRicochets = 0;
        _bundle.extraDarts = 0;
        _bundle.extraRerolls = 0;
        _bundle.startingTickets = 0;
        _bundle.adjacentBurstCount = 0;
        _bundle.miniHitCount = 0;
        _bundle.extraSpecialSpawns = 0;
        _bundle.extraHazardSpawns = 0;
        _bundle.recipeUnlockRoomDelta = 0;
        _bundle.scoreFlatBonus = 0;
        _bundle.ticketFlatBonus = 0;
        _bundle.finisherFlatCharge = 0;
        _bundle.launchSpeedBonus = 0f;
        _bundle.paintRadiusBonus = 0f;
        _bundle.scoreMultiplierBonus = 0f;
        _bundle.goldMultiplierBonus = 0f;
        _bundle.overflowTicketRateBonus = 0f;
        _bundle.overflowFinisherRateBonus = 0f;
        _bundle.finisherChargeMultiplierBonus = 0f;
        _bundle.hazardPenaltyScaleBonus = 0f;
        _bundle.shopDiscountBonus = 0f;
        _bundle.refundChanceBonus = 0f;
        _bundle.missReturnChanceBonus = 0f;
        _bundle.anchorRadiusBonus = 0f;
        _bundle.bounceScoreBonus = 0f;
        _bundle.colorFocusBonus = 0f;
        _bundle.stickerFocusBonus = 0f;
        _bundle.stickerSpawnBonus = 0f;
        _bundle.chaosSpawnBonus = 0f;
        _bundle.recipesAlwaysOn = false;
        _bundle.reversePaintOrder = false;
        _bundle.firstStuckDartAnchor = false;
        _bundle.anchorPaintPulse = false;
        _bundle.yellowCountsAsGold = false;
        _bundle.skullsAreJackpots = false;
        _bundle.missesReturnInsteadOfStick = false;
        _bundle.doubleFinisherCharge = false;
        _bundle.fewerPrizeBalloons = false;
    }
}
