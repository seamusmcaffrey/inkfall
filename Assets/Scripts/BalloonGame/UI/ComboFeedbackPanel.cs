using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compact combo window and perk-chain callout panel for the in-game HUD.
/// </summary>
[DisallowMultipleComponent]
public class ComboFeedbackPanel : MonoBehaviour
{
    private const float CalloutPunctureOffset = 6f;

    private CanvasGroup _canvasGroup;
    private RectTransform _root;
    private RectTransform _windowRoot;
    private RectTransform _calloutRoot;
    private CanvasGroup _calloutGroup;
    private Image _windowTrack;
    private Image _windowFill;
    private TextMeshProUGUI _windowLabel;
    private Image _calloutBackground;
    private Image _calloutAccent;
    private TextMeshProUGUI _calloutTitle;
    private TextMeshProUGUI _calloutBody;
    private TextMeshProUGUI _calloutFooter;

    private bool _windowActive;
    private int _windowComboCount;
    private float _windowRemaining;
    private float _windowMax;
    private float _calloutRemaining;
    private float _calloutDuration;
    private float _calloutPunch;

    private void Awake()
    {
        EnsureUi();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ComboWindowStateEvent>(HandleComboWindowState);
        EventBus.Subscribe<PerkChainTriggeredEvent>(HandlePerkChainTriggered);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ComboWindowStateEvent>(HandleComboWindowState);
        EventBus.Unsubscribe<PerkChainTriggeredEvent>(HandlePerkChainTriggered);
    }

    private void Update()
    {
        if (_calloutRemaining > 0f)
        {
            _calloutRemaining = Mathf.Max(0f, _calloutRemaining - Time.unscaledDeltaTime);
            float normalized = _calloutDuration > 0f ? _calloutRemaining / _calloutDuration : 0f;
            _calloutGroup.alpha = normalized * normalized;

            float punchT = 1f - _calloutGroup.alpha;
            float scale = Mathf.Lerp(1f, _calloutPunch, punchT);
            _calloutRoot.localScale = Vector3.one * scale;
            _calloutRoot.anchoredPosition = new Vector2(0f, -CalloutPunctureOffset * punchT);
        }
        else
        {
            _calloutGroup.alpha = 0f;
            _calloutRoot.localScale = Vector3.one;
            _calloutRoot.anchoredPosition = Vector2.zero;
        }

        bool hasVisibleContent = _windowActive || _calloutGroup.alpha > 0f;
        float targetAlpha = hasVisibleContent ? 1f : 0f;
        float fadeStep = Time.unscaledDeltaTime / Mathf.Max(UIConfigSO.Instance.comboCalloutFadeDuration, 0.01f);
        _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, fadeStep);

