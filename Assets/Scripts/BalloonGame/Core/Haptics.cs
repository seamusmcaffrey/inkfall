using UnityEngine;

/// <summary>
/// Platform-safe haptics entry point.
/// </summary>
public static class Haptics
{
    public enum HapticType
    {
        Light,
        Medium,
        Heavy,
        Success,
    }

    public static void Play(HapticType type)
    {
        if (!SaveManager.Instance.Data.hapticsEnabled)
        {
            return;
        }

#if UNITY_IOS || UNITY_ANDROID
        switch (type)
        {
            case HapticType.Light:
                Handheld.Vibrate();
                break;

            case HapticType.Medium:
                Handheld.Vibrate();
                break;

            case HapticType.Heavy:
                Handheld.Vibrate();
                break;

            case HapticType.Success:
                Handheld.Vibrate();
                break;
        }
#else
        Debug.Log($"[Haptics] {type}");
#endif
    }
}
