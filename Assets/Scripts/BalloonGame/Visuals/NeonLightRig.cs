using UnityEngine;

/// <summary>
/// Booth lighting rig — warm overhead spotlight on the board,
/// directional fill, and ambient point lights for the room.
/// </summary>
[DisallowMultipleComponent]
public class NeonLightRig : MonoBehaviour
{
    private const float MainIntensity = 1.2f;
    private const float FillIntensity = 0.5f;
    private const float BackIntensity = 0.25f;
    private const float BoothSpotIntensity = 2.5f;
    private const float BoothSpotRange = 18f;
    private const float BoothSpotAngle = 75f;
    private const float AmbientPointIntensity = 0.6f;
    private const float AmbientPointRange = 12f;

    private static readonly Color WarmWhite = new(1f, 0.92f, 0.82f);
    private static readonly Color NeutralCool = new(0.90f, 0.93f, 1f);
    private static readonly Color BoothWarm = new(1f, 0.88f, 0.72f);
    private static readonly Color AmbientWarm = new(0.95f, 0.82f, 0.65f);

    private void Awake() { BuildRig(); }

    [ContextMenu("Build Neon Rig")]
    public void BuildRig()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Main directional — warm key light from above-left
        EnsureLight("MainLight", LightType.Directional, Vector3.zero,
            Quaternion.Euler(50f, -30f, 0f), WarmWhite, MainIntensity);

        // Fill directional — cool fill from opposite side
        EnsureLight("FillLight", LightType.Directional, Vector3.zero,
            Quaternion.Euler(30f, 150f, 0f), NeutralCool, FillIntensity);

        // Back directional — subtle rim/separation
        EnsureLight("BackLight", LightType.Directional, Vector3.zero,
            Quaternion.Euler(-20f, 180f, 0f), WarmWhite, BackIntensity);

        // Booth overhead spotlight — warm light shining down on the board
        float boardCenterY = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;
        Vector3 spotPos = new(0f, GameConstants.ROOM_CEILING_Y - 0.5f, GameConstants.BOARD_Z - 1f);
        EnsureSpotLight("BoothSpot", spotPos,
            Quaternion.Euler(70f, 0f, 0f), BoothWarm, BoothSpotIntensity,
            BoothSpotRange, BoothSpotAngle);

        // Ambient point light near room entrance for depth
        Vector3 ambientPos = new(0f, boardCenterY, GameConstants.ROOM_FRONT_Z + 1f);
        EnsurePointLight("AmbientPoint", ambientPos, AmbientWarm,
            AmbientPointIntensity, AmbientPointRange);
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

    private void EnsureSpotLight(string lightName, Vector3 pos, Quaternion rot,
        Color color, float intensity, float range, float spotAngle)
    {
        Transform existing = transform.Find(lightName);
        GameObject go = existing != null ? existing.gameObject : new GameObject(lightName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = rot;
        Light light = go.GetComponent<Light>();
        if (light == null) light = go.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = spotAngle;
        light.renderMode = LightRenderMode.ForcePixel;
        light.shadows = LightShadows.Soft;
    }

    private void EnsurePointLight(string lightName, Vector3 pos,
        Color color, float intensity, float range)
    {
        Transform existing = transform.Find(lightName);
        GameObject go = existing != null ? existing.gameObject : new GameObject(lightName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.identity;
        Light light = go.GetComponent<Light>();
        if (light == null) light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.renderMode = LightRenderMode.Auto;
        light.shadows = LightShadows.None;
    }
}
