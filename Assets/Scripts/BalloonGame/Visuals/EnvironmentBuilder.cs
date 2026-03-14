using UnityEngine;

/// <summary>
/// Programmatically builds the noir carnival environment shell.
/// </summary>
[DisallowMultipleComponent]
public class EnvironmentBuilder : MonoBehaviour
{
    [ContextMenu("Build Environment")]
    public void BuildEnvironment()
    {
        EnsurePanel("BackWallNoir", PrimitiveType.Quad, new Vector3(0f, 3.8f, 1.1f), new Vector3(9.2f, 10.8f, 1f), new Color(0.08f, 0.07f, 0.08f));
        EnsurePanel("LaneFloorWet", PrimitiveType.Quad, new Vector3(0f, -5.4f, 1.2f), new Vector3(8.8f, 6.4f, 1f), new Color(0.06f, 0.06f, 0.07f));
        EnsurePanel("CurtainLeft", PrimitiveType.Cube, new Vector3(-4.7f, 3.5f, 0.2f), new Vector3(0.45f, 11.5f, 0.45f), new Color(0.2f, 0.03f, 0.08f));
        EnsurePanel("CurtainRight", PrimitiveType.Cube, new Vector3(4.7f, 3.5f, 0.2f), new Vector3(0.45f, 11.5f, 0.45f), new Color(0.06f, 0.04f, 0.18f));
        EnsureLaneLine("LaneLineLeft", -1.7f);
        EnsureLaneLine("LaneLineRight", 1.7f);
    }

    private void EnsureLaneLine(string name, float xPosition)
    {
        EnsurePanel(name, PrimitiveType.Quad, new Vector3(xPosition, -5.4f, 1.05f), new Vector3(0.08f, 6.2f, 1f), new Color(0.12f, 0.7f, 0.76f));
    }

    private void EnsurePanel(string name, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Color color)
    {
        Transform existing = transform.Find(name);
        GameObject panel = existing != null ? existing.gameObject : GameObject.CreatePrimitive(primitiveType);
        panel.name = name;
        panel.transform.SetParent(transform, false);
        panel.transform.localPosition = position;
        panel.transform.localScale = scale;
        panel.layer = GameConstants.LAYER_ENVIRONMENT;
        Renderer renderer = panel.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = CreateMaterial(color);
        }
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new(shader);
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.color = color;
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.18f);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }

        return material;
    }
}
