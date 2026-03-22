using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI construction methods for InGameHUD — builds top bar, progress bar,
/// dart tray, combo/floating anchors, and message overlay.
/// </summary>
public partial class InGameHUD
{
    private void BuildUi()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Object.Destroy(transform.GetChild(i).gameObject);

        _canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = Camera.main;
        _canvas.planeDistance = 1f;
        _canvas.sortingOrder = 200;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);

        GameObject vpRoot = new("ViewportRoot");
        vpRoot.transform.SetParent(transform, false);
        RectTransform vpRect = vpRoot.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        vpRoot.AddComponent<ViewportConstraint>();

        GameObject safeRoot = new("SafeArea");
        safeRoot.transform.SetParent(vpRoot.transform, false);
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
        float margin = GameConstants.HUD_SIDE_MARGIN;
        RectTransform topBar = CreatePanel("TopBar", _safeRoot, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -GameConstants.HUD_TOP_MARGIN), new Vector2(0f, TopBarHeight));
        Image bg = topBar.gameObject.AddComponent<Image>();
        bg.color = UIColors.TopBarBackground;

        BuildRoomBadge(topBar, margin);

        // Center: Score label, large value, and target
        _scoreLabel = CreateLabel("ScoreLabel", topBar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 22f), 18f, UIColors.TargetGray, TextAlignmentOptions.Center);
        _scoreLabel.text = "SCORE";
        _scoreValue = CreateLabel("ScoreValue", topBar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -2f), 36f, UIColors.ScoreWhite, TextAlignmentOptions.Center);
        _scoreValue.fontStyle = FontStyles.Bold;
        _targetLabel = CreateLabel("TargetLabel", topBar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -26f), 14f, UIColors.TargetGray, TextAlignmentOptions.Center);

        // Right: Darts count + Currency with ink icon
        _dartsText = CreateLabel("Darts", topBar, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-margin - 80f, 10f), 22f, UIColors.DartBlue, TextAlignmentOptions.Right);
        _dartsText.fontStyle = FontStyles.Bold;

        _currencyText = CreateLabel("Currency", topBar, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-margin, -12f), 16f, UIColors.CurrencyIconTint, TextAlignmentOptions.Right);
        _currencyText.text = "<color=#1AEDE0>\u25C6</color> 0";
    }

    private void BuildRoomBadge(RectTransform topBar, float margin)
    {
        RectTransform badgeBorder = CreatePanel("BadgeBorder", topBar, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(margin, 0f), new Vector2(BadgeSize + BadgeBorderWidth * 2f, BadgeSize + BadgeBorderWidth * 2f));
        Image borderImg = badgeBorder.gameObject.AddComponent<Image>();
        borderImg.color = UIColors.BadgeBorder;

        RectTransform badge = CreatePanel("Badge", badgeBorder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(BadgeSize, BadgeSize));
        Image badgeBg = badge.gameObject.AddComponent<Image>();
        badgeBg.color = UIColors.BadgeBackground;

        _roomLabel = CreateLabel("RoomLabel", badge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 12f), 13f, UIColors.TargetGray, TextAlignmentOptions.Center);
        _roomNumber = CreateLabel("RoomNum", badge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -6f), 24f, UIColors.ScoreWhite, TextAlignmentOptions.Center);
        _roomNumber.fontStyle = FontStyles.Bold;
    }

    private void BuildProgressBar()
    {
        float yOffset = -GameConstants.HUD_TOP_MARGIN - TopBarHeight - 4f;
        RectTransform bar = CreatePanel("ProgressBar", _safeRoot, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, yOffset), new Vector2(-GameConstants.HUD_SIDE_MARGIN * 2f, ProgressBarHeight));
        _progressTrack = bar.gameObject.AddComponent<Image>();
        _progressTrack.color = UIColors.ProgressBarTrack;

        RectTransform fill = CreatePanel("Fill", bar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        _progressFill = fill.gameObject.AddComponent<Image>();
        _progressFill.type = Image.Type.Filled;
        _progressFill.fillMethod = Image.FillMethod.Horizontal;
        _progressFill.fillAmount = 0f;
        _progressFill.color = UIColors.ProgressBarFillStart;

        _progressGlow = CreatePanel("Glow", bar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero)
            .gameObject.AddComponent<Image>();
        _progressGlow.color = Color.clear;
    }

    private void BuildDartTray()
    {
        RectTransform dartRoot = CreatePanel("DartTray", _safeRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 44f), new Vector2(220f, 40f));
        _dartTray = dartRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        _dartTray.spacing = 10f;
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
        cr.anchoredPosition = new Vector2(0f, 120f);
        combo.AddComponent<ComboDisplay>();

        GameObject comboFeedback = new("ComboFeedback");
        comboFeedback.transform.SetParent(_safeRoot, false);
        RectTransform feedbackRect = comboFeedback.AddComponent<RectTransform>();
        feedbackRect.anchorMin = feedbackRect.anchorMax = new Vector2(0.5f, 0.5f);
        feedbackRect.anchoredPosition = new Vector2(0f, UIConfigSO.Instance.comboFeedbackOffsetY);
        comboFeedback.AddComponent<ComboFeedbackPanel>();

        GameObject floats = new("FloatingScores");
        floats.transform.SetParent(_safeRoot, false);
        floats.AddComponent<RectTransform>();
        floats.AddComponent<FloatingScoreText>();
    }

    private void BuildMessageOverlay()
    {
        _messageText = CreateLabel("Message", _safeRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, UIConfigSO.Instance.messageFontSize, UIColors.ScoreWhite, TextAlignmentOptions.Center);
        _messageText.fontStyle = FontStyles.Bold;
        _messageText.gameObject.SetActive(false);
    }

    private static TextMeshProUGUI CreateLabel(string name, Transform parent, Vector2 aMin, Vector2 aMax,
        Vector2 pos, float size, Color color, TextAlignmentOptions align)
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

    private static RectTransform CreatePanel(string name, Transform parent, Vector2 aMin, Vector2 aMax,
        Vector2 pos, Vector2 size)
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
        r.sizeDelta = new Vector2(DartIconWidth + 6f, DartIconHeight + DartTipHeight);

        // Dart body (narrow rectangle)
        GameObject body = new("Body");
        body.transform.SetParent(go.transform, false);
        RectTransform bodyRect = body.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.5f, 0f);
        bodyRect.anchorMax = new Vector2(0.5f, 0f);
        bodyRect.pivot = new Vector2(0.5f, 0f);
        bodyRect.anchoredPosition = Vector2.zero;
        bodyRect.sizeDelta = new Vector2(DartIconWidth, DartIconHeight);
        body.AddComponent<Image>().color = UIColors.DartIconActive;

        // Dart tip
        GameObject tip = new("Tip");
        tip.transform.SetParent(go.transform, false);
        RectTransform tipRect = tip.AddComponent<RectTransform>();
        tipRect.anchorMin = new Vector2(0.5f, 0f);
        tipRect.anchorMax = new Vector2(0.5f, 0f);
        tipRect.pivot = new Vector2(0.5f, 0f);
        tipRect.anchoredPosition = new Vector2(0f, DartIconHeight);
        tipRect.sizeDelta = new Vector2(DartIconWidth + 6f, DartTipHeight);
        tip.AddComponent<Image>().color = UIColors.DartIconActive;
    }

    private static void SetDartIconColor(Transform icon, bool active)
    {
        Color color = active ? UIColors.DartIconActive : UIColors.DartIconSpent;
        foreach (Image img in icon.GetComponentsInChildren<Image>())
            img.color = color;
    }
}
