using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Premium in-game HUD matching the noir carnival reference.
/// Layout: Room badge (left), Score/Target bar (center), Darts + Currency (right).
/// </summary>
[DisallowMultipleComponent]
public class InGameHUD : MonoBehaviour
{
    private Canvas _canvas;
    private RectTransform _safeRoot;
    private TextMeshProUGUI _roomNumber;
    private TextMeshProUGUI _roomLabel;
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _targetText;
    private TextMeshProUGUI _dartsText;
    private TextMeshProUGUI _currencyText;
    private TextMeshProUGUI _messageText;
    private Image _progressTrack;
    private Image _progressFill;
    private Image _progressGlow;
    private HorizontalLayoutGroup _dartTray;
    private int _displayedScore;
    private int _actualScore;
    private int _targetScore;
    private float _displayedProgress;

    private void Awake() { BuildUi(); }

    private void OnEnable()
    {
        EventBus.Subscribe<ScoreChangedEvent>(HandleScoreChanged);
        EventBus.Subscribe<DartsRemainingChangedEvent>(HandleDartsChanged);
        EventBus.Subscribe<RoomStartedEvent>(HandleRoomStarted);
        EventBus.Subscribe<RoomClearedEvent>(HandleRoomCleared);
        EventBus.Subscribe<RoomFailedEvent>(HandleRoomFailed);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ScoreChangedEvent>(HandleScoreChanged);
        EventBus.Unsubscribe<DartsRemainingChangedEvent>(HandleDartsChanged);
        EventBus.Unsubscribe<RoomStartedEvent>(HandleRoomStarted);
        EventBus.Unsubscribe<RoomClearedEvent>(HandleRoomCleared);
        EventBus.Unsubscribe<RoomFailedEvent>(HandleRoomFailed);
    }

