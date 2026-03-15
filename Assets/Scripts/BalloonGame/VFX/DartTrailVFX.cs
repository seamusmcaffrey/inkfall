using UnityEngine;

/// <summary>
/// Speed-streak trail renderer attached to darts.
/// Uses a wide white-to-transparent gradient for high visibility.
/// </summary>
[DisallowMultipleComponent]
public class DartTrailVFX : MonoBehaviour
{
    private TrailRenderer _trail;

    private void Awake()
    {
        _trail = ComponentUtility.EnsureComponent<TrailRenderer>(gameObject);
        Configure(JuiceConfigSO.Instance);
    }

    public void Configure(JuiceConfigSO config)
    {
        if (_trail == null)
        {
            return;
        }

        _trail.time = config.trailLifetime;
        _trail.startWidth = config.trailStartWidth;
        _trail.endWidth = config.trailEndWidth;
        _trail.material = new Material(Shader.Find("Sprites/Default"));
        _trail.startColor = config.trailStartColor;
        _trail.endColor = config.trailEndColor;
        _trail.numCornerVertices = 4;
        _trail.numCapVertices = 4;
        _trail.emitting = config.dartTrailEnabled;
    }

    public void OnLaunch()
    {
        if (_trail != null)
        {
            _trail.Clear();
            _trail.emitting = JuiceConfigSO.Instance.dartTrailEnabled;
        }
    }
}
