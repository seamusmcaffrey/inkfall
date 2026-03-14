using UnityEngine;

/// <summary>
/// Builds the noir carnival environment: cork board, metal frame, floor, atmosphere.
/// </summary>
[DisallowMultipleComponent]
public class EnvironmentBuilder : MonoBehaviour
{
    private const float FrameThickness = 0.35f;
    private const float FrameDepth = 0.15f;
    private const float BoardZ = 0.5f;

    private static readonly Color CorkColor = new(0.22f, 0.16f, 0.10f);
    private static readonly Color FrameColor = new(0.28f, 0.26f, 0.24f);
    private static readonly Color FloorColor = new(0.08f, 0.07f, 0.06f);
    private static readonly Color FogColor = new(0.04f, 0.04f, 0.06f, 0.7f);
    private static readonly Color AccentCyan = new(0.12f, 0.7f, 0.76f);

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
        EnsurePanel("CorkBoard", PrimitiveType.Quad,
            new Vector3(cx, cy, BoardZ), new Vector3(w, h, 1f),
            CorkColor, 0.08f, 0f);
    }

    private void BuildMetalFrame()
    {
        float bw = GameConstants.BOARD_WIDTH + 0.6f;
        float bh = GameConstants.BOARD_HEIGHT + 0.8f;
        float cx = (GameConstants.BOARD_LEFT + GameConstants.BOARD_RIGHT) * 0.5f;
        float cy = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;
        float halfW = bw * 0.5f;
        float halfH = bh * 0.5f;

        EnsurePanel("FrameTop", PrimitiveType.Cube,
            new Vector3(cx, cy + halfH + FrameThickness * 0.5f, BoardZ - FrameDepth),
            new Vector3(bw + FrameThickness * 2f, FrameThickness, FrameDepth * 2f),
            FrameColor, 0.55f, 0.7f);
        EnsurePanel("FrameBottom", PrimitiveType.Cube,
            new Vector3(cx, cy - halfH - FrameThickness * 0.5f, BoardZ - FrameDepth),
            new Vector3(bw + FrameThickness * 2f, FrameThickness, FrameDepth * 2f),
            FrameColor, 0.55f, 0.7f);
        EnsurePanel("FrameLeft", PrimitiveType.Cube,
            new Vector3(cx - halfW - FrameThickness * 0.5f, cy, BoardZ - FrameDepth),
            new Vector3(FrameThickness, bh, FrameDepth * 2f),
            FrameColor, 0.55f, 0.7f);
        EnsurePanel("FrameRight", PrimitiveType.Cube,
            new Vector3(cx + halfW + FrameThickness * 0.5f, cy, BoardZ - FrameDepth),
            new Vector3(FrameThickness, bh, FrameDepth * 2f),
            FrameColor, 0.55f, 0.7f);

        EnsurePanel("FrameCornerTL", PrimitiveType.Cube,
            new Vector3(cx - halfW - FrameThickness * 0.3f, cy + halfH + FrameThickness * 0.3f, BoardZ - FrameDepth * 1.2f),
            new Vector3(FrameThickness * 0.6f, FrameThickness * 0.6f, FrameDepth * 0.8f),
            new Color(0.35f, 0.32f, 0.28f), 0.6f, 0.8f);
        EnsurePanel("FrameCornerTR", PrimitiveType.Cube,
            new Vector3(cx + halfW + FrameThickness * 0.3f, cy + halfH + FrameThickness * 0.3f, BoardZ - FrameDepth * 1.2f),
            new Vector3(FrameThickness * 0.6f, FrameThickness * 0.6f, FrameDepth * 0.8f),
            new Color(0.35f, 0.32f, 0.28f), 0.6f, 0.8f);
    }

    private void BuildFloorArea()
    {
        EnsurePanel("BackWall", PrimitiveType.Quad,
            new Vector3(0f, 0f, 1.5f), new Vector3(12f, 22f, 1f),
            new Color(0.03f, 0.03f, 0.04f), 0.05f, 0f);

        float floorY = GameConstants.LANE_TOP + (GameConstants.LANE_BOTTOM - GameConstants.LANE_TOP) * 0.5f;
        float floorH = Mathf.Abs(GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM);
        EnsurePanel("LaneFloor", PrimitiveType.Quad,
            new Vector3(0f, floorY, BoardZ + 0.1f), new Vector3(9f, floorH, 1f),
            FloorColor, 0.15f, 0f);

        BuildFloorStripes(floorY, floorH);
    }

    private void BuildFloorStripes(float floorY, float floorH)
    {
        Color[] stripeColors =
        {
            new(0.45f, 0.12f, 0.10f), new(0.12f, 0.35f, 0.18f),
            new(0.15f, 0.18f, 0.40f), new(0.40f, 0.30f, 0.10f),
            new(0.30f, 0.10f, 0.12f), new(0.10f, 0.28f, 0.32f),
        };
        float stripeWidth = 8.4f / stripeColors.Length;
        float startX = -8.4f * 0.5f + stripeWidth * 0.5f;
        for (int i = 0; i < stripeColors.Length; i++)
        {
            EnsurePanel($"FloorStripe{i}", PrimitiveType.Quad,
                new Vector3(startX + i * stripeWidth, floorY, BoardZ + 0.08f),
                new Vector3(stripeWidth - 0.04f, floorH - 0.1f, 1f),
                stripeColors[i], 0.22f, 0f);
        }
    }

    private void BuildAtmosphere()
    {
        EnsurePanel("FogLayerTop", PrimitiveType.Quad,
            new Vector3(0f, GameConstants.BOARD_TOP + 1.5f, BoardZ - 0.3f),
            new Vector3(10f, 3f, 1f), FogColor, 0f, 0f);
        EnsurePanel("FogLayerBottom", PrimitiveType.Quad,
            new Vector3(0f, GameConstants.BOARD_BOTTOM - 1f, BoardZ - 0.3f),
            new Vector3(10f, 2.5f, 1f), FogColor, 0f, 0f);
    }

    private void BuildLaneGuides()
    {
        EnsurePanel("LaneLineLeft", PrimitiveType.Quad,
            new Vector3(-1.7f, -5.4f, BoardZ - 0.05f),
            new Vector3(0.06f, 6.2f, 1f), AccentCyan, 0f, 0f);
        EnsurePanel("LaneLineRight", PrimitiveType.Quad,
            new Vector3(1.7f, -5.4f, BoardZ - 0.05f),
            new Vector3(0.06f, 6.2f, 1f), AccentCyan, 0f, 0f);
    }

    private void EnsurePanel(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color, float smoothness, float metallic)
    {
        Transform existing = transform.Find(name);
        GameObject panel = existing != null ? existing.gameObject : GameObject.CreatePrimitive(type);
        panel.name = name;
        panel.transform.SetParent(transform, false);
        panel.transform.localPosition = pos;
        panel.transform.localScale = scale;
        panel.layer = GameConstants.LAYER_ENVIRONMENT;
        Renderer r = panel.GetComponent<Renderer>();
        if (r != null)
        {
            r.sharedMaterial = CreateMaterial(color, smoothness, metallic);
        }
    }

    private static Material CreateMaterial(Color color, float smoothness, float metallic)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material mat = new(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.color = color;
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        return mat;
    }
}
