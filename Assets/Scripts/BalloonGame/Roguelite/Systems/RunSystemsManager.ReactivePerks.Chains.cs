using UnityEngine;

public partial class RunSystemsManager
{
    private void TriggerStaticSurge(PerkSO perk, BalloonNode balloon, BalloonScoreResult result)
    {
        if (balloon.BalloonColor != BalloonColor.Blue)
        {
            return;
        }

        int chainCount = 1 + GetChainCountBonus();
        for (int index = 0; index < chainCount; index++)
        {
            if (_balloonWall.TryPopNearest(balloon.transform.position, Config.staticSurgeRange, balloon, null, out BalloonNode target))
            {
                PublishPerkChain(perk, balloon, target, false, target.Row, PerkChainVisualStyle.Lightning);
            }
        }

        if (result.ComboCount >= Config.staticSurgeSweepComboThreshold)
        {
            ClearTriggeredRows(perk, balloon.Row, balloon, PerkChainVisualStyle.Sweep);
        }
    }

    private void TriggerAcidDrip(PerkSO perk, BalloonNode balloon)
    {
        if (balloon.BalloonColor != BalloonColor.Red)
        {
            return;
        }

        int dripCount = 1 + GetChainCountBonus();
        for (int index = 0; index < dripCount; index++)
        {
            if (_balloonWall.TryPopBelow(balloon, GameConstants.BOARD_ROWS, null, out BalloonNode target))
            {
                PublishPerkChain(perk, balloon, target, false, target.Row, PerkChainVisualStyle.Acid);
            }
        }
    }

    private void TriggerSewerRats(PerkSO perk, BalloonNode balloon)
    {
        if (balloon.BalloonColor != BalloonColor.Green)
        {
            return;
        }

        int ratCount = HasReactiveRelic(ReactivePerkIds.BurrowCrown) ? Config.burrowCrownRatCount : 1;
        for (int index = 0; index < ratCount; index++)
        {
            if (_balloonWall.TryPopNearest(balloon.transform.position, Config.sewerRatRange, balloon, null, out BalloonNode target))
            {
                PublishPerkChain(perk, balloon, target, false, target.Row, PerkChainVisualStyle.Rat);
            }
        }
    }

    private void TriggerGoldGlimmer(PerkSO perk, BalloonNode balloon)
    {
        if (balloon.BalloonColor != BalloonColor.Yellow)
        {
            return;
        }

        foreach (BalloonNode target in _balloonWall.PopAdjacentInRow(balloon, 1 + GetChainCountBonus(), null))
        {
            PublishPerkChain(perk, balloon, target, false, target.Row, PerkChainVisualStyle.Sweep);
        }
    }

    private void TriggerVioletRelay(PerkSO perk, BalloonNode balloon)
    {
        if (balloon.BalloonColor != BalloonColor.Purple)
        {
            return;
        }

        if (_balloonWall.TryPopMirrored(balloon, null, out BalloonNode target))
        {
            PublishPerkChain(perk, balloon, target, false, target.Row, PerkChainVisualStyle.Echo);
        }
    }
}