        if (_windowActive)
        {
            float maxSeconds = Mathf.Max(_windowMax, 0.01f);
            float normalized = Mathf.Clamp01(_windowRemaining / maxSeconds);
            _windowFill.fillAmount = normalized;
            _windowLabel.text = $"x{_windowComboCount}  {_windowRemaining:0.0}s";
            _windowFill.color = Color.Lerp(UIColors.HazardRed, UIColors.ComboGold, normalized);
            _windowTrack.color = new Color(UIColors.BadgeBackground.r, UIColors.BadgeBackground.g, UIColors.BadgeBackground.b, 0.92f);
        }
        else
        {
            _windowFill.fillAmount = 0f;
            _windowLabel.text = string.Empty;
        }
    }

    private void HandleComboWindowState(ComboWindowStateEvent evt)
    {
        EnsureUi();
        _windowActive = evt.IsActive && evt.ComboCount > 0;
        _windowComboCount = evt.ComboCount;
        _windowRemaining = Mathf.Max(0f, evt.RemainingSeconds);
        _windowMax = Mathf.Max(evt.MaxSeconds, 0.01f);

        if (_windowActive)
        {
            _windowLabel.text = $"x{evt.ComboCount}  {evt.RemainingSeconds:0.0}s";
        }
        else
        {
            _windowLabel.text = string.Empty;
        }
    }

    private void HandlePerkChainTriggered(PerkChainTriggeredEvent evt)
    {
        EnsureUi();

        UIConfigSO ui = UIConfigSO.Instance;
        _calloutRemaining = ui.comboCalloutDuration;
        _calloutDuration = Mathf.Max(ui.comboCalloutDuration, 0.01f);
        _calloutPunch = ui.comboCalloutPunchScale;

        _calloutTitle.text = FormatStyle(evt.VisualStyle);
        _calloutBody.text = string.IsNullOrEmpty(evt.DisplayName) ? evt.PerkId : evt.DisplayName;
        _calloutFooter.text = BuildFooter(evt);

        Color accent = evt.AccentColor;
        _calloutAccent.color = accent;
        _calloutBackground.color = new Color(accent.r, accent.g, accent.b, 0.16f);
        _calloutGroup.alpha = 1f;
        _calloutRoot.localScale = Vector3.one * _calloutPunch;
        _canvasGroup.alpha = 1f;
    }

    private void EnsureUi()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _canvasGroup.alpha = 0f;
        }

        if (_root == null)
        {
            _root = ComponentUtility.EnsureComponent<RectTransform>(gameObject);
            _root.sizeDelta = new Vector2(UIConfigSO.Instance.comboCalloutWidth, UIConfigSO.Instance.comboCalloutHeight + 34f);
        }

        if (_windowRoot == null)
        {
            _windowRoot = CreatePanel("Window", _root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -2f), new Vector2(UIConfigSO.Instance.comboTimerWidth, UIConfigSO.Instance.comboTimerHeight + 16f));
            _windowTrack = CreateImage("Track", _windowRoot, new Vector2(0f, 0f), new Vector2(1f, 0f),
                Vector2.zero, new Vector2(0f, UIConfigSO.Instance.comboTimerHeight), UIColors.BadgeBackground);
            _windowFill = CreateImage("Fill", _windowTrack.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero, UIColors.ProgressBarFillStart);
            _windowFill.type = Image.Type.Filled;
            _windowFill.fillMethod = Image.FillMethod.Horizontal;
            _windowFill.fillAmount = 0f;
            _windowLabel = CreateLabel("Label", _windowRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -1f), UIConfigSO.Instance.comboCalloutTitleSize, UIColors.ComboGold, TextAlignmentOptions.Center);
            _windowLabel.fontStyle = FontStyles.Bold;
        }

        if (_calloutRoot == null)
        {
            _calloutRoot = CreatePanel("Callout", _root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 0f), new Vector2(UIConfigSO.Instance.comboCalloutWidth, UIConfigSO.Instance.comboCalloutHeight));
            _calloutGroup = ComponentUtility.EnsureComponent<CanvasGroup>(_calloutRoot.gameObject);
            _calloutGroup.blocksRaycasts = false;
            _calloutGroup.interactable = false;
            _calloutGroup.alpha = 0f;

            _calloutBackground = CreateImage("Background", _calloutRoot, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, UIColors.BadgeBackground);
            _calloutAccent = CreateImage("Accent", _calloutRoot, new Vector2(0f, 0f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(4f, 0f), UIColors.ComboGold);

            _calloutTitle = CreateLabel("Title", _calloutRoot, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(10f, -5f), UIConfigSO.Instance.comboCalloutTitleSize, UIColors.ScoreWhite, TextAlignmentOptions.TopLeft);
            _calloutTitle.fontStyle = FontStyles.Bold;

            _calloutBody = CreateLabel("Body", _calloutRoot, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(10f, 0f), UIConfigSO.Instance.comboCalloutBodySize, UIColors.ScoreWhite, TextAlignmentOptions.MidlineLeft);
            _calloutBody.fontStyle = FontStyles.Bold;

            _calloutFooter = CreateLabel("Footer", _calloutRoot, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(10f, 5f), UIConfigSO.Instance.comboCalloutRowSize, UIColors.TargetGray, TextAlignmentOptions.BottomLeft);
        }
    }

    private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        return rect;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        RectTransform rect = CreatePanel(name, parent, anchorMin, anchorMax, anchoredPosition, sizeDelta);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateLabel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, float size, Color color, TextAlignmentOptions alignment)
    {
        RectTransform rect = CreatePanel(name, parent, anchorMin, anchorMax, anchoredPosition, new Vector2(220f, 48f));
        rect.pivot = new Vector2(anchorMin.x, anchorMin.y);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.enableWordWrapping = false;
        return label;
    }

    private static string FormatStyle(PerkChainVisualStyle style) => style switch
    {
        PerkChainVisualStyle.Lightning => "LIGHTNING",
        PerkChainVisualStyle.Acid => "ACID",
        PerkChainVisualStyle.Rat => "RAT",
        PerkChainVisualStyle.Sweep => "SWEEP",
        PerkChainVisualStyle.Echo => "ECHO",
        _ => "CHAIN",
    };

    private static string BuildFooter(PerkChainTriggeredEvent evt)
    {
        string sourceRow = FormatRow(evt.SourceRow);
        string targetRow = FormatRow(evt.TargetRow);
        string arrow = evt.TargetPosition.x >= evt.SourcePosition.x ? "->" : "<-";
        return evt.ClearsRow
            ? $"{sourceRow} clears {targetRow}"
            : $"{sourceRow} {arrow} {targetRow}";
    }

    private static string FormatRow(int row) => row >= 0 ? $"R{row + 1}" : "R?";
}
