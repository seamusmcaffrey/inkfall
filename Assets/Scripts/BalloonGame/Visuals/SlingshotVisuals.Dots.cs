using UnityEngine;

public partial class SlingshotVisuals
{
    [Header("Trajectory Dots")]
    [SerializeField] private int _trajectoryPoints = GameConstants.TRAJECTORY_POINT_COUNT;
    [SerializeField] private float _trajectoryDuration = GameConstants.TRAJECTORY_DURATION;
    [SerializeField] private float _dotBaseScale = 0.12f;
    [SerializeField] private float _dotPulseAmplitude = 0.025f;
    [SerializeField] private float _dotPulseSpeed = 3.5f;
    [SerializeField] private float _dotAlphaStart = 0.85f;
    [SerializeField] private float _dotAlphaEnd = 0.08f;

    private Transform _dotContainer;
    private Transform[] _dots;
    private MeshRenderer[] _dotRenderers;
    private Material _dotMaterial;
    private MaterialPropertyBlock _dotPropertyBlock;

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
}