    private void Update()
    {
        if (_scoreText == null || _progressFill == null) return;
        _displayedScore = Mathf.RoundToInt(Mathf.MoveTowards(_displayedScore, _actualScore, UIConfigSO.Instance.scoreCountSpeed * Time.unscaledDeltaTime));
        _scoreText.text = $"SCORE: {_displayedScore:N0} / TARGET: {_targetScore:N0}";
        float desired = _targetScore <= 0 ? 0f : Mathf.Clamp01((float)_displayedScore / _targetScore);
        _displayedProgress = Mathf.MoveTowards(_displayedProgress, desired, UIConfigSO.Instance.progressBarLerpSpeed * Time.unscaledDeltaTime);
        _progressFill.fillAmount = _displayedProgress;
        float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * UIConfigSO.Instance.progressBarGlowPulseSpeed * Mathf.PI * 2f) * 0.5f;
        _progressGlow.color = _displayedProgress >= UIConfigSO.Instance.progressBarGlowThreshold
            ? new Color(UIColors.ProgressBarGlow.r, UIColors.ProgressBarGlow.g, UIColors.ProgressBarGlow.b, pulse * UIColors.ProgressBarGlow.a)
            : Color.clear;
    }

    private void HandleScoreChanged(ScoreChangedEvent evt)
    {
        _actualScore = evt.CurrentScore;
        _targetScore = evt.TargetScore;
    }

    private void HandleDartsChanged(DartsRemainingChangedEvent evt)
    {
        _dartsText.text = $"DARTS: {evt.DartsRemaining}";
        while (_dartTray.transform.childCount < evt.StartingDarts)
            CreateDartIcon(_dartTray.transform);
        for (int i = 0; i < _dartTray.transform.childCount; i++)
        {
            Image icon = _dartTray.transform.GetChild(i).GetComponent<Image>();
            icon.color = i < evt.DartsRemaining ? UIColors.DartBlue : new Color(1f, 1f, 1f, 0.12f);
        }
    }

    private void HandleRoomStarted(RoomStartedEvent evt)
    {
        _roomLabel.text = "ROOM";
        _roomNumber.text = evt.RoomNumber.ToString();
        _messageText.gameObject.SetActive(false);
    }

    private void HandleRoomCleared(RoomClearedEvent evt) { ShowMessage("ROOM CLEARED", UIColors.ClearedGreen); }
    private void HandleRoomFailed(RoomFailedEvent evt) { ShowMessage("ROOM FAILED", UIColors.FailedRed); }

    private void ShowMessage(string msg, Color color)
    {
        _messageText.text = msg;
        _messageText.color = color;
        _messageText.gameObject.SetActive(true);
    }

    private void BuildUi()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Object.Destroy(transform.GetChild(i).gameObject);

        _canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);

        GameObject safeRoot = new("SafeArea");
        safeRoot.transform.SetParent(transform, false);
        _safeRoot = safeRoot.AddComponent<RectTransform>();
        safeRoot.AddComponent<SafeAreaHandler>();

        BuildTopBar();
        BuildProgressBar();
        BuildDartTray();
        BuildComboAndFloating();
        BuildMessageOverlay();
    }

    private void BuildTopBar()
    {
        RectTransform topBar = CreatePanel("TopBar", _safeRoot, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -GameConstants.HUD_TOP_MARGIN), new Vector2(0f, 64f));
        Image bg = topBar.gameObject.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.02f, 0.04f, 0.85f);

        _roomLabel = CreateLabel("RoomLabel", topBar, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(GameConstants.HUD_SIDE_MARGIN, 12f), 18f, UIColors.TargetGray, TextAlignmentOptions.Left);
        _roomNumber = CreateLabel("RoomNum", topBar, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(GameConstants.HUD_SIDE_MARGIN, -8f), 36f, UIColors.ScoreWhite, TextAlignmentOptions.Left);
        _scoreText = CreateLabel("ScoreTarget", topBar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f), 20f, UIColors.ScoreWhite, TextAlignmentOptions.Center);
        _dartsText = CreateLabel("Darts", topBar, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-GameConstants.HUD_SIDE_MARGIN - 80f, 0f), 22f, UIColors.DartBlue, TextAlignmentOptions.Right);
        _currencyText = CreateLabel("Currency", topBar, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-GameConstants.HUD_SIDE_MARGIN, 0f), 20f, UIColors.ComboGold, TextAlignmentOptions.Right);
        _currencyText.text = "0";
    }

    private void BuildProgressBar()
    {
        RectTransform bar = CreatePanel("ProgressBar", _safeRoot, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -GameConstants.HUD_TOP_MARGIN - 68f), new Vector2(-GameConstants.HUD_SIDE_MARGIN * 2f, 10f));
        _progressTrack = bar.gameObject.AddComponent<Image>();
        _progressTrack.color = UIColors.ProgressBarTrack;
        RectTransform fill = CreatePanel("Fill", bar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        _progressFill = fill.gameObject.AddComponent<Image>();
        _progressFill.type = Image.Type.Filled;
        _progressFill.fillMethod = Image.FillMethod.Horizontal;
        _progressFill.color = UIColors.ProgressBarFillStart;
        _progressGlow = CreatePanel("Glow", bar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject.AddComponent<Image>();
        _progressGlow.color = Color.clear;
    }

    private void BuildDartTray()
    {
        RectTransform dartRoot = CreatePanel("DartTray", _safeRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 80f), new Vector2(320f, 60f));
        _dartTray = dartRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        _dartTray.spacing = 12f;
        _dartTray.childAlignment = TextAnchor.MiddleCenter;
        _dartTray.childControlHeight = false;
        _dartTray.childControlWidth = false;
    }

    private void BuildComboAndFloating()
    {
        GameObject combo = new("ComboDisplay");
        combo.transform.SetParent(_safeRoot, false);
        RectTransform cr = combo.AddComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
        cr.anchoredPosition = new Vector2(0f, 220f);
        combo.AddComponent<ComboDisplay>();

        GameObject floats = new("FloatingScores");
        floats.transform.SetParent(_safeRoot, false);
        floats.AddComponent<RectTransform>();
        floats.AddComponent<FloatingScoreText>();
    }

    private void BuildMessageOverlay()
    {
        _messageText = CreateLabel("Message", _safeRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, UIConfigSO.Instance.messageFontSize, UIColors.ScoreWhite, TextAlignmentOptions.Center);
        _messageText.gameObject.SetActive(false);
    }

    private static TextMeshProUGUI CreateLabel(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, float size, Color color, TextAlignmentOptions align)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = aMin; r.anchorMax = aMax;
        r.pivot = new Vector2(aMin.x, aMin.y);
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(420f, 120f);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = size; t.color = color; t.alignment = align; t.raycastTarget = false;
        return t;
    }

    private static RectTransform CreatePanel(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = aMin; r.anchorMax = aMax;
        r.anchoredPosition = pos; r.sizeDelta = size;
        return r;
    }

    private static void CreateDartIcon(Transform parent)
    {
        GameObject go = new("DartIcon");
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(24f, 48f);
        go.AddComponent<Image>().color = UIColors.DartBlue;
    }
}
