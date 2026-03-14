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
    [Range(10, 40)] public int popParticleCount = 15;
    public float popParticleLifetime = 0.45f;
    public float popParticleSpeed = 4f;
    public float popParticleGravity = 2f;

    [Header("Particles - Paint Splatter")]
    public bool paintSplatterEnabled = true;
    [Range(20, 60)] public int paintParticleCount = 30;
    public float paintParticleLifetime = 0.9f;
    public float paintParticleSpeed = 6f;
    public float paintParticleGravity = 3f;
    [Range(1, 6)] public int paintDripCount = 3;
    public float paintDripSpeed = 0.8f;
    public float paintDripLifetime = 2f;

    [Header("Particles - Impact")]
    public bool impactSparkEnabled = true;
    [Range(4, 20)] public int sparkParticleCount = 10;
    public float sparkParticleLifetime = 0.15f;
    public float sparkParticleSpeed = 8f;

    [Header("Particles - Wall Hit")]
    public bool wallHitEnabled = true;
    [Range(3, 12)] public int wallHitParticleCount = 6;
    public float wallHitParticleLifetime = 0.28f;
    public float wallHitParticleSpeed = 2f;

    [Header("Dart Trail")]
    public bool dartTrailEnabled = true;
    public float trailWidth = 0.02f;
    public float trailLifetime = 0.3f;
    public Color trailColor = new(0.8f, 0.9f, 1f, 0.6f);

    [Header("Screen Shake")]
    public bool screenShakeEnabled = true;
    public float shakeIntensityPop = 0.1f;
    public float shakeIntensityCombo = 0.25f;
    public float shakeIntensityPaint = 0.38f;
    public float shakeDuration = 0.2f;

    [Header("Chromatic Pulse")]
    public bool chromaticPulseEnabled = true;
    public int chromaticComboThreshold = 3;
    public float chromaticMaxIntensity = 0.5f;
    public float chromaticPulseDuration = 0.3f;

    [Header("Combo Flash")]
    public bool comboFlashEnabled = true;
    public float comboFlashMaxAlpha = 0.15f;
    public float comboFlashDuration = 0.2f;

    [Header("Slow Motion")]
    public bool slowMotionEnabled = true;
    public int slowMotionComboThreshold = 5;
    public float slowMotionTimeScale = 0.4f;
    public float slowMotionDuration = 0.8f;
    public float slowMotionRampUpTime = 0.05f;
    public float slowMotionRampDownTime = 0.3f;

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
