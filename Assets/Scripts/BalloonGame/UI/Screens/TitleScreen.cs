using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Main title screen with start and settings actions.
/// </summary>
[DisallowMultipleComponent]
public class TitleScreen : MonoBehaviour
{
    [SerializeField] private RunManager _runManager;
    [SerializeField] private SettingsPanel _settingsPanel = null;
    private CanvasGroup _group;

    private void Awake()
    {
        EnsureEventSystem();
        ResolveDependencies();
        BuildUi();
        Show();
        if (GameConstants.DEV_SKIP_INTRO) Hide();
    }

    /// <summary>
    /// Unity UI requires an EventSystem to process clicks/touches. Create one if the scene lacks it.
    /// </summary>
    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    /// <summary>
    /// Injects title-screen dependencies created by the scene builder.
    /// </summary>
    public void SetDependencies(RunManager runManager, SettingsPanel settingsPanel)
    {
        _runManager = runManager;
        _settingsPanel = settingsPanel;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _group.alpha = 1f;
        _group.blocksRaycasts = true;
    }

    private void StartRun()
    {
        ResolveDependencies();
        if (_runManager == null)
        {
#if UNITY_EDITOR
            Debug.LogError("TitleScreen could not find RunManager in the active scene.");
#endif
            return;
        }

        ScreenTransition.FadeTo(() =>
        {
            Hide();
            _runManager.StartNewRun();
        });
    }

    private void OpenSettings()
    {
        ResolveDependencies();
        if (_settingsPanel != null)
        {
            _settingsPanel.Show();
        }
    }

    private void Hide()
    {
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void BuildUi()
    {
        // Destroy scene-saved children to avoid duplicates (SceneBuilder creates them
        // at edit time, but onClick listeners are not serialized so they'd be dead buttons).
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 850;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        GameObject backdrop = new("Backdrop");
        backdrop.transform.SetParent(transform, false);
        RectTransform rect = backdrop.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        backdrop.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.05f, 0.94f);

        GameObject vpRoot = new("ViewportRoot");
        vpRoot.transform.SetParent(transform, false);
        RectTransform vpRect = vpRoot.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        vpRoot.AddComponent<ViewportConstraint>();

        CreateText(vpRoot.transform, "INKSHOT", new Vector2(0f, 220f), 84f, UIColors.RoomPink);
        CreateText(vpRoot.transform, "Chrome darts. Neon carnival. One more room.",
            new Vector2(0f, 150f), 26f, UIColors.InkCyan);
        CreateButton(vpRoot.transform, "START RUN", new Vector2(0f, -40f), StartRun);
        CreateButton(vpRoot.transform, "SETTINGS", new Vector2(0f, -120f), OpenSettings);
    }

    private void ResolveDependencies()
    {
        ComponentUtility.ResolveSceneReference(this, ref _runManager);
        ComponentUtility.ResolveSceneReference(this, ref _settingsPanel);
    }

    private static void CreateText(Transform parent, string text, Vector2 anchoredPosition, float size, Color color)
    {
        GameObject go = new(text);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(900f, 100f);
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
    }

    private static void CreateButton(Transform parent, string text, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new(text);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(280f, 64f);
        go.AddComponent<Image>().color = UIColors.RoomPink;
        Button button = go.AddComponent<Button>();
        button.onClick.AddListener(action);

        TextMeshProUGUI label = new GameObject("Label").AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(go.transform, false);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 28f;
        label.color = Color.black;
        label.raycastTarget = false;
    }
}
