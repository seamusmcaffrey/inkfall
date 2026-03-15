using UnityEngine;

/// <summary>
/// Builds the metal frame, corner bolts, and inner bevel for depth illusion.
/// </summary>
public partial class EnvironmentBuilder
{
    private const float BevelThickness = 0.08f;
    private const float BoltSize = 0.32f;
    private const float BoltZScale = 0.8f;
    private const float BoltPositionRatio = 0.3f;
    private const float BoltZDepthFactor = 1.3f;
    private const int TextureResolution = 128;

    private static readonly Color BevelHighlightColor = new(0.15f, 0.14f, 0.12f);
    private static readonly Color FramePatina = new(0.08f, 0.10f, 0.08f);

    private void BuildMetalFrame()
    {
        float bw = GameConstants.BOARD_WIDTH + BoardPaddingX;
        float bh = GameConstants.BOARD_HEIGHT + BoardPaddingY;
        float cx = (GameConstants.BOARD_LEFT + GameConstants.BOARD_RIGHT) * 0.5f;
        float cy = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;
        float halfW = bw * 0.5f;
        float halfH = bh * 0.5f;

        Texture2D frameTex = ProceduralTextures.GenerateFrameTexture(TextureResolution, TextureResolution);
        Material frameMat = CreateTexturedMaterial(frameTex, Color.white, smoothness: 0.45f, metallic: 0.65f);
        Material boltMat = CreateMaterial(BoltColor, smoothness: 0.55f, metallic: 0.75f);
        Material bevelMat = CreateMaterial(BevelHighlightColor, smoothness: 0.3f, metallic: 0.4f);

        float frameZ = BoardZ - FrameDepth;
        float frameDepth2 = FrameDepth * 2f;
        float frameFullW = bw + FrameThickness * 2f;

        SetPanel("FrameTop", PrimitiveType.Cube,
            new Vector3(cx, cy + halfH + FrameThickness * 0.5f, frameZ),
            new Vector3(frameFullW, FrameThickness, frameDepth2), frameMat);
        SetPanel("FrameBottom", PrimitiveType.Cube,
            new Vector3(cx, cy - halfH - FrameThickness * 0.5f, frameZ),
            new Vector3(frameFullW, FrameThickness, frameDepth2), frameMat);
        SetPanel("FrameLeft", PrimitiveType.Cube,
            new Vector3(cx - halfW - FrameThickness * 0.5f, cy, frameZ),
            new Vector3(FrameThickness, bh, frameDepth2), frameMat);
        SetPanel("FrameRight", PrimitiveType.Cube,
            new Vector3(cx + halfW + FrameThickness * 0.5f, cy, frameZ),
            new Vector3(FrameThickness, bh, frameDepth2), frameMat);

        BuildInnerBevel(cx, cy, halfW, halfH, bw, bh, bevelMat);
        BuildCornerBolts(cx, cy, halfW, halfH, boltMat);
    }

    private void BuildInnerBevel(float cx, float cy, float halfW, float halfH, float bw, float bh, Material mat)
    {
        float bevelZ = BoardZ - FrameDepth * 0.5f;

        SetPanel("BevelTop", PrimitiveType.Quad,
            new Vector3(cx, cy + halfH - BevelThickness * 0.5f, bevelZ),
            new Vector3(bw, BevelThickness, 1f), mat);
        SetPanel("BevelBottom", PrimitiveType.Quad,
            new Vector3(cx, cy - halfH + BevelThickness * 0.5f, bevelZ),
            new Vector3(bw, BevelThickness, 1f), mat);
        SetPanel("BevelLeft", PrimitiveType.Quad,
            new Vector3(cx - halfW + BevelThickness * 0.5f, cy, bevelZ),
            new Vector3(BevelThickness, bh, 1f), mat);
        SetPanel("BevelRight", PrimitiveType.Quad,
            new Vector3(cx + halfW - BevelThickness * 0.5f, cy, bevelZ),
            new Vector3(BevelThickness, bh, 1f), mat);
    }

    private void BuildCornerBolts(float cx, float cy, float halfW, float halfH, Material mat)
    {
        float boltZ = BoardZ - FrameDepth * BoltZDepthFactor;
        Vector3 bs = new(BoltSize, BoltSize, FrameDepth * BoltZScale);
        float bx = halfW + FrameThickness * BoltPositionRatio;
        float by = halfH + FrameThickness * BoltPositionRatio;

        SetPanel("BoltTL", PrimitiveType.Cube, new Vector3(cx - bx, cy + by, boltZ), bs, mat);
        SetPanel("BoltTR", PrimitiveType.Cube, new Vector3(cx + bx, cy + by, boltZ), bs, mat);
        SetPanel("BoltBL", PrimitiveType.Cube, new Vector3(cx - bx, cy - by, boltZ), bs, mat);
        SetPanel("BoltBR", PrimitiveType.Cube, new Vector3(cx + bx, cy - by, boltZ), bs, mat);
    }
}
