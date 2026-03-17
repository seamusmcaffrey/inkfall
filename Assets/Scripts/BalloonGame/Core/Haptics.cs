using System.Runtime.InteropServices;

/// <summary>
/// Platform-safe haptics entry point. Uses iOS Taptic Engine via native plugin
/// for differentiated feedback (Light/Medium/Heavy/Selection/Success).
/// </summary>
public static class Haptics
{
    public enum HapticType
    {
        Light,
        Medium,
        Heavy,
        Selection,
        Success,
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void _HapticImpactLight();
    [DllImport("__Internal")] private static extern void _HapticImpactMedium();
    [DllImport("__Internal")] private static extern void _HapticImpactHeavy();
    [DllImport("__Internal")] private static extern void _HapticSelection();
    [DllImport("__Internal")] private static extern void _HapticNotificationSuccess();
#endif

    public static void Play(HapticType type)
    {
        if (!SaveManager.Instance.Data.hapticsEnabled)
        {
            return;
        }

#if UNITY_IOS && !UNITY_EDITOR
        switch (type)
        {
            case HapticType.Light:
                _HapticImpactLight();
                break;
            case HapticType.Medium:
                _HapticImpactMedium();
                break;
            case HapticType.Heavy:
                _HapticImpactHeavy();
                break;
            case HapticType.Selection:
                _HapticSelection();
                break;
            case HapticType.Success:
                _HapticNotificationSuccess();
                break;
        }
#elif UNITY_EDITOR
        UnityEngine.Debug.Log($"[Haptics] {type}");
#endif
    }
}
