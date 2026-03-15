using UnityEngine;

/// <summary>
/// Previously constrained UI to a 9:16 aspect ratio to match camera pillarboxing.
/// Now that the camera fills the full screen (width-locked adaptive sizing),
/// this component simply ensures the RectTransform stretches to fill its parent.
/// Kept as a component so the 9 UI builder files that AddComponent it continue to work.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ViewportConstraint : MonoBehaviour
{
    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        ApplyFullStretch();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyFullStretch();
    }

    private void ApplyFullStretch()
    {
        if (_rect == null) return;

        _rect.anchorMin = Vector2.zero;
        _rect.anchorMax = Vector2.one;
        _rect.offsetMin = Vector2.zero;
        _rect.offsetMax = Vector2.zero;
    }
}
