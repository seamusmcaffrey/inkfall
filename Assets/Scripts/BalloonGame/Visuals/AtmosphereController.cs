using UnityEngine;

/// <summary>
/// Ambient fog and mist particles for the noir carnival atmosphere.
/// </summary>
[DisallowMultipleComponent]
public class AtmosphereController : MonoBehaviour
{
    private const int MaxMistParticles = 40;
    private const float MistLifetime = 10f;
    private const float MistSpeed = 0.12f;
    private const float MistSize = 3.2f;
    private const float MistRate = 4f;

    private static readonly Color BaseMistColor = new(0.15f, 0.18f, 0.25f, 0.08f);
    private static readonly Color NeonMistColor = new(0.4f, 0.08f, 0.35f, 0.06f);

    private ParticleSystem _mist;

    private void Awake() { EnsureMist(); }

    private void Update()
    {
        if (_mist == null) return;
        var main = _mist.main;
        float t = 0.5f + Mathf.Sin(Time.time * 0.3f) * 0.5f;
        main.startColor = Color.Lerp(BaseMistColor, NeonMistColor, t);
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
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
        if (shader != null)
        {
            renderer.sharedMaterial = new Material(shader);
        }
    }
}
