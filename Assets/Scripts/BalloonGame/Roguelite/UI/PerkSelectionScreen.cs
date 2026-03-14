using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Overlay that presents a set of perk choices.
/// </summary>
[DisallowMultipleComponent]
public class PerkSelectionScreen : MonoBehaviour
{
    private RectTransform _contentRoot;
    private CanvasGroup _group;
    private readonly List<PerkCardUI> _cards = new();

    private void Awake()
    {
        EnsureUi();
        Hide();
    }

    public void Show(IReadOnlyList<PerkSO> perks, Action<PerkSO> onSelected)
    {
        EnsureUi();
        while (_cards.Count < perks.Count)
        {
            GameObject cardGo = new("PerkCard");
            cardGo.transform.SetParent(_contentRoot, false);
            _cards.Add(cardGo.AddComponent<PerkCardUI>());
        }

        for (int index = 0; index < _cards.Count; index++)
        {
            bool active = index < perks.Count;
            _cards[index].gameObject.SetActive(active);
            if (active)
            {
                _cards[index].Build(perks[index], perk =>
                {
                    EventBus.Publish(new PerkSelectedEvent { Perk = perk });
                    Hide();
                    onSelected?.Invoke(perk);
                });
            }
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
        if (_contentRoot == null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 450;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;

        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        if (_contentRoot == null)
        {
            GameObject panel = new("Panel");
            panel.transform.SetParent(transform, false);
            _contentRoot = panel.AddComponent<RectTransform>();
            _contentRoot.anchorMin = new Vector2(0.5f, 0.5f);
            _contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _contentRoot.sizeDelta = new Vector2(960f, 420f);
            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            Image background = panel.AddComponent<Image>();
            background.color = UIColors.PanelBackground;
        }
    }
}
