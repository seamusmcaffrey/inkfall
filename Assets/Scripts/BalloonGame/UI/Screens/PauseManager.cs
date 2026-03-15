using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private Canvas _pauseButtonCanvas;

    private void Awake()
    {
        ResolveDependencies();
        BuildUi();
        BuildPauseButton();
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
        bool escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        if (escapePressed)
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
        if (_pauseButtonCanvas != null) _pauseButtonCanvas.gameObject.SetActive(false);
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
        if (_pauseButtonCanvas != null) _pauseButtonCanvas.gameObject.SetActive(true);
    }

    private void BuildPauseButton()
    {
        // Destroy any stale pause-button canvas saved by the scene builder.
        if (_pauseButtonCanvas != null)
        {
            Object.Destroy(_pauseButtonCanvas.gameObject);
        }
        else
        {
            Transform stale = transform.parent != null
                ? transform.parent.Find("PauseButtonCanvas")
                : null;
            if (stale != null) Object.Destroy(stale.gameObject);
        }

        // Separate canvas so the button stays visible when the pause overlay is hidden.
        GameObject btnCanvas = new("PauseButtonCanvas");
        btnCanvas.transform.SetParent(transform.parent, false);
        _pauseButtonCanvas = btnCanvas.AddComponent<Canvas>();
        _pauseButtonCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _pauseButtonCanvas.sortingOrder = 750; // below pause overlay (760)
        CanvasScaler scaler = btnCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0f;
        btnCanvas.AddComponent<GraphicRaycaster>();

        // Viewport constraint so button stays within 9:16 game area.
        GameObject vpRoot = new("ViewportRoot");
        vpRoot.transform.SetParent(btnCanvas.transform, false);
        RectTransform vpRect = vpRoot.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        vpRoot.AddComponent<ViewportConstraint>();

        // Safe-area root so the button respects notch / status-bar insets.
        GameObject safeRoot = new("SafeArea");
        safeRoot.transform.SetParent(vpRoot.transform, false);
        RectTransform safeRect = safeRoot.AddComponent<RectTransform>();
        safeRoot.AddComponent<SafeAreaHandler>();

        // Pause button anchored to top-right corner.
        GameObject btnGo = new("PauseButton");
        btnGo.transform.SetParent(safeRect, false);
        RectTransform rect = btnGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-GameConstants.HUD_SIDE_MARGIN, -GameConstants.HUD_TOP_MARGIN);
        rect.sizeDelta = new Vector2(56f, 56f);

        Image bg = btnGo.AddComponent<Image>();
        bg.color = UIColors.PanelBackground;

        Button button = btnGo.AddComponent<Button>();
        button.onClick.AddListener(Show);

        TextMeshProUGUI label = new GameObject("Label").AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(btnGo.transform, false);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        label.text = "||";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 28f;
        label.color = UIColors.ScoreWhite;
        label.raycastTarget = false;
    }

    private void BuildUi()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 760;
        CanvasScaler scaler2 = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler2.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler2.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler2.matchWidthOrHeight = 0f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        GameObject vpRoot2 = new("ViewportRoot");
        vpRoot2.transform.SetParent(transform, false);
        RectTransform vpRect2 = vpRoot2.AddComponent<RectTransform>();
        vpRect2.anchorMin = Vector2.zero;
        vpRect2.anchorMax = Vector2.one;
        vpRect2.offsetMin = Vector2.zero;
        vpRect2.offsetMax = Vector2.zero;
        vpRoot2.AddComponent<ViewportConstraint>();

        GameObject panel = new("PausePanel");
        panel.transform.SetParent(vpRoot2.transform, false);
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
        label.raycastTarget = false;
    }
}
