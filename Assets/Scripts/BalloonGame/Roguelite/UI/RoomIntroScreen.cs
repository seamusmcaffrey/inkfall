using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cinematic room intro overlay with room number, name, and target score.
/// </summary>
[DisallowMultipleComponent]
public class RoomIntroScreen : MonoBehaviour
{
    private const float DisplayDuration = 1.4f;
    private const float FadeOutDuration = 0.5f;
    private const float RoomNumberSize = 96f;
    private const float RoomNameSize = 42f;
    private const float TargetSize = 28f;

    private CanvasGroup _group;
    private Image _backdrop;
    private TextMeshProUGUI _roomNumber;
    private TextMeshProUGUI _roomName;
    private TextMeshProUGUI _targetScore;

    private void Awake()
    {
        EnsureUi();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
    }

    public void Show(RoomConfig config)
    {
        EnsureUi();
        _roomNumber.text = $"ROOM {config.roomNumber}";
        _roomName.text = config.roomName.ToUpperInvariant();
        _targetScore.text = $"TARGET SCORE: {config.targetScore:N0}";

        Color accent = config.accentColor;
        _roomNumber.color = UIColors.ScoreWhite;
        _roomName.color = accent.a > 0.01f ? accent : UIColors.TargetGray;
        _targetScore.color = UIColors.TargetGray;

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        _group.alpha = 1f;
        _group.blocksRaycasts = true;
        yield return new WaitForSecondsRealtime(DisplayDuration);

        float elapsed = 0f;
        while (elapsed < FadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(1f, 0f, elapsed / FadeOutDuration);
            yield return null;
        }

        _group.alpha = 0f;
        _group.blocksRaycasts = false;
    }

    private void EnsureUi()
    {
        if (_roomNumber != null) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
            Object.Destroy(transform.GetChild(i).gameObject);

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 430;
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);

        GameObject bg = new("Backdrop");
        bg.transform.SetParent(transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        _backdrop = bg.AddComponent<Image>();
        _backdrop.color = new Color(0.02f, 0.02f, 0.04f, 0.92f);

        _roomNumber = CreateCenteredLabel("RoomNum", 60f, RoomNumberSize);
        _roomName = CreateCenteredLabel("RoomName", 0f, RoomNameSize);
        _targetScore = CreateCenteredLabel("Target", -60f, TargetSize);
    }

    private TextMeshProUGUI CreateCenteredLabel(string name, float yOffset, float fontSize)
    {
        GameObject go = new(name);
        go.transform.SetParent(transform, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(900f, 120f);
        r.anchoredPosition = new Vector2(0f, yOffset);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = fontSize;
        t.raycastTarget = false;
        return t;
    }
}
