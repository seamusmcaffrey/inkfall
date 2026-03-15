using UnityEngine;

/// <summary>
/// Scene cleanup: destroys stale root objects and overrides non-URP renderers.
/// </summary>
public partial class EnvironmentBuilder
{
    private void OverrideStaleRenderers()
    {
        Material darkUnlit = CreateMaterial(WallDarkColor, unlit: true);
        var allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int overridden = 0;
        foreach (var r in allRenderers)
        {
            if (r == null) continue;
            if (r is SpriteRenderer || r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            Material mat = r.sharedMaterial;
            if (mat == null) { r.enabled = false; overridden++; continue; }
            string shaderName = mat.shader.name;
            bool isStale = shaderName == "Standard"
                || shaderName == "Hidden/InternalErrorShader"
                || (shaderName.Contains("Lit") && !shaderName.Contains("Unlit")
                    && !shaderName.StartsWith("Inkshot/"));
            if (isStale)
            {
                r.sharedMaterial = darkUnlit;
                overridden++;
            }
        }
        // Reduce ambient light to minimize illumination on any remaining Lit surfaces
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.04f, 0.04f);
#if UNITY_EDITOR
        if (overridden > 0) Debug.Log($"[EnvironmentBuilder] Overrode {overridden} stale renderer materials");
#endif
    }

    private static void DestroyAllRootObjects(string objectName)
    {
        var allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        int destroyed = 0;
        foreach (var t in allTransforms)
        {
            if (t != null && t.name == objectName && t.parent == null)
            {
                DestroyImmediate(t.gameObject);
                destroyed++;
            }
        }
#if UNITY_EDITOR
        if (destroyed > 0) Debug.Log($"[EnvironmentBuilder] Destroyed {destroyed} root '{objectName}' objects");
#endif
    }
}
