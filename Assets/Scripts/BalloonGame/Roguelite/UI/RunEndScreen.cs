using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Premium run-end summary overlay with dark backdrop, neon accents, and stat rows.
/// Displays: rooms cleared, total score, best combo, ink earned.
/// </summary>
[DisallowMultipleComponent]
public partial class RunEndScreen : MonoBehaviour
{
    private const float TitleFontSize = 72f;
    private const float ScoreFontSize = 96f;
    private const float ScoreCaptionSize = 22f;
    private const float StatLabelSize = 26f;
    private const float StatValueSize = 30f;
    private const float ButtonLabelSize = 32f;
    private const float PanelWidth = 820f;
    private const float PanelHeight = 960f;
    private const float StatRowHeight = 48f;
    private const float StatRowSpacing = 8f;
    private const float DividerHeight = 1.5f;
    private const int CanvasSortOrder = 500;

    private GameObject _vpRoot;
    private CanvasGroup _group;
    private Image _backdrop;
    private TextMeshProUGUI _titleLabel;
    private TextMeshProUGUI _scoreValue;
    private TextMeshProUGUI _roomLabel;
    private TextMeshProUGUI _lastRoomScoreLabel;
    private TextMeshProUGUI _bestComboLabel;
    private TextMeshProUGUI _inkLabel;
    private Button _restartButton;

    private void Awake()
    {
        EnsureUi();
        Hide();
    }

    public void Show(RunEndData data, UnityEngine.Events.UnityAction restartAction)
    {
        EnsureUi();

        _titleLabel.text = data.cleared ? "RUN COMPLETE" : "RUN FAILED";
        _titleLabel.color = data.cleared ? UIColors.ClearedGreen : UIColors.FailedRed;

        _scoreValue.text = data.totalScore.ToString("N0");

        _roomLabel.text = data.roomsReached.ToString();
        _lastRoomScoreLabel.text = data.lastRoomScore.ToString("N0");
        _bestComboLabel.text = data.bestCombo > 0 ? $"x{data.bestCombo}" : "-";
        _bestComboLabel.color = data.bestCombo >= 7 ? UIColors.ComboTier4
            : data.bestCombo >= 5 ? UIColors.ComboTier3
            : UIColors.ScoreWhite;
        _inkLabel.text = data.totalInk.ToString("N0");

        _restartButton.onClick.RemoveAllListeners();
        _restartButton.onClick.AddListener(restartAction);

        _group.alpha = 1f;
        _group.blocksRaycasts = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Legacy overload for backward compatibility with plain-text callers.
    /// </summary>
    public void Show(string summary, UnityEngine.Events.UnityAction restartAction)
    {
        var data = new RunEndData
        {
            cleared = summary.Contains("CLEARED") || summary.Contains("COMPLETE"),
            roomsReached = 0,
            totalScore = 0,
            lastRoomScore = 0,
            totalInk = 0,
            bestCombo = 0,
        };
        Show(data, restartAction);
        _scoreValue.text = "";
        _titleLabel.text = summary;
        _titleLabel.fontSize = StatLabelSize;
    }

    public void Hide()
    {
        EnsureUi();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void EnsureUi()
    {
        if (_titleLabel != null) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
            Object.Destroy(transform.GetChild(i).gameObject);

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasSortOrder;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        _vpRoot = new GameObject("ViewportRoot");
        _vpRoot.transform.SetParent(transform, false);
        RectTransform vpRect = _vpRoot.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        _vpRoot.AddComponent<ViewportConstraint>();

        // Backdrop covers full screen including pillarbox bars.
        GameObject bgGo = new("Backdrop");
        bgGo.transform.SetParent(transform, false);
        RectTransform bgRect = bgGo.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        _backdrop = bgGo.AddComponent<Image>();
        _backdrop.color = UIColors.PanelBackground;
        _backdrop.raycastTarget = true;

        GameObject panel = CreatePanel();

        // Title
        _titleLabel = CreateLabel(panel.transform, "Title", TitleFontSize, 350f);
        _titleLabel.fontStyle = FontStyles.Bold;

        // Large score display
        _scoreValue = CreateLabel(panel.transform, "ScoreValue", ScoreFontSize, 230f);
        _scoreValue.color = UIColors.ComboGold;
        _scoreValue.fontStyle = FontStyles.Bold;

        TextMeshProUGUI scoreCaption = CreateLabel(panel.transform, "ScoreCaption", ScoreCaptionSize, 175f);
        scoreCaption.text = "TOTAL SCORE";
        scoreCaption.color = UIColors.TargetGray;

        // Divider
        CreateDivider(panel.transform, 130f);

        // Stat rows
        float rowY = 80f;
        _roomLabel = CreateStatRow(panel.transform, "ROOMS REACHED", ref rowY);
        _lastRoomScoreLabel = CreateStatRow(panel.transform, "LAST ROOM SCORE", ref rowY);
        _bestComboLabel = CreateStatRow(panel.transform, "BEST COMBO", ref rowY);
        _inkLabel = CreateStatRow(panel.transform, "INK EARNED", ref rowY);

        // Divider before button
        CreateDivider(panel.transform, rowY - StatRowSpacing);

        // Play Again button
        CreateRestartButton(panel.transform);
    }

}
