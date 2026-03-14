using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Resolves scene-local dependencies without relying on Unity's global Find APIs.
/// </summary>
public static class SceneDependencyResolver
{
    /// <summary>
    /// Searches the active scene roots for the first component of the requested type.
    /// Intended only as a serialization fallback during scene bootstrap.
    /// </summary>
    public static T ResolveInScene<T>(Component context) where T : Component
    {
        if (context == null)
        {
            return null;
        }

        Scene scene = context.gameObject.scene;
        if (!scene.IsValid())
        {
            scene = SceneManager.GetActiveScene();
        }

        if (!scene.IsValid())
        {
            return null;
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T match = root.GetComponentInChildren<T>(true);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
