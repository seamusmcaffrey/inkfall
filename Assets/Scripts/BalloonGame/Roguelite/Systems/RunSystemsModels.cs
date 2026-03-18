using System;

public struct BalloonScoreResult
{
    public int FinalPoints;
    public int TicketDelta;
    public float FinisherChargeDelta;
    public bool WasJackpot;
    public int AdjacentBursts;
    public int ComboCount;
    public float ComboMultiplier;
}

public struct RoomRewardPreview
{
    public int OverflowScore;
    public int TicketGain;
    public float FinisherGain;
}

public struct ShotResolution
{
    public int MissTax;
    public bool OfferBankDecision;
    public string RecipeName;
    public string FinisherName;
}

[Serializable]
public class RunShopOffer
{
    public string offerId;
    public string title;
    public string description;
    public int cost;
    public RunShopOfferType offerType;
    public PerkSO perkReward;
}

public enum RunShopOfferType
{
    BuyPerk,
    BuyRelic,
    AddDart,
    PolishEngine,
    RefreshStock,
}
