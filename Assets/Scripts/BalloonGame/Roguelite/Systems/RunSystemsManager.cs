using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PerkManager))]
public partial class RunSystemsManager : MonoBehaviour
{
    private sealed class ShotContext
    {
        public readonly List<BalloonColor> Tags = new();
        public readonly HashSet<StickerFamily> Stickers = new();
        public Vector3 LastPosition;
        public int PopCount;
        public int ScoreBurst;
        public int RecipeCount;
        public int BounceCount;
        public bool Active;
        public bool ReverseOrder;

        public void Reset()
        {
            Tags.Clear();
            Stickers.Clear();
            LastPosition = Vector3.zero;
            PopCount = 0;
            ScoreBurst = 0;
            RecipeCount = 0;
            BounceCount = 0;
            Active = false;
            ReverseOrder = false;
        }
    }

    [SerializeField] private PerkManager _perkManager;
    [SerializeField] private BalloonWall _balloonWall;

    private readonly List<PerkSO> _activeRelics = new();
    private readonly List<RunShopOffer> _cachedShopOffers = new();
    private readonly ShotContext _shot = new();
    private int _freeRerollsRemaining;
    private int _rerollsUsedThisRoom;
    private int _finisherCooldownShots;
    private bool _washProtectionArmed;

    public StarterLoadoutDefinition SelectedLoadout { get; private set; }
    public int CurrentRoomNumber { get; private set; }
    public int PrizeTickets { get; private set; }
    public float FinisherCharge { get; private set; }
    public IReadOnlyList<PerkSO> ActiveRelics => _activeRelics;

    private void Awake()
    {
        _perkManager = ComponentUtility.EnsureComponent<PerkManager>(gameObject);
        ComponentUtility.ResolveSceneReference(this, ref _balloonWall);
    }

    public IReadOnlyList<MetaUpgradeSO> GetPurchasedMetaUpgrades()
    {
        var upgrades = new List<MetaUpgradeSO>();
        foreach (MetaUpgradeSO upgrade in GameConfigSO.Instance.metaUpgrades)
        {
            if (upgrade != null && SaveManager.Instance.Data.purchasedMetaUpgradeIds.Contains(upgrade.upgradeId))
            {
                upgrades.Add(upgrade);
            }
        }

        return upgrades;
    }

    public IReadOnlyList<StarterLoadoutDefinition> GetAvailableLoadouts()
    {
        var loadouts = new List<StarterLoadoutDefinition>();
        foreach (StarterLoadoutDefinition loadout in GameConfigSO.Instance.starterLoadouts)
        {
            if (loadout == null || !IsUnlocked(loadout.requiredMetaUpgradeId))
            {
                continue;
            }

            loadouts.Add(loadout);
        }

        return loadouts;
    }

    public StarterLoadoutDefinition ResolveSelectedLoadout()
    {
        IReadOnlyList<StarterLoadoutDefinition> loadouts = GetAvailableLoadouts();
        foreach (StarterLoadoutDefinition loadout in loadouts)
        {
            if (loadout.loadoutId == SaveManager.Instance.Data.selectedLoadoutId)
            {
                return loadout;
            }
        }

        return loadouts.Count > 0 ? loadouts[0] : null;
    }

    public void SaveSelectedLoadout(StarterLoadoutDefinition loadout)
    {
        SaveManager.Instance.Data.selectedLoadoutId = loadout != null ? loadout.loadoutId : "scatter-string";
        SaveManager.Instance.Save();
    }

    public IReadOnlyList<StickerFamily> GetUnlockedStickerFamilies()
    {
        var families = new List<StickerFamily> { StickerFamily.Crown, StickerFamily.Star, StickerFamily.Target };
        foreach (MetaUpgradeSO upgrade in GetPurchasedMetaUpgrades())
        {
            foreach (StickerFamily family in upgrade.unlockedStickerFamilies)
            {
                if (family != StickerFamily.None && !families.Contains(family))
                {
                    families.Add(family);
                }
            }
        }

        return families;
    }

    public void InitializeRun(StarterLoadoutDefinition loadout)
    {
        SelectedLoadout = loadout;
        _activeRelics.Clear();
        _cachedShopOffers.Clear();
        _perkManager.ResetRun();
        _perkManager.SetMetaUpgrades(GetPurchasedMetaUpgrades());
        _perkManager.SetStarterLoadout(loadout);
        PrizeTickets = Mathf.Max(0, GameConfigSO.Instance.startingPrizeTickets + _perkManager.StartingTickets);
        FinisherCharge = Mathf.Max(0f, _perkManager.FinisherFlatCharge);
        _freeRerollsRemaining = _perkManager.ExtraRerolls;
        _rerollsUsedThisRoom = 0;
        _finisherCooldownShots = 0;
        _washProtectionArmed = false;
        _shot.Reset();
        PublishCurrency();
        PublishFinisherCharge();
        EventBus.Publish(new LoadoutSelectedEvent { Loadout = loadout });
    }

    public void BeginRoom(RoomConfig room)
    {
        CurrentRoomNumber = room != null ? room.roomNumber : 1;
        _freeRerollsRemaining = _perkManager.ExtraRerolls;
        _rerollsUsedThisRoom = 0;
        _cachedShopOffers.Clear();
        _washProtectionArmed = false;
        _shot.Reset();
    }

    public void BeginShot()
    {
        _shot.Reset();
        _shot.Active = true;
    }

    public void RegisterBounce()
    {
        if (_shot.Active)
        {
            _shot.BounceCount++;
        }
    }

    public void AddPerk(PerkSO perk)
    {
        _perkManager.AddPerk(perk);
    }

    public void AddRelic(PerkSO relic)
    {
        if (relic == null)
        {
            return;
        }

        _activeRelics.Add(relic);
        _perkManager.AddRelic(relic);
        EventBus.Publish(new RelicSelectedEvent { Relic = relic });
    }

    public bool TrySpendTickets(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (PrizeTickets < amount)
        {
            return false;
        }

        PrizeTickets -= amount;
        PublishCurrency(-amount);
        return true;
    }

    public void GainTickets(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        PrizeTickets = Mathf.Max(0, PrizeTickets + amount);
        PublishCurrency(amount);
    }

    public void GainFinisherCharge(float amount)
    {
        if (Mathf.Approximately(amount, 0f))
        {
            return;
        }

        float multiplier = _perkManager.FinisherChargeMultiplier * (_perkManager.DoubleFinisherCharge ? 2f : 1f);
        FinisherCharge = Mathf.Clamp(FinisherCharge + amount * multiplier, 0f, 3f);
        PublishFinisherCharge();
    }

    public void CommitRunRewards()
    {
        if (PrizeTickets > 0)
        {
            SaveManager.Instance.AddInk(PrizeTickets);
        }
    }

    private bool IsUnlocked(string requiredMetaUpgradeId)
    {
        return string.IsNullOrEmpty(requiredMetaUpgradeId) || SaveManager.Instance.Data.purchasedMetaUpgradeIds.Contains(requiredMetaUpgradeId);
    }

    private void PublishCurrency(int delta = 0)
    {
        EventBus.Publish(new RunCurrencyChangedEvent { PrizeTickets = PrizeTickets, Delta = delta });
    }

    private void PublishFinisherCharge()
    {
        EventBus.Publish(new FinisherChargeChangedEvent { Charge = FinisherCharge, MaxCharge = 3f });
    }
}
