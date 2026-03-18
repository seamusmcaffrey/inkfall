public struct RunCurrencyChangedEvent
{
    public int PrizeTickets;
    public int Delta;
}

public struct FinisherChargeChangedEvent
{
    public float Charge;
    public float MaxCharge;
}

public struct LoadoutSelectedEvent
{
    public StarterLoadoutDefinition Loadout;
}

public struct RelicSelectedEvent
{
    public PerkSO Relic;
}

public struct RecipeTriggeredEvent
{
    public string RecipeId;
    public string DisplayName;
}

public struct FinisherTriggeredEvent
{
    public string FinisherId;
    public string DisplayName;
}
