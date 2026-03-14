using System.Collections.Generic;
using UnityEngine;

public partial class BalloonWall
{
    private readonly Dictionary<string, Material> _specialMaterials = new();

    private void EnsureMaterials()
    {
        if (_materials.Count > 0) return;

        Shader shader = Shader.Find("Inkshot/BalloonLit")
            ?? Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        foreach (BalloonColor color in _colors)
        {
            _materials[color] = CreateBalloonMaterial(shader, color.ToUnityColor());
        }
    }

    private static Material CreateBalloonMaterial(Shader shader, Color color, float glossiness = 0.85f, float specular = 1.2f)
    {
        var mat = new Material(shader);
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", glossiness);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", glossiness);
        if (mat.HasProperty("_SpecularIntensity")) mat.SetFloat("_SpecularIntensity", specular);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
        mat.enableInstancing = true;
        return mat;
    }

    private Material ResolveMaterial(BalloonTypeSO type)
    {
        if (type != null && type.materialOverride != null)
            return type.materialOverride;
        if (type != null && type.specialType == BalloonSpecialType.Gold)
            return GetSpecialMaterial("_gold", new Color(1f, 0.82f, 0.18f), 0.92f, 1.8f);
        if (type != null && type.specialType == BalloonSpecialType.Hazard)
            return GetSpecialMaterial("_hazard", new Color(0.18f, 0.14f, 0.16f), 0.6f, 0.8f);
        BalloonColor color = type != null ? type.balloonColor : BalloonColor.Red;
        return _materials[color];
    }

    private Material GetSpecialMaterial(string key, Color color, float gloss, float spec)
    {
        if (!_specialMaterials.ContainsKey(key))
        {
            Shader shader = Shader.Find("Inkshot/BalloonLit") ?? Shader.Find("Universal Render Pipeline/Lit");
            _specialMaterials[key] = CreateBalloonMaterial(shader, color, gloss, spec);
        }
        return _specialMaterials[key];
    }

    private Color ResolveDisplayColor(BalloonTypeSO type)
    {
        if (type != null && type.materialOverride != null)
        {
            if (type.materialOverride.HasProperty("_BaseColor"))
            {
                return type.materialOverride.GetColor("_BaseColor");
            }

            if (type.materialOverride.HasProperty("_Color"))
            {
                return type.materialOverride.GetColor("_Color");
            }
        }

        return (type != null ? type.balloonColor : BalloonColor.Red).ToUnityColor();
    }
}
