using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Central tuning asset for particles, audio, haptics, and screen FX.
/// </summary>
[CreateAssetMenu(fileName = "JuiceConfig", menuName = "INKSHOT/Juice Config")]
public class JuiceConfigSO : ScriptableObject
{
    private const string AssetPath = "Assets/ScriptableObjects/JuiceConfig.asset";
    private static JuiceConfigSO _instance;

    [Header("Master Controls")]
    public bool juiceEnabled = true;
    [Range(0f, 2f)] public float globalIntensity = 1f;

    [Header("Particles - Balloon Pop")]
    public bool balloonPopEnabled = true;
    [Range(10, 60)] public int popParticleCount = 25;
    public float popParticleLifetime = 0.55f;
    public float popParticleSpeed = 5f;
    public float popParticleGravity = 2f;
    [Range(1f, 2f)] public float popNeonBoost = 1.4f;

    [Header("Particles - Paint Splatter")]
    public bool paintSplatterEnabled = true;
    [Range(20, 80)] public int paintParticleCount = 45;
    public float paintParticleLifetime = 1.1f;
    public float paintParticleSpeed = 7f;
    public float paintParticleGravity = 3f;
    [Range(1, 6)] public int paintDripCount = 4;
    public float paintDripSpeed = 0.6f;
    public float paintDripLifetime = 2.5f;
    [Range(1f, 2f)] public float paintNeonBoost = 1.45f;
    public float paintDripStartWidth = 0.1f;
    public float paintDripEndWidth = 0.03f;

    [Header("Particles - Impact")]
    public bool impactSparkEnabled = true;
    [Range(4, 30)] public int sparkParticleCount = 18;
    public float sparkParticleLifetime = 0.1f;
    public float sparkParticleSpeed = 16f;
    public Color sparkColorStart = new(1f, 1f, 1f, 1f);
    public Color sparkColorEnd = new(0.6f, 0.75f, 0.95f, 0f);
    public float sparkGravity = GameConstants.SPARK_GRAVITY;

    [Header("Particles - Wall Hit")]
    public bool wallHitEnabled = true;
    [Range(3, 12)] public int wallHitParticleCount = 6;
    public float wallHitParticleLifetime = 0.28f;
    public float wallHitParticleSpeed = 2f;

    [Header("Dart Trail")]
    public bool dartTrailEnabled = true;
    public float trailStartWidth = 0.06f;
    public float trailEndWidth = 0f;
    public float trailLifetime = 0.45f;
    public Color trailStartColor = new(1f, 1f, 1f, 0.8f);
    public Color trailEndColor = new(1f, 1f, 1f, 0f);

    [Header("Screen Shake")]
    public bool screenShakeEnabled = true;
    public float shakeIntensityPop = 0.1f;
    public float shakeIntensityCombo = 0.25f;
    public float shakeIntensityPaint = 0.38f;
    public float shakeDuration = 0.2f;
    public float shakePerlinSpeed = GameConstants.SHAKE_PERLIN_SPEED;
    public float shakeDecayExponent = GameConstants.SHAKE_DECAY_EXPONENT;
    public float shakeComboEscalationPerHit = GameConstants.SHAKE_COMBO_ESCALATION_PER_HIT;
    public int shakeComboEscalationCap = GameConstants.SHAKE_COMBO_ESCALATION_CAP;

    [Header("Chromatic Pulse")]
    public bool chromaticPulseEnabled = true;
    public int chromaticComboThreshold = 3;
    public float chromaticMaxIntensity = 0.5f;
    public float chromaticPulseDuration = 0.3f;

    [Header("Combo Flash")]
    public bool comboFlashEnabled = true;
    public float comboFlashMaxAlpha = 0.15f;
    public float comboFlashDuration = 0.2f;
    public float comboFlashAttackRatio = 0.15f;
    public float comboFlashEdgeThickness = GameConstants.COMBO_FLASH_EDGE_THICKNESS;

    [Header("Slow Motion")]
    public bool slowMotionEnabled = true;
    public int slowMotionComboThreshold = GameConstants.SLOMO_COMBO_THRESHOLD_DEFAULT;
    public float slowMotionTimeScale = 0.4f;
    public float slowMotionMinTimeScale = GameConstants.SLOMO_MIN_TIME_SCALE;
    public float slowMotionDuration = 0.8f;
    public float slowMotionRampUpTime = 0.05f;
    public float slowMotionRampDownTime = 0.3f;
    public float slowMotionComboScalePerHit = GameConstants.SLOMO_COMBO_SCALE_PER_HIT;
    public float slowMotionComboDurationPerHit = GameConstants.SLOMO_COMBO_DURATION_PER_HIT;

    [Header("Audio")]
    public bool audioEnabled = true;
    public float masterVolume = GameConstants.DEFAULT_MASTER_VOLUME;
    public float sfxVolume = GameConstants.DEFAULT_SFX_VOLUME;
    public float ambientVolume = 0.3f;

    [Header("Haptics")]
    public bool hapticsEnabled = true;

    [Header("Paint Decals")]
    public bool paintDecalsEnabled = true;
    [Range(10, 80)] public int maxActiveDecals = 50;
    public float decalMinSize = 0.15f;
    public float decalMaxSize = 0.4f;
    public float decalAlpha = 0.7f;

    [Header("Persistent Splatters")]
    public bool persistentSplattersEnabled = true;
    [Range(10, 50)] public int maxPersistentSplatters = GameConstants.MAX_PERSISTENT_SPLATTERS;
    public float splatterMinScale = GameConstants.SPLATTER_MIN_SCALE;
    public float splatterMaxScale = GameConstants.SPLATTER_MAX_SCALE;
    public float splatterNeonBoost = GameConstants.SPLATTER_NEON_BOOST;
    public float splatterBaseAlpha = GameConstants.SPLATTER_BASE_ALPHA;
    public float splatterFadeDuration = GameConstants.SPLATTER_FADE_DURATION;

    public static JuiceConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = LoadAsset() ?? CreateInstance<JuiceConfigSO>();
                _instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }

            return _instance;
        }
    }

    private static JuiceConfigSO LoadAsset()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<JuiceConfigSO>(AssetPath);
#else
        return null;
#endif
    }
}
