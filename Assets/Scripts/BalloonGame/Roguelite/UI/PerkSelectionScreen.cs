using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Premium perk selection overlay with dark backdrop and card fan layout.
/// </summary>
[DisallowMultipleComponent]
public class PerkSelectionScreen : MonoBehaviour
{
    private RectTransform _contentRoot;
    private CanvasGroup _group;
    private Image _backdrop;
    private Button _rerollButton;
    private Text _rerollLabel;
    private readonly List<PerkCardUI> _cards = new();

    private void Awake()
    {
        EnsureUi();
        Hide();
    }

    public void Show(IReadOnlyList<PerkSO> perks, Action<PerkSO> onSelected, Action onReroll = null, int rerollCost = 0, int currentTickets = 0)
    {
        EnsureUi();
        while (_cards.Count < perks.Count)
        {
            GameObject cardGo = new("PerkCard");
            cardGo.transform.SetParent(_contentRoot, false);
            _cards.Add(cardGo.AddComponent<PerkCardUI>());
        }

        for (int i = 0; i < _cards.Count; i++)
        {
            bool active = i < perks.Count;
            _cards[i].gameObject.SetActive(active);
            if (active)
            {
                _cards[i].Build(perks[i], perk =>
                {
                    Hide();
                    onSelected?.Invoke(perk);
                });
            }
        }

        _rerollButton.gameObject.SetActive(onReroll != null);
        _rerollButton.onClick.RemoveAllListeners();
        if (onReroll != null)
        {
            _rerollButton.onClick.AddListener(() => onReroll.Invoke());
            _rerollLabel.text = rerollCost <= 0 ? $"REROLL  [{currentTickets} TICKETS]" : $"REROLL {rerollCost}  [{currentTickets}]";
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
        if (_contentRoot != null) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 450;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        GameObject bg = new("Backdrop");
        bg.transform.SetParent(transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        _backdrop = bg.AddComponent<Image>();
        _backdrop.color = new Color(0.02f, 0.02f, 0.04f, 0.88f);

        GameObject vpRoot = new("ViewportRoot");
        vpRoot.transform.SetParent(transform, false);
        RectTransform vpRect = vpRoot.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        vpRoot.AddComponent<ViewportConstraint>();

        GameObject panel = new("CardPanel");
        panel.transform.SetParent(vpRoot.transform, false);
        _contentRoot = panel.AddComponent<RectTransform>();
        _contentRoot.anchorMin = _contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        _contentRoot.sizeDelta = new Vector2(960f, 400f);
        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        GameObject reroll = new("RerollButton");
        reroll.transform.SetParent(vpRoot.transform, false);
        RectTransform rerollRect = reroll.AddComponent<RectTransform>();
        rerollRect.anchorMin = rerollRect.anchorMax = new Vector2(0.5f, 0.5f);
        rerollRect.anchoredPosition = new Vector2(0f, 300f);
        rerollRect.sizeDelta = new Vector2(320f, 60f);
        reroll.AddComponent<Image>().color = UIColors.InkCyan;
        _rerollButton = reroll.AddComponent<Button>();
        _rerollLabel = new GameObject("Label").AddComponent<Text>();
        _rerollLabel.transform.SetParent(reroll.transform, false);
        RectTransform labelRect = _rerollLabel.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        _rerollLabel.alignment = TextAnchor.MiddleCenter;
        _rerollLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _rerollLabel.color = Color.black;
    }
}
