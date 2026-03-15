using UnityEngine;

/// <summary>
/// Ambient fog and mist particles for the noir carnival atmosphere.
/// Two layers: foreground wisps and background haze.
/// </summary>
[DisallowMultipleComponent]
public class AtmosphereController : MonoBehaviour
{
    private const int MaxMistParticles = 50;
    private const float MistLifetime = 14f;
    private const float MistSpeed = 0.15f;
    private const float MistSize = 5.0f;
    private const float MistRate = 5.0f;
    private const float ColorOscillationSpeed = 0.22f;

    private const int MaxHazeParticles = 30;
    private const float HazeLifetime = 20f;
    private const float HazeSpeed = 0.05f;
    private const float HazeSize = 8.0f;
    private const float HazeRate = 2.5f;

    private static readonly Color BaseMistColor = new(0.08f, 0.06f, 0.10f, 0.30f);
    private static readonly Color MagentaMistColor = new(0.16f, 0.05f, 0.12f, 0.25f);
    private static readonly Color CyanMistColor = new(0.05f, 0.10f, 0.16f, 0.25f);
    private static readonly Color BaseHazeColor = new(0.05f, 0.04f, 0.08f, 0.20f);

    private ParticleSystem _mist;
    private ParticleSystem _haze;

    private void Awake()
    {
        EnsureMist();
        EnsureHaze();
    }

    private void Update()
    {
        if (_mist == null) return;
        var main = _mist.main;
        float cycle = Time.time * ColorOscillationSpeed;
        float tMagenta = 0.5f + Mathf.Sin(cycle) * 0.5f;
        float tCyan = 0.5f + Mathf.Sin(cycle + Mathf.PI * 0.67f) * 0.5f;
        Color blended = Color.Lerp(BaseMistColor, MagentaMistColor, tMagenta);
        main.startColor = Color.Lerp(blended, CyanMistColor, tCyan * 0.5f);
    }

    private void EnsureMist()
    {
        if (_mist != null) return;
        _mist = BuildParticleLayer("AmbientMist", new Vector3(0f, 0f, 0.3f),
            MistLifetime, MistSpeed, MistSize, MaxMistParticles, MistRate,
            new Vector3(9f, 12f, 0.3f), BaseMistColor);
    }

    private void EnsureHaze()
    {
        if (_haze != null) return;
        _haze = BuildParticleLayer("BackgroundHaze", new Vector3(0f, 2f, 0.4f),
            HazeLifetime, HazeSpeed, HazeSize, MaxHazeParticles, HazeRate,
            new Vector3(10f, 14f, 0.2f), BaseHazeColor);
    }

    private ParticleSystem BuildParticleLayer(
        string layerName, Vector3 localPos,
        float lifetime, float speed, float size, int maxParticles,
        float rate, Vector3 shapeScale, Color color)
    {
        GameObject go = new(layerName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = true;
        main.startLifetime = lifetime;
        main.startSpeed = speed;
        main.startSize = size;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = color;

        var emission = ps.emission;
        emission.rateOverTime = rate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = shapeScale;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
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
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            renderer.sharedMaterial = mat;
        }

        return ps;
    }
}
