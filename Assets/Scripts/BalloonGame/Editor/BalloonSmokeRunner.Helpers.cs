#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static partial class BalloonSmokeRunner
{
    private static void ClickButton(string rootObjectName, string buttonName)
    {
        GameObject root = FindNamedObject(rootObjectName);
        if (root == null)
        {
            throw new InvalidOperationException($"Could not find root object '{rootObjectName}'.");
        }

        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            if (button.name == buttonName)
            {
                button.onClick.Invoke();
                return;
            }
        }

        throw new InvalidOperationException($"Could not find button '{buttonName}' under '{rootObjectName}'.");
    }

    private static T RequireComponent<T>(string objectName) where T : Component
    {
        GameObject root = FindNamedObject(objectName);
        if (root == null)
        {
            throw new InvalidOperationException($"Could not find object '{objectName}' in play mode.");
        }

        T component = root.GetComponent<T>();
        if (component == null)
        {
            throw new InvalidOperationException($"Object '{objectName}' is missing component '{typeof(T).Name}'.");
        }

        return component;
    }

    private static GameObject FindNamedObject(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            return null;
        }

        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            GameObject match = FindNamedObjectRecursive(root.transform, objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static GameObject FindNamedObjectRecursive(Transform current, string objectName)
    {
        if (current.name == objectName)
        {
            return current.gameObject;
        }

        for (int childIndex = 0; childIndex < current.childCount; childIndex++)
        {
            GameObject match = FindNamedObjectRecursive(current.GetChild(childIndex), objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
#endif
