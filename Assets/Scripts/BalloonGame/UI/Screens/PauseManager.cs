using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pause button and overlay menu.
/// </summary>
[DisallowMultipleComponent]
public class PauseManager : MonoBehaviour
{
    [SerializeField] private SettingsPanel _settingsPanel = null;
    private CanvasGroup _group;

    private void Awake()
    {
        ResolveDependencies();
        BuildUi();
        Hide();
    }

    /// <summary>
    /// Injects pause-menu dependencies created by the scene builder.
    /// </summary>
    public void SetDependencies(SettingsPanel settingsPanel)
    {
        _settingsPanel = settingsPanel;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_group.alpha > 0.01f)
            {
                Resume();
            }
            else
            {
                Show();
            }
        }
    }

    public void Show()
    {
        Time.timeScale = 0f;
        _group.alpha = 1f;
        _group.blocksRaycasts = true;
        gameObject.SetActive(true);
    }

    private void OpenSettings()
    {
        ResolveDependencies();
        if (_settingsPanel != null)
        {
            _settingsPanel.Show();
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        Hide();
    }

    private void RestartScene()
    {
        Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    private void Hide()
    {
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void BuildUi()
    {
        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 760;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        GameObject panel = new("PausePanel");
        panel.transform.SetParent(transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(520f, 340f);
        panel.AddComponent<Image>().color = UIColors.PanelBackground;

        CreateButton(panel.transform, "RESUME", new Vector2(0f, -70f), Resume);
        CreateButton(panel.transform, "SETTINGS", new Vector2(0f, -150f), OpenSettings);
        CreateButton(panel.transform, "RESTART", new Vector2(0f, -230f), RestartScene);
    }

    private void ResolveDependencies()
    {
        ComponentUtility.ResolveSceneReference(this, ref _settingsPanel);
    }

    private static void CreateButton(Transform parent, string text, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonGo = new(text);
        buttonGo.transform.SetParent(parent, false);
        RectTransform rect = buttonGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(240f, 56f);
        buttonGo.AddComponent<Image>().color = UIColors.RoomPink;
        Button button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(action);
        TextMeshProUGUI label = new GameObject("Label").AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(buttonGo.transform, false);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 24f;
        label.color = Color.black;
    }
}
