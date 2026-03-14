using UnityEngine;

/// <summary>
/// Simple trail renderer attached to darts.
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
        _trail.startWidth = config.trailWidth;
        _trail.endWidth = 0f;
        _trail.material = new Material(Shader.Find("Sprites/Default"));
        _trail.startColor = config.trailColor;
        _trail.endColor = new Color(config.trailColor.r, config.trailColor.g, config.trailColor.b, 0f);
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
