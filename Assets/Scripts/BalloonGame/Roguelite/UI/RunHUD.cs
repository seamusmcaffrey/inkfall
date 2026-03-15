using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Run-level HUD showing room number, Ink, and perks.
/// </summary>
[DisallowMultipleComponent]
public class RunHUD : MonoBehaviour
{
    private RectTransform _safeRoot;
    private TextMeshProUGUI _roomText;
    private TextMeshProUGUI _inkText;
    private TextMeshProUGUI _perkText;

    private void Awake()
    {
        EnsureUi();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RoomStartedEvent>(HandleRoomStarted);
        EventBus.Subscribe<CurrencyChangedEvent>(HandleCurrencyChanged);
        EventBus.Subscribe<PerkSelectedEvent>(HandlePerkSelected);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RoomStartedEvent>(HandleRoomStarted);
        EventBus.Unsubscribe<CurrencyChangedEvent>(HandleCurrencyChanged);
        EventBus.Unsubscribe<PerkSelectedEvent>(HandlePerkSelected);
    }

    private void HandleRoomStarted(RoomStartedEvent evt)
    {
        _roomText.text = $"RUN ROOM {evt.RoomNumber}";
    }

    private void HandleCurrencyChanged(CurrencyChangedEvent evt)
    {
        _inkText.text = $"INK {evt.TotalInk}";
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

        if (_inkText == null)
        {
            _inkText = CreateText("Ink", new Vector2(32f, -200f), 16f);
        }

        if (_perkText == null)
        {
            _perkText = CreateText("Perks", new Vector2(32f, -220f), 14f);
        }

        _inkText.text = $"INK {SaveManager.Instance.Data.totalInk}";
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
}
