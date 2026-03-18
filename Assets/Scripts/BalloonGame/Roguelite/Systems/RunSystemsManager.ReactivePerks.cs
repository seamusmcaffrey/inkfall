using UnityEngine;

public partial class RunSystemsManager
{
    private static GameConfigSO Config => GameConfigSO.Instance;

    private void ApplyReactivePerks(BalloonNode balloon, BalloonScoreResult result)
    {
        if (balloon == null || _balloonWall == null || _perkManager == null)
        {
            return;
        }

        foreach (PerkSO perk in _perkManager.ActivePerks)
        {
            ApplyReactivePerk(perk, balloon, result);
        }

        foreach (PerkSO relic in _perkManager.ActiveRelics)
        {
            ApplyReactivePerk(relic, balloon, result);
        }
    }

    private void ApplyReactivePerk(PerkSO perk, BalloonNode balloon, BalloonScoreResult result)
    {
        if (perk == null)
        {
            return;
        }

        switch (perk.perkId)
        {
            case ReactivePerkIds.StaticSurge:
                TriggerStaticSurge(perk, balloon, result);
                break;
            case ReactivePerkIds.AcidDrip:
                TriggerAcidDrip(perk, balloon);
                break;
            case ReactivePerkIds.SewerRats:
                TriggerSewerRats(perk, balloon);
                break;
            case ReactivePerkIds.GoldGlimmer:
                TriggerGoldGlimmer(perk, balloon);
                break;
            case ReactivePerkIds.VioletRelay:
                TriggerVioletRelay(perk, balloon);
                break;
            case ReactivePerkIds.Stormfront:
                TriggerStormfront(perk, balloon, result);
                break;
            case ReactivePerkIds.CausticFlood:
                TriggerCausticFlood(perk, balloon, result);
                break;
            case ReactivePerkIds.RatKing:
                TriggerRatKing(perk, balloon, result);
                break;
            case ReactivePerkIds.JackpotLane:
                TriggerJackpotLane(perk, balloon, result);
                break;
            case ReactivePerkIds.CurtainCall:
                TriggerCurtainCall(perk, balloon, result);
                break;
            case ReactivePerkIds.PrismSweep:
                TriggerPrismSweep(perk, result);
                break;
        }
    }

    private int CountUniqueShotColors()
    {
        var seen = new bool[5];
        int count = 0;
        foreach (BalloonColor color in _shot.Tags)
        {
            int index = (int)color;
            if (index < 0 || index >= seen.Length || seen[index])
            {
                continue;
            }

            seen[index] = true;
            count++;
        }

        return count;
    }

    private int GetChainCountBonus()
    {
        return HasReactiveRelic(ReactivePerkIds.ChainParade) ? Config.chainParadeBonusChains : 0;
    }

    private bool HasReactiveRelic(string perkId)
    {
        foreach (PerkSO relic in _perkManager.ActiveRelics)
        {
            if (relic != null && relic.perkId == perkId)
            {
                return true;
            }
        }

        return false;
    }

    private int GetBoardRowCount()
    {
        int maxRow = -1;
        foreach (BalloonNode balloon in _balloonWall.Balloons)
        {
            if (balloon != null)
            {
                maxRow = Mathf.Max(maxRow, balloon.Row);
            }
        }

        return maxRow >= 0 ? maxRow + 1 : GameConstants.BOARD_ROWS;
    }
}
