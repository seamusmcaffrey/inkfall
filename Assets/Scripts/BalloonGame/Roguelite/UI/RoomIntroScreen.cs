using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cinematic room intro overlay with large room number, name, and target score.
/// Dark noir backdrop with neon accent text for dramatic room transitions.
/// </summary>
[DisallowMultipleComponent]
public class RoomIntroScreen : MonoBehaviour
{
    private const float DisplayDuration = 1.6f;
    private const float FadeOutDuration = 0.6f;
    private const float SlideInDuration = 0.35f;
    private const float RoomNumberSize = 112f;
    private const float RoomNameSize = 38f;
    private const float TargetSize = 24f;
    private const float DividerHeight = 2f;
    private const float DividerWidth = 200f;

    private CanvasGroup _group;
    private Image _backdrop;
    private TextMeshProUGUI _roomNumber;
    private TextMeshProUGUI _roomName;
    private TextMeshProUGUI _targetScore;
    private RectTransform _contentRoot;

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
        _targetScore.text = $"TARGET: {config.targetScore:N0}";

        Color accent = config.accentColor;
        _roomNumber.color = UIColors.ScoreWhite;
        _roomNumber.fontStyle = FontStyles.Bold;
        _roomName.color = accent.a > 0.01f ? accent : UIColors.InkCyan;
        _targetScore.color = UIColors.TargetGray;

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        _group.alpha = 0f;
        _group.blocksRaycasts = true;

        // Slide up + fade in
        float slideElapsed = 0f;
        Vector2 startPos = new(0f, -40f);
        Vector2 endPos = Vector2.zero;
        while (slideElapsed < SlideInDuration)
        {
            slideElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(slideElapsed / SlideInDuration);
            float eased = EaseOutCubic(t);
            _group.alpha = eased;
            _contentRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            yield return null;
        }

        _group.alpha = 1f;
        _contentRoot.anchoredPosition = endPos;

        yield return new WaitForSecondsRealtime(DisplayDuration);

        // Fade out
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
        scaler.matchWidthOrHeight = 0f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);

        // Full-screen dark backdrop covers entire screen.
        GameObject bg = new("Backdrop");
        bg.transform.SetParent(transform, false);

        // ViewportRoot stretches to fill screen; SafeArea handles notch insets.
        GameObject vpRoot = new("ViewportRoot");
        vpRoot.transform.SetParent(transform, false);
        RectTransform vpRect = vpRoot.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        vpRoot.AddComponent<ViewportConstraint>();
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        _backdrop = bg.AddComponent<Image>();
        _backdrop.color = new Color(0.01f, 0.01f, 0.03f, 0.94f);

        // Content root for slide animation
        GameObject content = new("Content");
        content.transform.SetParent(vpRoot.transform, false);
        _contentRoot = content.AddComponent<RectTransform>();
        _contentRoot.anchorMin = _contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        _contentRoot.sizeDelta = new Vector2(900f, 400f);

        _roomNumber = CreateCenteredLabel("RoomNum", 70f, RoomNumberSize);
        _roomNumber.fontStyle = FontStyles.Bold;

        // Divider line beneath room number
        CreateDivider();

        _roomName = CreateCenteredLabel("RoomName", -10f, RoomNameSize);
        _targetScore = CreateCenteredLabel("Target", -65f, TargetSize);
    }

    private void CreateDivider()
    {
        GameObject divider = new("Divider");
        divider.transform.SetParent(_contentRoot, false);
        RectTransform r = divider.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(DividerWidth, DividerHeight);
        r.anchoredPosition = new Vector2(0f, 20f);
        Image img = divider.AddComponent<Image>();
        img.color = UIColors.TargetGray;
        img.raycastTarget = false;
    }

    private TextMeshProUGUI CreateCenteredLabel(string name, float yOffset, float fontSize)
    {
        GameObject go = new(name);
        go.transform.SetParent(_contentRoot, false);
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

    private static float EaseOutCubic(float t)
    {
        float f = t - 1f;
        return 1f + f * f * f;
    }
}
