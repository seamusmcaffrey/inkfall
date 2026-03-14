using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lazy singleton full-screen fade transition.
/// </summary>
public class ScreenTransition : MonoBehaviour
{
    private static ScreenTransition _instance;
    private CanvasGroup _canvasGroup;
    private Coroutine _activeTransition;

    private static ScreenTransition Instance
    {
        get
        {
            if (_instance == null)
            {
                CreateInstance();
            }

            return _instance;
        }
    }

    private static void CreateInstance()
    {
        var go = new GameObject("[ScreenTransition]");
        DontDestroyOnLoad(go);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;

        var imageGo = new GameObject("BlackOverlay");
        imageGo.transform.SetParent(go.transform, false);
        var rect = imageGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = imageGo.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        var group = imageGo.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        _instance = go.AddComponent<ScreenTransition>();
        _instance._canvasGroup = group;
    }

    public static void FadeTo(Action onBlack, float fadeIn = 0.3f, float fadeOut = 0.3f)
    {
        Instance.StartFadeTo(onBlack, fadeIn, fadeOut);
    }

    public static Coroutine FadeOut(float duration = 0.3f)
    {
        return Instance.StartFade(Instance._canvasGroup.alpha, 1f, duration);
    }

    public static Coroutine FadeIn(float duration = 0.3f)
    {
        return Instance.StartFade(Instance._canvasGroup.alpha, 0f, duration);
    }

    private void StartFadeTo(Action onBlack, float fadeIn, float fadeOut)
    {
        if (_activeTransition != null)
        {
            StopCoroutine(_activeTransition);
        }

        _activeTransition = StartCoroutine(FadeToSequence(onBlack, fadeIn, fadeOut));
    }

    private Coroutine StartFade(float from, float to, float duration)
    {
        if (_activeTransition != null)
        {
            StopCoroutine(_activeTransition);
        }

        _activeTransition = StartCoroutine(LerpAlpha(from, to, duration));
        return _activeTransition;
    }

    private IEnumerator FadeToSequence(Action onBlack, float fadeIn, float fadeOut)
    {
        yield return LerpAlpha(_canvasGroup.alpha, 1f, fadeIn);
        onBlack?.Invoke();
        yield return null;
        yield return LerpAlpha(1f, 0f, fadeOut);
        _activeTransition = null;
    }

    private IEnumerator LerpAlpha(float from, float to, float duration)
    {
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = from;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        _canvasGroup.alpha = to;
        _canvasGroup.blocksRaycasts = to > 0.01f;
        _activeTransition = null;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
