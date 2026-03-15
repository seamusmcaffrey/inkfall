using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent paint splatter decals on the back wall with neon-boosted colors
/// and drip streaks that run downward from each splatter point.
/// </summary>
[DisallowMultipleComponent]
public class PaintDecalManager : MonoBehaviour
{
    private const float AsymmetryMin = 0.7f;
    private const float AsymmetryMax = 1.3f;

    private readonly Queue<MeshRenderer> _activeDecals = new();
    private Material _decalMaterial;
    private MaterialPropertyBlock _propertyBlock;

    public void SpawnDecal(Vector3 position, Color color, JuiceConfigSO config)
    {
        if (!config.paintDecalsEnabled || QualityTier.IsLow)
        {
            return;
        }

        while (_activeDecals.Count > config.maxActiveDecals)
        {
            MeshRenderer disabled = _activeDecals.Dequeue();
            if (disabled != null)
            {
                disabled.gameObject.SetActive(false);
            }
        }

        MeshRenderer renderer = _activeDecals.Count < config.maxActiveDecals
            ? CreateDecal()
            : _activeDecals.Dequeue();
        Transform decalTransform = renderer.transform;
        decalTransform.SetParent(transform, false);
        decalTransform.position = position + Vector3.forward * GameConstants.DECAL_Z_OFFSET;
        float baseScale = Random.Range(config.decalMinSize, config.decalMaxSize);
        float asymmetry = Random.Range(AsymmetryMin, AsymmetryMax);
        decalTransform.localScale = new Vector3(baseScale * asymmetry, baseScale / asymmetry, 1f);
        decalTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        renderer.gameObject.SetActive(true);

        Color neonColor = VFXFactory.BoostNeon(color, config.paintNeonBoost);
        _propertyBlock ??= new MaterialPropertyBlock();
        _propertyBlock.SetColor("_Color", new Color(neonColor.r, neonColor.g, neonColor.b, config.decalAlpha));
        renderer.SetPropertyBlock(_propertyBlock);
        _activeDecals.Enqueue(renderer);
    }

    private MeshRenderer CreateDecal()
    {
        GameObject decal = GameObject.CreatePrimitive(PrimitiveType.Quad);
        decal.name = "PaintDecal";
        decal.transform.SetParent(transform, false);
        Object.Destroy(decal.GetComponent<Collider>());
        MeshRenderer renderer = decal.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = GetDecalMaterial();
        return renderer;
    }

    private Material GetDecalMaterial()
    {
        if (_decalMaterial != null)
        {
            return _decalMaterial;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            _decalMaterial = new Material(shader);
            _decalMaterial.mainTexture = SplatterTextureGenerator.GetSplatterTexture();
        }
        return _decalMaterial;
    }
}
