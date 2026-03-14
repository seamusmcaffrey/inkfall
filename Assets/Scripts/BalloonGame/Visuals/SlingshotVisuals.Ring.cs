using UnityEngine;

public partial class SlingshotVisuals
{
    [Header("Launch Origin Indicator")]
    [SerializeField] private float _originRingRadius = 0.28f;
    [SerializeField] private int _originRingSegments = 48;
    [SerializeField] private float _originRingWidth = 0.025f;
    [SerializeField] private float _originPulseSpeed = 2.2f;
    [SerializeField] private float _originAlphaMin = 0.15f;
    [SerializeField] private float _originAlphaMax = 0.5f;

    private void CreateOriginRing()
    {
        _originRing = CreateLine("OriginRing", OriginRingColor, _originRingWidth, _originRingWidth);
        _originRing.loop = true;
        _originRing.positionCount = 0;
    }

    private void UpdateOriginRing(Vector3 origin)
    {
        float time = Time.time;
        float pulseAlpha = Mathf.Lerp(_originAlphaMin, _originAlphaMax,
            (Mathf.Sin(time * _originPulseSpeed) + 1f) * 0.5f);

        Color ringColor = new(OriginRingColor.r, OriginRingColor.g, OriginRingColor.b, pulseAlpha);
        _originRing.startColor = ringColor;
        _originRing.endColor = ringColor;

        // Subtle radius breathing
        float radius = _originRingRadius + 0.015f * Mathf.Sin(time * _originPulseSpeed * 0.7f);

        _originRing.positionCount = _originRingSegments;
        float zPos = origin.z - 0.8f; // Behind the band
        for (int i = 0; i < _originRingSegments; i++)
        {
            float angle = (float)i / _originRingSegments * Mathf.PI * 2f;
            float x = origin.x + Mathf.Cos(angle) * radius;
            float y = origin.y + Mathf.Sin(angle) * radius;
            _originRing.SetPosition(i, new Vector3(x, y, zPos));
        }
    }
}
