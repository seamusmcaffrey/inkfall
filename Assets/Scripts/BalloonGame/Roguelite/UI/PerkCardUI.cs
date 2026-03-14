using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual representation of a perk choice.
/// </summary>
[DisallowMultipleComponent]
public class PerkCardUI : MonoBehaviour
{
    private Image _background;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _description;
    private Button _button;
    private PerkSO _perk;

    public void Build(PerkSO perk, Action<PerkSO> onClick)
    {
        _perk = perk;
        EnsureUi();
        _background.color = UIColors.GetRarityColor(perk.rarity) * new Color(0.25f, 0.25f, 0.25f, 0.9f);
        _title.text = perk.perkName;
        _description.text = perk.description;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onClick?.Invoke(_perk));
    }

    private void EnsureUi()
    {
        RectTransform rect = ComponentUtility.EnsureComponent<RectTransform>(gameObject);
        rect.sizeDelta = new Vector2(280f, 220f);
        _background = ComponentUtility.EnsureComponent<Image>(gameObject);
        _button = ComponentUtility.EnsureComponent<Button>(gameObject);

        if (_title == null)
        {
            _title = CreateText("Title", rect, new Vector2(0.5f, 1f), new Vector2(0f, -28f), 28f, TextAlignmentOptions.Center);
            _description = CreateText("Description", rect, new Vector2(0.5f, 0.5f), Vector2.zero, 18f, TextAlignmentOptions.Center);
            _description.rectTransform.sizeDelta = new Vector2(220f, 120f);
        }
    }

    private static TextMeshProUGUI CreateText(string name, RectTransform parent, Vector2 anchor, Vector2 position, float size, TextAlignmentOptions alignment)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(240f, 48f);
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.fontSize = size;
        label.alignment = alignment;
        return label;
    }
}
