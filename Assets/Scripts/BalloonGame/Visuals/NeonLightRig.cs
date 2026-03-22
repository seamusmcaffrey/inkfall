using UnityEngine;

/// <summary>
/// Booth lighting rig — warm overhead spotlight on the board,
/// directional fill, and ambient point lights for the room.
/// </summary>
[DisallowMultipleComponent]
public class NeonLightRig : MonoBehaviour
{
    private const float MainIntensity = 1.5f;
    private const float FillIntensity = 0.5f;
    private const float BackIntensity = 0.25f;
    private const float BoothSpotIntensity = 3.5f;
    private const float BoothSpotRange = 18f;
    private const float BoothSpotAngle = 75f;
    private const float AmbientPointIntensity = 1.0f;
    private const float AmbientPointRange = 16f;
    private const float NeonPointIntensity = 4.5f;
    private const float NeonPointRange = 16f;
    private const float NeonLowerIntensityRatio = 0.6f;
    private const float FrontFillIntensity = 1.2f;
    private const float FloorSpotIntensity = 1.5f;

    private static readonly Color WarmWhite = new(1f, 0.92f, 0.82f);
    private static readonly Color NeutralCool = new(0.90f, 0.93f, 1f);
    private static readonly Color BoothWarm = new(1f, 0.88f, 0.72f);
    private static readonly Color AmbientWarm = new(0.95f, 0.82f, 0.65f);
    private static readonly Color NeonGreen = new(0.1f, 1f, 0.3f);
    private static readonly Color NeonMagenta = new(1f, 0.1f, 0.55f);
    private static readonly Color NeonCyan = new(0f, 0.9f, 1f);

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

        // Front fill directional — illuminates balloon front faces from camera direction
        EnsureLight("FrontFill", LightType.Directional, Vector3.zero,
            Quaternion.Euler(5f, 0f, 0f), WarmWhite, FrontFillIntensity);

        // Ambient point light near room entrance for depth
        Vector3 ambientPos = new(0f, boardCenterY, GameConstants.ROOM_FRONT_Z + 1f);
        EnsurePointLight("AmbientPoint", ambientPos, AmbientWarm,
            AmbientPointIntensity, AmbientPointRange);

        // Neon accent lights — green, magenta, cyan glow on the board edges
        float frameLeft = GameConstants.BOARD_LEFT - 1.2f;
        float frameRight = GameConstants.BOARD_RIGHT + 1.2f;
        float frameMidY = boardCenterY;
        float neonZ = GameConstants.BOARD_Z - 1.5f;

        EnsurePointLight("NeonGreenLeft", new Vector3(frameLeft, frameMidY + 2f, neonZ),
            NeonGreen, NeonPointIntensity, NeonPointRange);
        EnsurePointLight("NeonMagentaRight", new Vector3(frameRight, frameMidY + 2f, neonZ),
            NeonMagenta, NeonPointIntensity, NeonPointRange);
        EnsurePointLight("NeonCyanTop", new Vector3(0f, GameConstants.BOARD_TOP + 1.5f, neonZ),
            NeonCyan, NeonPointIntensity * 0.7f, NeonPointRange * 0.8f);

        // Lower neon accents for fuller coverage
        EnsurePointLight("NeonGreenLowerLeft", new Vector3(frameLeft, frameMidY - 2f, neonZ),
            NeonGreen, NeonPointIntensity * NeonLowerIntensityRatio, NeonPointRange);
        EnsurePointLight("NeonMagentaLowerRight", new Vector3(frameRight, frameMidY - 2f, neonZ),
            NeonMagenta, NeonPointIntensity * NeonLowerIntensityRatio, NeonPointRange);

        // Floor illumination — warm spotlight on the bowling lane
        Vector3 floorSpotPos = new(0f, boardCenterY - 2f, GameConstants.ROOM_FRONT_Z + 2f);
        EnsureSpotLight("FloorSpot", floorSpotPos,
            Quaternion.Euler(60f, 0f, 0f), BoothWarm, FloorSpotIntensity, 14f, 65f);
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
