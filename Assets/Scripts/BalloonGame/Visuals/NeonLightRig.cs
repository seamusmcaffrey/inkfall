using UnityEngine;

/// <summary>
/// Creates the dramatic neon lighting rig for the noir carnival aesthetic.
/// </summary>
[DisallowMultipleComponent]
public class NeonLightRig : MonoBehaviour
{
    private const float NeonRange = 9f;
    private const float RimRange = 10f;
    private const float MidRange = 8f;
    private const float AmberRange = 8f;

    private const float KeyIntensity = 0.20f;
    private const float FillIntensity = 0.05f;
    private const float NeonIntensity = 10.0f;
    private const float GoldIntensity = 7.5f;
    private const float GreenRimIntensity = 5.0f;
    private const float AmberIntensity = 5.0f;
    private const float MidIntensity = 7.0f;

    private static readonly Color KeyColor = new(0.95f, 0.88f, 0.72f);
    private static readonly Color FillColor = new(0.35f, 0.40f, 0.55f);
    private static readonly Color MagentaColor = new(1f, 0.12f, 0.58f);
    private static readonly Color CyanColor = new(0.08f, 0.92f, 0.95f);
    private static readonly Color GoldColor = new(1f, 0.82f, 0.2f);
    private static readonly Color GreenColor = new(0.15f, 1f, 0.4f);
    private static readonly Color AmberColor = new(1f, 0.72f, 0.28f);
    private static readonly Color MagentaMidColor = new(0.85f, 0.15f, 0.5f);
    private static readonly Color CyanMidColor = new(0.1f, 0.8f, 0.85f);

    private void Awake() { BuildRig(); }

    [ContextMenu("Build Neon Rig")]
    public void BuildRig()
    {
        EnsureLight("KeyLight", LightType.Directional, Vector3.zero, Quaternion.Euler(55f, -20f, 0f), KeyColor, KeyIntensity, 0f);
        EnsureLight("FillLight", LightType.Directional, Vector3.zero, Quaternion.Euler(20f, 160f, 0f), FillColor, FillIntensity, 0f);
        EnsureLight("MagentaNeon", LightType.Point, new Vector3(-3.8f, 5f, -0.8f), Quaternion.identity, MagentaColor, NeonIntensity, NeonRange);
        EnsureLight("CyanNeon", LightType.Point, new Vector3(3.8f, 4.5f, -0.8f), Quaternion.identity, CyanColor, NeonIntensity, NeonRange);
        EnsureLight("GoldTop", LightType.Point, new Vector3(0f, 9f, -0.5f), Quaternion.identity, GoldColor, GoldIntensity, RimRange);
        EnsureLight("GreenRim", LightType.Point, new Vector3(0f, -1f, -1.0f), Quaternion.identity, GreenColor, GreenRimIntensity, RimRange);
        EnsureLight("AmberBottom", LightType.Point, new Vector3(0f, -2.5f, -0.8f), Quaternion.identity, AmberColor, AmberIntensity, AmberRange);
        EnsureLight("MagentaMid", LightType.Point, new Vector3(-3.2f, 1f, -0.6f), Quaternion.identity, MagentaMidColor, MidIntensity, MidRange);
        EnsureLight("CyanMid", LightType.Point, new Vector3(3.2f, 0.5f, -0.6f), Quaternion.identity, CyanMidColor, MidIntensity, MidRange);
    }

    private void EnsureLight(string name, LightType type, Vector3 pos, Quaternion rot, Color color, float intensity, float range)
    {
        Transform existing = transform.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = rot;
        Light light = go.GetComponent<Light>();
        if (light == null) light = go.AddComponent<Light>();
        light.type = type;
        light.color = color;
        light.intensity = intensity;
        if (type != LightType.Directional) light.range = range;
        light.renderMode = LightRenderMode.Auto;
        light.shadows = type == LightType.Directional ? LightShadows.Soft : LightShadows.None;
    }
}
