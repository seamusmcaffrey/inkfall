using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen-edge vignette flash on combo chains. Colors escalate with combo count
/// (white -> yellow -> orange -> magenta). Uses four edge images for a vignette
/// effect rather than a full-screen wash.
/// </summary>
[DisallowMultipleComponent]
public class ComboFlashVFX : MonoBehaviour
{
    private CanvasGroup _group;
    private Image[] _edgeImages;

    private void Awake()
    {
        EnsureUi();
    }

    public void Flash(Color color, JuiceConfigSO config)
    {
        Flash(color, config, comboCount: 2);
    }

    public void Flash(Color color, JuiceConfigSO config, int comboCount)
    {
        EnsureUi();
        StopAllCoroutines();

        float escalation = Mathf.Min(
            comboCount * GameConstants.COMBO_FLASH_ALPHA_PER_COMBO,
            GameConstants.COMBO_FLASH_ALPHA_CAP);
        float alpha = Mathf.Min(config.comboFlashMaxAlpha + escalation, GameConstants.COMBO_FLASH_ALPHA_CAP);

        for (int i = 0; i < _edgeImages.Length; i++)
        {
            _edgeImages[i].color = color;
        }

        StartCoroutine(FlashRoutine(alpha, config.comboFlashDuration, config.comboFlashAttackRatio));
    }

    private IEnumerator FlashRoutine(float peakAlpha, float duration, float attackRatio)
    {
        float attackTime = duration * attackRatio;
        float decayTime = duration - attackTime;

        // Attack: quick ramp up
        float elapsed = 0f;
        while (elapsed < attackTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / attackTime);
            _group.alpha = Mathf.Lerp(0f, peakAlpha, t * t);
            yield return null;
        }

        // Decay: smooth ease-out
        elapsed = 0f;
        while (elapsed < decayTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / decayTime);
            float eased = 1f - (1f - t) * (1f - t);
            _group.alpha = Mathf.Lerp(peakAlpha, 0f, eased);
            yield return null;
        }

        _group.alpha = 0f;
    }

    private void EnsureUi()
    {
        if (_group != null && _edgeImages != null)
        {
            return;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
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
        _group.alpha = 0f;

        _edgeImages = new Image[GameConstants.COMBO_FLASH_EDGE_COUNT];
        _edgeImages[0] = CreateEdge("EdgeTop", Vector2.up, Vector2.one, Vector2.zero, new Vector2(0f, -1f));
        _edgeImages[1] = CreateEdge("EdgeBottom", Vector2.zero, Vector2.right, Vector2.zero, new Vector2(0f, 1f));
        _edgeImages[2] = CreateEdge("EdgeLeft", Vector2.zero, Vector2.up, Vector2.zero, new Vector2(1f, 0f));
        _edgeImages[3] = CreateEdge("EdgeRight", Vector2.right, Vector2.one, Vector2.zero, new Vector2(-1f, 0f));
    }

    private Image CreateEdge(string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 gradientDir)
    {
        GameObject go = new(name);
        go.transform.SetParent(transform, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        float thickness = JuiceConfigSO.Instance.comboFlashEdgeThickness;
        bool isHorizontal = Mathf.Abs(gradientDir.x) > Mathf.Abs(gradientDir.y);
        if (isHorizontal)
        {
            float refWidth = GameConstants.UI_REFERENCE_RESOLUTION.x;
            rect.sizeDelta = new Vector2(refWidth * thickness, 0f);
        }
        else
        {
            float refHeight = GameConstants.UI_REFERENCE_RESOLUTION.y;
            rect.sizeDelta = new Vector2(0f, refHeight * thickness);
        }

        Image image = go.AddComponent<Image>();
        image.raycastTarget = false;
        return image;
    }
}
