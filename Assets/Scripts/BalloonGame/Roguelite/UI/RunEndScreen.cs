using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Run-end summary overlay.
/// </summary>
[DisallowMultipleComponent]
public class RunEndScreen : MonoBehaviour
{
    private CanvasGroup _group;
    private TextMeshProUGUI _summary;
    private Button _restartButton;

    private void Awake()
    {
        EnsureUi();
        Hide();
    }

    public void Show(string summary, UnityEngine.Events.UnityAction restartAction)
    {
        EnsureUi();
        _summary.text = summary;
        _restartButton.onClick.RemoveAllListeners();
        _restartButton.onClick.AddListener(restartAction);
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
        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        if (_summary == null)
        {
            GameObject panel = new("Panel");
            panel.transform.SetParent(transform, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 420f);
            Image bg = panel.AddComponent<Image>();
            bg.color = UIColors.PanelBackground;

            GameObject summaryGo = new("Summary");
            summaryGo.transform.SetParent(panel.transform, false);
            RectTransform summaryRect = summaryGo.AddComponent<RectTransform>();
            summaryRect.anchorMin = new Vector2(0.5f, 0.7f);
            summaryRect.anchorMax = new Vector2(0.5f, 0.7f);
            summaryRect.sizeDelta = new Vector2(620f, 220f);
            _summary = summaryGo.AddComponent<TextMeshProUGUI>();
            _summary.alignment = TextAlignmentOptions.Center;
            _summary.fontSize = 28f;

            GameObject buttonGo = new("RestartButton");
            buttonGo.transform.SetParent(panel.transform, false);
            RectTransform buttonRect = buttonGo.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.2f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.2f);
            buttonRect.sizeDelta = new Vector2(220f, 64f);
            Image buttonBg = buttonGo.AddComponent<Image>();
            buttonBg.color = UIColors.RoomPink;
            _restartButton = buttonGo.AddComponent<Button>();
            TextMeshProUGUI label = CreateButtonText(buttonGo.transform, "PLAY AGAIN");
            label.color = Color.black;
        }
    }

    private static TextMeshProUGUI CreateButtonText(Transform parent, string text)
    {
        GameObject labelGo = new("Label");
        labelGo.transform.SetParent(parent, false);
        RectTransform rect = labelGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 28f;
        return label;
    }
}
