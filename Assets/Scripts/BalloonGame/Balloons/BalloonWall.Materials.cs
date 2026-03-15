using System.Collections.Generic;
using UnityEngine;

public partial class BalloonWall
{
    private const float DefaultSmoothness = 0.88f;
    private const float DefaultEmissionIntensity = 0.06f;
    private const float GoldSmoothness = 0.95f;
    private const float GoldMetallic = 0.85f;
    private const float GoldEmissionIntensity = 0.25f;
    private const float HazardSmoothness = 0.55f;
    private const float HazardEmissionIntensity = 0.35f;
    private const float PaintSmoothness = 0.92f;
    private const float PaintEmissionIntensity = 0.18f;
    private const float ShieldSmoothness = 0.7f;
    private const float ShieldMetallic = 0.15f;

    private readonly Dictionary<string, Material> _specialMaterials = new();

    private void EnsureMaterials()
    {
        if (_materials.Count > 0) return;

        Shader shader = Shader.Find("Inkshot/BalloonLit")
            ?? Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        foreach (BalloonColor color in _colors)
        {
            Color c = color.ToUnityColor();
            _materials[color] = CreateBalloonMaterial(shader, c, DefaultSmoothness,
                metallic: 0f, emissionColor: c * DefaultEmissionIntensity);
        }
    }

    private static Material CreateBalloonMaterial(
        Shader shader, Color color, float smoothness,
        float metallic = 0f, Color? emissionColor = null)
    {
        var mat = new Material(shader);
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

        if (emissionColor.HasValue)
        {
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emissionColor.Value);
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionEnabled"))
                    mat.SetFloat("_EmissionEnabled", 1f);
            }
        }

        mat.enableInstancing = true;
        return mat;
    }

    private Material ResolveMaterial(BalloonTypeSO type)
    {
        if (type != null && type.materialOverride != null)
            return type.materialOverride;

        if (type != null)
        {
            switch (type.specialType)
            {
                case BalloonSpecialType.Gold:
                    return GetGoldMaterial();
                case BalloonSpecialType.Hazard:
                    return GetHazardMaterial();
                case BalloonSpecialType.Paint:
                    return GetPaintMaterial(type.balloonColor);
                case BalloonSpecialType.Shield:
                    return GetShieldMaterial();
            }
        }

        BalloonColor color = type != null ? type.balloonColor : BalloonColor.Red;
        return _materials[color];
    }

    private Material GetSpecialMaterial(string key, Color color, float smoothness, float metallic, Color emission)
    {
        if (!_specialMaterials.ContainsKey(key))
        {
            Shader shader = FindBalloonShader();
            _specialMaterials[key] = CreateBalloonMaterial(shader, color, smoothness, metallic, emission);
        }
        return _specialMaterials[key];
    }

    private Material GetGoldMaterial()
    {
        Color gold = new(1f, 0.84f, 0.0f);
        return GetSpecialMaterial("_gold", gold, GoldSmoothness, GoldMetallic, gold * GoldEmissionIntensity);
    }

    private Material GetHazardMaterial()
    {
        Color dark = new(0.12f, 0.08f, 0.1f);
        Color glowBase = new(0.9f, 0.15f, 0.0f);
        Color glow = glowBase * HazardEmissionIntensity;
        return GetSpecialMaterial("_hazard", dark, HazardSmoothness, 0f, glow);
    }

    private Material GetPaintMaterial(BalloonColor balloonColor)
    {
        Color c = balloonColor.ToUnityColor();
        return GetSpecialMaterial("_paint_" + balloonColor, c, PaintSmoothness, 0f, c * PaintEmissionIntensity);
    }

    private Material GetShieldMaterial()
    {
        Color frost = new(0.7f, 0.82f, 0.95f);
        return GetSpecialMaterial("_shield", frost, ShieldSmoothness, ShieldMetallic, frost * 0.08f);
    }

    private static Shader FindBalloonShader()
    {
        return Shader.Find("Inkshot/BalloonLit") ?? Shader.Find("Universal Render Pipeline/Lit");
    }

    private Color ResolveDisplayColor(BalloonTypeSO type)
    {
        if (type?.materialOverride != null)
        {
            if (type.materialOverride.HasProperty("_BaseColor"))
                return type.materialOverride.GetColor("_BaseColor");
            if (type.materialOverride.HasProperty("_Color"))
                return type.materialOverride.GetColor("_Color");
        }
        return (type != null ? type.balloonColor : BalloonColor.Red).ToUnityColor();
    }
}
