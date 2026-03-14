using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CanvasGroup alpha fade utility.
/// </summary>
[DisallowMultipleComponent]
public class FadeOverlay : MonoBehaviour
{
    private CanvasGroup _group;

    private void Awake()
    {
        EnsureUi();
    }

    public Coroutine FadeTo(float targetAlpha, float duration)
    {
        EnsureUi();
        return StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    public void ShowInstant(float alpha = 1f)
    {
        EnsureUi();
        _group.alpha = alpha;
        _group.blocksRaycasts = alpha > 0.01f;
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = _group.alpha;
        float elapsed = 0f;
        _group.blocksRaycasts = true;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        _group.alpha = targetAlpha;
        _group.blocksRaycasts = targetAlpha > 0.01f;
    }

    private void EnsureUi()
    {
        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        if (_group == null)
        {
            GameObject cover = new("Cover");
            cover.transform.SetParent(transform, false);
            RectTransform rect = cover.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = cover.AddComponent<Image>();
            image.color = Color.black;
            _group = cover.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
        }
    }
}
