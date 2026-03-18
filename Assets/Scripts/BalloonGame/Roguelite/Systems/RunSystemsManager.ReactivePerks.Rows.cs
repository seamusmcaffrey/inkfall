using UnityEngine;

public partial class RunSystemsManager
{
    private void TriggerStormfront(PerkSO perk, BalloonNode balloon, BalloonScoreResult result)
    {
        if (balloon.BalloonColor == BalloonColor.Blue && result.ComboCount >= Config.stormfrontComboThreshold)
        {
            ClearTriggeredRows(perk, balloon.Row, balloon, PerkChainVisualStyle.Sweep);
        }
    }

    private void TriggerCausticFlood(PerkSO perk, BalloonNode balloon, BalloonScoreResult result)
    {
        if (balloon.BalloonColor != BalloonColor.Red || result.ComboCount < Config.causticFloodComboThreshold)
        {
            return;
        }

        int targetRow = Mathf.Min(GetBoardRowCount() - 1, balloon.Row + 1);
        ClearTriggeredRows(perk, targetRow, balloon, PerkChainVisualStyle.Sweep);
    }

    private void TriggerRatKing(PerkSO perk, BalloonNode balloon, BalloonScoreResult result)
    {
        if (balloon.BalloonColor != BalloonColor.Green || result.ComboCount < Config.ratKingComboThreshold)
        {
            return;
        }

        int targetRow = _balloonWall.GetLowestActiveRow();
        if (targetRow >= 0)
        {
            ClearTriggeredRows(perk, targetRow, balloon, PerkChainVisualStyle.Sweep);
        }
    }

    private void TriggerJackpotLane(PerkSO perk, BalloonNode balloon, BalloonScoreResult result)
    {
        if (balloon.BalloonColor != BalloonColor.Yellow || result.ComboCount < Config.jackpotLaneComboThreshold)
        {
            return;
        }

        GainTickets(Config.jackpotLaneTicketBonus);
        ClearTriggeredRows(perk, balloon.Row, balloon, PerkChainVisualStyle.Sweep);
    }

    private void TriggerCurtainCall(PerkSO perk, BalloonNode balloon, BalloonScoreResult result)
    {
        if (balloon.BalloonColor != BalloonColor.Purple || result.ComboCount < Config.curtainCallComboThreshold)
        {
            return;
        }

        int topRow = _balloonWall.GetHighestActiveRow();
        if (topRow >= 0)
        {
            ClearTriggeredRows(perk, topRow, balloon, PerkChainVisualStyle.Sweep);
        }
    }

    private void TriggerPrismSweep(PerkSO perk, BalloonScoreResult result)
    {
        if (result.ComboCount < Config.prismSweepComboThreshold || CountUniqueShotColors() < Config.prismSweepUniqueColorsRequired)
        {
            return;
        }

        int topRow = _balloonWall.GetHighestActiveRow();
        int bottomRow = _balloonWall.GetLowestActiveRow();
        if (topRow >= 0)
        {
            ClearTriggeredRows(perk, topRow, null, PerkChainVisualStyle.Sweep);
        }

        if (bottomRow >= 0 && bottomRow != topRow)
        {
            ClearTriggeredRows(perk, bottomRow, null, PerkChainVisualStyle.Sweep);
        }
    }

    private void ClearTriggeredRows(PerkSO perk, int row, BalloonNode source, PerkChainVisualStyle style)
    {
        if (row < 0)
        {
            return;
        }

        Vector3 sourcePosition = source != null ? source.transform.position : _balloonWall.GetRowCenter(row);
        int sourceRow = source != null ? source.Row : row;
        PublishRowClear(perk, sourcePosition, sourceRow, row, style);
        _balloonWall.PopRow(row, null);
        if (HasReactiveRelic(ReactivePerkIds.MonsoonReel))
        {
            int adjacentRow = Mathf.Clamp(row + 1, 0, GetBoardRowCount() - 1);
            if (adjacentRow != row)
            {
                PublishRowClear(perk, sourcePosition, sourceRow, adjacentRow, style);
                _balloonWall.PopRow(adjacentRow, null);
            }
        }
    }

    private void PublishPerkChain(PerkSO perk, BalloonNode source, BalloonNode target, bool clearsRow, int targetRow, PerkChainVisualStyle style)
    {
        if (perk == null || source == null || target == null)
        {
            return;
        }

        EventBus.Publish(new PerkChainTriggeredEvent
        {
            PerkId = perk.perkId,
            DisplayName = perk.perkName,
            AccentColor = perk.accentColor,
            SourcePosition = source.transform.position,
            TargetPosition = target.transform.position,
            SourceRow = source.Row,
            TargetRow = targetRow,
            ClearsRow = clearsRow,
            VisualStyle = style,
        });
    }

    private void PublishRowClear(PerkSO perk, Vector3 sourcePosition, int sourceRow, int targetRow, PerkChainVisualStyle style)
    {
        if (perk == null)
        {
            return;
        }

        EventBus.Publish(new PerkChainTriggeredEvent
        {
            PerkId = perk.perkId,
            DisplayName = perk.perkName,
            AccentColor = perk.accentColor,
            SourcePosition = sourcePosition,
            TargetPosition = _balloonWall.GetRowCenter(targetRow),
            SourceRow = sourceRow,
            TargetRow = targetRow,
            ClearsRow = true,
            VisualStyle = style,
        });
    }
}
