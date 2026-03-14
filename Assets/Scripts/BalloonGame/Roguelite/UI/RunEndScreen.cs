using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Premium run-end summary overlay with dark backdrop, neon accents, and stat rows.
/// </summary>
[DisallowMultipleComponent]
public class RunEndScreen : MonoBehaviour
{
    private const float TitleFontSize = 72f;
    private const float ScoreFontSize = 96f;
    private const float StatLabelSize = 26f;
    private const float StatValueSize = 30f;
    private const float ButtonLabelSize = 32f;
    private const float PanelWidth = 820f;
    private const float PanelHeight = 900f;
    private const float StatRowHeight = 48f;
    private const float StatRowSpacing = 8f;
    private const int CanvasSortOrder = 500;

    private CanvasGroup _group;
    private Image _backdrop;
    private TextMeshProUGUI _titleLabel;
    private TextMeshProUGUI _scoreValue;
    private TextMeshProUGUI _roomLabel;
    private TextMeshProUGUI _lastRoomScoreLabel;
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
        };
        Show(data, restartAction);
        // Override with raw text for fallback display
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
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        // Full-screen dark backdrop
        _backdrop = CreateFullScreenImage("Backdrop", UIColors.PanelBackground);

        // Central panel
        GameObject panel = CreatePanel();

        // Title ("RUN COMPLETE" / "RUN FAILED")
        _titleLabel = CreateLabel(panel.transform, "Title", TitleFontSize, 320f);
        _titleLabel.fontStyle = FontStyles.Bold;

        // Large score display
        _scoreValue = CreateLabel(panel.transform, "ScoreValue", ScoreFontSize, 190f);
        _scoreValue.color = UIColors.ComboGold;
        _scoreValue.fontStyle = FontStyles.Bold;

        TextMeshProUGUI scoreCaption = CreateLabel(panel.transform, "ScoreCaption", StatLabelSize, 140f);
        scoreCaption.text = "TOTAL SCORE";
        scoreCaption.color = UIColors.TargetGray;

        // Stat rows
        float rowY = 40f;
        _roomLabel = CreateStatRow(panel.transform, "ROOMS REACHED", ref rowY);
        _lastRoomScoreLabel = CreateStatRow(panel.transform, "LAST ROOM SCORE", ref rowY);
        _inkLabel = CreateStatRow(panel.transform, "INK EARNED", ref rowY);

        // Play Again button
        CreateRestartButton(panel.transform);
    }

    private Image CreateFullScreenImage(string name, Color color)
    {
        GameObject go = new(name);
        go.transform.SetParent(transform, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;
        return img;
    }

    private GameObject CreatePanel()
    {
        GameObject panel = new("Panel");
        panel.transform.SetParent(transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        return panel;
    }

    private static TextMeshProUGUI CreateLabel(
        Transform parent, string name, float fontSize, float yPos)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(PanelWidth - GameConstants.SAFE_AREA_PADDING * 2f, 120f);
        r.anchoredPosition = new Vector2(0f, yPos);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = fontSize;
        t.color = UIColors.ScoreWhite;
        t.raycastTarget = false;
        return t;
    }

    private TextMeshProUGUI CreateStatRow(Transform parent, string label, ref float yPos)
    {
        float rowWidth = PanelWidth - GameConstants.SAFE_AREA_PADDING * 2f;

        // Row container
        GameObject row = new($"Stat_{label}");
        row.transform.SetParent(parent, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.sizeDelta = new Vector2(rowWidth, StatRowHeight);
        rowRect.anchoredPosition = new Vector2(0f, yPos);

        // Label (left-aligned)
        GameObject labelGo = new("Label");
        labelGo.transform.SetParent(row.transform, false);
        RectTransform labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0.6f, 1f);
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.alignment = TextAlignmentOptions.Left;
        labelTmp.fontSize = StatLabelSize;
        labelTmp.color = UIColors.TargetGray;
        labelTmp.raycastTarget = false;

        // Value (right-aligned)
        GameObject valueGo = new("Value");
        valueGo.transform.SetParent(row.transform, false);
        RectTransform valueRect = valueGo.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.6f, 0f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.offsetMin = valueRect.offsetMax = Vector2.zero;
        TextMeshProUGUI valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
        valueTmp.alignment = TextAlignmentOptions.Right;
        valueTmp.fontSize = StatValueSize;
        valueTmp.color = UIColors.ScoreWhite;
        valueTmp.fontStyle = FontStyles.Bold;
        valueTmp.raycastTarget = false;

        yPos -= StatRowHeight + StatRowSpacing;
        return valueTmp;
    }

    private void CreateRestartButton(Transform parent)
    {
        GameObject buttonGo = new("RestartButton");
        buttonGo.transform.SetParent(parent, false);
        RectTransform buttonRect = buttonGo.AddComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(320f, 72f);
        buttonRect.anchoredPosition = new Vector2(0f, -320f);

        Image buttonBg = buttonGo.AddComponent<Image>();
        buttonBg.color = UIColors.RoomPink;
        _restartButton = buttonGo.AddComponent<Button>();

        // Button label
        GameObject labelGo = new("Label");
        labelGo.transform.SetParent(buttonGo.transform, false);
        RectTransform labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = "PLAY AGAIN";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = ButtonLabelSize;
        label.color = Color.white;
        label.fontStyle = FontStyles.Bold;
        label.raycastTarget = false;
    }
}
