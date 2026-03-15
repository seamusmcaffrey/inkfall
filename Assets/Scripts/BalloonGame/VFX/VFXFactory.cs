using UnityEngine;

/// <summary>
/// Factory for runtime-created particle systems.
/// Provides specialized configurations for balloon pops (dramatic neon burst),
/// paint splatters (large dripping paint with gravity-heavy particles),
/// and impact sparks (fast, bright chrome-metallic streaks).
/// </summary>
public static class VFXFactory
{
    private static Material _particleMaterial;

    public static ParticleSystem EnsureBalloonPopSystem(Transform parent, ParticleSystem system, JuiceConfigSO config, Color color)
    {
        system ??= CreateBurst(parent, "BalloonPopVFX");
        int count = QualityTier.ScaleParticleCount(config.popParticleCount);
        Color neonColor = BoostNeon(color, config.popNeonBoost);
        ConfigureBurst(system, neonColor, count, config.popParticleLifetime, config.popParticleSpeed, config.popParticleGravity);
        ConfigurePopParticles(system, neonColor);
        return system;
    }

    public static ParticleSystem EnsurePaintSplatterSystem(Transform parent, ParticleSystem system, JuiceConfigSO config, Color color)
    {
        system ??= CreateBurst(parent, "PaintSplatterVFX");
        int count = QualityTier.ScaleParticleCount(config.paintParticleCount);
        Color neonColor = BoostNeon(color, config.paintNeonBoost);
        ConfigureBurst(system, neonColor, count, config.paintParticleLifetime, config.paintParticleSpeed, config.paintParticleGravity);
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

    /// <summary>
    /// Boost color channels for HDR-like neon vibrancy.
    /// </summary>
    public static Color BoostNeon(Color baseColor, float boostFactor)
    {
        return new Color(
            Mathf.Clamp01(baseColor.r * boostFactor),
            Mathf.Clamp01(baseColor.g * boostFactor),
            Mathf.Clamp01(baseColor.b * boostFactor),
            baseColor.a);
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
    /// Balloon pop: wide size variation with alpha fade for organic, vibrant burst.
    /// </summary>
    private static void ConfigurePopParticles(ParticleSystem system, Color neonColor)
    {
        var main = system.main;
        float baseSize = GameConstants.POP_PARTICLE_BASE_SIZE;
        float variation = GameConstants.POP_PARTICLE_SIZE_VARIATION;
        main.startSize = new ParticleSystem.MinMaxCurve(baseSize - variation, baseSize + variation * 2f);

        var col = system.colorOverLifetime;
        col.enabled = true;
        col.color = BuildFadeGradient(neonColor);

        var sol = system.sizeOverLifetime;
        sol.enabled = true;
        sol.separateAxes = false;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.6f), new Keyframe(0.15f, 1f), new Keyframe(1f, 0f)));
    }

    /// <summary>
    /// Impact sparks: fast chrome-metallic streaks with velocity stretching.
    /// </summary>
    private static void ConfigureSparkParticles(ParticleSystem system, JuiceConfigSO config)
    {
        var main = system.main;
        main.startSize = new ParticleSystem.MinMaxCurve(GameConstants.SPARK_SIZE_MIN, GameConstants.SPARK_SIZE_MAX);

        var col = system.colorOverLifetime;
        col.enabled = true;
        col.color = BuildTwoColorGradient(config.sparkColorStart, config.sparkColorEnd);

        var sol = system.sizeOverLifetime;
        sol.enabled = true;
        sol.separateAxes = false;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.4f, 0.6f), new Keyframe(1f, 0f)));

        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        if (renderer == null) return;
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = GameConstants.SPARK_VELOCITY_STRETCH;
        renderer.lengthScale = 1f;
    }

    /// <summary>
    /// Paint splatter: large elongated drops with heavy gravity and shrink-over-lifetime
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

    private static Gradient BuildFadeGradient(Color color)
    {
        Gradient g = new();
        g.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.3f), new GradientAlphaKey(0f, 1f) });
        return g;
    }

    private static Gradient BuildTwoColorGradient(Color start, Color end)
    {
        Gradient g = new();
        g.SetKeys(
            new[] { new GradientColorKey(start, 0f), new GradientColorKey(end, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.3f), new GradientAlphaKey(0f, 1f) });
        return g;
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
