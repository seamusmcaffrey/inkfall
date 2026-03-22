using UnityEngine;

/// <summary>
/// Generates runtime Texture2D objects using Perlin noise. All textures tile with bilinear filtering.
/// </summary>
public static class ProceduralTextures
{
    private const int NoiseOctaves = 3;
    private const float BaseNoiseScale = 4f;
    private const float OctaveFrequencyMul = 2.2f;
    private const float OctaveAmpDecay = 0.5f;

    private const float CorkPitScale = 18f;
    private const float CorkPitThreshold = 0.62f;
    private const float CorkPitDarken = 0.15f;
    private const float CorkWarmShift = 0.04f;
    private const float CorkGrainR = 0.16f;
    private const float CorkGrainG = 0.12f;
    private const float CorkGrainB = 0.06f;

    private const int PlankHeight = 24;
    private const float PlankSeamWidth = 1.2f;
    private const float PlankSeamDarken = 0.45f;
    private const float WoodGrainScaleX = 14f;
    private const float WoodGrainScaleY = 1.2f;
    private const float PlankColorVariation = 0.03f;
    private const float PlankOffsetPerIndex = 7f;
    private const float FloorGrainR = 0.040f;
    private const float FloorGrainG = 0.032f;
    private const float FloorGrainB = 0.022f;
    private const float FloorTintGFactor = 0.8f;
    private const float FloorTintBFactor = 0.6f;
    private const float LaneStripeWidth = 3f;
    private const float LaneStripeBrightness = 0.08f;
    private const float PatinaGreenRatio = 0.7f;
    private const float PatinaRedReduction = 0.3f;

    private const float ScratchScaleX = 20f;
    private const float ScratchScaleY = 3f;
    private const float ScratchIntensity = 0.08f;
    private const float FineScratchMul = 3f;
    private const float PatinaScale = 3f;
    private const float PatinaThreshold = 0.58f;
    private const float PatinaStrength = 0.10f;

    private static readonly Color CorkBase = new(0.35f, 0.26f, 0.17f);
    private static readonly Color FloorBase = new(0.10f, 0.08f, 0.06f);
    private static readonly Color MetalBase = new(0.16f, 0.15f, 0.13f);
    private static readonly Color WallBase = new(0.14f, 0.11f, 0.09f);

    private const float WallStuccoScale = 6f;
    private const float WallStuccoDetail = 14f;
    private const float WallGrainStrength = 0.04f;
    private const float WallDetailStrength = 0.02f;
    private const float WallVerticalGradient = 0.03f;

    public static Texture2D GenerateCorkTexture(int width, int height)
    {
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float u = (float)x / width, v = (float)y / height;
            float grain = SampleOctaves(u, v, BaseNoiseScale);
            float pits = Mathf.PerlinNoise(u * CorkPitScale, v * CorkPitScale);
            float warmth = Mathf.PerlinNoise(u * 2f + 50f, v * 2f + 50f);
            Color c = CorkBase;
            c.r += grain * CorkGrainR + warmth * CorkWarmShift;
            c.g += grain * CorkGrainG;
            c.b += grain * CorkGrainB - warmth * CorkWarmShift;
            if (pits > CorkPitThreshold)
                c *= 1f - CorkPitDarken * ((pits - CorkPitThreshold) / (1f - CorkPitThreshold));
            pixels[y * width + x] = c;
        }
        return ApplyPixels(width, height, pixels);
    }

    public static Texture2D GenerateFloorTexture(int width, int height)
    {
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float u = (float)x / width, v = (float)y / height;
            int plankIndex = y / PlankHeight;
            float withinPlank = y % PlankHeight;
            float seamDist = Mathf.Min(withinPlank, PlankHeight - withinPlank);
            float seamFactor = seamDist < PlankSeamWidth ? 1f - seamDist / PlankSeamWidth : 0f;
            float grain = Mathf.PerlinNoise(u * WoodGrainScaleX, v * WoodGrainScaleY + plankIndex * PlankOffsetPerIndex);
            float tint = (plankIndex % 3) * PlankColorVariation;

            // Bowling lane stripe pattern — alternating light/dark planks
            bool isLightPlank = plankIndex % 2 == 0;
            float stripeBrightness = isLightPlank ? LaneStripeBrightness : 0f;

            Color c = FloorBase;
            c.r += grain * FloorGrainR + tint + stripeBrightness;
            c.g += grain * FloorGrainG + tint * FloorTintGFactor + stripeBrightness * 0.9f;
            c.b += grain * FloorGrainB + tint * FloorTintBFactor + stripeBrightness * 0.7f;
            c *= 1f - seamFactor * PlankSeamDarken;
            pixels[y * width + x] = c;
        }
        return ApplyPixels(width, height, pixels);
    }

    public static Texture2D GenerateFrameTexture(int width, int height)
    {
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float u = (float)x / width, v = (float)y / height;
            float coarse = Mathf.PerlinNoise(u * ScratchScaleX, v * ScratchScaleY);
            float fine = Mathf.PerlinNoise(u * ScratchScaleX * FineScratchMul + 100f, v * ScratchScaleY);
            float patina = Mathf.PerlinNoise(u * PatinaScale + 200f, v * PatinaScale + 200f);
            float sv = (coarse - 0.5f) * ScratchIntensity + (fine - 0.5f) * ScratchIntensity * 0.5f;
            Color c = MetalBase;
            c.r += sv;
            c.g += sv;
            c.b += sv;
            if (patina > PatinaThreshold)
            {
                float pa = (patina - PatinaThreshold) / (1f - PatinaThreshold) * PatinaStrength;
                c.g += pa;
                c.b += pa * PatinaGreenRatio;
                c.r -= pa * PatinaRedReduction;
            }
            pixels[y * width + x] = c;
        }
        return ApplyPixels(width, height, pixels);
    }

    public static Texture2D GenerateWallTexture(int width, int height)
    {
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float u = (float)x / width, v = (float)y / height;
            float stucco = SampleOctaves(u, v, WallStuccoScale);
            float detail = Mathf.PerlinNoise(u * WallStuccoDetail + 300f, v * WallStuccoDetail + 300f);
            float vGrad = v * WallVerticalGradient;
            Color c = WallBase;
            c.r += stucco * WallGrainStrength + detail * WallDetailStrength - vGrad;
            c.g += stucco * WallGrainStrength * 0.9f + detail * WallDetailStrength * 0.9f - vGrad;
            c.b += stucco * WallGrainStrength * 0.7f + detail * WallDetailStrength * 0.7f - vGrad;
            pixels[y * width + x] = c;
        }
        return ApplyPixels(width, height, pixels);
    }

    private static Texture2D ApplyPixels(int width, int height, Color[] pixels)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private static float SampleOctaves(float u, float v, float baseScale)
    {
        float value = 0f, amplitude = 1f, frequency = baseScale, maxValue = 0f;
        for (int i = 0; i < NoiseOctaves; i++)
        {
            value += Mathf.PerlinNoise(u * frequency, v * frequency) * amplitude;
            maxValue += amplitude;
            frequency *= OctaveFrequencyMul;
            amplitude *= OctaveAmpDecay;
        }
        return value / maxValue;
    }
}
