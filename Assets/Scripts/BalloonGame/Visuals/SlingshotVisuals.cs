using UnityEngine;

[RequireComponent(typeof(SlingshotInput))]
public class SlingshotVisuals : MonoBehaviour
{
    [Header("Trajectory Dots")]
    [SerializeField] private int _trajectoryPoints = GameConstants.TRAJECTORY_POINT_COUNT;
    [SerializeField] private float _trajectoryDuration = GameConstants.TRAJECTORY_DURATION;
    [SerializeField] private float _dotBaseScale = 0.12f;
    [SerializeField] private float _dotPulseAmplitude = 0.025f;
    [SerializeField] private float _dotPulseSpeed = 3.5f;
    [SerializeField] private float _dotAlphaStart = 0.85f;
    [SerializeField] private float _dotAlphaEnd = 0.08f;

    [Header("Rubber Band")]
    [SerializeField] private float _bandStartWidth = 0.15f;
    [SerializeField] private float _bandEndWidth = 0.08f;

    [Header("Launch Origin Indicator")]
    [SerializeField] private float _originRingRadius = 0.28f;
    [SerializeField] private int _originRingSegments = 48;
    [SerializeField] private float _originRingWidth = 0.025f;
    [SerializeField] private float _originPulseSpeed = 2.2f;
    [SerializeField] private float _originAlphaMin = 0.15f;
    [SerializeField] private float _originAlphaMax = 0.5f;

    private SlingshotInput _input;
    private LineRenderer _bandLine;
    private LineRenderer _trajectoryLine;
    private LineRenderer _originRing;

    private Transform _dotContainer;
    private Transform[] _dots;
    private MeshRenderer[] _dotRenderers;
    private Material _dotMaterial;
    private MaterialPropertyBlock _dotPropertyBlock;

    private static readonly Color BandColor = new(0.8f, 0.3f, 0.2f, 0.95f);
    private static readonly Color DotColor = new(1f, 1f, 1f, 0.85f);
    private static readonly Color OriginRingColor = new(1f, 0.85f, 0.5f, 0.4f);

    private void Awake()
    {
        _input = GetComponent<SlingshotInput>();
        _input.OnPullUpdate += OnPullUpdate;
        _input.OnPullCancel += HideAll;
        _input.OnPullStart += ShowAll;

        _bandLine = CreateLine("RubberBand", BandColor, _bandStartWidth, _bandEndWidth);
        _trajectoryLine = CreateLine("Trajectory", new Color(1f, 1f, 1f, 0.15f), 0.03f, 0.01f);

        CreateTrajectoryDots();
        CreateOriginRing();

        HideAll();
    }

    private void OnDestroy()
    {
        if (_input == null)
        {
            return;
        }

        _input.OnPullUpdate -= OnPullUpdate;
        _input.OnPullCancel -= HideAll;
        _input.OnPullStart -= ShowAll;

        if (_dotMaterial != null)
        {
            Destroy(_dotMaterial);
        }
    }

    private void OnPullUpdate(Vector2 pullVector)
    {
        Vector3 origin = GameConstants.LAUNCH_POSITION;
        Vector3 pullEnd = origin + new Vector3(pullVector.x, pullVector.y, 0f);

        // Rubber band
        _bandLine.positionCount = 2;
        _bandLine.SetPosition(0, origin + Vector3.back);
        _bandLine.SetPosition(1, pullEnd + Vector3.back);

        // Origin ring
        UpdateOriginRing(origin);

        // Trajectory
        Vector3 velocity = _input.PreviewVelocity;
        if (velocity.sqrMagnitude < 1f)
        {
            _trajectoryLine.positionCount = 0;
            HideDots();
            return;
        }

        Vector3 gravity = Physics.gravity;

        // Keep the thin line as a subtle connector
        _trajectoryLine.positionCount = _trajectoryPoints;
        for (int index = 0; index < _trajectoryPoints; index++)
        {
            float time = (float)index / (_trajectoryPoints - 1) * _trajectoryDuration;
            Vector3 position = origin + velocity * time + 0.5f * gravity * time * time;
            _trajectoryLine.SetPosition(index, position + Vector3.back);
        }

        // Position the trajectory dots along the same arc
        UpdateTrajectoryDots(origin, velocity, gravity);
    }

    private void LateUpdate()
    {
        if (!_input.IsAiming)
        {
            HideAll();
        }
    }

    #region Trajectory Dots

    private void CreateTrajectoryDots()
    {
        _dotContainer = new GameObject("TrajectoryDots").transform;
        _dotContainer.SetParent(transform, false);

        // URP-compatible unlit transparent material shared by all dots
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                        ?? Shader.Find("Particles/Standard Unlit")
                        ?? Shader.Find("Sprites/Default");
        _dotMaterial = new Material(shader);
        _dotMaterial.SetFloat("_Surface", 1f); // Transparent
        _dotMaterial.SetFloat("_Blend", 0f);   // Alpha blend
        _dotMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _dotMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _dotMaterial.SetInt("_ZWrite", 0);
        _dotMaterial.renderQueue = 3000;
        _dotMaterial.color = Color.white;
        _dotMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        _dots = new Transform[_trajectoryPoints];
        _dotRenderers = new MeshRenderer[_trajectoryPoints];

        for (int i = 0; i < _trajectoryPoints; i++)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Quad);
            dot.name = $"TrajDot_{i}";
            dot.transform.SetParent(_dotContainer, false);
            dot.transform.localScale = Vector3.one * _dotBaseScale;

