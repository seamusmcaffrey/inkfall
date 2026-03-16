using UnityEditor;
using UnityEngine;

/// <summary>
/// Scene View dart tester. Open via INKSHOT > Scene Dart Tester.
/// Toggle on, click-drag near DART LAUNCH gizmo during Play mode to fire.
/// </summary>
public class SceneViewDartTester : EditorWindow
{
    private const int TrajectorySegments = 40;
    private const float TrajectoryDuration = 1.2f;
    private const float DragActivationRadius = 3f;

    private static bool _enabled;
    private static bool _isDragging;
    private static Vector2 _pullVector;

    [MenuItem("INKSHOT/Scene Dart Tester")]
    private static void Open() => GetWindow<SceneViewDartTester>("Dart Tester");

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    private void OnGUI()
    {
        _enabled = EditorGUILayout.Toggle("Enable Scene Firing", _enabled);

        if (_enabled && !Application.isPlaying)
            EditorGUILayout.HelpBox("Enter Play Mode to fire darts.", MessageType.Warning);
        else if (_enabled)
            EditorGUILayout.HelpBox("Drag near DART LAUNCH in Scene View. Release to fire.", MessageType.Info);

        if (!_isDragging) return;
        Vector3 aim = SlingshotInput.ComputeAimPoint(_pullVector);
        EditorGUILayout.LabelField("Pull", _pullVector.ToString("F2"));
        EditorGUILayout.LabelField("Aim", $"({aim.x:F1}, {aim.y:F1})");
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!_enabled || !Application.isPlaying)
        {
            return;
        }

        DrawLaunchIndicator();
        HandleInput(sceneView);

        if (_isDragging && _pullVector.sqrMagnitude > 0.01f)
        {
            DrawPullLine();
            DrawTrajectoryArc();
            DrawAimCrosshair();
        }
    }

    private static void HandleInput(SceneView sceneView)
    {
        Event e = Event.current;
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        Vector2 launchXY = new(GameConstants.FIRE_ORIGIN.x, GameConstants.FIRE_ORIGIN.y);

        switch (e.type)
        {
            case EventType.MouseDown when e.button == 0 && !e.alt:
            {
                Vector3 worldPos = ScreenToLaunchPlane(e.mousePosition);
                float dist = Vector2.Distance(new Vector2(worldPos.x, worldPos.y), launchXY);
                if (dist <= DragActivationRadius)
                {
                    _isDragging = true;
                    _pullVector = Vector2.zero;
                    GUIUtility.hotControl = controlId;
                    e.Use();
                }

                break;
            }
            case EventType.MouseDrag when _isDragging:
            {
                Vector3 dragPos = ScreenToLaunchPlane(e.mousePosition);
                Vector2 rawPull = new Vector2(dragPos.x, dragPos.y) - launchXY;
                float mag = Mathf.Min(rawPull.magnitude, GameConstants.MAX_PULL_DISTANCE);
                _pullVector = rawPull.sqrMagnitude > Mathf.Epsilon
                    ? rawPull.normalized * mag
                    : Vector2.zero;
                e.Use();
                sceneView.Repaint();
                break;
            }
            case EventType.MouseUp when _isDragging:
            {
                if (_pullVector.magnitude >= GameConstants.MIN_PULL_DISTANCE)
                {
                    FireDart();
                }

                _isDragging = false;
                _pullVector = Vector2.zero;
                GUIUtility.hotControl = 0;
                e.Use();
                break;
            }
        }
    }

    private static Vector3 ScreenToLaunchPlane(Vector2 guiPosition)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);
        float launchZ = GameConstants.FIRE_ORIGIN.z;

        if (Mathf.Abs(ray.direction.z) < 0.001f)
        {
            return GameConstants.FIRE_ORIGIN;
        }

        float t = (launchZ - ray.origin.z) / ray.direction.z;
        return ray.origin + ray.direction * t;
    }

    private static void FireDart()
    {
        Vector3 aim = SlingshotInput.ComputeAimPoint(_pullVector);
        Vector3 velocity = SlingshotInput.ComputeBallisticVelocity(
            GameConstants.FIRE_ORIGIN, aim);

        var launcher = FindAnyObjectByType<DartLauncher>();
        if (launcher != null)
        {
            launcher.SpawnAndLaunch(velocity);
        }
    }

    private static void DrawLaunchIndicator()
    {
        Handles.color = new Color(1f, 1f, 0f, 0.5f);
        Handles.DrawWireDisc(GameConstants.FIRE_ORIGIN, Vector3.forward, DragActivationRadius);
        Handles.DrawWireDisc(GameConstants.FIRE_ORIGIN, Vector3.forward, 0.3f);
    }

    private static void DrawPullLine()
    {
        Vector3 pullEnd = GameConstants.FIRE_ORIGIN + new Vector3(_pullVector.x, _pullVector.y, 0f);
        Handles.color = new Color(1f, 0.3f, 0.2f, 0.9f);
        Handles.DrawLine(GameConstants.FIRE_ORIGIN, pullEnd, 3f);
    }

    private static void DrawTrajectoryArc()
    {
        Vector3 origin = GameConstants.FIRE_ORIGIN;
        Vector3 aim = SlingshotInput.ComputeAimPoint(_pullVector);
        Vector3 velocity = SlingshotInput.ComputeBallisticVelocity(origin, aim);
        Vector3 gravity = new(0f, GameConstants.DART_GRAVITY, 0f);

        var points = new Vector3[TrajectorySegments];
        int count = 0;

        for (int i = 0; i < TrajectorySegments; i++)
        {
            float t = (float)i / (TrajectorySegments - 1) * TrajectoryDuration;
            Vector3 pos = origin + velocity * t + 0.5f * gravity * t * t;
            if (pos.z > GameConstants.BOARD_Z + 0.5f)
            {
                break;
            }

            points[count++] = pos;
        }

        if (count < 2)
        {
            return;
        }

        Handles.color = new Color(1f, 0.9f, 0.3f, 0.85f);
        for (int i = 0; i < count - 1; i++)
        {
            Handles.DrawLine(points[i], points[i + 1], 2f);
        }
    }

    private static void DrawAimCrosshair()
    {
        Vector3 aim = SlingshotInput.ComputeAimPoint(_pullVector);
        const float size = 0.5f;

        Handles.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        Handles.DrawLine(aim + Vector3.left * size, aim + Vector3.right * size, 2f);
        Handles.DrawLine(aim + Vector3.up * size, aim + Vector3.down * size, 2f);
        Handles.DrawWireDisc(aim, Vector3.forward, size * 0.7f);
        Handles.Label(aim + Vector3.up * 0.6f, $"({aim.x:F1}, {aim.y:F1})");
    }
}
