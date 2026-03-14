using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI construction methods for the settings panel.
/// </summary>
public partial class SettingsPanel
{
    private void BuildUi()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 700;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        GameObject backdrop = new("Backdrop");
        backdrop.transform.SetParent(transform, false);
        RectTransform backdropRect = backdrop.AddComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;
        backdrop.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

        GameObject panel = new("Panel");
        panel.transform.SetParent(transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(720f, 520f);
        panel.AddComponent<Image>().color = UIColors.PanelBackground;

        CreateHeading(panel.transform, "SETTINGS", new Vector2(0f, -40f));
        _masterSlider = CreateSlider(panel.transform, "Master", new Vector2(0f, -120f), Save);
        _sfxSlider = CreateSlider(panel.transform, "SFX", new Vector2(0f, -200f), Save);
        _musicSlider = CreateSlider(panel.transform, "Music", new Vector2(0f, -280f), Save);
        _hapticsToggle = CreateToggle(panel.transform, "Haptics", new Vector2(0f, -360f), Save);

        CreateButton(panel.transform, "Close", new Vector2(0f, -440f), Hide);
    }

    private static void CreateHeading(Transform parent, string text, Vector2 anchoredPosition)
    {
        GameObject go = new("Heading");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(400f, 48f);
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 34f;
        label.color = UIColors.ScoreWhite;
        label.raycastTarget = false;
    }

    private static Slider CreateSlider(Transform parent, string labelText, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onChanged)
    {
        GameObject root = new($"{labelText}Slider");
        root.transform.SetParent(parent, false);
        RectTransform rect = root.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(540f, 56f);

        TextMeshProUGUI label = new GameObject("Label").AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(root.transform, false);
        label.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        label.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        label.rectTransform.pivot = new Vector2(0f, 0.5f);
        label.rectTransform.anchoredPosition = new Vector2(-240f, 0f);
        label.rectTransform.sizeDelta = new Vector2(150f, 36f);
        label.text = labelText;
        label.fontSize = 24f;
        label.color = UIColors.ScoreWhite;
        label.raycastTarget = false;

        Slider slider = root.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.direction = Slider.Direction.LeftToRight;
        slider.onValueChanged.AddListener(_ => onChanged?.Invoke());

        GameObject trackGo = new("Track");
        trackGo.transform.SetParent(root.transform, false);
        RectTransform trackRect = trackGo.AddComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(1f, 0.5f);
        trackRect.anchorMax = new Vector2(1f, 0.5f);
        trackRect.pivot = new Vector2(1f, 0.5f);
        trackRect.anchoredPosition = Vector2.zero;
        trackRect.sizeDelta = new Vector2(SliderWidth, 16f);
        Image trackImage = trackGo.AddComponent<Image>();
        trackImage.color = UIColors.ProgressBarTrack;
        slider.targetGraphic = trackImage;

        GameObject fillArea = new("FillArea");
        fillArea.transform.SetParent(trackGo.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(0f, 0f);
        fillAreaRect.offsetMax = new Vector2(-12f, 0f);

        GameObject fill = new("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = UIColors.RoomPink;

        GameObject handleArea = new("HandleSlideArea");
        handleArea.transform.SetParent(trackGo.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(8f, 0f);
        handleAreaRect.offsetMax = new Vector2(-8f, 0f);

        GameObject handle = new("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(24f, 24f);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = UIColors.InkCyan;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;

        TextMeshProUGUI valueLabel = new GameObject("Value").AddComponent<TextMeshProUGUI>();
        valueLabel.transform.SetParent(root.transform, false);
        valueLabel.rectTransform.anchorMin = new Vector2(1f, 0.5f);
        valueLabel.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        valueLabel.rectTransform.pivot = new Vector2(1f, 0.5f);
        valueLabel.rectTransform.anchoredPosition = new Vector2(74f, 0f);
        valueLabel.rectTransform.sizeDelta = new Vector2(64f, 32f);
        valueLabel.alignment = TextAlignmentOptions.Right;
        valueLabel.fontSize = 20f;
        valueLabel.color = UIColors.TargetGray;
        valueLabel.raycastTarget = false;
        valueLabel.text = "100%";
        slider.onValueChanged.AddListener(value => valueLabel.text = $"{Mathf.RoundToInt(value * 100f)}%");
        return slider;
    }

    private static Toggle CreateToggle(Transform parent, string labelText, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onChanged)
    {
        GameObject root = new($"{labelText}Toggle");
        root.transform.SetParent(parent, false);
        RectTransform rect = root.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(420f, 48f);
        Toggle toggle = root.AddComponent<Toggle>();
        toggle.onValueChanged.AddListener(_ => onChanged?.Invoke());

        GameObject backgroundGo = new("Background");
        backgroundGo.transform.SetParent(root.transform, false);
        RectTransform backgroundRect = backgroundGo.AddComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.anchoredPosition = new Vector2(-180f, 0f);
        backgroundRect.sizeDelta = new Vector2(30f, 30f);
        Image backgroundImage = backgroundGo.AddComponent<Image>();
        backgroundImage.color = UIColors.ProgressBarTrack;

        GameObject checkmarkGo = new("Checkmark");
        checkmarkGo.transform.SetParent(backgroundGo.transform, false);
        RectTransform checkmarkRect = checkmarkGo.AddComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkmarkRect.sizeDelta = new Vector2(18f, 18f);
        Image checkmarkImage = checkmarkGo.AddComponent<Image>();
        checkmarkImage.color = UIColors.InkCyan;

        TextMeshProUGUI label = new GameObject("Label").AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(root.transform, false);
        label.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        label.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        label.rectTransform.sizeDelta = new Vector2(260f, 36f);
        label.rectTransform.anchoredPosition = new Vector2(40f, 0f);
        label.text = labelText;
        label.fontSize = 24f;
        label.color = UIColors.ScoreWhite;
        label.alignment = TextAlignmentOptions.Left;
        label.raycastTarget = false;

        toggle.targetGraphic = backgroundImage;
        toggle.graphic = checkmarkImage;
        return toggle;
    }

    private static void CreateButton(Transform parent, string text, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonGo = new(text);
        buttonGo.transform.SetParent(parent, false);
        RectTransform rect = buttonGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(220f, 60f);
        buttonGo.AddComponent<Image>().color = UIColors.RoomPink;
        Button button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(action);
        TextMeshProUGUI label = new GameObject("Label").AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(buttonGo.transform, false);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.text = text.ToUpperInvariant();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 24f;
        label.color = Color.black;
        label.raycastTarget = false;
    }
}
