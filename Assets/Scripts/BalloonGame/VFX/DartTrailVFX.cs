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
        Shader trailShader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                             ?? Shader.Find("Particles/Standard Unlit")
                             ?? Shader.Find("Sprites/Default");
        Material trailMat = new Material(trailShader);
        trailMat.SetFloat("_Surface", 1f);
        trailMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        trailMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        trailMat.SetInt("_ZWrite", 0);
        trailMat.renderQueue = 3100;
        trailMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        _trail.material = trailMat;
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
