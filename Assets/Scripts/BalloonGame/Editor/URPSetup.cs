#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Creates and assigns the URP asset used by the project.
/// </summary>
public static class URPSetup
{
    private const string SettingsFolder = "Assets/Settings";
    private const string RendererAssetPath = SettingsFolder + "/URPRenderer.asset";
    private const string PipelineAssetPath = SettingsFolder + "/URPAsset.asset";

    [MenuItem("INKSHOT/Setup URP Pipeline")]
    public static void SetupURP()
    {
        EnsureFolder(SettingsFolder);

        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        rendererData.name = "URPRenderer";
        AssetDatabase.CreateAsset(rendererData, RendererAssetPath);

        var pipelineAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
        pipelineAsset.name = "URPAsset";

        var so = new SerializedObject(pipelineAsset);
        SetInt(so, "m_MainLightRenderingMode", 1);
        SetBool(so, "m_MainLightShadowsSupported", true);
        SetInt(so, "m_MainLightShadowmapResolution", 1024);
        SetInt(so, "m_AdditionalLightsRenderingMode", 0);
        SetFloat(so, "m_ShadowDistance", 15f);
        SetBool(so, "m_SoftShadowsSupported", true);
        SetInt(so, "m_MSAA", 2);
        SetBool(so, "m_UseSRPBatcher", true);
        SetBool(so, "m_SupportsHDR", false);
        SetBool(so, "m_RequireDepthTexture", true);
        SetBool(so, "m_RequireOpaqueTexture", false);
        SetManagedReference(so, "m_RendererDataList.Array.data[0]", rendererData);
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
        AssetDatabase.SaveAssets();

        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        QualitySettings.renderPipeline = pipelineAsset;
        EditorUtility.SetDirty(pipelineAsset);
        AssetDatabase.SaveAssets();

        Debug.Log("INKSHOT URP pipeline configured.");
    }

    private static void SetInt(SerializedObject so, string propertyPath, int value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.intValue = value;
        }
    }

    private static void SetBool(SerializedObject so, string propertyPath, bool value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetFloat(SerializedObject so, string propertyPath, float value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetManagedReference(SerializedObject so, string propertyPath, Object value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string[] segments = path.Split('/');
        string current = segments[0];
        for (int index = 1; index < segments.Length; index++)
        {
            string next = $"{current}/{segments[index]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, segments[index]);
            }

            current = next;
        }
    }
}
#endif
