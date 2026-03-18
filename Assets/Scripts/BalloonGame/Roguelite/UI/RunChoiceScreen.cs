using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class RunChoiceScreen : MonoBehaviour
{
    private readonly List<Button> _optionButtons = new();
    private CanvasGroup _group;
    private RectTransform _contentRoot;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _subtitleText;

    public void Show(string title, string subtitle, IReadOnlyList<RunChoiceOption> options)
    {
        EnsureUi();
        _titleText.text = title;
        _subtitleText.text = subtitle;

        while (_optionButtons.Count < options.Count)
        {
            _optionButtons.Add(CreateOptionButton());
        }

        for (int index = 0; index < _optionButtons.Count; index++)
        {
            bool active = index < options.Count;
            Button button = _optionButtons[index];
            button.gameObject.SetActive(active);
            if (!active)
            {
                continue;
            }

            BuildOption(button, options[index]);
        }

        _group.alpha = 1f;
        _group.blocksRaycasts = true;
        gameObject.SetActive(true);
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
        if (_contentRoot != null)
        {
            return;
        }

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 950;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        Image backdrop = new GameObject("Backdrop").AddComponent<Image>();
        backdrop.transform.SetParent(transform, false);
        RectTransform backdropRect = backdrop.rectTransform;
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;
        backdrop.color = new Color(0.02f, 0.02f, 0.06f, 0.92f);

        GameObject panel = new("Panel");
        panel.transform.SetParent(transform, false);
        _contentRoot = panel.AddComponent<RectTransform>();
        _contentRoot.anchorMin = _contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        _contentRoot.sizeDelta = new Vector2(920f, 900f);

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = UIColors.PanelBackground;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 40, 40);
        layout.spacing = 18f;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;

        _titleText = CreateText("Title", 38f, FontStyles.Bold);
        _subtitleText = CreateText("Subtitle", 22f, FontStyles.Normal);
        _subtitleText.color = UIColors.TargetGray;
    }

    private Button CreateOptionButton()
    {
        GameObject go = new("Option");
        go.transform.SetParent(_contentRoot, false);
        Image bg = go.AddComponent<Image>();
        bg.color = UIColors.BadgeBackground;
        Button button = go.AddComponent<Button>();
        LayoutElement element = go.AddComponent<LayoutElement>();
        element.minHeight = 120f;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 120f);

        TextMeshProUGUI title = CreateText("OptionTitle", 26f, FontStyles.Bold, go.transform);
        title.alignment = TextAlignmentOptions.TopLeft;
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = new Vector2(1f, 1f);
        title.rectTransform.pivot = new Vector2(0f, 1f);
        title.rectTransform.anchoredPosition = new Vector2(18f, -16f);
        title.rectTransform.sizeDelta = new Vector2(-36f, 40f);

        TextMeshProUGUI desc = CreateText("OptionDesc", 18f, FontStyles.Normal, go.transform);
        desc.alignment = TextAlignmentOptions.TopLeft;
        desc.color = UIColors.TargetGray;
        desc.rectTransform.anchorMin = new Vector2(0f, 0f);
        desc.rectTransform.anchorMax = new Vector2(1f, 1f);
        desc.rectTransform.offsetMin = new Vector2(18f, 16f);
        desc.rectTransform.offsetMax = new Vector2(-160f, -48f);

        TextMeshProUGUI price = CreateText("OptionPrice", 18f, FontStyles.Bold, go.transform);
        price.alignment = TextAlignmentOptions.MidlineRight;
        price.rectTransform.anchorMin = new Vector2(1f, 0.5f);
        price.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        price.rectTransform.pivot = new Vector2(1f, 0.5f);
        price.rectTransform.anchoredPosition = new Vector2(-18f, 0f);
        price.rectTransform.sizeDelta = new Vector2(140f, 40f);

        button.targetGraphic = bg;
        return button;
    }

    private void BuildOption(Button button, RunChoiceOption option)
    {
        Image bg = button.GetComponent<Image>();
        bg.color = option.AccentColor * new Color(0.15f, 0.15f, 0.15f, 0.95f);
        TextMeshProUGUI[] labels = button.GetComponentsInChildren<TextMeshProUGUI>();
        labels[0].text = option.Title;
        labels[0].color = option.AccentColor;
        labels[1].text = option.Description;
        labels[2].text = option.CostLabel;
        labels[2].color = option.AccentColor;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            option.OnSelected?.Invoke();
            if (option.HideOnSelect)
            {
                Hide();
            }
        });
    }

    private TextMeshProUGUI CreateText(string name, float size, FontStyles fontStyle, Transform parentOverride = null)
    {
        GameObject go = new(name);
        go.transform.SetParent(parentOverride ?? _contentRoot, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = fontStyle;
        text.color = UIColors.ScoreWhite;
        text.enableWordWrapping = true;
        LayoutElement element = go.AddComponent<LayoutElement>();
        element.minHeight = size + 16f;
        return text;
    }
}

public sealed class RunChoiceOption
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string CostLabel { get; set; }
    public Color AccentColor { get; set; }
    public Action OnSelected { get; set; }
    public bool HideOnSelect { get; set; } = true;
}
