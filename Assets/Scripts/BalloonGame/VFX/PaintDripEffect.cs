using System.Collections;
using UnityEngine;

/// <summary>
/// Downward drip streak created after a paint explosion.
/// Thin, elongated line that slowly extends downward with neon-boosted color.
/// </summary>
[DisallowMultipleComponent]
public class PaintDripEffect : MonoBehaviour
{
    private static Material _lineMaterial;
    private LineRenderer _line;
    private Coroutine _routine;

    private void Awake()
    {
        _line = ComponentUtility.EnsureComponent<LineRenderer>(gameObject);
        _line.positionCount = 2;
        _line.useWorldSpace = false;
        _line.material = GetLineMaterial();
        _line.enabled = false;
    }

    public void Play(Color color, JuiceConfigSO config, Vector3 localOffset)
    {
        transform.localPosition = localOffset;
        Color vivid = VFXFactory.BoostNeon(color, config.paintNeonBoost);
        _line.startColor = vivid;
        _line.endColor = new Color(vivid.r, vivid.g, vivid.b, 0f);
        _line.startWidth = config.paintDripStartWidth;
        _line.endWidth = config.paintDripEndWidth;
        _line.enabled = true;

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(DripRoutine(config));
    }

    private IEnumerator DripRoutine(JuiceConfigSO config)
    {
        float elapsed = 0f;
        while (elapsed < config.paintDripLifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            float yOffset = elapsed * config.paintDripSpeed;
            _line.SetPosition(0, Vector3.zero);
            _line.SetPosition(1, Vector3.down * yOffset);
            yield return null;
        }

        _line.enabled = false;
        _routine = null;
    }

    private static Material GetLineMaterial()
    {
        if (_lineMaterial != null)
        {
            return _lineMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                        ?? Shader.Find("Sprites/Default");
        if (shader != null)
        {
            _lineMaterial = new Material(shader);
            _lineMaterial.SetFloat("_Surface", 1f);
            _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            _lineMaterial.SetInt("_ZWrite", 0);
            _lineMaterial.renderQueue = 3050;
            _lineMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        return _lineMaterial;
    }
}
