using UnityEngine;

/// <summary>
/// Fits the attached RectTransform to the device safe area.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;
    private Vector2Int _lastResolution;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea ||
            _lastResolution.x != Screen.width ||
            _lastResolution.y != Screen.height)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;
        _lastResolution = new Vector2Int(Screen.width, Screen.height);

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
        _rectTransform.offsetMin = Vector2.one * GameConstants.SAFE_AREA_PADDING;
        _rectTransform.offsetMax = -Vector2.one * GameConstants.SAFE_AREA_PADDING;
    }
}
