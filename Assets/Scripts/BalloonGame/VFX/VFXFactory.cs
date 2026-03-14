using UnityEngine;

/// <summary>
/// Factory for runtime-created particle systems.
/// Provides specialized configurations for balloon pops (small burst) and
/// paint splatters (neon dripping paint with elongated, gravity-heavy particles).
/// </summary>
public static class VFXFactory
{
    private static Material _particleMaterial;

    public static ParticleSystem EnsureBalloonPopSystem(Transform parent, ParticleSystem system, JuiceConfigSO config, Color color)
    {
        system ??= CreateBurst(parent, "BalloonPopVFX");
        int count = QualityTier.ScaleParticleCount(config.popParticleCount);
        ConfigureBurst(system, color, count, config.popParticleLifetime, config.popParticleSpeed, config.popParticleGravity);
        ConfigurePopParticles(system);
        return system;
    }

    public static ParticleSystem EnsurePaintSplatterSystem(Transform parent, ParticleSystem system, JuiceConfigSO config, Color color)
    {
        system ??= CreateBurst(parent, "PaintSplatterVFX");
        int count = QualityTier.ScaleParticleCount(config.paintParticleCount);
        ConfigureBurst(system, color, count, config.paintParticleLifetime, config.paintParticleSpeed, config.paintParticleGravity);
        ConfigurePaintParticles(system, config.paintParticleGravity);
        return system;
    }

    public static ParticleSystem EnsureImpactSparkSystem(Transform parent, ParticleSystem system, JuiceConfigSO config)
    {
        system ??= CreateBurst(parent, "ImpactSparkVFX");
        int count = QualityTier.ScaleParticleCount(config.sparkParticleCount);
        ConfigureBurst(system, config.sparkColorStart, count, config.sparkParticleLifetime, config.sparkParticleSpeed, config.sparkGravity);
        ConfigureSparkParticles(system, config);
        return system;
    }

    public static ParticleSystem EnsureWallHitSystem(Transform parent, ParticleSystem system, JuiceConfigSO config)
    {
        system ??= CreateBurst(parent, "WallHitVFX");
        int count = QualityTier.ScaleParticleCount(config.wallHitParticleCount);
        ConfigureBurst(system, color: new Color(0.85f, 0.85f, 0.92f, 1f), count, config.wallHitParticleLifetime, config.wallHitParticleSpeed, gravity: 0f);
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
        if (system.isPlaying || system.particleCount > 0)
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        var main = system.main;
        main.duration = lifetime;
        main.startLifetime = lifetime;
        main.startSpeed = speed;
        main.startSize = GameConstants.DEFAULT_PARTICLE_SIZE;
        main.maxParticles = count;
        main.gravityModifier = gravity;
        main.startColor = color;
        main.loop = false;
        main.playOnAwake = false;

        var emission = system.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
    }

    /// <summary>
    /// Balloon pop: small particles with slight size variation for an organic burst.
    /// </summary>
    private static void ConfigurePopParticles(ParticleSystem system)
    {
        var main = system.main;
        main.startSize = new ParticleSystem.MinMaxCurve(
            GameConstants.POP_PARTICLE_BASE_SIZE - GameConstants.POP_PARTICLE_SIZE_VARIATION,
            GameConstants.POP_PARTICLE_BASE_SIZE + GameConstants.POP_PARTICLE_SIZE_VARIATION);
    }

    /// <summary>
    /// Impact sparks: small, fast, chrome-metallic particles with velocity stretching,
    /// color fade from bright white to cool silver-blue, and slight gravity pull.
    /// </summary>
    private static void ConfigureSparkParticles(ParticleSystem system, JuiceConfigSO config)
    {
        var main = system.main;
        main.startSize = new ParticleSystem.MinMaxCurve(
            GameConstants.SPARK_SIZE_MIN, GameConstants.SPARK_SIZE_MAX);

        var colorOverLifetime = system.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(config.sparkColorStart, 0f),
                new GradientColorKey(config.sparkColorEnd, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = system.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.separateAxes = false;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.4f, 0.6f),
            new Keyframe(1f, 0f)));

        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = GameConstants.SPARK_VELOCITY_STRETCH;
            renderer.lengthScale = 1f;
        }
    }

    /// <summary>
    /// Paint splatter: large elongated drops with gravity pull and shrink-over-lifetime
    /// for a neon dripping paint look.
    /// </summary>
    private static void ConfigurePaintParticles(ParticleSystem system, float baseGravity)
    {
        var main = system.main;
        main.startSize3D = true;
        main.startSizeX = GameConstants.PAINT_PARTICLE_SIZE_STRETCH_X;
        main.startSizeY = GameConstants.PAINT_PARTICLE_SIZE_STRETCH_Y;
        main.startSizeZ = GameConstants.PAINT_PARTICLE_SIZE_STRETCH_Z;
        main.gravityModifier = baseGravity + GameConstants.PAINT_EXTRA_GRAVITY;

        var sizeOverLifetime = system.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.separateAxes = false;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, GameConstants.PAINT_SIZE_OVER_LIFETIME_END)));
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
