using UnityEngine;

/// <summary>
/// Constrains a RectTransform to the camera's target aspect ratio (9:16).
/// Keeps UI elements within the game viewport when the screen is wider or
/// taller than 9:16, preventing HUD from extending into pillarbox/letterbox bars.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ViewportConstraint : MonoBehaviour
{
    private const float TargetAspect = 9f / 16f;

    private RectTransform _rect;
    private bool _applying;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        ApplyConstraint();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (_applying) return;
        ApplyConstraint();
    }

    private void ApplyConstraint()
    {
        if (_rect == null) return;

        RectTransform canvasRect = _rect.parent as RectTransform;
        if (canvasRect == null) return;

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        if (canvasWidth <= 0f || canvasHeight <= 0f) return;

        _applying = true;

        float canvasAspect = canvasWidth / canvasHeight;

        _rect.anchorMin = Vector2.zero;
        _rect.anchorMax = Vector2.one;

        if (canvasAspect > TargetAspect)
        {
            float targetWidth = canvasHeight * TargetAspect;
            float excess = canvasWidth - targetWidth;
            _rect.offsetMin = new Vector2(excess / 2f, 0f);
            _rect.offsetMax = new Vector2(-excess / 2f, 0f);
        }
        else if (canvasAspect < TargetAspect)
        {
            float targetHeight = canvasWidth / TargetAspect;
            float excess = canvasHeight - targetHeight;
            _rect.offsetMin = new Vector2(0f, excess / 2f);
            _rect.offsetMax = new Vector2(0f, -excess / 2f);
        }
        else
        {
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }

        _applying = false;
    }
}
