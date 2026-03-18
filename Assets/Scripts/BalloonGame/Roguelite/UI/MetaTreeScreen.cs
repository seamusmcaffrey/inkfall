using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MetaTreeScreen : MonoBehaviour
{
    [SerializeField] private MetaProgressionManager _metaProgressionManager;

    private CanvasGroup _group;
    private RectTransform _contentRoot;
    private readonly List<Button> _buttons = new();

    private void Awake()
    {
        EnsureUi();
        Hide();
    }

    public void Show()
    {
        EnsureUi();
        _metaProgressionManager = ComponentUtility.EnsureComponent<MetaProgressionManager>(gameObject);
        BuildList();
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
        canvas.sortingOrder = 960;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        Image backdrop = new GameObject("Backdrop").AddComponent<Image>();
        backdrop.transform.SetParent(transform, false);
        backdrop.rectTransform.anchorMin = Vector2.zero;
        backdrop.rectTransform.anchorMax = Vector2.one;
        backdrop.rectTransform.offsetMin = Vector2.zero;
        backdrop.rectTransform.offsetMax = Vector2.zero;
        backdrop.color = new Color(0.02f, 0.02f, 0.05f, 0.95f);

        GameObject panel = new("Panel");
        panel.transform.SetParent(transform, false);
        _contentRoot = panel.AddComponent<RectTransform>();
        _contentRoot.anchorMin = _contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        _contentRoot.sizeDelta = new Vector2(940f, 1240f);
        panel.AddComponent<Image>().color = UIColors.PanelBackground;
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 40, 40);
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        CreateHeader("META TREE", 40f, FontStyles.Bold);
        CreateHeader("Spend permanent Ink to unlock more run content.", 22f, FontStyles.Normal, UIColors.TargetGray);

        Button closeButton = CreateButton("Close", "Return to title.", UIColors.RoomPink);
        closeButton.onClick.AddListener(Hide);
    }

    private void BuildList()
    {
        while (_buttons.Count < GameConfigSO.Instance.metaUpgrades.Count + 1)
        {
            _buttons.Add(CreateButton("Node", string.Empty, UIColors.InkCyan));
        }

        for (int index = 0; index < GameConfigSO.Instance.metaUpgrades.Count; index++)
        {
            MetaUpgradeSO upgrade = GameConfigSO.Instance.metaUpgrades[index];
            Button button = _buttons[index + 1];
            button.gameObject.SetActive(true);
            ConfigureButton(button, upgrade);
        }

        for (int index = GameConfigSO.Instance.metaUpgrades.Count + 1; index < _buttons.Count; index++)
        {
            _buttons[index].gameObject.SetActive(false);
        }
    }

    private void ConfigureButton(Button button, MetaUpgradeSO upgrade)
    {
        bool unlocked = _metaProgressionManager.IsUnlocked(upgrade);
        bool canPurchase = _metaProgressionManager.CanPurchase(upgrade, out string reason);
        TextMeshProUGUI[] labels = button.GetComponentsInChildren<TextMeshProUGUI>();
        labels[0].text = $"{upgrade.displayName}  [{upgrade.branchLabel}]";
        labels[1].text = unlocked ? "Unlocked" : $"{upgrade.description}\n{reason}";
        labels[2].text = unlocked ? "OWNED" : $"INK {upgrade.cost}";
        labels[0].color = unlocked ? UIColors.ClearedGreen : UIColors.ScoreWhite;
        labels[2].color = unlocked ? UIColors.ClearedGreen : (canPurchase ? UIColors.ComboGold : UIColors.TargetGray);
        button.onClick.RemoveAllListeners();
        button.interactable = !unlocked && canPurchase;
        if (button.interactable)
        {
            button.onClick.AddListener(() =>
            {
                if (_metaProgressionManager.Purchase(upgrade))
                {
                    BuildList();
                }
            });
        }
    }

    private void CreateHeader(string text, float size, FontStyles style, Color? color = null)
    {
        TextMeshProUGUI label = new GameObject("Header").AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(_contentRoot, false);
        label.fontSize = size;
        label.fontStyle = style;
        label.color = color ?? UIColors.ScoreWhite;
        label.text = text;
        label.enableWordWrapping = true;
        label.raycastTarget = false;
        LayoutElement element = label.gameObject.AddComponent<LayoutElement>();
        element.minHeight = size + 18f;
    }

    private Button CreateButton(string title, string description, Color accent)
    {
        GameObject go = new(title);
        go.transform.SetParent(_contentRoot, false);
        Image bg = go.AddComponent<Image>();
        bg.color = UIColors.BadgeBackground;
        Button button = go.AddComponent<Button>();
        LayoutElement element = go.AddComponent<LayoutElement>();
        element.minHeight = 120f;

        TextMeshProUGUI header = CreateLabel(go.transform, 24f, FontStyles.Bold, new Vector2(18f, -16f), TextAlignmentOptions.TopLeft);
        header.text = title;
        header.color = accent;
        TextMeshProUGUI body = CreateLabel(go.transform, 18f, FontStyles.Normal, new Vector2(18f, -52f), TextAlignmentOptions.TopLeft);
        body.text = description;
        body.color = UIColors.TargetGray;
        body.rectTransform.sizeDelta = new Vector2(640f, 60f);
        TextMeshProUGUI price = CreateLabel(go.transform, 18f, FontStyles.Bold, new Vector2(-18f, 0f), TextAlignmentOptions.MidlineRight, new Vector2(1f, 0.5f));
        price.text = string.Empty;
        price.color = accent;
        _buttons.Add(button);
        return button;
    }

    private static TextMeshProUGUI CreateLabel(Transform parent, float size, FontStyles style, Vector2 anchoredPosition,
        TextAlignmentOptions alignment, Vector2? anchor = null)
    {
        TextMeshProUGUI label = new GameObject("Label").AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(parent, false);
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = alignment;
        label.enableWordWrapping = true;
        RectTransform rect = label.rectTransform;
        Vector2 resolvedAnchor = anchor ?? new Vector2(0f, 1f);
        rect.anchorMin = resolvedAnchor;
        rect.anchorMax = resolvedAnchor;
        rect.pivot = resolvedAnchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(880f, 32f);
        return label;
    }
}
