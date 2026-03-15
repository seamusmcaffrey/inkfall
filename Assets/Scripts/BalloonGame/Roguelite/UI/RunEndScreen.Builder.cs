using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class RunEndScreen
{
    private Image CreateFullScreenImage(string name, Color color)
    {
        GameObject go = new(name);
        go.transform.SetParent(_vpRoot.transform, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;
        return img;
    }

    private GameObject CreatePanel()
    {
        GameObject panel = new("Panel");
        panel.transform.SetParent(_vpRoot.transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        return panel;
    }

    private void CreateDivider(Transform parent, float yPos)
    {
        GameObject divider = new("Divider");
        divider.transform.SetParent(parent, false);
        RectTransform r = divider.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        float rowWidth = PanelWidth - GameConstants.SAFE_AREA_PADDING * 2f;
        r.sizeDelta = new Vector2(rowWidth, DividerHeight);
        r.anchoredPosition = new Vector2(0f, yPos);
        Image img = divider.AddComponent<Image>();
        img.color = new Color(UIColors.TargetGray.r, UIColors.TargetGray.g, UIColors.TargetGray.b, 0.3f);
        img.raycastTarget = false;
    }

    private static TextMeshProUGUI CreateLabel(Transform parent, string name, float fontSize, float yPos)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(PanelWidth - GameConstants.SAFE_AREA_PADDING * 2f, 120f);
        r.anchoredPosition = new Vector2(0f, yPos);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = fontSize;
        t.color = UIColors.ScoreWhite;
        t.raycastTarget = false;
        return t;
    }

    private TextMeshProUGUI CreateStatRow(Transform parent, string label, ref float yPos)
    {
        float rowWidth = PanelWidth - GameConstants.SAFE_AREA_PADDING * 2f;
        GameObject row = new($"Stat_{label}");
        row.transform.SetParent(parent, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.sizeDelta = new Vector2(rowWidth, StatRowHeight);
        rowRect.anchoredPosition = new Vector2(0f, yPos);

        GameObject labelGo = new("Label");
        labelGo.transform.SetParent(row.transform, false);
        RectTransform labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0.6f, 1f);
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.alignment = TextAlignmentOptions.Left;
        labelTmp.fontSize = StatLabelSize;
        labelTmp.color = UIColors.TargetGray;
        labelTmp.raycastTarget = false;

        GameObject valueGo = new("Value");
        valueGo.transform.SetParent(row.transform, false);
        RectTransform valueRect = valueGo.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.6f, 0f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.offsetMin = valueRect.offsetMax = Vector2.zero;
        TextMeshProUGUI valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
        valueTmp.alignment = TextAlignmentOptions.Right;
        valueTmp.fontSize = StatValueSize;
        valueTmp.color = UIColors.ScoreWhite;
        valueTmp.fontStyle = FontStyles.Bold;
        valueTmp.raycastTarget = false;

        yPos -= StatRowHeight + StatRowSpacing;
        return valueTmp;
    }

    private void CreateRestartButton(Transform parent)
    {
        GameObject buttonGo = new("RestartButton");
        buttonGo.transform.SetParent(parent, false);
        RectTransform buttonRect = buttonGo.AddComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(320f, 72f);
        buttonRect.anchoredPosition = new Vector2(0f, -350f);

        Image buttonBg = buttonGo.AddComponent<Image>();
        buttonBg.color = UIColors.RoomPink;
        _restartButton = buttonGo.AddComponent<Button>();

        GameObject labelGo = new("Label");
        labelGo.transform.SetParent(buttonGo.transform, false);
        RectTransform labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = "PLAY AGAIN";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = ButtonLabelSize;
        label.color = Color.white;
        label.fontStyle = FontStyles.Bold;
        label.raycastTarget = false;
    }
}
