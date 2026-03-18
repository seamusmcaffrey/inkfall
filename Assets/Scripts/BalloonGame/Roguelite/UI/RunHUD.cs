using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Run-level HUD showing room number, Ink, and perks.
/// </summary>
[DisallowMultipleComponent]
public class RunHUD : MonoBehaviour
{
    [SerializeField] private PerkManager _perkManager;

    private RectTransform _safeRoot;
    private TextMeshProUGUI _roomText;
    private TextMeshProUGUI _ticketText;
    private TextMeshProUGUI _finisherText;
    private TextMeshProUGUI _loadoutText;
    private TextMeshProUGUI _perkText;
    private TextMeshProUGUI _statusText;

    private void Awake()
    {
        ComponentUtility.ResolveSceneReference(this, ref _perkManager);
        EnsureUi();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RoomStartedEvent>(HandleRoomStarted);
        EventBus.Subscribe<RunCurrencyChangedEvent>(HandleCurrencyChanged);
        EventBus.Subscribe<PerkSelectedEvent>(HandlePerkSelected);
        EventBus.Subscribe<LoadoutSelectedEvent>(HandleLoadoutSelected);
        EventBus.Subscribe<RelicSelectedEvent>(HandleRelicSelected);
        EventBus.Subscribe<FinisherChargeChangedEvent>(HandleFinisherChargeChanged);
        EventBus.Subscribe<PerkChainTriggeredEvent>(HandlePerkChainTriggered);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RoomStartedEvent>(HandleRoomStarted);
        EventBus.Unsubscribe<RunCurrencyChangedEvent>(HandleCurrencyChanged);
        EventBus.Unsubscribe<PerkSelectedEvent>(HandlePerkSelected);
        EventBus.Unsubscribe<LoadoutSelectedEvent>(HandleLoadoutSelected);
        EventBus.Unsubscribe<RelicSelectedEvent>(HandleRelicSelected);
        EventBus.Unsubscribe<FinisherChargeChangedEvent>(HandleFinisherChargeChanged);
        EventBus.Unsubscribe<PerkChainTriggeredEvent>(HandlePerkChainTriggered);
    }

    private void HandleRoomStarted(RoomStartedEvent evt)
    {
        _roomText.text = $"RUN ROOM {evt.RoomNumber}";
        _statusText.text = _perkManager != null && (_perkManager.ActivePerks.Count > 0 || _perkManager.ActiveRelics.Count > 0)
            ? "REACTIONS LIVE"
            : "BUILD ENGINE";
    }

    private void HandleCurrencyChanged(RunCurrencyChangedEvent evt)
    {
        _ticketText.text = $"TICKETS {evt.PrizeTickets}";
    }

    private void HandlePerkSelected(PerkSelectedEvent evt)
    {
        if (string.IsNullOrEmpty(_perkText.text))
        {
            _perkText.text = evt.Perk.perkName;
        }
        else
        {
            _perkText.text += $"  •  {evt.Perk.perkName}";
        }

        _statusText.text = $"READY {evt.Perk.perkName}";
    }

    private void HandleLoadoutSelected(LoadoutSelectedEvent evt)
    {
        _loadoutText.text = evt.Loadout != null ? $"LOADOUT {evt.Loadout.displayName}" : "LOADOUT NONE";
    }

    private void HandleRelicSelected(RelicSelectedEvent evt)
    {
        if (!string.IsNullOrEmpty(evt.Relic?.perkName))
        {
            _statusText.text = $"RELIC {evt.Relic.perkName}";
        }
    }

    private void HandleFinisherChargeChanged(FinisherChargeChangedEvent evt)
    {
        _finisherText.text = $"FINISHER {evt.Charge:0.0}/{evt.MaxCharge:0.0}";
    }

    private void HandlePerkChainTriggered(PerkChainTriggeredEvent evt)
    {
        _statusText.text = $"{FormatStyle(evt.VisualStyle)} {evt.DisplayName}";
    }

    private void EnsureUi()
    {
        if (_roomText == null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);

        if (_safeRoot == null)
        {
            GameObject vp = new("ViewportRoot");
            vp.transform.SetParent(transform, false);
            RectTransform vpRect = vp.AddComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            vp.AddComponent<ViewportConstraint>();

            GameObject safe = new("SafeArea");
            safe.transform.SetParent(vp.transform, false);
            _safeRoot = safe.AddComponent<RectTransform>();
            safe.AddComponent<SafeAreaHandler>();
        }

        if (_roomText == null)
        {
            _roomText = CreateText("Room", new Vector2(32f, -180f), 16f);
        }

        if (_ticketText == null)
        {
            _ticketText = CreateText("Tickets", new Vector2(32f, -200f), 16f);
        }

        if (_finisherText == null)
        {
            _finisherText = CreateText("Finisher", new Vector2(32f, -220f), 16f);
        }

        if (_loadoutText == null)
        {
            _loadoutText = CreateText("Loadout", new Vector2(32f, -240f), 14f);
        }

        if (_perkText == null)
        {
            _perkText = CreateText("Perks", new Vector2(32f, -260f), 14f);
        }

        if (_statusText == null)
        {
            _statusText = CreateText("Status", new Vector2(32f, -280f), 14f);
        }

        _ticketText.text = "TICKETS 0";
        _finisherText.text = "FINISHER 0.0/3.0";
        _statusText.text = "BUILD ENGINE";
    }

    private TextMeshProUGUI CreateText(string name, Vector2 anchoredPosition, float fontSize)
    {
        GameObject go = new(name);
        go.transform.SetParent(_safeRoot, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(700f, 40f);
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.fontSize = fontSize;
        label.color = new Color(0f, 0.55f, 0.55f, 0.7f);
        label.alignment = TextAlignmentOptions.Left;
        label.raycastTarget = false;
        return label;
    }

    private static string FormatStyle(PerkChainVisualStyle style) => style switch
    {
        PerkChainVisualStyle.Lightning => "LIGHTNING",
        PerkChainVisualStyle.Acid => "ACID",
        PerkChainVisualStyle.Rat => "RATS",
        PerkChainVisualStyle.Sweep => "SWEEP",
        PerkChainVisualStyle.Echo => "ECHO",
        _ => "CHAIN",
    };
}
