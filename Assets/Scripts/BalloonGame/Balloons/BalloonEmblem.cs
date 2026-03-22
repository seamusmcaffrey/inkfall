using UnityEngine;

/// <summary>
/// Renders procedural special and sticker emblems on the balloon face.
/// </summary>
[DisallowMultipleComponent]
public class BalloonEmblem : MonoBehaviour
{
    private const float EmblemScale = 0.80f;
    private const float EmblemZOffset = -0.52f;
    private const float StickerDefaultGlow = 0.85f;
    private static readonly Vector3 StickerOffset = new(0.24f, 0.24f, EmblemZOffset - 0.01f);
    private static readonly Vector3 StickerScale = new(0.48f, 0.48f, 1f);

    private static Material _emblemMaterial;
    private static Mesh _quadMesh;

    private MeshRenderer _specialRenderer;
    private MeshRenderer _stickerRenderer;
    private MaterialPropertyBlock _specialProps;
    private MaterialPropertyBlock _stickerProps;

    public void Configure(BalloonSpecialType specialType, BalloonColor balloonColor, StickerFamily stickerFamily)
    {
        EnsureEmblems();
        ConfigureSpecial(specialType);
        ConfigureSticker(stickerFamily);
    }

    public void HideEmblem()
    {
        if (_specialRenderer != null)
        {
            _specialRenderer.gameObject.SetActive(false);
        }

        if (_stickerRenderer != null)
        {
            _stickerRenderer.gameObject.SetActive(false);
        }
    }

    private void ConfigureSpecial(BalloonSpecialType specialType)
    {
        if (specialType == BalloonSpecialType.Standard)
        {
            _specialRenderer.gameObject.SetActive(false);
            return;
        }

        _specialProps ??= new MaterialPropertyBlock();
        _specialRenderer.GetPropertyBlock(_specialProps);
        _specialProps.SetColor("_EmblemColor", GetSpecialColor(specialType));
        _specialProps.SetFloat("_Shape", GetSpecialShapeIndex(specialType));
        _specialProps.SetFloat("_Glow", GetSpecialGlow(specialType));
        _specialRenderer.SetPropertyBlock(_specialProps);
        _specialRenderer.gameObject.SetActive(true);
    }

    private void ConfigureSticker(StickerFamily stickerFamily)
    {
        if (stickerFamily == StickerFamily.None)
        {
            _stickerRenderer.gameObject.SetActive(false);
            return;
        }

        _stickerProps ??= new MaterialPropertyBlock();
        _stickerRenderer.GetPropertyBlock(_stickerProps);
        _stickerProps.SetColor("_EmblemColor", stickerFamily.ToColor());
        _stickerProps.SetFloat("_Shape", GetStickerShapeIndex(stickerFamily));
        _stickerProps.SetFloat("_Glow", StickerDefaultGlow);
        _stickerRenderer.SetPropertyBlock(_stickerProps);
        _stickerRenderer.gameObject.SetActive(true);
    }

    private void EnsureEmblems()
    {
        if (_specialRenderer != null && _stickerRenderer != null)
        {
            return;
        }

        EnsureSharedAssets();
        _specialRenderer = CreateRenderer("SpecialEmblem", new Vector3(0f, 0.05f, EmblemZOffset), new Vector3(EmblemScale, EmblemScale, 1f));
        _stickerRenderer = CreateRenderer("StickerEmblem", StickerOffset, StickerScale);
    }

    private MeshRenderer CreateRenderer(string name, Vector3 localPosition, Vector3 localScale)
    {
        GameObject emblem = new(name);
        emblem.transform.SetParent(transform, false);
        emblem.transform.localPosition = localPosition;
        emblem.transform.localScale = localScale;
        emblem.transform.localRotation = Quaternion.identity;
        emblem.AddComponent<MeshFilter>().sharedMesh = _quadMesh;
        MeshRenderer renderer = emblem.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = _emblemMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return renderer;
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
        var mesh = new Mesh { name = "EmblemQuad" };
        mesh.SetVertices(new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
        });
        mesh.SetUVs(0, new[]
        {
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(1f, 1f), new Vector2(0f, 1f),
        });
        mesh.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Color GetSpecialColor(BalloonSpecialType type)
    {
        return type switch
        {
            BalloonSpecialType.Gold => new Color(1f, 0.95f, 0.4f, 0.95f),
            BalloonSpecialType.Paint => new Color(1f, 1f, 1f, 0.9f),
            BalloonSpecialType.Hazard => new Color(1f, 0.3f, 0.0f, 0.95f),
            BalloonSpecialType.Shield => new Color(0.9f, 0.95f, 1f, 0.92f),
            BalloonSpecialType.Mixer => new Color(0.3f, 0.85f, 1f, 0.95f),
            BalloonSpecialType.Invert => new Color(0.9f, 0.35f, 1f, 0.95f),
            BalloonSpecialType.Wash => new Color(0.3f, 1f, 0.75f, 0.95f),
            BalloonSpecialType.Clone => new Color(1f, 0.45f, 0.45f, 0.95f),
            BalloonSpecialType.Rainbow => new Color(1f, 0.85f, 0.35f, 0.95f),
            _ => new Color(1f, 1f, 1f, 0.7f),
        };
    }

    private static float GetSpecialShapeIndex(BalloonSpecialType type)
    {
        return type switch
        {
            BalloonSpecialType.Gold => 3f,
            BalloonSpecialType.Paint => 1f,
            BalloonSpecialType.Hazard => 2f,
            BalloonSpecialType.Shield => 4f,
            BalloonSpecialType.Mixer => 6f,
            BalloonSpecialType.Invert => 8f,
            BalloonSpecialType.Wash => 7f,
            BalloonSpecialType.Clone => 1f,
            BalloonSpecialType.Rainbow => 8f,
            _ => 0f,
        };
    }

    private static float GetSpecialGlow(BalloonSpecialType type)
    {
        return type switch
        {
            BalloonSpecialType.Gold => 1.1f,
            BalloonSpecialType.Hazard => 1.3f,
            BalloonSpecialType.Paint => 0.8f,
            BalloonSpecialType.Shield => 0.75f,
            BalloonSpecialType.Mixer => 0.95f,
            BalloonSpecialType.Invert => 0.95f,
            BalloonSpecialType.Wash => 0.85f,
            BalloonSpecialType.Clone => 1.0f,
            BalloonSpecialType.Rainbow => 1.15f,
            _ => 0.6f,
        };
    }

    private static float GetStickerShapeIndex(StickerFamily family)
    {
        return family switch
        {
            StickerFamily.Crown => 3f,
            StickerFamily.Skull => 5f,
            StickerFamily.Star => 1f,
            StickerFamily.Bolt => 6f,
            StickerFamily.Clover => 7f,
            StickerFamily.Target => 8f,
            _ => 0f,
        };
    }
}
