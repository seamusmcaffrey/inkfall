using UnityEngine;

/// <summary>
/// Controls ambient mist and light pulsing in the playfield.
/// </summary>
[DisallowMultipleComponent]
public class AtmosphereController : MonoBehaviour
{
    [SerializeField] private Color _mistColor = new(0.45f, 0.75f, 0.95f, 0.15f);
    private ParticleSystem _mist;

    private void Awake()
    {
        EnsureMist();
    }

    private void Update()
    {
        if (_mist == null)
        {
            return;
        }

        var main = _mist.main;
        main.startColor = Color.Lerp(_mistColor, new Color(0.95f, 0.2f, 0.65f, 0.12f), 0.5f + Mathf.Sin(Time.time * 0.4f) * 0.5f);
    }

    private void EnsureMist()
    {
        if (_mist != null)
        {
            return;
        }

        GameObject go = new("AmbientMist");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, 0.6f);
        _mist = go.AddComponent<ParticleSystem>();
        var main = _mist.main;
        main.loop = true;
        main.startLifetime = 8f;
        main.startSpeed = 0.2f;
        main.startSize = 2.4f;
        main.maxParticles = 48;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = _mistColor;

        var emission = _mist.emission;
        emission.rateOverTime = 6f;
        var shape = _mist.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(8.5f, 11f, 0.2f);
    }
}
