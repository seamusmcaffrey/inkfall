using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RunManager
{
    private IEnumerator PromptLoadoutSelection()
    {
        CurrentState = RunState.LoadoutDraft;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = _runData.currentRoomNumber });
        IReadOnlyList<StarterLoadoutDefinition> loadouts = _runSystemsManager.GetAvailableLoadouts();
        StarterLoadoutDefinition chosenLoadout = _runSystemsManager.ResolveSelectedLoadout();
        if (_choiceScreen != null && loadouts.Count > 1)
        {
            bool decided = false;
            var options = new List<RunChoiceOption>();
            foreach (StarterLoadoutDefinition loadout in loadouts)
            {
                options.Add(new RunChoiceOption
                {
                    Title = loadout.displayName,
                    Description = $"{loadout.description}\nTradeoff: {loadout.tradeoff}",
                    CostLabel = "START",
                    AccentColor = loadout.accentColor,
                    OnSelected = () => { chosenLoadout = loadout; decided = true; },
                });
            }

            _choiceScreen.Show("Choose Loadout", "Pick the opening bow package for this run.", options);
            while (!decided)
            {
                yield return null;
            }
        }

        _runSystemsManager.SaveSelectedLoadout(chosenLoadout);
        CurrentState = RunState.Idle;
    }

    private IEnumerator BetweenRoomFlow()
    {
        yield return PromptPerkDraft();
        if (_runSystemsManager.ShouldOfferRelic(_runData.currentRoomNumber + 1))
        {
            yield return PromptRelicReward();
        }

        if (_runSystemsManager.ShouldOfferShop(_runData.currentRoomNumber + 1))
        {
            yield return PromptShop();
        }

        _runData.currentRoomNumber++;
        EnterRoom(_runData.currentRoomNumber);
    }

    private IEnumerator PromptPerkDraft()
    {
        CurrentState = RunState.PerkDraft;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = _runData.currentRoomNumber });
        List<PerkSO> choices = _runSystemsManager.GetPerkChoices(_runData.currentRoomNumber + 1);
        if (_perkSelectionScreen == null || choices.Count == 0)
        {
            if (choices.Count > 0)
            {
                AwardPerk(choices[0]);
            }

            yield break;
        }

        bool selectionMade = false;
        System.Action showChoices = null;
        showChoices = () =>
        {
            _perkSelectionScreen.Show(choices, perk =>
            {
                AwardPerk(perk);
                selectionMade = true;
            }, () =>
            {
                if (_runSystemsManager.ConsumePerkReroll())
                {
                    choices = _runSystemsManager.GetPerkChoices(_runData.currentRoomNumber + 1);
                    showChoices();
                }
            }, _runSystemsManager.GetPerkRerollCost(), _runSystemsManager.PrizeTickets);
        };
        showChoices();
        while (!selectionMade)
        {
            yield return null;
        }
    }

}
