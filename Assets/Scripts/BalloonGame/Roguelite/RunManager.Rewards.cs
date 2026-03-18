using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RunManager
{
    private IEnumerator PromptRelicReward()
    {
        CurrentState = RunState.RelicDraft;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = _runData.currentRoomNumber });
        List<PerkSO> relics = _runSystemsManager.GetRelicChoices(_runData.currentRoomNumber + 1);
        if (_choiceScreen == null || relics.Count == 0)
        {
            if (relics.Count > 0) _runSystemsManager.AddRelic(relics[0]);
            yield break;
        }

        bool selected = false;
        var options = new List<RunChoiceOption>();
        foreach (PerkSO relic in relics)
        {
            options.Add(new RunChoiceOption
            {
                Title = relic.perkName,
                Description = relic.description,
                CostLabel = "TAKE",
                AccentColor = relic.accentColor,
                OnSelected = () => { _runSystemsManager.AddRelic(relic); selected = true; },
            });
        }

        _choiceScreen.Show("Keystone Relic", "Pick the run-defining engine piece.", options);
        while (!selected) yield return null;
    }

    private IEnumerator PromptShop()
    {
        CurrentState = RunState.Shop;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = _runData.currentRoomNumber });
        List<RunShopOffer> offers = _runSystemsManager.GetShopOffers(_runData.currentRoomNumber + 1);
        if (_choiceScreen == null || offers.Count == 0) yield break;

        bool finished = false;
        var options = new List<RunChoiceOption>();
        foreach (RunShopOffer offer in offers)
        {
            options.Add(new RunChoiceOption
            {
                Title = offer.title,
                Description = offer.description,
                CostLabel = $"TICKETS {offer.cost}",
                AccentColor = offer.offerType == RunShopOfferType.BuyRelic ? UIColors.ComboGold : UIColors.InkCyan,
                OnSelected = () => { if (ApplyShopOffer(offer)) finished = true; },
                HideOnSelect = false,
            });
        }

        options.Add(new RunChoiceOption
        {
            Title = "Leave",
            Description = "Keep your tickets and head to the next booth.",
            CostLabel = "EXIT",
            AccentColor = UIColors.TargetGray,
            OnSelected = () => finished = true,
        });
        _choiceScreen.Show("Midway Shop", $"Prize Tickets: {_runSystemsManager.PrizeTickets}", options);
        while (!finished) yield return null;
        _runSystemsManager.ClearShopOffers();
        _choiceScreen.Hide();
    }

    private bool ApplyShopOffer(RunShopOffer offer)
    {
        if (!_runSystemsManager.TrySpendTickets(offer.cost)) return false;
        switch (offer.offerType)
        {
            case RunShopOfferType.BuyPerk: AwardPerk(offer.perkReward); return true;
            case RunShopOfferType.BuyRelic: _runSystemsManager.AddRelic(offer.perkReward); return true;
            case RunShopOfferType.AddDart: AwardPerk(CreateRuntimePerk("shop-darts", "Fresh Darts", "+1 dart for the rest of the run.", PerkEffectType.ExtraDart, 0f, 1)); return true;
            case RunShopOfferType.PolishEngine: AwardPerk(CreateRuntimePerk("shop-polish", "Polish Engine", "Small permanent run score polish.", PerkEffectType.ScoreMultiplier, 0.1f, 0)); return true;
            default: return false;
        }
    }
}
