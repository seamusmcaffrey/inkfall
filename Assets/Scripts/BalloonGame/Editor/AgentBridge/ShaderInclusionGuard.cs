#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Inkshot.Editor.AgentBridge
{
    /// <summary>
    /// Ensures BalloonLit.shader is in the Always Included Shaders list
    /// to prevent URP variant stripping during automated visual tests.
    /// </summary>
    [InitializeOnLoad]
    public static class ShaderInclusionGuard
    {
        private const string BalloonLitPath = "Assets/Shaders/BalloonLit.shader";

        static ShaderInclusionGuard()
        {
            EnsureShadersIncluded();
        }

        private static void EnsureShadersIncluded()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(BalloonLitPath);
            if (shader == null) return;

            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (assets == null || assets.Length == 0) return;

            var so = new SerializedObject(assets[0]);
            var arrayProp = so.FindProperty("m_AlwaysIncludedShaders");
            if (arrayProp == null || !arrayProp.isArray) return;

            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                var element = arrayProp.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == shader) return;
            }

            int newIndex = arrayProp.arraySize;
            arrayProp.InsertArrayElementAtIndex(newIndex);
            arrayProp.GetArrayElementAtIndex(newIndex).objectReferenceValue = shader;
            so.ApplyModifiedPropertiesWithoutUndo();

            string msg = $"[ShaderInclusionGuard] Added {shader.name} to Always Included Shaders";
            if (Application.isBatchMode) Console.WriteLine(msg);
            else UnityEngine.Debug.Log(msg);
        }
    }
}
#endif
