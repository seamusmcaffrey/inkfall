using UnityEngine;

/// <summary>
/// Renders a procedural emblem on the balloon face based on special type.
/// </summary>
[DisallowMultipleComponent]
public class BalloonEmblem : MonoBehaviour
{
    private const float EmblemScale = 0.55f;
    private const float EmblemZOffset = -0.52f;

    private static Material _emblemMaterial;
    private static Mesh _quadMesh;

    private MeshRenderer _emblemRenderer;
    private MeshFilter _emblemFilter;
    private MaterialPropertyBlock _props;

    public void Configure(BalloonSpecialType specialType, BalloonColor balloonColor)
    {
        if (specialType == BalloonSpecialType.Standard)
        {
            HideEmblem();
            return;
        }

        EnsureEmblem();
        _props ??= new MaterialPropertyBlock();
        _emblemRenderer.GetPropertyBlock(_props);
        _props.SetColor("_EmblemColor", GetEmblemColor(specialType));
        _props.SetFloat("_Shape", GetShapeIndex(specialType));
        _props.SetFloat("_Glow", GetGlowIntensity(specialType));
        _emblemRenderer.SetPropertyBlock(_props);
        _emblemRenderer.gameObject.SetActive(true);
    }

    public void HideEmblem()
    {
        if (_emblemRenderer != null)
        {
            _emblemRenderer.gameObject.SetActive(false);
        }
    }

    private void EnsureEmblem()
    {
        if (_emblemRenderer != null) return;

        EnsureSharedAssets();

        GameObject emblem = new("Emblem");
        emblem.transform.SetParent(transform, false);
        emblem.transform.localPosition = new Vector3(0f, 0.05f, EmblemZOffset);
        emblem.transform.localScale = new Vector3(EmblemScale, EmblemScale, 1f);
        emblem.transform.localRotation = Quaternion.identity;

        _emblemFilter = emblem.AddComponent<MeshFilter>();
        _emblemFilter.sharedMesh = _quadMesh;
        _emblemRenderer = emblem.AddComponent<MeshRenderer>();
        _emblemRenderer.sharedMaterial = _emblemMaterial;
        _emblemRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _emblemRenderer.receiveShadows = false;
    }

    private static void EnsureSharedAssets()
    {
        if (_quadMesh == null)
        {
            _quadMesh = CreateQuadMesh();
        }

        if (_emblemMaterial == null)
        {
            Shader shader = Shader.Find("Inkshot/BalloonEmblem");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            }

            _emblemMaterial = new Material(shader);
        }
    }

    private static Mesh CreateQuadMesh()
    {
        Mesh mesh = new() { name = "EmblemQuad" };
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(1f, 1f), new Vector2(0f, 1f),
        };
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Color GetEmblemColor(BalloonSpecialType type)
    {
        return type switch
        {
            BalloonSpecialType.Gold => new Color(1f, 0.85f, 0.15f, 0.9f),
            BalloonSpecialType.Paint => new Color(0.2f, 0.95f, 0.5f, 0.85f),
            BalloonSpecialType.Hazard => new Color(0.95f, 0.2f, 0.15f, 0.9f),
            BalloonSpecialType.Shield => new Color(0.6f, 0.7f, 0.85f, 0.85f),
            _ => new Color(1f, 1f, 1f, 0.7f),
        };
    }

    private static float GetShapeIndex(BalloonSpecialType type)
    {
        return type switch
        {
            BalloonSpecialType.Gold => 3f,
            BalloonSpecialType.Paint => 1f,
            BalloonSpecialType.Hazard => 2f,
            BalloonSpecialType.Shield => 4f,
            _ => 0f,
        };
    }

    private static float GetGlowIntensity(BalloonSpecialType type)
    {
        return type switch
        {
            BalloonSpecialType.Gold => 0.6f,
            BalloonSpecialType.Hazard => 0.8f,
            _ => 0.3f,
        };
    }
}