            // Remove collider from the primitive
            Collider col = dot.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }

            MeshRenderer mr = dot.GetComponent<MeshRenderer>();
            mr.sharedMaterial = _dotMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            _dots[i] = dot.transform;
            _dotRenderers[i] = mr;

            dot.SetActive(false);
        }
    }

    private void UpdateTrajectoryDots(Vector3 origin, Vector3 velocity, Vector3 gravity)
    {
        float time = Time.time;

        for (int i = 0; i < _trajectoryPoints; i++)
        {
            float t = (float)i / (_trajectoryPoints - 1) * _trajectoryDuration;
            Vector3 position = origin + velocity * t + 0.5f * gravity * t * t;

            _dots[i].gameObject.SetActive(true);
            _dots[i].position = position + Vector3.back * 0.5f;

            // Face camera (quads need to face forward in 2D)
            _dots[i].rotation = Quaternion.identity;

            // Normalized position along the chain [0..1]
            float normalizedIndex = (float)i / (_trajectoryPoints - 1);

            // Alpha fades along the chain
            float baseAlpha = Mathf.Lerp(_dotAlphaStart, _dotAlphaEnd, normalizedIndex);

            // Pulsing/breathing: each dot pulses with a phase offset for a wave effect
            float pulsePhase = time * _dotPulseSpeed - normalizedIndex * 2.5f;
            float pulse = (Mathf.Sin(pulsePhase) + 1f) * 0.5f; // [0..1]
            float alpha = baseAlpha * Mathf.Lerp(0.7f, 1f, pulse);

            // Scale also breathes slightly
            float scale = _dotBaseScale + _dotPulseAmplitude * pulse;
            // Dots shrink slightly toward the tail
            scale *= Mathf.Lerp(1f, 0.5f, normalizedIndex);
            _dots[i].localScale = Vector3.one * scale;

            _dotPropertyBlock ??= new MaterialPropertyBlock();
            _dotPropertyBlock.SetColor("_BaseColor", new Color(DotColor.r, DotColor.g, DotColor.b, alpha));
            _dotPropertyBlock.SetColor("_Color", new Color(DotColor.r, DotColor.g, DotColor.b, alpha));
            _dotRenderers[i].SetPropertyBlock(_dotPropertyBlock);
        }
    }

    private void HideDots()
    {
        if (_dots == null) return;

        for (int i = 0; i < _dots.Length; i++)
        {
            _dots[i].gameObject.SetActive(false);
        }
    }

    #endregion

    #region Origin Ring

    private void CreateOriginRing()
    {
        _originRing = CreateLine("OriginRing", OriginRingColor, _originRingWidth, _originRingWidth);
        _originRing.loop = true;
        _originRing.positionCount = 0;
    }

    private void UpdateOriginRing(Vector3 origin)
    {
        float time = Time.time;
        float pulseAlpha = Mathf.Lerp(_originAlphaMin, _originAlphaMax,
            (Mathf.Sin(time * _originPulseSpeed) + 1f) * 0.5f);

        Color ringColor = new(OriginRingColor.r, OriginRingColor.g, OriginRingColor.b, pulseAlpha);
        _originRing.startColor = ringColor;
        _originRing.endColor = ringColor;

        // Subtle radius breathing
        float radius = _originRingRadius + 0.015f * Mathf.Sin(time * _originPulseSpeed * 0.7f);

        _originRing.positionCount = _originRingSegments;
        float zPos = origin.z - 0.8f; // Behind the band
        for (int i = 0; i < _originRingSegments; i++)
        {
            float angle = (float)i / _originRingSegments * Mathf.PI * 2f;
            float x = origin.x + Mathf.Cos(angle) * radius;
            float y = origin.y + Mathf.Sin(angle) * radius;
            _originRing.SetPosition(i, new Vector3(x, y, zPos));
        }
    }

    #endregion

    #region Visibility

    private void HideAll()
    {
        _bandLine.positionCount = 0;
        _trajectoryLine.positionCount = 0;
        HideDots();

        if (_originRing != null)
        {
            _originRing.positionCount = 0;
        }
    }

    private void ShowAll()
    {
        _bandLine.enabled = true;
        _trajectoryLine.enabled = true;

        if (_originRing != null)
        {
            _originRing.enabled = true;
        }
    }

    #endregion

    #region Helpers

    private LineRenderer CreateLine(string name, Color color, float startWidth, float endWidth)
    {
        var lineObject = new GameObject(name);
        lineObject.transform.SetParent(transform, false);

        var lineRenderer = lineObject.AddComponent<LineRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                        ?? Shader.Find("Sprites/Default");
        Material mat = new Material(shader);
        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        lineRenderer.material = mat;
        lineRenderer.startColor = color;
        lineRenderer.endColor = new Color(color.r, color.g, color.b, color.a * 0.2f);
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.textureMode = LineTextureMode.Tile;
        return lineRenderer;
    }

    #endregion
}
