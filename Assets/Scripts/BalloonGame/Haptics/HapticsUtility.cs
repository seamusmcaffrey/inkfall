/// <summary>
/// Convenience wrapper around the core haptics service.
/// </summary>
public static class HapticsUtility
{
    public static void Light() => Haptics.Play(Haptics.HapticType.Light);
    public static void Medium() => Haptics.Play(Haptics.HapticType.Medium);
    public static void Heavy() => Haptics.Play(Haptics.HapticType.Heavy);
    public static void Selection() => Haptics.Play(Haptics.HapticType.Selection);
    public static void Success() => Haptics.Play(Haptics.HapticType.Success);
}
