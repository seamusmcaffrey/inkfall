using UnityEngine;

[RequireComponent(typeof(SlingshotInput))]
public class SlingshotVisuals : MonoBehaviour
{
    [SerializeField] private int _trajectoryPoints = GameConstants.TRAJECTORY_POINT_COUNT;
    [SerializeField] private float _trajectoryDuration = GameConstants.TRAJECTORY_DURATION;

    private SlingshotInput _input;
    private LineRenderer _bandLine;
    private LineRenderer _trajectoryLine;

    private void Awake()
    {
        _input = GetComponent<SlingshotInput>();
        _input.OnPullUpdate += OnPullUpdate;
        _input.OnPullCancel += HideAll;
        _input.OnPullStart += ShowAll;

        _bandLine = CreateLine("RubberBand", new Color(0.8f, 0.3f, 0.2f, 0.9f), 0.08f, 0.04f);
        _trajectoryLine = CreateLine("Trajectory", new Color(1f, 1f, 1f, 0.4f), 0.06f, 0.02f);
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
    }

    private void OnPullUpdate(Vector2 pullVector)
    {
        Vector3 origin = GameConstants.LAUNCH_POSITION;
        Vector3 pullEnd = origin + new Vector3(pullVector.x, pullVector.y, 0f);

        _bandLine.positionCount = 2;
        _bandLine.SetPosition(0, origin + Vector3.back);
        _bandLine.SetPosition(1, pullEnd + Vector3.back);

        Vector3 velocity = _input.PreviewVelocity;
        if (velocity.sqrMagnitude < 1f)
        {
            _trajectoryLine.positionCount = 0;
            return;
        }

        Vector3 gravity = Physics.gravity;
        _trajectoryLine.positionCount = _trajectoryPoints;
        for (int index = 0; index < _trajectoryPoints; index++)
        {
            float time = (float)index / (_trajectoryPoints - 1) * _trajectoryDuration;
            Vector3 position = origin + velocity * time + 0.5f * gravity * time * time;
            _trajectoryLine.SetPosition(index, position + Vector3.back);
        }
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
    }

    private void ShowAll()
    {
        _bandLine.enabled = true;
        _trajectoryLine.enabled = true;
    }

    private LineRenderer CreateLine(string name, Color color, float startWidth, float endWidth)
    {
        var lineObject = new GameObject(name);
        lineObject.transform.SetParent(transform, false);

        var lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
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
