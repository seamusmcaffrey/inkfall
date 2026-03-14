using UnityEngine;

/// <summary>
/// Factory for runtime-created particle systems.
/// </summary>
public static class VFXFactory
{
    private static Material _particleMaterial;

    public static ParticleSystem EnsureBalloonPopSystem(Transform parent, ParticleSystem system, JuiceConfigSO config, Color color)
    {
        return EnsureBurst(parent, system, "BalloonPopVFX", color, QualityTier.ScaleParticleCount(config.popParticleCount), config.popParticleLifetime, config.popParticleSpeed, config.popParticleGravity);
    }

    public static ParticleSystem EnsurePaintSplatterSystem(Transform parent, ParticleSystem system, JuiceConfigSO config, Color color)
    {
        return EnsureBurst(parent, system, "PaintSplatterVFX", color, QualityTier.ScaleParticleCount(config.paintParticleCount), config.paintParticleLifetime, config.paintParticleSpeed, config.paintParticleGravity);
    }

    public static ParticleSystem EnsureImpactSparkSystem(Transform parent, ParticleSystem system, JuiceConfigSO config)
    {
        return EnsureBurst(parent, system, "ImpactSparkVFX", new Color(1f, 0.9f, 0.5f, 1f), QualityTier.ScaleParticleCount(config.sparkParticleCount), config.sparkParticleLifetime, config.sparkParticleSpeed, 0f);
    }

    public static ParticleSystem EnsureWallHitSystem(Transform parent, ParticleSystem system, JuiceConfigSO config)
    {
        return EnsureBurst(parent, system, "WallHitVFX", new Color(0.85f, 0.85f, 0.92f, 1f), QualityTier.ScaleParticleCount(config.wallHitParticleCount), config.wallHitParticleLifetime, config.wallHitParticleSpeed, 0f);
    }

    private static ParticleSystem EnsureBurst(Transform parent, ParticleSystem system, string name, Color color, int count, float lifetime, float speed, float gravity)
    {
        system ??= CreateBurst(parent, name);
        ConfigureBurst(system, color, count, lifetime, speed, gravity);
        return system;
    }

    private static ParticleSystem CreateBurst(Transform parent, string name)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        ParticleSystem system = go.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = GetParticleMaterial();
        var shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        var emission = system.emission;
        emission.rateOverTime = 0f;
        return system;
    }

    private static void ConfigureBurst(ParticleSystem system, Color color, int count, float lifetime, float speed, float gravity)
    {
        var main = system.main;
        main.duration = lifetime;
        main.startLifetime = lifetime;
        main.startSpeed = speed;
        main.startSize = 0.08f;
        main.maxParticles = count;
        main.gravityModifier = gravity;
        main.startColor = color;
        main.loop = false;
        main.playOnAwake = false;

        var emission = system.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
    }

    private static Material GetParticleMaterial()
    {
        if (_particleMaterial != null)
        {
            return _particleMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
        _particleMaterial = shader != null ? new Material(shader) : null;
        return _particleMaterial;
    }
}
