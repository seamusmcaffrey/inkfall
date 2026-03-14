using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent paint splatter decals on the back wall.
/// </summary>
[DisallowMultipleComponent]
public class PaintDecalManager : MonoBehaviour
{
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
        decalTransform.localScale = Vector3.one * Random.Range(config.decalMinSize, config.decalMaxSize);
        renderer.gameObject.SetActive(true);

        _propertyBlock ??= new MaterialPropertyBlock();
        _propertyBlock.SetColor("_Color", new Color(color.r, color.g, color.b, config.decalAlpha));
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
        _decalMaterial = shader != null ? new Material(shader) : null;
        return _decalMaterial;
    }
}
