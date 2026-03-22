#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Sets the default Device Simulator device to iPhone 13 Pro Max on first load.
/// </summary>
[InitializeOnLoad]
public static class SimulatorDefaults
{
    private const string PrefsKey = "Inkshot_SimulatorDefaultSet";
    private const string DeviceName = "Apple iPhone 13 Pro Max";

    static SimulatorDefaults()
    {
        if (EditorPrefs.GetBool(PrefsKey, false))
            return;

        EditorApplication.delayCall += SetDefaultDevice;
    }

    [MenuItem("INKSHOT/Reset Simulator to iPhone 13 Pro Max")]
    public static void SetDefaultDevice()
    {
        EditorPrefs.SetString("DeviceSimulator.SelectedDeviceName", DeviceName);
        EditorPrefs.SetBool(PrefsKey, true);
    }
}
#endif
