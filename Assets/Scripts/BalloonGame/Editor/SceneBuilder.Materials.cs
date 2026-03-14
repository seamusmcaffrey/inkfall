#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class BalloonSceneBuilder
{
    private static void CreateBalloonMaterials()
    {
        CreateOrUpdateMaterial(BalloonRedMaterialPath, BalloonColor.Red.ToUnityColor(), 0.7f);
        CreateOrUpdateMaterial(BalloonBlueMaterialPath, BalloonColor.Blue.ToUnityColor(), 0.7f);
        CreateOrUpdateMaterial(BalloonYellowMaterialPath, BalloonColor.Yellow.ToUnityColor(), 0.7f);
        CreateOrUpdateMaterial(BalloonGreenMaterialPath, BalloonColor.Green.ToUnityColor(), 0.7f);
        CreateOrUpdateMaterial(BalloonPurpleMaterialPath, BalloonColor.Purple.ToUnityColor(), 0.7f);
    }

    private static Material CreateOrUpdateMaterial(string assetPath, Color color, float smoothness)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (material == null)
        {
            material = new Material(GetSurfaceShader());
            AssetDatabase.CreateAsset(material, assetPath);
        }

        material.color = color;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", smoothness);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static PhysicsMaterial CreateOrUpdatePhysicsMaterial(
        string assetPath,
        float bounciness,
        float dynamicFriction,
        float staticFriction,
        PhysicsMaterialCombine bounceCombine)
    {
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(assetPath);
        if (material == null)
        {
            material = new PhysicsMaterial();
            AssetDatabase.CreateAsset(material, assetPath);
        }

        material.bounciness = bounciness;
        material.dynamicFriction = dynamicFriction;
        material.staticFriction = staticFriction;
        material.bounceCombine = bounceCombine;

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Shader GetSurfaceShader()
    {
        return Shader.Find("Inkshot/BalloonLit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
    }

    private static void EnsureRenderPipeline()
    {
        if (GraphicsSettings.defaultRenderPipeline != null)
        {
            return;
        }

        const string folder = "Assets/Settings/Rendering";
        EnsureFolder("Assets/Settings");
        EnsureFolder(folder);

        string rendererPath = folder + "/InkshotRenderer.asset";
        string pipelinePath = folder + "/InkshotPipeline.asset";

        var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
        if (rendererData == null)
        {
            rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, rendererPath);
        }

        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
        if (pipeline == null)
        {
            pipeline = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            AssetDatabase.CreateAsset(pipeline, pipelinePath);

            var so = new SerializedObject(pipeline);
            var rendererList = so.FindProperty("m_RendererDataList");
            if (rendererList != null)
            {
                rendererList.arraySize = 1;
                rendererList.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
            }

            var defaultIndex = so.FindProperty("m_DefaultRendererIndex");
            if (defaultIndex != null)
            {
                defaultIndex.intValue = 0;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
        }

        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
        AssetDatabase.SaveAssets();
        Debug.Log($"URP pipeline created and assigned: {pipelinePath}");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string[] parts = path.Split('/');
        string current = parts[0];

        for (int index = 1; index < parts.Length; index++)
        {
            string next = current + "/" + parts[index];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[index]);
            }

            current = next;
        }
    }
}
#endif
