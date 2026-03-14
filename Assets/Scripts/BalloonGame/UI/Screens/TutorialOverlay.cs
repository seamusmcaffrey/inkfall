using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// First-run tutorial overlay shown once.
/// </summary>
[DisallowMultipleComponent]
public class TutorialOverlay : MonoBehaviour
{
    private readonly string[] _steps =
    {
        "Pull back from the launch slot to arm the dart.",
        "Bounce off side walls to chase awkward angles.",
        "Paint and gold balloons create your best chains.",
    };

    private int _currentStep;
    private CanvasGroup _group;
    private TextMeshProUGUI _label;

    private void Awake()
    {
        BuildUi();
        if (!SaveManager.Instance.Data.hasCompletedTutorial)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _group.alpha = 1f;
        _group.blocksRaycasts = true;
        UpdateStep();
    }

    private void Next()
    {
        _currentStep++;
        if (_currentStep >= _steps.Length)
        {
            SaveManager.Instance.SetTutorialCompleted();
            Hide();
            return;
        }

        UpdateStep();
    }

    private void Hide()
    {
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void UpdateStep()
    {
        _label.text = $"{_currentStep + 1}/{_steps.Length}\n{_steps[_currentStep]}";
    }

    private void BuildUi()
    {
        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 720;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        GameObject panel = new("TutorialPanel");
        panel.transform.SetParent(transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(680f, 320f);
        panel.AddComponent<Image>().color = UIColors.PanelBackground;

        GameObject labelGo = new("Label");
        labelGo.transform.SetParent(panel.transform, false);
        RectTransform labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = new Vector2(580f, 180f);
        _label = labelGo.AddComponent<TextMeshProUGUI>();
        _label.alignment = TextAlignmentOptions.Center;
        _label.fontSize = 28f;

        GameObject buttonGo = new("Next");
        buttonGo.transform.SetParent(panel.transform, false);
        RectTransform buttonRect = buttonGo.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 44f);
        buttonRect.sizeDelta = new Vector2(180f, 52f);
        buttonGo.AddComponent<Image>().color = UIColors.InkCyan;
        Button button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(Next);
        TextMeshProUGUI buttonLabel = new GameObject("Label").AddComponent<TextMeshProUGUI>();
        buttonLabel.transform.SetParent(buttonGo.transform, false);
        buttonLabel.rectTransform.anchorMin = Vector2.zero;
        buttonLabel.rectTransform.anchorMax = Vector2.one;
        buttonLabel.text = "NEXT";
        buttonLabel.alignment = TextAlignmentOptions.Center;
        buttonLabel.fontSize = 24f;
        buttonLabel.color = Color.black;
    }
}
