using UnityEngine;

public partial class RunSystemsManager
{
    public BalloonScoreResult CalculateBalloonScore(BalloonNode balloon, int comboCount, float comboMultiplier)
    {
        int basePoints = balloon.PointValue + _perkManager.ScoreFlatBonus;
        float scoreMultiplier = comboMultiplier * _perkManager.ScoreMultiplier;
        scoreMultiplier *= 1f + Mathf.Max(0, CurrentRoomNumber - 1) * GameConfigSO.Instance.roomDepthScoreBonusPerRoom;
        scoreMultiplier *= 1f + _perkManager.GetColorFocusBonus(balloon.BalloonColor);
        scoreMultiplier *= 1f + _perkManager.GetStickerFocusBonus(balloon.StickerFamily);
        if (balloon.SpecialType == BalloonSpecialType.Gold || (_perkManager.YellowCountsAsGold && balloon.BalloonColor == BalloonColor.Yellow))
        {
            scoreMultiplier *= _perkManager.GoldMultiplier;
        }

        scoreMultiplier *= balloon.StickerFamily switch
        {
            StickerFamily.Crown => 1.25f,
            StickerFamily.Skull => 1.4f,
            StickerFamily.Target => 1.1f,
            _ => 1f,
        };

        int points = Mathf.RoundToInt(basePoints * Mathf.Max(0.1f, scoreMultiplier));
        if (points < 0)
        {
            points = Mathf.RoundToInt(points * Mathf.Max(0.25f, _perkManager.HazardPenaltyScale));
        }

        var result = new BalloonScoreResult
        {
            FinalPoints = points,
            WasJackpot = balloon.StickerFamily == StickerFamily.Skull || balloon.StickerFamily == StickerFamily.Clover,
            AdjacentBursts = _perkManager.AdjacentBurstCount + (balloon.StickerFamily == StickerFamily.Bolt ? 1 : 0),
            TicketDelta = _perkManager.TicketFlatBonus + (balloon.StickerFamily == StickerFamily.Clover ? 1 : 0),
            FinisherChargeDelta = GameConfigSO.Instance.finisherChargePerPop + (balloon.StickerFamily == StickerFamily.Star ? 0.18f : 0f),
            ComboCount = comboCount,
            ComboMultiplier = comboMultiplier,
        };
        if (balloon.SpecialType == BalloonSpecialType.Gold || (_perkManager.YellowCountsAsGold && balloon.BalloonColor == BalloonColor.Yellow))
        {
            result.TicketDelta += 2;
        }

        return result;
    }

    public void RegisterBalloonResolved(BalloonNode balloon, BalloonScoreResult result)
    {
        GainTickets(result.TicketDelta);
        GainFinisherCharge(result.FinisherChargeDelta);
        if (!_shot.Active || balloon == null)
        {
            return;
        }

        _shot.PopCount++;
        _shot.ScoreBurst += result.FinalPoints;
        _shot.LastPosition = balloon.transform.position;
        _shot.Tags.Add(balloon.BalloonColor);
        if (balloon.StickerFamily != StickerFamily.None)
        {
            _shot.Stickers.Add(balloon.StickerFamily);
        }

        switch (balloon.SpecialType)
        {
            case BalloonSpecialType.Mixer: _shot.ReverseOrder = !_shot.ReverseOrder; _shot.Tags.Add(balloon.BalloonColor == BalloonColor.Blue ? BalloonColor.Red : BalloonColor.Blue); break;
            case BalloonSpecialType.Invert: _shot.ReverseOrder = !_shot.ReverseOrder; GainFinisherCharge(0.2f); break;
            case BalloonSpecialType.Wash: GainTickets(1); break;
            case BalloonSpecialType.Clone: _balloonWall?.PopRandomNearby(balloon.transform.position, GameConstants.DEFAULT_PAINT_RADIUS, 2, null, balloon); break;
            case BalloonSpecialType.Rainbow: _shot.Tags.Add(BalloonColor.Red); _shot.Tags.Add(BalloonColor.Blue); _shot.Tags.Add(BalloonColor.Yellow); GainFinisherCharge(0.35f); break;
        }

        if (result.AdjacentBursts > 0)
        {
            _balloonWall?.PopRandomNearby(balloon.transform.position, GameConstants.DEFAULT_PAINT_RADIUS * _perkManager.PaintRadiusMultiplier, result.AdjacentBursts, null, balloon);
        }

        ApplyReactivePerks(balloon, result);
    }
}
