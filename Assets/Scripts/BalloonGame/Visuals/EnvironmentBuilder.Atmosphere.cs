using UnityEngine;

/// <summary>
/// Atmosphere overlays — fog, edge glow, and neon haze for noir carnival.
/// </summary>
public partial class EnvironmentBuilder
{
    private const float FogPlaneWidth = 22f;
    private const float FogPlaneHeight = 16f;
    private const float FogAlpha = 0.14f;
    private const float EdgeGlowAlpha = 0.22f;
    private const float EdgeGlowWidth = 3.5f;
    private const float BottomFogAlpha = 0.22f;
    private const float TopGlowAlpha = 0.12f;
    private const float RoomHazeAlpha = 0.04f;

    private static readonly Color FogColor = new(0.04f, 0.06f, 0.08f, FogAlpha);
    private static readonly Color EdgeGlowGreen = new(0.1f, 1f, 0.3f, EdgeGlowAlpha);
    private static readonly Color EdgeGlowMagenta = new(1f, 0.1f, 0.55f, EdgeGlowAlpha * 0.8f);

    private void BuildAtmosphereOverlays()
    {
        float boardCenterX = GameConstants.BOARD_CENTER_X;
        float boardCenterY = GameConstants.BOARD_CENTER_Y;
        float fogZ = GameConstants.BOARD_Z - 2f;

        // Subtle fog layer behind balloons
        SetPanel("FogLayer", PrimitiveType.Quad,
            new Vector3(boardCenterX, boardCenterY, fogZ),
            new Vector3(FogPlaneWidth, FogPlaneHeight, 1f),
            CreateMaterial(FogColor, unlit: true));

        // Neon edge glow — left (green), taller for full board coverage
        float frameLeft = GameConstants.BOARD_LEFT - 0.5f;
        float edgeHeight = GameConstants.BOARD_HEIGHT + 4f;
        SetPanel("EdgeGlowLeft", PrimitiveType.Quad,
            new Vector3(frameLeft - EdgeGlowWidth * 0.3f, boardCenterY, fogZ + 0.1f),
            new Vector3(EdgeGlowWidth, edgeHeight, 1f),
            CreateMaterial(EdgeGlowGreen, unlit: true));

        // Neon edge glow — right (magenta)
        float frameRight = GameConstants.BOARD_RIGHT + 0.5f;
        SetPanel("EdgeGlowRight", PrimitiveType.Quad,
            new Vector3(frameRight + EdgeGlowWidth * 0.3f, boardCenterY, fogZ + 0.1f),
            new Vector3(EdgeGlowWidth, edgeHeight, 1f),
            CreateMaterial(EdgeGlowMagenta, unlit: true));

        // Bottom fog gradient below the board for depth fade
        float bottomFogY = GameConstants.BOARD_BOTTOM - 1.5f;
        SetPanel("BottomFog", PrimitiveType.Quad,
            new Vector3(boardCenterX, bottomFogY, fogZ - 0.5f),
            new Vector3(FogPlaneWidth, 4f, 1f),
            CreateMaterial(new Color(0.02f, 0.02f, 0.03f, BottomFogAlpha), unlit: true));

        // Top ambient glow — warm glow above the board
        float topGlowY = GameConstants.BOARD_TOP + 1.5f;
        SetPanel("TopGlow", PrimitiveType.Quad,
            new Vector3(boardCenterX, topGlowY, fogZ - 0.3f),
            new Vector3(FogPlaneWidth * 0.8f, 3f, 1f),
            CreateMaterial(new Color(1f, 0.88f, 0.72f, TopGlowAlpha), unlit: true));

        // Room-wide warm haze for atmospheric depth
        SetPanel("RoomHaze", PrimitiveType.Quad,
            new Vector3(boardCenterX, boardCenterY, fogZ - 1f),
            new Vector3(FogPlaneWidth * 1.2f, FogPlaneHeight * 1.2f, 1f),
            CreateMaterial(new Color(0.08f, 0.06f, 0.04f, RoomHazeAlpha), unlit: true));
    }
}
