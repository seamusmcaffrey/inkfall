using UnityEngine;

/// <summary>
/// Builds the noir carnival environment: cork board, metal frame, floor, atmosphere.
/// Cleans up stale root-level scene objects from SceneBuilder on Awake.
/// </summary>
[DisallowMultipleComponent]
public partial class EnvironmentBuilder : MonoBehaviour
{
    private const float FrameThickness = 0.70f;
    private const float FrameDepth = 0.25f;
    private const float BoardZ = GameConstants.BOARD_Z + 0.5f;
    private const float BoardPaddingX = 0.6f;
    private const float BoardPaddingY = 0.8f;
    private const float CorkTextureSize = 256;
    private const float CorkSmoothness = 0.25f;
    private const float CorkMetallic = 0.08f;
    private const int RoomTextureSize = 256;

    private static readonly Color CorkColor = new(0.06f, 0.04f, 0.025f);
    private static readonly Color BoltColor = new(0.55f, 0.48f, 0.38f);
    private static readonly Color WallDarkColor = new(0.05f, 0.04f, 0.04f);
    private static readonly Color CeilingColor = new(0.04f, 0.035f, 0.03f);
    private static readonly Color FloorTint = new(1.2f, 1.1f, 0.95f);
    private static readonly string[] StaleVisualRoots =
    {
        "BackWall", "LaneFloor", "Directional Light", "LaunchOrigin",
        "StripeGlow0", "StripeGlow1", "StripeGlow2", "StripeGlow3",
        "FloorStripe0", "FloorStripe1", "FloorStripe2", "FloorStripe3",
        "LaneLineLeft", "LaneLineRight",
    };
    private static readonly string[] WallNames = { "LeftWall", "RightWall", "TopWall" };

    private void Awake()
    {
        foreach (string objName in StaleVisualRoots)
            DestroyAllRootObjects(objName);
        OverrideStaleRenderers();
        Material dark = CreateMaterial(WallDarkColor, unlit: true);
        foreach (string wallName in WallNames)
        {
            GameObject wall = GameObject.Find(wallName);
            if (wall == null) continue;
            Renderer r = wall.GetComponent<Renderer>();
            if (r != null) { r.sharedMaterial = dark; r.enabled = false; }
            Vector3 pos = wall.transform.position;
            wall.transform.position = new Vector3(pos.x, pos.y, GameConstants.BOARD_Z);
        }
        BuildEnvironment();
    }

    [ContextMenu("Build Environment")]
    public void BuildEnvironment()
    {
        BuildRoom();
        BuildCorkBoard();
        BuildMetalFrame();
        BuildAtmosphere();
    }

    private void BuildCorkBoard()
    {
        float w = GameConstants.BOARD_WIDTH + BoardPaddingX;
        float h = GameConstants.BOARD_HEIGHT + BoardPaddingY;
        float cx = (GameConstants.BOARD_LEFT + GameConstants.BOARD_RIGHT) * 0.5f;
        float cy = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;
        Texture2D corkTex = ProceduralTextures.GenerateCorkTexture((int)CorkTextureSize, (int)CorkTextureSize);
        SetPanel("CorkBoard", PrimitiveType.Quad, new Vector3(cx, cy, BoardZ), new Vector3(w, h, 1f),
            CreateTexturedLitMaterial(corkTex, tiling: 3f));
    }

    private void BuildAtmosphere()
    {
        BuildAtmosphereOverlays();
    }

    private void SetPanel(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
    {
        SetPanel(name, type, pos, Quaternion.identity, scale, mat);
    }

    private void SetPanel(string name, PrimitiveType type, Vector3 pos, Quaternion rotation,
        Vector3 scale, Material mat)
    {
        Transform existing = transform.Find(name);
        GameObject panel = existing != null ? existing.gameObject : GameObject.CreatePrimitive(type);
        panel.name = name;
        panel.transform.SetParent(transform, false);
        panel.transform.localPosition = pos;
        panel.transform.localRotation = rotation;
        panel.transform.localScale = scale;
        panel.layer = GameConstants.LAYER_ENVIRONMENT;
        Renderer r = panel.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;
    }

    private static Material CreateMaterial(Color color, float smoothness = 0f, float metallic = 0f, bool unlit = false)
    {
        Shader shader = unlit
            ? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default")
            : Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
        Material mat = new(shader);
        bool isTransparent = color.a < 0.99f;
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", isTransparent ? 1f : 0f);
            if (isTransparent)
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
        }
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.color = color;
        if (!unlit)
        {
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        }
        return mat;
    }

}
