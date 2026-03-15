using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Premium in-game HUD matching the noir carnival reference.
/// Layout: Room badge (left), Score/Target bar (center), Darts + Currency (right).
/// Builder methods are in InGameHUD.Builder.cs.
/// </summary>
[DisallowMultipleComponent]
public partial class InGameHUD : MonoBehaviour
{
    private const float TopBarHeight = 28f;
    private const float BadgeSize = 22f;
    private const float BadgeBorderWidth = 1f;
    private const float ProgressBarHeight = 4f;
    private const float DartIconWidth = 6f;
    private const float DartIconHeight = 22f;
    private const float DartTipHeight = 5f;

    private Canvas _canvas;
    private RectTransform _safeRoot;
    private TextMeshProUGUI _roomNumber;
    private TextMeshProUGUI _roomLabel;
    private TextMeshProUGUI _scoreLabel;
    private TextMeshProUGUI _scoreValue;
    private TextMeshProUGUI _targetLabel;
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
        if (_scoreValue == null || _progressFill == null) return;

        UIConfigSO ui = UIConfigSO.Instance;
        _displayedScore = Mathf.RoundToInt(
            Mathf.MoveTowards(_displayedScore, _actualScore, ui.scoreCountSpeed * Time.unscaledDeltaTime));
        _scoreValue.text = _displayedScore.ToString("N0");
        _targetLabel.text = $"TARGET: {_targetScore:N0}";

        float desired = _targetScore <= 0 ? 0f : Mathf.Clamp01((float)_displayedScore / _targetScore);
        _displayedProgress = Mathf.MoveTowards(_displayedProgress, desired, ui.progressBarLerpSpeed * Time.unscaledDeltaTime);
        _progressFill.fillAmount = _displayedProgress;
        _progressFill.color = UIColors.GetProgressFillColor(_displayedProgress);

        float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * ui.progressBarGlowPulseSpeed * Mathf.PI * 2f) * 0.5f;
        _progressGlow.color = _displayedProgress >= ui.progressBarGlowThreshold
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
        _dartsText.text = $"{evt.DartsRemaining}";
        while (_dartTray.transform.childCount < evt.StartingDarts)
            CreateDartIcon(_dartTray.transform);
        for (int i = 0; i < _dartTray.transform.childCount; i++)
        {
            Transform icon = _dartTray.transform.GetChild(i);
            SetDartIconColor(icon, i < evt.DartsRemaining);
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
}
