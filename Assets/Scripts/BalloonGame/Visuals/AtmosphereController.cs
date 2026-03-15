using UnityEngine;

/// <summary>
/// Ambient fog and mist particles for the noir carnival atmosphere.
/// </summary>
[DisallowMultipleComponent]
public class AtmosphereController : MonoBehaviour
{
    private const int MaxMistParticles = 20;
    private const float MistLifetime = 12f;
    private const float MistSpeed = 0.1f;
    private const float MistSize = 2.5f;
    private const float MistRate = 2.0f;
    private const float ColorOscillationSpeed = 0.22f;

    private static readonly Color BaseMistColor = new(0.05f, 0.05f, 0.08f, 0.03f);
    private static readonly Color MagentaMistColor = new(0.06f, 0.04f, 0.06f, 0.02f);
    private static readonly Color CyanMistColor = new(0.04f, 0.06f, 0.08f, 0.02f);

    private ParticleSystem _mist;

    private void Awake() { EnsureMist(); }

    private void Update()
    {
        if (_mist == null) return;
        var main = _mist.main;
        float cycle = Time.time * ColorOscillationSpeed;
        float tMagenta = 0.5f + Mathf.Sin(cycle) * 0.5f;
        float tCyan = 0.5f + Mathf.Sin(cycle + Mathf.PI * 0.67f) * 0.5f;
        Color blended = Color.Lerp(BaseMistColor, MagentaMistColor, tMagenta);
        main.startColor = Color.Lerp(blended, CyanMistColor, tCyan * 0.4f);
    }

    private void EnsureMist()
    {
        if (_mist != null) return;
        GameObject go = new("AmbientMist");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, 0.3f);
        _mist = go.AddComponent<ParticleSystem>();

        var main = _mist.main;
        main.loop = true;
        main.startLifetime = MistLifetime;
        main.startSpeed = MistSpeed;
        main.startSize = MistSize;
        main.maxParticles = MaxMistParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = BaseMistColor;

        var emission = _mist.emission;
        emission.rateOverTime = MistRate;

        var shape = _mist.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(9f, 12f, 0.3f);

        var renderer = _mist.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                        ?? Shader.Find("Particles/Standard Unlit")
                        ?? Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.SetFloat("_Surface", 1f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            renderer.sharedMaterial = mat;
        }
    }
}
