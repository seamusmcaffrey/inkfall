using UnityEngine;

/// <summary>
/// Subtle scale jiggle used to keep the balloon wall alive.
/// </summary>
[DisallowMultipleComponent]
public class BalloonJiggle : MonoBehaviour
{
    private Vector3 _baseScale;
    private float _offset;
    private bool _hasBaseScale;

    /// <summary>
    /// Extra scale multiplier driven by AimAssist highlight pulse.
    /// 1.0 = no highlight, >1.0 = highlighted.
    /// </summary>
    public float HighlightMultiplier { get; set; } = 1f;

    private void Awake()
    {
        _offset = Random.Range(0f, Mathf.PI * 2f);
        CaptureBaseScale();
    }

    private void OnEnable()
    {
        CaptureBaseScale();
    }

    private void Update()
    {
        if (!_hasBaseScale)
        {
            CaptureBaseScale();
        }

        float pulse = Mathf.Sin(Time.time * 1.2f + _offset) * 0.012f;
        transform.localScale = _baseScale * ((1f + pulse) * HighlightMultiplier);
    }

    /// <summary>
    /// Refreshes the scale that the jiggle animates around.
    /// Call after the wall assigns the balloon's perspective-scaled size.
    /// </summary>
    public void CaptureBaseScale()
    {
        _baseScale = transform.localScale;
        _hasBaseScale = true;
    }
}
