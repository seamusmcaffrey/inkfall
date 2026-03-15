using UnityEngine;

/// <summary>
/// Material creation helpers for textured environment surfaces.
/// </summary>
public partial class EnvironmentBuilder
{
    private const float CorkEmissionMultiplier = 0.25f;

    private static Material CreateTexturedMaterial(Texture2D texture, Color tint,
        float smoothness = 0f, float metallic = 0f, bool unlit = false, float tiling = 1f)
    {
        Shader shader = unlit
            ? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Sprites/Default")
            : Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
        Material mat = new(shader);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
        if (mat.HasProperty("_Color")) mat.color = tint;
        ApplyTiling(mat, tiling);
        if (!unlit)
        {
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        }
        return mat;
    }

    private static Material CreateTexturedLitMaterial(Texture2D texture, float tiling = 1f)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
        Material mat = new(shader);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", CorkSmoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", CorkMetallic);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", CorkColor * CorkEmissionMultiplier);
            mat.EnableKeyword("_EMISSION");
        }
        ApplyTiling(mat, tiling);
        return mat;
    }

    private static void ApplyTiling(Material mat, float tiling)
    {
        if (tiling <= 1f) return;
        Vector2 tile = new(tiling, tiling);
        if (mat.HasProperty("_BaseMap")) mat.SetTextureScale("_BaseMap", tile);
        if (mat.HasProperty("_MainTex")) mat.SetTextureScale("_MainTex", tile);
    }
}
