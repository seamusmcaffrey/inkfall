using UnityEngine;

/// <summary>
/// Creates the neon side-light look for the carnival wall.
/// </summary>
[DisallowMultipleComponent]
public class NeonLightRig : MonoBehaviour
{
    [ContextMenu("Build Neon Rig")]
    public void BuildRig()
    {
        EnsureLight("KeyLight", LightType.Directional, new Vector3(0f, 0f, 0f), Quaternion.Euler(46f, -34f, 0f), Color.white, 0.75f);
        EnsureLight("MagentaNeon", LightType.Point, new Vector3(-3.6f, 4.6f, -1.2f), Quaternion.identity, new Color(1f, 0.16f, 0.64f), 4f);
        EnsureLight("CyanNeon", LightType.Point, new Vector3(3.6f, 4.3f, -1.2f), Quaternion.identity, new Color(0.1f, 0.94f, 0.96f), 4f);
        EnsureLight("GoldNeon", LightType.Point, new Vector3(0f, 8f, -1.2f), Quaternion.identity, new Color(1f, 0.82f, 0.24f), 3.5f);
    }

    private void EnsureLight(string name, LightType type, Vector3 position, Quaternion rotation, Color color, float intensity)
    {
        Transform existing = transform.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
        Light light = go.GetComponent<Light>();
        if (light == null)
        {
            light = go.AddComponent<Light>();
        }

        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.range = 10f;
        light.renderMode = LightRenderMode.Auto;
    }
}
