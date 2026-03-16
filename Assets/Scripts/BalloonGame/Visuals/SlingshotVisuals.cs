using UnityEngine;

[RequireComponent(typeof(SlingshotInput))]
public partial class SlingshotVisuals : MonoBehaviour
{
    [Header("Rubber Band")]
    [SerializeField] private float _bandStartWidth = 0.15f;
    [SerializeField] private float _bandEndWidth = 0.08f;

    private SlingshotInput _input;
    private LineRenderer _bandLine;
    private LineRenderer _trajectoryLine;
    private LineRenderer _originRing;

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
        Vector3 slingshotPos = GameConstants.LAUNCH_POSITION;
        Vector3 fireOrigin = GameConstants.FIRE_ORIGIN;
        Vector3 pullEnd = slingshotPos + new Vector3(pullVector.x, pullVector.y, 0f);

        // Rubber band — anchored at slingshot (bottom of screen, where the finger is)
        _bandLine.positionCount = 2;
        _bandLine.SetPosition(0, slingshotPos + Vector3.back);
        _bandLine.SetPosition(1, pullEnd + Vector3.back);

        // Origin ring
        UpdateOriginRing(slingshotPos);

        // Trajectory — starts from fire origin (mid-screen, where dart spawns)
        Vector3 velocity = _input.PreviewVelocity;
        if (velocity.sqrMagnitude < 1f)
        {
            _trajectoryLine.positionCount = 0;
            HideDots();
            return;
        }

        Vector3 gravity = new Vector3(0f, GameConstants.DART_GRAVITY, 0f);

        int visibleCount = 0;
        for (int index = 0; index < _trajectoryPoints; index++)
        {
            float time = (float)index / (_trajectoryPoints - 1) * _trajectoryDuration;
            Vector3 position = fireOrigin + velocity * time + 0.5f * gravity * time * time;
            if (position.z > GameConstants.BOARD_Z + 0.5f)
            {
                break;
            }
            visibleCount++;
        }

        _trajectoryLine.positionCount = visibleCount;
        for (int index = 0; index < visibleCount; index++)
        {
            float time = (float)index / (_trajectoryPoints - 1) * _trajectoryDuration;
            Vector3 position = fireOrigin + velocity * time + 0.5f * gravity * time * time;
            _trajectoryLine.SetPosition(index, position + Vector3.back);
        }

        // Position the trajectory dots along the same arc
        UpdateTrajectoryDots(fireOrigin, velocity, gravity);
    }

    private void LateUpdate()
    {
        if (!_input.IsAiming)
        {
            HideAll();
        }
    }

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

    private LineRenderer CreateLine(string name, Color color, float startWidth, float endWidth)
    {
        var lineObject = new GameObject(name);
        lineObject.transform.SetParent(transform, false);

        var lineRenderer = lineObject.AddComponent<LineRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                        ?? Shader.Find("Universal Render Pipeline/Unlit")
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
}
