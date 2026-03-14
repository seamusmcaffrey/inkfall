using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen flash used on bigger combo spikes.
/// </summary>
[DisallowMultipleComponent]
public class ComboFlashVFX : MonoBehaviour
{
    private CanvasGroup _group;

    private void Awake()
    {
        EnsureUi();
    }

    public void Flash(Color color, JuiceConfigSO config)
    {
        EnsureUi();
        StopAllCoroutines();
        Image image = GetComponentInChildren<Image>();
        image.color = color;
        StartCoroutine(FlashRoutine(config.comboFlashMaxAlpha, config.comboFlashDuration));
    }

    private IEnumerator FlashRoutine(float alpha, float duration)
    {
        _group.alpha = alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(alpha, 0f, elapsed / duration);
            yield return null;
        }
        _group.alpha = 0f;
    }

    private void EnsureUi()
    {
        if (_group == null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 800;
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);
        _group.blocksRaycasts = false;

        if (GetComponentInChildren<Image>() == null)
        {
            GameObject go = new("Flash");
            go.transform.SetParent(transform, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.AddComponent<Image>();
        }
    }
}
