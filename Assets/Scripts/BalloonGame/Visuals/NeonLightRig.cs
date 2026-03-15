using UnityEngine;

/// <summary>
/// Simple even lighting rig — neutral, well-lit room feel.
/// </summary>
[DisallowMultipleComponent]
public class NeonLightRig : MonoBehaviour
{
    private const float MainIntensity = 1.0f;
    private const float FillIntensity = 0.6f;
    private const float BackIntensity = 0.3f;

    private static readonly Color NeutralWarm = new(1f, 0.96f, 0.92f);
    private static readonly Color NeutralCool = new(0.90f, 0.93f, 1f);

    private void Awake() { BuildRig(); }

    [ContextMenu("Build Neon Rig")]
    public void BuildRig()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        EnsureLight("MainLight", LightType.Directional, Vector3.zero,
            Quaternion.Euler(50f, -30f, 0f), NeutralWarm, MainIntensity);
        EnsureLight("FillLight", LightType.Directional, Vector3.zero,
            Quaternion.Euler(30f, 150f, 0f), NeutralCool, FillIntensity);
        EnsureLight("BackLight", LightType.Directional, Vector3.zero,
            Quaternion.Euler(-20f, 180f, 0f), NeutralWarm, BackIntensity);
    }

    private void EnsureLight(string lightName, LightType type, Vector3 pos, Quaternion rot,
        Color color, float intensity)
    {
        Transform existing = transform.Find(lightName);
        GameObject go = existing != null ? existing.gameObject : new GameObject(lightName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = rot;
        Light light = go.GetComponent<Light>();
        if (light == null) light = go.AddComponent<Light>();
        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.renderMode = LightRenderMode.Auto;
        light.shadows = LightShadows.Soft;
    }
}
