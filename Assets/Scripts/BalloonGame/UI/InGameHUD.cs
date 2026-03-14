using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Premium in-game HUD driven entirely by EventBus updates.
/// </summary>
[DisallowMultipleComponent]
public class InGameHUD : MonoBehaviour
{
    private Canvas _canvas;
    private RectTransform _safeRoot;
    private TextMeshProUGUI _scoreValue;
    private TextMeshProUGUI _targetValue;
    private TextMeshProUGUI _roomBadge;
    private TextMeshProUGUI _messageText;
    private Image _progressFill;
    private Image _progressGlow;
    private HorizontalLayoutGroup _dartTray;
    private int _displayedScore;
    private int _actualScore;
    private int _targetScore;
    private float _displayedProgress;

    private void Awake()
    {
        BuildUi();
    }

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
        if (_scoreValue == null || _progressFill == null)
        {
            return;
        }

        _displayedScore = Mathf.RoundToInt(Mathf.MoveTowards(_displayedScore, _actualScore, UIConfigSO.Instance.scoreCountSpeed * Time.unscaledDeltaTime));
        _scoreValue.text = _displayedScore.ToString();

        float desiredFill = _targetScore <= 0 ? 0f : Mathf.Clamp01((float)_displayedScore / _targetScore);
        _displayedProgress = Mathf.MoveTowards(_displayedProgress, desiredFill, UIConfigSO.Instance.progressBarLerpSpeed * Time.unscaledDeltaTime);
        _progressFill.fillAmount = _displayedProgress;

        float glowPulse = 0.5f + Mathf.Sin(Time.unscaledTime * UIConfigSO.Instance.progressBarGlowPulseSpeed * Mathf.PI * 2f) * 0.5f;
        _progressGlow.color = _displayedProgress >= UIConfigSO.Instance.progressBarGlowThreshold
            ? new Color(UIColors.ProgressBarGlow.r, UIColors.ProgressBarGlow.g, UIColors.ProgressBarGlow.b, glowPulse * UIColors.ProgressBarGlow.a)
            : Color.clear;
    }

    private void HandleScoreChanged(ScoreChangedEvent evt)
    {
        _actualScore = evt.CurrentScore;
        _targetScore = evt.TargetScore;
        _targetValue.text = $"TARGET {evt.TargetScore}";
    }

    private void HandleDartsChanged(DartsRemainingChangedEvent evt)
    {
        while (_dartTray.transform.childCount < evt.StartingDarts)
        {
            CreateDartIcon(_dartTray.transform);
        }

        for (int index = 0; index < _dartTray.transform.childCount; index++)
        {
            Image icon = _dartTray.transform.GetChild(index).GetComponent<Image>();
            icon.color = index < evt.DartsRemaining ? UIColors.DartBlue : new Color(1f, 1f, 1f, 0.12f);
        }
    }

    private void HandleRoomStarted(RoomStartedEvent evt)
    {
        _roomBadge.text = $"ROOM {evt.RoomNumber}\n{evt.RoomName.ToUpperInvariant()}";
        _messageText.gameObject.SetActive(false);
    }

    private void HandleRoomCleared(RoomClearedEvent evt)
    {
        ShowMessage("ROOM CLEARED", UIColors.ClearedGreen);
    }

    private void HandleRoomFailed(RoomFailedEvent evt)
    {
        ShowMessage("ROOM FAILED", UIColors.FailedRed);
    }

    private void ShowMessage(string message, Color color)
    {
        _messageText.text = message;
        _messageText.color = color;
        _messageText.gameObject.SetActive(true);
    }

    private void BuildUi()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }

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

        _scoreValue = CreateLabel("ScoreValue", _safeRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(GameConstants.HUD_SIDE_MARGIN, -GameConstants.HUD_TOP_MARGIN), 72f, UIColors.ScoreWhite, TextAlignmentOptions.Left);
        _targetValue = CreateLabel("TargetValue", _safeRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-GameConstants.HUD_SIDE_MARGIN, -GameConstants.HUD_TOP_MARGIN), 32f, UIColors.TargetGray, TextAlignmentOptions.Right);
        _roomBadge = CreateLabel("RoomBadge", _safeRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -GameConstants.HUD_TOP_MARGIN), 28f, UIColors.RoomPink, TextAlignmentOptions.Center);
        _messageText = CreateLabel("Message", _safeRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, UIConfigSO.Instance.messageFontSize, UIColors.ScoreWhite, TextAlignmentOptions.Center);
        _messageText.gameObject.SetActive(false);

        RectTransform progressRoot = CreatePanel("ProgressRoot", _safeRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -140f), new Vector2(-GameConstants.HUD_SIDE_MARGIN * 2f, GameConstants.HUD_PROGRESS_HEIGHT));
        Image track = progressRoot.gameObject.AddComponent<Image>();
        track.color = UIColors.ProgressBarTrack;

        RectTransform fillRoot = CreatePanel("Fill", progressRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        _progressFill = fillRoot.gameObject.AddComponent<Image>();
        _progressFill.type = Image.Type.Filled;
        _progressFill.fillMethod = Image.FillMethod.Horizontal;
        _progressFill.color = UIColors.ProgressBarFillStart;

        _progressGlow = CreatePanel("Glow", progressRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject.AddComponent<Image>();
        _progressGlow.color = Color.clear;

        RectTransform dartRoot = CreatePanel("Darts", _safeRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(320f, 60f));
        _dartTray = dartRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        _dartTray.spacing = 12f;
        _dartTray.childAlignment = TextAnchor.MiddleCenter;
        _dartTray.childControlHeight = false;
        _dartTray.childControlWidth = false;

        GameObject comboGo = new("ComboDisplay");
        comboGo.transform.SetParent(_safeRoot, false);
        RectTransform comboRect = comboGo.AddComponent<RectTransform>();
        comboRect.anchorMin = new Vector2(0.5f, 0.5f);
        comboRect.anchorMax = new Vector2(0.5f, 0.5f);
        comboRect.anchoredPosition = new Vector2(0f, 220f);
        comboGo.AddComponent<ComboDisplay>();

        GameObject floatGo = new("FloatingScores");
        floatGo.transform.SetParent(_safeRoot, false);
        floatGo.AddComponent<RectTransform>();
        floatGo.AddComponent<FloatingScoreText>();
    }

    private static TextMeshProUGUI CreateLabel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, float size, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMin.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(420f, 120f);

        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
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

    private static void CreateDartIcon(Transform parent)
    {
        GameObject go = new("DartIcon");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(24f, 48f);
        Image image = go.AddComponent<Image>();
        image.color = UIColors.DartBlue;
    }
}
