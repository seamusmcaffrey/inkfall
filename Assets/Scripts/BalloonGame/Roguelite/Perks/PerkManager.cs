using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks active perks and exposes their aggregate gameplay modifiers.
/// </summary>
[DisallowMultipleComponent]
public partial class PerkManager : MonoBehaviour
{
    private readonly List<PerkSO> _activePerks = new();
    private readonly List<PerkSO> _activeRelics = new();
    private readonly List<MetaUpgradeSO> _activeMetaUpgrades = new();
    private readonly Dictionary<BalloonColor, float> _colorFocusBonuses = new();
    private readonly Dictionary<StickerFamily, float> _stickerFocusBonuses = new();
    private StarterLoadoutDefinition _starterLoadout;
    private readonly RunModifierBundle _bundle = new();

    public IReadOnlyList<PerkSO> ActivePerks => _activePerks;
    public IReadOnlyList<PerkSO> ActiveRelics => _activeRelics;
    public StarterLoadoutDefinition StarterLoadout => _starterLoadout;
    public float LaunchSpeedMultiplier { get; private set; } = 1f;
    public int AdditionalPierce { get; private set; }
    public int AdditionalRicochet { get; private set; }
    public int AdditionalDarts { get; private set; }
    public float ComboWindowBonus { get; private set; }
    public float ComboBonusPoints { get; private set; }
    public float PaintRadiusMultiplier { get; private set; } = 1f;
    public float ScoreMultiplier { get; private set; } = 1f;
    public float GoldMultiplier { get; private set; } = 1f;
    public bool HazardShieldArmed { get; private set; }
    public int ExtraRerolls => _bundle.extraRerolls;
    public int StartingTickets => _bundle.startingTickets;
    public int AdjacentBurstCount => _bundle.adjacentBurstCount;
    public int MiniHitCount => _bundle.miniHitCount;
    public int ExtraSpecialSpawns => _bundle.extraSpecialSpawns;
    public int ExtraHazardSpawns => _bundle.extraHazardSpawns;
    public int RecipeUnlockRoomDelta => _bundle.recipeUnlockRoomDelta;
    public int ScoreFlatBonus => _bundle.scoreFlatBonus;
    public int TicketFlatBonus => _bundle.ticketFlatBonus;
    public int FinisherFlatCharge => _bundle.finisherFlatCharge;
    public float OverflowTicketRateBonus => _bundle.overflowTicketRateBonus;
    public float OverflowFinisherRateBonus => _bundle.overflowFinisherRateBonus;
    public float FinisherChargeMultiplier => 1f + _bundle.finisherChargeMultiplierBonus;
    public float HazardPenaltyScale => 1f + _bundle.hazardPenaltyScaleBonus;
    public float ShopDiscount => _bundle.shopDiscountBonus;
    public float RefundChanceBonus => _bundle.refundChanceBonus;
    public float MissReturnChanceBonus => _bundle.missReturnChanceBonus;
    public float AnchorRadiusBonus => _bundle.anchorRadiusBonus;
    public float BounceScoreBonus => _bundle.bounceScoreBonus;
    public float StickerSpawnBonus => _bundle.stickerSpawnBonus;
    public float ChaosSpawnBonus => _bundle.chaosSpawnBonus;
    public bool RecipesAlwaysOn => _bundle.recipesAlwaysOn;
    public bool ReversePaintOrder => _bundle.reversePaintOrder;
    public bool FirstStuckDartAnchor => _bundle.firstStuckDartAnchor;
    public bool AnchorPaintPulse => _bundle.anchorPaintPulse;
    public bool YellowCountsAsGold => _bundle.yellowCountsAsGold;
    public bool SkullsAreJackpots => _bundle.skullsAreJackpots;
    public bool MissesReturnInsteadOfStick => _bundle.missesReturnInsteadOfStick;
    public bool DoubleFinisherCharge => _bundle.doubleFinisherCharge;
    public bool FewerPrizeBalloons => _bundle.fewerPrizeBalloons;

    public void ResetRun()
    {
        _activePerks.Clear();
        _activeRelics.Clear();
        _activeMetaUpgrades.Clear();
        _starterLoadout = null;
        Recalculate();
    }

    public void SetStarterLoadout(StarterLoadoutDefinition starterLoadout)
    {
        _starterLoadout = starterLoadout;
        Recalculate();
    }

    public void SetMetaUpgrades(IReadOnlyList<MetaUpgradeSO> upgrades)
    {
        _activeMetaUpgrades.Clear();
        if (upgrades != null)
        {
            _activeMetaUpgrades.AddRange(upgrades);
        }

        Recalculate();
    }

    public void AddPerk(PerkSO perk)
    {
        if (perk == null || perk.isKeystone)
        {
            return;
        }

        _activePerks.Add(perk);
        Recalculate();
    }

    public void AddRelic(PerkSO relic)
    {
        if (relic == null)
        {
            return;
        }

        _activeRelics.Add(relic);
        Recalculate();
    }

    public bool ConsumeHazardShield()
    {
        if (!HazardShieldArmed)
        {
            return false;
        }

        HazardShieldArmed = false;
        return true;
    }
}
