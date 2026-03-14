using UnityEngine;

/// <summary>
/// Creates the dramatic neon lighting rig for the noir carnival aesthetic.
/// </summary>
[DisallowMultipleComponent]
public class NeonLightRig : MonoBehaviour
{
    private const float NeonRange = 12f;
    private const float RimRange = 8f;

    [ContextMenu("Build Neon Rig")]
    public void BuildRig()
    {
        EnsureLight("KeyLight", LightType.Directional, Vector3.zero, Quaternion.Euler(42f, -30f, 0f), new Color(0.9f, 0.88f, 0.82f), 0.65f, 0f);
        EnsureLight("FillLight", LightType.Directional, Vector3.zero, Quaternion.Euler(20f, 160f, 0f), new Color(0.3f, 0.35f, 0.5f), 0.25f, 0f);
        EnsureLight("MagentaNeon", LightType.Point, new Vector3(-4.2f, 5f, -1.5f), Quaternion.identity, new Color(1f, 0.12f, 0.58f), 5f, NeonRange);
        EnsureLight("CyanNeon", LightType.Point, new Vector3(4.2f, 4.5f, -1.5f), Quaternion.identity, new Color(0.08f, 0.92f, 0.95f), 5f, NeonRange);
        EnsureLight("GoldTop", LightType.Point, new Vector3(0f, 9f, -1f), Quaternion.identity, new Color(1f, 0.82f, 0.2f), 3.5f, RimRange);
        EnsureLight("GreenRim", LightType.Point, new Vector3(0f, -2f, -2f), Quaternion.identity, new Color(0.15f, 1f, 0.4f), 2.5f, RimRange);
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
