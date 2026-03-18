using UnityEngine;

public partial class RunSystemsManager
{
    private PaintRecipeDefinition TryResolveRecipe()
    {
        if (_shot.Tags.Count < 2)
        {
            return null;
        }

        int unlockRoom = GameConfigSO.Instance.recipeUnlockRoom + _perkManager.RecipeUnlockRoomDelta;
        if (!_perkManager.RecipesAlwaysOn && CurrentRoomNumber < unlockRoom)
        {
            return null;
        }

        BalloonColor primary = _shot.Tags[_shot.Tags.Count - 1];
        BalloonColor secondary = _shot.Tags[_shot.Tags.Count - 2];
        if (_perkManager.ReversePaintOrder ^ _shot.ReverseOrder)
        {
            (primary, secondary) = (secondary, primary);
        }

        foreach (PaintRecipeDefinition recipe in GameConfigSO.Instance.paintRecipes)
        {
            if (recipe != null && recipe.primaryColor == primary && recipe.secondaryColor == secondary)
            {
                return recipe;
            }
        }

        return null;
    }

    private void ApplyRecipe(PaintRecipeDefinition recipe, ScoreManager scoreManager)
    {
        _shot.RecipeCount++;
        GainFinisherCharge(GameConfigSO.Instance.finisherChargePerRecipe);
        switch (recipe.effectType)
        {
            case PaintRecipeEffectType.AdjacentBurst:
            case PaintRecipeEffectType.CloneBurst:
                _balloonWall?.PopRandomNearby(_shot.LastPosition, recipe.radius * _perkManager.PaintRadiusMultiplier, recipe.power, null, null);
                break;
            case PaintRecipeEffectType.TicketBurst:
                GainTickets(recipe.power + 1);
                break;
            case PaintRecipeEffectType.SafetyDividend:
                GainTickets(recipe.power);
                GainFinisherCharge(0.15f * recipe.power);
                break;
            case PaintRecipeEffectType.ScoreSpike:
                scoreManager.AddScoreDelta(120 * recipe.power);
                break;
            case PaintRecipeEffectType.AnchorPulse:
                _balloonWall?.PopRandomNearby(_shot.LastPosition, recipe.radius * 1.2f, recipe.power + 1, null, null);
                break;
            case PaintRecipeEffectType.JackpotShift:
                GainTickets(recipe.power);
                scoreManager.AddScoreDelta(90 * recipe.power);
                break;
            case PaintRecipeEffectType.PrimerMark:
                _shot.ReverseOrder = !_shot.ReverseOrder;
                break;
            case PaintRecipeEffectType.RicochetEcho:
                GainFinisherCharge(0.25f);
                _balloonWall?.PopRandomNearby(_shot.LastPosition, recipe.radius, recipe.power, null, null);
                break;
        }

        EventBus.Publish(new RecipeTriggeredEvent { RecipeId = recipe.recipeId, DisplayName = recipe.displayName });
    }
}
