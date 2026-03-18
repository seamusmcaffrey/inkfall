using System.Collections.Generic;
using UnityEngine;

public partial class RunSystemsManager
{
    public bool ShouldOfferShop(int roomNumber)
    {
        return roomNumber > 1 && roomNumber % Mathf.Max(1, GameConfigSO.Instance.shopIntervalRooms) == 0;
    }

    public List<RunShopOffer> GetShopOffers(int roomNumber)
    {
        if (_cachedShopOffers.Count > 0)
        {
            return new List<RunShopOffer>(_cachedShopOffers);
        }

        List<PerkSO> perks = GetPerkChoices(roomNumber + 1);
        List<PerkSO> relics = GetRelicChoices(roomNumber + 1);
        if (perks.Count > 0)
        {
            _cachedShopOffers.Add(new RunShopOffer
            {
                offerId = "perk",
                title = perks[0].perkName,
                description = perks[0].description,
                cost = Mathf.Max(2, Mathf.RoundToInt(perks[0].shopCost * (1f - _perkManager.ShopDiscount))),
                offerType = RunShopOfferType.BuyPerk,
                perkReward = perks[0],
            });
        }

        if (relics.Count > 0)
        {
            _cachedShopOffers.Add(new RunShopOffer
            {
                offerId = "relic",
                title = relics[0].perkName,
                description = relics[0].description,
                cost = Mathf.Max(6, Mathf.RoundToInt(relics[0].shopCost * (1f - _perkManager.ShopDiscount))),
                offerType = RunShopOfferType.BuyRelic,
                perkReward = relics[0],
            });
        }

        _cachedShopOffers.Add(new RunShopOffer
        {
            offerId = "dart",
            title = "Fresh Darts",
            description = "Gain +1 dart for the rest of the run.",
            cost = Mathf.Max(2, Mathf.RoundToInt(4f * (1f - _perkManager.ShopDiscount))),
            offerType = RunShopOfferType.AddDart,
        });
        _cachedShopOffers.Add(new RunShopOffer
        {
            offerId = "polish",
            title = "Polish Engine",
            description = "Gain a small permanent run score polish.",
            cost = Mathf.Max(2, Mathf.RoundToInt(3f * (1f - _perkManager.ShopDiscount))),
            offerType = RunShopOfferType.PolishEngine,
        });

        return new List<RunShopOffer>(_cachedShopOffers);
    }

    public void ClearShopOffers()
    {
        _cachedShopOffers.Clear();
    }
}
