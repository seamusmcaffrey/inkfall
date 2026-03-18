using UnityEngine;

public partial class RunSystemsManager
{
    private ComboFinisherDefinition TryResolveFinisher()
    {
        if (_finisherCooldownShots > 0)
        {
            return null;
        }

        foreach (ComboFinisherDefinition finisher in GameConfigSO.Instance.comboFinishers)
        {
            if (finisher != null &&
                FinisherCharge >= finisher.chargeCost &&
                _shot.PopCount >= finisher.minPopCount &&
                _shot.ScoreBurst >= finisher.minScoreBurst &&
                _shot.RecipeCount >= finisher.minRecipeCount &&
                _shot.Stickers.Count >= finisher.minStickerCount)
            {
                return finisher;
            }
        }

        return null;
    }

    private void ApplyFinisher(ComboFinisherDefinition finisher, ScoreManager scoreManager)
    {
        FinisherCharge = Mathf.Max(0f, FinisherCharge - finisher.chargeCost);
        _finisherCooldownShots = finisher.cooldownShots;
        switch (finisher.effectType)
        {
            case FinisherEffectType.RadialPop:
                _balloonWall?.PopRandomNearby(_shot.LastPosition, finisher.radius, finisher.power, null, null);
                break;
            case FinisherEffectType.DartRain:
                _balloonWall?.PopRandomAnywhere(finisher.power + 1, null);
                break;
            case FinisherEffectType.GoldRush:
                GainTickets(finisher.power + 2);
                scoreManager.AddScoreDelta(finisher.power * 90);
                break;
            case FinisherEffectType.CloneStorm:
                _balloonWall?.PopRandomNearby(_shot.LastPosition, finisher.radius * 0.9f, finisher.power + 2, null, null);
                break;
            case FinisherEffectType.EchoBurst:
                _balloonWall?.PopRandomNearby(_shot.LastPosition, finisher.radius, finisher.power + _shot.BounceCount, null, null);
                break;
            case FinisherEffectType.CleanupSweep:
                GainTickets(1);
                _balloonWall?.PopHazardsFirst(finisher.power, null);
                break;
        }

        PublishFinisherCharge();
        EventBus.Publish(new FinisherTriggeredEvent { FinisherId = finisher.finisherId, DisplayName = finisher.displayName });
    }
}
