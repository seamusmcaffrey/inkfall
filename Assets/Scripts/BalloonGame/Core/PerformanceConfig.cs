using UnityEngine;

/// <summary>
/// Detects device capability on startup and exposes a static quality tier.
/// </summary>
public static class QualityTier
{
    public enum Tier
    {
        Low,
        High,
    }

    private static Tier _current;
    private static bool _initialized;
    private static bool _forced;

    public static Tier Current
    {
        get
        {
            if (!_initialized)
            {
                Detect();
            }

            return _current;
        }
    }

    public static bool IsHigh => Current == Tier.High;
    public static bool IsLow => Current == Tier.Low;

    public static int ScaleParticleCount(int fullCount)
    {
        return IsHigh ? fullCount : Mathf.Max(1, fullCount / 2);
    }

    public static void ForceSet(Tier tier)
    {
        _current = tier;
        _initialized = true;
        _forced = true;
    }

    public static void ResetToAuto()
    {
        _forced = false;
        _initialized = false;
    }

    private static void Detect()
    {
        _initialized = true;
        if (_forced)
        {
            return;
        }

        bool isHigh = SystemInfo.graphicsMemorySize >= 4096 && SystemInfo.processorCount >= 6;
        _current = isHigh ? Tier.High : Tier.Low;
    }
}

/// <summary>
/// Applies global device-tier settings.
/// </summary>
[DisallowMultipleComponent]
public class PerformanceConfig : MonoBehaviour
{
    [SerializeField] private bool _forceOverride = false;
    [SerializeField] private QualityTier.Tier _forcedTier = QualityTier.Tier.High;

    private void Awake()
    {
        if (_forceOverride)
        {
            QualityTier.ForceSet(_forcedTier);
        }

        _ = QualityTier.Current;
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = QualityTier.IsLow ? GameConstants.DEFAULT_FIXED_TIMESTEP_LOW : GameConstants.DEFAULT_FIXED_TIMESTEP_HIGH;
    }
}
