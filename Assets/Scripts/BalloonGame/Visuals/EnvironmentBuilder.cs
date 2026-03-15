using UnityEngine;

/// <summary>
/// Builds the noir carnival environment: cork board, metal frame, floor, atmosphere.
/// Cleans up stale root-level scene objects from SceneBuilder on Awake.
/// </summary>
[DisallowMultipleComponent]
public class EnvironmentBuilder : MonoBehaviour
{
    private const float FrameThickness = 0.35f;
    private const float FrameDepth = 0.15f;
    private const float BoardZ = 0.5f;

    private static readonly Color CorkColor = new(0.14f, 0.09f, 0.05f);
    private static readonly Color FrameColor = new(0.22f, 0.20f, 0.18f);
    private static readonly Color FloorColor = new(0.04f, 0.03f, 0.03f);
    private static readonly Color BoltColor = new(0.35f, 0.32f, 0.28f);
    private static readonly Color FogColor = new(0.04f, 0.04f, 0.06f, 0.7f);
    private static readonly Color AccentCyan = new(0.06f, 0.35f, 0.38f);
    private static readonly Color WallDarkColor = new(0.05f, 0.04f, 0.04f);
    private static readonly string[] StaleVisualRoots = { "BackWall", "LaneFloor" };
    private static readonly string[] WallNames = { "LeftWall", "RightWall", "TopWall" };

    private void Awake()
    {
        foreach (string objName in StaleVisualRoots)
        {
            GameObject stale = GameObject.Find(objName);
            if (stale != null && stale.transform.parent == null) Destroy(stale);
        }
        Material dark = CreateMaterial(WallDarkColor, unlit: true);
        foreach (string wallName in WallNames)
        {
            GameObject wall = GameObject.Find(wallName);
            if (wall != null) { Renderer r = wall.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = dark; }
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
        float w = GameConstants.BOARD_WIDTH + 0.6f;
        float h = GameConstants.BOARD_HEIGHT + 0.8f;
        float cx = (GameConstants.BOARD_LEFT + GameConstants.BOARD_RIGHT) * 0.5f;
        float cy = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;
        SetPanel("CorkBoard", PrimitiveType.Quad, new Vector3(cx, cy, BoardZ), new Vector3(w, h, 1f),
            CreateMaterial(CorkColor, 0.04f, 0f));
    }

    private void BuildMetalFrame()
    {
        float bw = GameConstants.BOARD_WIDTH + 0.6f;
        float bh = GameConstants.BOARD_HEIGHT + 0.8f;
        float cx = (GameConstants.BOARD_LEFT + GameConstants.BOARD_RIGHT) * 0.5f;
        float cy = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;
        float halfW = bw * 0.5f;
        float halfH = bh * 0.5f;
        Material frameMat = CreateMaterial(FrameColor, 0.45f, 0.5f);
        Material boltMat = CreateMaterial(BoltColor, 0.6f, 0.7f);

        SetPanel("FrameTop", PrimitiveType.Cube,
            new Vector3(cx, cy + halfH + FrameThickness * 0.5f, BoardZ - FrameDepth),
            new Vector3(bw + FrameThickness * 2f, FrameThickness, FrameDepth * 2f), frameMat);
        SetPanel("FrameBottom", PrimitiveType.Cube,
            new Vector3(cx, cy - halfH - FrameThickness * 0.5f, BoardZ - FrameDepth),
            new Vector3(bw + FrameThickness * 2f, FrameThickness, FrameDepth * 2f), frameMat);
        SetPanel("FrameLeft", PrimitiveType.Cube,
            new Vector3(cx - halfW - FrameThickness * 0.5f, cy, BoardZ - FrameDepth),
            new Vector3(FrameThickness, bh, FrameDepth * 2f), frameMat);
        SetPanel("FrameRight", PrimitiveType.Cube,
            new Vector3(cx + halfW + FrameThickness * 0.5f, cy, BoardZ - FrameDepth),
            new Vector3(FrameThickness, bh, FrameDepth * 2f), frameMat);

        float boltZ = BoardZ - FrameDepth * 1.3f;
        Vector3 bs = new(0.12f, 0.12f, FrameDepth * 0.6f);
        float bx = halfW + FrameThickness * 0.3f, by = halfH + FrameThickness * 0.3f;
        SetPanel("BoltTL", PrimitiveType.Cube, new Vector3(cx - bx, cy + by, boltZ), bs, boltMat);
        SetPanel("BoltTR", PrimitiveType.Cube, new Vector3(cx + bx, cy + by, boltZ), bs, boltMat);
        SetPanel("BoltBL", PrimitiveType.Cube, new Vector3(cx - bx, cy - by, boltZ), bs, boltMat);
        SetPanel("BoltBR", PrimitiveType.Cube, new Vector3(cx + bx, cy - by, boltZ), bs, boltMat);
    }

    private void BuildFloorArea()
    {
        Material unlitFloor = CreateMaterial(FloorColor, unlit: true);
        SetPanel("BackWall", PrimitiveType.Quad, new Vector3(0f, 0f, 1.5f), new Vector3(12f, 22f, 1f),
            CreateMaterial(new Color(0.015f, 0.015f, 0.02f), unlit: true));

        float floorY = (GameConstants.LANE_TOP + GameConstants.LANE_BOTTOM) * 0.5f;
        float floorH = GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM;
        SetPanel("LaneFloor", PrimitiveType.Quad,
            new Vector3(0f, floorY, BoardZ + 0.1f), new Vector3(9f, floorH, 1f), unlitFloor);

        Color[] stripeColors =
        {
            new(0.18f, 0.03f, 0.05f), new(0.03f, 0.15f, 0.08f),
            new(0.06f, 0.05f, 0.18f), new(0.18f, 0.12f, 0.02f),
            new(0.14f, 0.02f, 0.1f), new(0.02f, 0.12f, 0.15f),
        };
        float startY = floorY + floorH * 0.35f;
        for (int i = 0; i < stripeColors.Length; i++)
        {
            SetPanel($"FloorStripe{i}", PrimitiveType.Quad,
                new Vector3(0f, startY - i * 1.2f, BoardZ + 0.08f),
                new Vector3(6f, 0.08f, 1f), CreateMaterial(stripeColors[i], unlit: true));
        }
    }

    private void BuildAtmosphere()
    {
        Material fog = CreateMaterial(FogColor, unlit: true);
        SetPanel("FogLayerTop", PrimitiveType.Quad,
            new Vector3(0f, GameConstants.BOARD_TOP + 1.5f, BoardZ - 0.3f), new Vector3(10f, 3f, 1f), fog);
        SetPanel("FogLayerBottom", PrimitiveType.Quad,
            new Vector3(0f, GameConstants.BOARD_BOTTOM - 1f, BoardZ - 0.3f), new Vector3(10f, 2.5f, 1f), fog);
    }

    private void BuildLaneGuides()
    {
        float laneMidY = (GameConstants.LANE_TOP + GameConstants.LANE_BOTTOM) * 0.5f;
        float laneH = GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM;
        Material cyan = CreateMaterial(AccentCyan, unlit: true);
        SetPanel("LaneLineLeft", PrimitiveType.Quad,
            new Vector3(-1.7f, laneMidY, BoardZ - 0.05f), new Vector3(0.06f, laneH, 1f), cyan);
        SetPanel("LaneLineRight", PrimitiveType.Quad,
            new Vector3(1.7f, laneMidY, BoardZ - 0.05f), new Vector3(0.06f, laneH, 1f), cyan);
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
            ? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color")
            : Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material mat = new(shader);
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
