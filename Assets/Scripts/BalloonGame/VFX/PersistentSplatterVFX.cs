using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent paint splatter quads on the cork board surface.
/// Listens for BalloonPoppedEvent, spawns neon-boosted splatter marks,
/// and fades oldest when the configurable cap is reached.
/// </summary>
[DisallowMultipleComponent]
public class PersistentSplatterVFX : MonoBehaviour
{
    private readonly Queue<SplatterInstance> _activeSplatters = new();
    private readonly List<SplatterInstance> _fadingSplatters = new();
    private readonly Queue<SplatterInstance> _pool = new();
    private Material _splatterMaterial;
    private MaterialPropertyBlock _propertyBlock;
    private JuiceConfigSO _config;

    private const float AsymmetryMin = 0.7f;
    private const float AsymmetryMax = 1.3f;

    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    public void SetConfig(JuiceConfigSO config) => _config = config;

    private void OnEnable() => EventBus.Subscribe<BalloonPoppedEvent>(HandleBalloonPopped);
    private void OnDisable() => EventBus.Unsubscribe<BalloonPoppedEvent>(HandleBalloonPopped);

    private void Update() => UpdateFadingInstances();

    private void HandleBalloonPopped(BalloonPoppedEvent evt)
    {
        JuiceConfigSO config = _config != null ? _config : JuiceConfigSO.Instance;
        if (config.persistentSplattersEnabled)
        {
            SpawnSplatter(evt.WorldPosition, evt.BalloonColor, config);
        }
    }

    private void SpawnSplatter(Vector3 worldPosition, BalloonColor balloonColor, JuiceConfigSO config)
    {
        RecycleOldestIfNeeded(config);

        SplatterInstance splatter = GetFromPool();
        Transform t = splatter.Renderer.transform;
        t.SetParent(transform, false);
        t.position = new Vector3(worldPosition.x, worldPosition.y, GameConstants.SPLATTER_Z_OFFSET);

        float baseScale = Random.Range(config.splatterMinScale, config.splatterMaxScale);
        float asymmetry = Random.Range(AsymmetryMin, AsymmetryMax);
        t.localScale = new Vector3(baseScale * asymmetry, baseScale / asymmetry, 1f);
        t.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        Color neon = BoostToNeon(UIColors.GetBalloonTextColor(balloonColor), config.splatterNeonBoost);
        splatter.TargetColor = new Color(neon.r, neon.g, neon.b, config.splatterBaseAlpha);
        splatter.CurrentAlpha = config.splatterBaseAlpha;
        splatter.FadeTimer = -1f;
        ApplyColor(splatter);

        splatter.Renderer.gameObject.SetActive(true);
        _activeSplatters.Enqueue(splatter);
    }

    private void RecycleOldestIfNeeded(JuiceConfigSO config)
    {
        while (_activeSplatters.Count >= config.maxPersistentSplatters)
        {
            SplatterInstance oldest = _activeSplatters.Dequeue();
            if (oldest.Renderer != null)
            {
                BeginFade(oldest, config);
            }
        }
    }

    private void BeginFade(SplatterInstance splatter, JuiceConfigSO config)
    {
        splatter.FadeTimer = config.splatterFadeDuration;
        _fadingSplatters.Add(splatter);
    }

    private void UpdateFadingInstances()
    {
        if (_fadingSplatters.Count == 0) return;

        JuiceConfigSO config = _config != null ? _config : JuiceConfigSO.Instance;
        for (int i = _fadingSplatters.Count - 1; i >= 0; i--)
        {
            SplatterInstance splatter = _fadingSplatters[i];
            if (splatter.Renderer == null)
            {
                _fadingSplatters.RemoveAt(i);
                continue;
            }

            splatter.FadeTimer -= Time.deltaTime;
            if (splatter.FadeTimer <= 0f)
            {
                _fadingSplatters.RemoveAt(i);
                ReturnToPool(splatter);
                continue;
            }

            splatter.CurrentAlpha = splatter.TargetColor.a
                * Mathf.Clamp01(splatter.FadeTimer / config.splatterFadeDuration);
            ApplyColor(splatter);
        }
    }

    private void ApplyColor(SplatterInstance splatter)
    {
        _propertyBlock ??= new MaterialPropertyBlock();
        Color c = splatter.TargetColor;
        _propertyBlock.SetColor(ColorProperty, new Color(c.r, c.g, c.b, splatter.CurrentAlpha));
        splatter.Renderer.SetPropertyBlock(_propertyBlock);
    }

    private static Color BoostToNeon(Color baseColor, float boost)
    {
        float maxChannel = Mathf.Max(baseColor.r, Mathf.Max(baseColor.g, baseColor.b));
        if (maxChannel < 0.01f) return baseColor;

        float factor = 1f + boost / maxChannel;
        return new Color(
            Mathf.Clamp01(baseColor.r * factor),
            Mathf.Clamp01(baseColor.g * factor),
            Mathf.Clamp01(baseColor.b * factor),
            baseColor.a);
    }

    private SplatterInstance GetFromPool()
    {
        if (_pool.Count > 0)
        {
            SplatterInstance recycled = _pool.Dequeue();
            recycled.Renderer.gameObject.SetActive(true);
            return recycled;
        }
        return CreateSplatter();
    }

    private void ReturnToPool(SplatterInstance splatter)
    {
        splatter.Renderer.gameObject.SetActive(false);
        splatter.FadeTimer = -1f;
        _pool.Enqueue(splatter);
    }

    private SplatterInstance CreateSplatter()
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "PersistentSplatter";
        quad.transform.SetParent(transform, false);
        Object.Destroy(quad.GetComponent<Collider>());
        MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = GetSplatterMaterial();
        return new SplatterInstance { Renderer = renderer, FadeTimer = -1f };
    }

    private Material GetSplatterMaterial()
    {
        if (_splatterMaterial != null) return _splatterMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            _splatterMaterial = new Material(shader);
            _splatterMaterial.mainTexture = SplatterTextureGenerator.GetSplatterTexture();
            _splatterMaterial.renderQueue = 2999;
        }
        return _splatterMaterial;
    }

    /// <summary>Removes all active splatters (e.g., on room reset).</summary>
    public void ClearAll()
    {
        while (_activeSplatters.Count > 0)
        {
            SplatterInstance splatter = _activeSplatters.Dequeue();
            if (splatter.Renderer != null) ReturnToPool(splatter);
        }
        foreach (SplatterInstance fading in _fadingSplatters)
        {
            if (fading.Renderer != null) ReturnToPool(fading);
        }
        _fadingSplatters.Clear();
    }

    private class SplatterInstance
    {
        public MeshRenderer Renderer;
        public Color TargetColor;
        public float CurrentAlpha;
        public float FadeTimer;
    }
}
