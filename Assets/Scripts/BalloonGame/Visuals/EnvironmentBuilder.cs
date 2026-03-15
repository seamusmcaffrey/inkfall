using UnityEngine;

/// <summary>
/// Builds the noir carnival environment: cork board, metal frame, floor, atmosphere.
/// Cleans up stale root-level scene objects from SceneBuilder on Awake.
/// </summary>
[DisallowMultipleComponent]
public partial class EnvironmentBuilder : MonoBehaviour
{
    private const float FrameThickness = 0.60f;
    private const float FrameDepth = 0.22f;
    private const float BoardZ = 0.5f;
    private const float BoardPaddingX = 0.6f;
    private const float BoardPaddingY = 0.8f;
    private const float CorkTextureSize = 256;
    private const float FloorTextureSize = 256;
    private const float CorkSmoothness = 0.25f;
    private const float CorkMetallic = 0.08f;
    private const float StripeHeight = 0.18f;
    private const float StripeWidth = 7f;
    private const float BackWallZ = 1.5f;
    private const float BackWallExtraWidth = 16f;
    private const float BackWallExtraHeight = 4f;

    private static readonly Color CorkColor = new(0.06f, 0.04f, 0.025f);
    private static readonly Color FloorColor = new(0.03f, 0.025f, 0.02f);
    private static readonly Color BoltColor = new(0.55f, 0.48f, 0.38f);
    private static readonly Color AccentCyan = new(0.06f, 0.30f, 0.32f);
    private static readonly Color WallDarkColor = new(0.05f, 0.04f, 0.04f);
    private static readonly Color BackWallColor = new(0.008f, 0.008f, 0.012f);
    private static readonly string[] StaleVisualRoots = { "BackWall", "LaneFloor", "Directional Light", "LaunchOrigin" };
    private static readonly string[] WallNames = { "LeftWall", "RightWall", "TopWall" };

    private static readonly Color[] StripeColors =
    {
        new(0.45f, 0.06f, 0.10f), new(0.06f, 0.38f, 0.15f),
        new(0.12f, 0.10f, 0.45f), new(0.45f, 0.30f, 0.04f),
    };

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
        }
        BuildEnvironment();
    }

    [ContextMenu("Build Environment")]
    public void BuildEnvironment()
    {
        BuildCorkBoard();
        BuildMetalFrame();
        BuildFloorArea();
        BuildAtmosphere();
        BuildLaneGuides();
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

    private void BuildFloorArea()
    {
        float bwH = GameConstants.MAX_ORTHO_SIZE * 2f + BackWallExtraHeight;
        SetPanel("BackWall", PrimitiveType.Quad,
            new Vector3(0f, GameConstants.CAMERA_Y_CENTER, BackWallZ),
            new Vector3(GameConstants.TARGET_WORLD_WIDTH + BackWallExtraWidth, bwH, 1f),
            CreateMaterial(BackWallColor, unlit: true));

        Texture2D floorTex = ProceduralTextures.GenerateFloorTexture((int)FloorTextureSize, (int)FloorTextureSize);
        float floorBottom = GameConstants.CAMERA_Y_CENTER - GameConstants.MAX_ORTHO_SIZE - 1f;
        float floorY = (GameConstants.LANE_TOP + floorBottom) * 0.5f;
        float floorH = GameConstants.LANE_TOP - floorBottom;
        SetPanel("LaneFloor", PrimitiveType.Quad,
            new Vector3(0f, floorY, BoardZ + 0.1f),
            new Vector3(GameConstants.TARGET_WORLD_WIDTH + 1f, floorH, 1f),
            CreateTexturedMaterial(floorTex, Color.white, unlit: true, tiling: 2f));

        float stripeSpacing = (GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM) / (StripeColors.Length + 1);
        float startY = (GameConstants.LANE_TOP + GameConstants.LANE_BOTTOM) * 0.5f
            + (GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM) * 0.35f;
        for (int i = 0; i < StripeColors.Length; i++)
        {
            float sy = startY - i * stripeSpacing;
            Color glowColor = new(StripeColors[i].r, StripeColors[i].g, StripeColors[i].b, 0.35f);
            SetPanel($"StripeGlow{i}", PrimitiveType.Quad,
                new Vector3(0f, sy, BoardZ + 0.09f),
                new Vector3(StripeWidth + 0.5f, StripeHeight * 2.5f, 1f), CreateMaterial(glowColor, unlit: true));
            SetPanel($"FloorStripe{i}", PrimitiveType.Quad,
                new Vector3(0f, sy, BoardZ + 0.08f),
                new Vector3(StripeWidth, StripeHeight, 1f), CreateMaterial(StripeColors[i], unlit: true));
        }
    }

    private void BuildAtmosphere()
    {
        BuildAtmosphereOverlays();
    }

    private void BuildLaneGuides()
    {
        float laneMidY = (GameConstants.LANE_TOP + GameConstants.LANE_BOTTOM) * 0.5f;
        float laneH = GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM;
        Material cyan = CreateMaterial(AccentCyan, unlit: true);
        SetPanel("LaneLineLeft", PrimitiveType.Quad,
            new Vector3(-1.7f, laneMidY, BoardZ - 0.05f), new Vector3(0.10f, laneH, 1f), cyan);
        SetPanel("LaneLineRight", PrimitiveType.Quad,
            new Vector3(1.7f, laneMidY, BoardZ - 0.05f), new Vector3(0.10f, laneH, 1f), cyan);
    }

    private void SetPanel(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
    {
        Transform existing = transform.Find(name);
        GameObject panel = existing != null ? existing.gameObject : GameObject.CreatePrimitive(type);
        panel.name = name;
        panel.transform.SetParent(transform, false);
        panel.transform.localPosition = pos;
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
