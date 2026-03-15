using UnityEngine;

/// <summary>
/// Builds fog overlays, vignettes, and atmospheric darkening layers.
/// </summary>
public partial class EnvironmentBuilder
{
    private static readonly Color TopFogColor = new(0.01f, 0.005f, 0.02f, 0.60f);
    private static readonly Color BottomFogColor = new(0.01f, 0.01f, 0.02f, 0.55f);
    private static readonly Color SideVignetteColor = new(0.005f, 0.005f, 0.01f, 0.65f);
    private static readonly Color TopVignetteColor = new(0.005f, 0.005f, 0.01f, 0.60f);
    private static readonly Color BottomVignetteColor = new(0.005f, 0.005f, 0.01f, 0.55f);

    private const float SideVignetteWidth = 6.0f;
    private const float SideVignetteX = 5.5f;
    private const float TopBottomVignetteHeight = 5.0f;
    private const float FogWidthPadding = 2f;
    private const float TopFogYOffset = 1.5f;
    private const float BottomFogYOffset = 1.5f;
    private const float TopFogHeight = 3.0f;
    private const float BottomFogHeight = 2.5f;
    private const float FogZOffset = 0.3f;
    private const float VignetteZOffset = 0.25f;
    private const float ViewportPadding = 2f;

    private void BuildAtmosphereOverlays()
    {
        float viewportH = GameConstants.MAX_ORTHO_SIZE * 2f + ViewportPadding;
        float fogWidth = GameConstants.TARGET_WORLD_WIDTH + FogWidthPadding;
        float fullWidth = GameConstants.TARGET_WORLD_WIDTH + 16f;

        Material topFog = CreateMaterial(TopFogColor, unlit: true);
        Material bottomFog = CreateMaterial(BottomFogColor, unlit: true);

        SetPanel("FogLayerTop", PrimitiveType.Quad,
            new Vector3(0f, GameConstants.BOARD_TOP + TopFogYOffset, BoardZ - FogZOffset),
            new Vector3(fogWidth, TopFogHeight, 1f), topFog);
        SetPanel("FogLayerBottom", PrimitiveType.Quad,
            new Vector3(0f, GameConstants.BOARD_BOTTOM - BottomFogYOffset, BoardZ - FogZOffset),
            new Vector3(fogWidth, BottomFogHeight, 1f), bottomFog);

        Material sideMat = CreateMaterial(SideVignetteColor, unlit: true);
        SetPanel("VignetteLeft", PrimitiveType.Quad,
            new Vector3(-SideVignetteX, GameConstants.CAMERA_Y_CENTER, BoardZ - VignetteZOffset),
            new Vector3(SideVignetteWidth, viewportH, 1f), sideMat);
        SetPanel("VignetteRight", PrimitiveType.Quad,
            new Vector3(SideVignetteX, GameConstants.CAMERA_Y_CENTER, BoardZ - VignetteZOffset),
            new Vector3(SideVignetteWidth, viewportH, 1f), sideMat);

        Material topVig = CreateMaterial(TopVignetteColor, unlit: true);
        Material botVig = CreateMaterial(BottomVignetteColor, unlit: true);
        float topY = GameConstants.CAMERA_Y_CENTER + GameConstants.MAX_ORTHO_SIZE;
        float botY = GameConstants.CAMERA_Y_CENTER - GameConstants.MAX_ORTHO_SIZE;
        SetPanel("VignetteTop", PrimitiveType.Quad,
            new Vector3(0f, topY, BoardZ - VignetteZOffset),
            new Vector3(fullWidth, TopBottomVignetteHeight, 1f), topVig);
        SetPanel("VignetteBottom", PrimitiveType.Quad,
            new Vector3(0f, botY, BoardZ - VignetteZOffset),
            new Vector3(fullWidth, TopBottomVignetteHeight, 1f), botVig);
    }
}
