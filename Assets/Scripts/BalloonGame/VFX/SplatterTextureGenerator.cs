using UnityEngine;

/// <summary>
/// Generates a procedural paint splatter texture using layered Perlin noise.
/// The texture is white with alpha defining the irregular blob shape,
/// allowing color tinting via material _Color or vertex color.
/// </summary>
public static class SplatterTextureGenerator
{
    private const int TextureSize = 64;
    private const float PrimaryNoiseScale = 4.5f;
    private const float SecondaryNoiseScale = 9.0f;
    private const float TertiaryNoiseScale = 18.0f;
    private const float PrimaryWeight = 0.6f;
    private const float SecondaryWeight = 0.25f;
    private const float TertiaryWeight = 0.15f;
    private const float AlphaThreshold = 0.42f;
    private const float EdgeSoftness = 0.08f;
    private const float RadialFalloffPower = 2.2f;

    private static Texture2D _cachedTexture;

    /// <summary>
    /// Returns a cached procedural splatter texture (64x64, white + alpha).
    /// Generated lazily on first call.
    /// </summary>
    public static Texture2D GetSplatterTexture()
    {
        if (_cachedTexture != null)
        {
            return _cachedTexture;
        }

        _cachedTexture = GenerateSplatterTexture();
        return _cachedTexture;
    }

    private static Texture2D GenerateSplatterTexture()
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        float offsetX = Random.Range(0f, 100f);
        float offsetY = Random.Range(0f, 100f);
        var pixels = new Color[TextureSize * TextureSize];
        float halfSize = TextureSize * 0.5f;

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float nx = (x - halfSize) / halfSize;
                float ny = (y - halfSize) / halfSize;
                float radialDist = Mathf.Sqrt(nx * nx + ny * ny);
                float radialFalloff = 1f - Mathf.Pow(Mathf.Clamp01(radialDist), RadialFalloffPower);

                float sx = x / (float)TextureSize;
                float sy = y / (float)TextureSize;

                float noise = PrimaryWeight * Mathf.PerlinNoise(sx * PrimaryNoiseScale + offsetX, sy * PrimaryNoiseScale + offsetY)
                    + SecondaryWeight * Mathf.PerlinNoise(sx * SecondaryNoiseScale + offsetX, sy * SecondaryNoiseScale + offsetY)
                    + TertiaryWeight * Mathf.PerlinNoise(sx * TertiaryNoiseScale + offsetX, sy * TertiaryNoiseScale + offsetY);

                float combined = noise * radialFalloff;
                float alpha = Mathf.Clamp01((combined - AlphaThreshold) / EdgeSoftness);

                pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }
}
