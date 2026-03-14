using UnityEngine;

/// <summary>
/// Unity-safe helpers for ensuring components exist without relying on fake-null-hostile operators.
/// </summary>
public static class ComponentUtility
{
    /// <summary>
    /// Returns an existing component or adds it if missing.
    /// </summary>
    public static T EnsureComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    /// <summary>
    /// Returns the current reference or assigns the provided fallback when Unity considers it missing.
    /// </summary>
    public static T EnsureReference<T>(ref T reference, T fallback) where T : UnityEngine.Object
    {
        if (reference == null)
        {
            reference = fallback;
        }

        return reference;
    }

    /// <summary>
    /// Resolves a scene-local dependency when the serialized reference is missing.
    /// </summary>
    public static T ResolveSceneReference<T>(Component context, ref T reference) where T : Component
    {
        if (reference == null)
        {
            reference = SceneDependencyResolver.ResolveInScene<T>(context);
        }

        return reference;
    }

    /// <summary>
    /// Returns the existing local component or assigns it from the same GameObject.
    /// </summary>
    public static T ResolveLocalComponent<T>(Component context, ref T reference) where T : Component
    {
        if (reference == null && context != null)
        {
            reference = context.GetComponent<T>();
        }

        return reference;
    }
}
