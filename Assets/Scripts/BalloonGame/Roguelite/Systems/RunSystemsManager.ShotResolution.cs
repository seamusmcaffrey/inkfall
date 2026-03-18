using UnityEngine;

public partial class RunSystemsManager
{
    public ShotResolution ResolveShotEnd(ScoreManager scoreManager)
    {
        var resolution = new ShotResolution();
        _shot.Active = false;
        if (_finisherCooldownShots > 0)
        {
            _finisherCooldownShots--;
        }

        PaintRecipeDefinition recipe = TryResolveRecipe();
        if (recipe != null)
        {
            resolution.RecipeName = recipe.displayName;
            ApplyRecipe(recipe, scoreManager);
        }

        ComboFinisherDefinition finisher = TryResolveFinisher();
        if (finisher != null)
        {
            resolution.FinisherName = finisher.displayName;
            ApplyFinisher(finisher, scoreManager);
        }
        return resolution;
    }

    public bool ShouldOfferBankDecision(int currentScore, int targetScore, int dartsRemaining)
    {
        return currentScore >= targetScore && dartsRemaining > 0;
    }
}
