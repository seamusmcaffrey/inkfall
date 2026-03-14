using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Premium perk card with rarity border, bold icon area, and effect text.
/// </summary>
[DisallowMultipleComponent]
public class PerkCardUI : MonoBehaviour
{
    private const float CardWidth = 280f;
    private const float CardHeight = 380f;
    private const float BorderWidth = 3f;
    private const float IconAreaHeight = 140f;

    private Image _border;
    private Image _background;
    private Image _iconArea;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _description;
    private TextMeshProUGUI _rarityLabel;
    private Button _button;
    private PerkSO _perk;

    public void Build(PerkSO perk, Action<PerkSO> onClick)
    {
        _perk = perk;
        EnsureUi();

        Color rarityColor = UIColors.GetRarityColor(perk.rarity);
        _border.color = rarityColor;
        _background.color = new Color(0.06f, 0.06f, 0.10f, 0.95f);
        _iconArea.color = new Color(rarityColor.r * 0.15f, rarityColor.g * 0.15f, rarityColor.b * 0.15f, 0.9f);
        _title.text = perk.perkName.ToUpperInvariant();
        _title.color = rarityColor;
        _description.text = perk.description;
        _description.color = new Color(0.78f, 0.78f, 0.82f);
        _rarityLabel.text = perk.rarity.ToString().ToUpperInvariant();
        _rarityLabel.color = rarityColor;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onClick?.Invoke(_perk));
    }

    private void EnsureUi()
    {
        RectTransform rect = ComponentUtility.EnsureComponent<RectTransform>(gameObject);
        rect.sizeDelta = new Vector2(CardWidth, CardHeight);

        if (_border != null) return;

        _border = gameObject.AddComponent<Image>();

        GameObject inner = new("Inner");
        inner.transform.SetParent(rect, false);
        RectTransform innerRect = inner.AddComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(BorderWidth, BorderWidth);
        innerRect.offsetMax = new Vector2(-BorderWidth, -BorderWidth);
        _background = inner.AddComponent<Image>();

        _iconArea = CreateArea("IconArea", innerRect, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, 0f), new Vector2(0f, IconAreaHeight));
        _title = CreateText("Title", innerRect, new Vector2(0.5f, 0.5f),
            new Vector2(0f, 30f), 24f, TextAlignmentOptions.Center);
        _description = CreateText("Desc", innerRect, new Vector2(0.5f, 0.5f),
            new Vector2(0f, -30f), 16f, TextAlignmentOptions.Center);
        _description.rectTransform.sizeDelta = new Vector2(240f, 100f);
        _rarityLabel = CreateText("Rarity", innerRect, new Vector2(0.5f, 0f),
            new Vector2(0f, 16f), 14f, TextAlignmentOptions.Center);

        _button = ComponentUtility.EnsureComponent<Button>(gameObject);
    }

    private static Image CreateArea(string name, RectTransform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = aMin; r.anchorMax = aMax;
        r.anchoredPosition = pos; r.sizeDelta = size;
        r.pivot = new Vector2(0.5f, 1f);
        return go.AddComponent<Image>();
    }

    private static TextMeshProUGUI CreateText(string name, RectTransform parent, Vector2 anchor, Vector2 pos, float size, TextAlignmentOptions align)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = anchor; r.anchorMax = anchor;
        r.pivot = anchor;
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(260f, 48f);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = size; t.alignment = align; t.raycastTarget = false;
        return t;
    }
}
