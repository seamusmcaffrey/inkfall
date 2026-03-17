using System;
using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// In-play-mode MonoBehaviour that fires darts at three pull levels
/// and captures screenshots after each via RenderTexture (works in batch mode).
/// Signals completion to the DartPhysicsTest state machine via a static flag.
/// </summary>
public class DartPhysicsTestRunner : MonoBehaviour
{
    private const float DelayBeforeFire = 0.3f;
    private const float FlightDuration = 3.0f;
    private const string OutputDir = "Logs/agent-feedback/screenshots";
    private const int CaptureWidth = 1080;
    private const int CaptureHeight = 1920;

    public static bool IsComplete { get; set; }

    private static readonly float[] PullLevels = { 0.3f, 0.6f, 1.0f };
    private static readonly string[] PullLabels = { "pull_30", "pull_60", "pull_100" };
    private static readonly float[] ColumnOffsets = { -1.5f, 0.5f, 2.5f };

    public IEnumerator FireSequenceWithScreenshots()
    {
        float exponent = GameConfigSO.Instance.pullSpeedExponent;

        // Force shader compilation before first capture to avoid magenta frames
        Shader.WarmupAllShaders();
        yield return null; // Wait one frame for compilation to complete

        yield return new WaitForEndOfFrame();
        RenderScreenshot("dart_test_before");

        for (int i = 0; i < PullLevels.Length; i++)
        {
            float pull = PullLevels[i];
            float power = Mathf.Pow(pull, exponent);
            float aimY = GameConstants.BOARD_CENTER_Y + power * GameConstants.AIM_RANGE_Y;
            aimY = Mathf.Min(aimY, GameConstants.BOARD_TOP);
            Vector3 aimPoint = new Vector3(0f, aimY, GameConstants.BOARD_Z);
            Vector3 velocity = SlingshotInput.ComputeBallisticVelocity(
                GameConstants.LAUNCH_POSITION, aimPoint);

            float theoreticalPeak = velocity.y > 0f
                ? GameConstants.LAUNCH_POSITION.y +
                  (velocity.y * velocity.y) / (2f * Mathf.Abs(GameConstants.DART_GRAVITY))
                : GameConstants.LAUNCH_POSITION.y;
#if UNITY_EDITOR
            Debug.Log($"[DartTest] {PullLabels[i]}: pull={pull}, power={power:F3}, aimY={aimY:F1}, velocity={velocity}");
            Debug.Log($"[DartTest]   Config: exponent={exponent}, gravity={GameConstants.DART_GRAVITY}, forwardSpeed={GameConstants.DART_FORWARD_SPEED}");
            Debug.Log($"[DartTest]   Launch Y={GameConstants.LAUNCH_POSITION.y}, Board Y={GameConstants.BOARD_BOTTOM} to {GameConstants.BOARD_TOP}");
            Debug.Log($"[DartTest]   Theoretical peak Y={theoreticalPeak:F1} (board bottom={GameConstants.BOARD_BOTTOM}, top={GameConstants.BOARD_TOP})");
#endif

            yield return new WaitForSeconds(DelayBeforeFire);
            var dart = FireDartTracked(velocity, ColumnOffsets[i]);
            float peakY = GameConstants.LAUNCH_POSITION.y;
            float elapsed = 0f;
            while (elapsed < FlightDuration && dart != null)
            {
                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
                if (dart != null && dart.transform.position.y > peakY)
                    peakY = dart.transform.position.y;
            }
#if UNITY_EDITOR
            Debug.Log($"[DartTest]   ACTUAL peak Y={peakY:F2}, final Y={(dart != null ? dart.transform.position.y : float.NaN):F2}");
#endif

            yield return new WaitForEndOfFrame();
            RenderScreenshot($"dart_test_{PullLabels[i]}");
        }

        yield return new WaitForEndOfFrame();
        RenderScreenshot("dart_test_final");
        yield return new WaitForSeconds(0.3f);

        IsComplete = true;
        Destroy(gameObject);
    }

    private static void RenderScreenshot(string label)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = FindAnyObjectByType<Camera>();
        if (cam == null) return;

        Directory.CreateDirectory(OutputDir);
        string filename = $"{label}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
        string path = Path.Combine(OutputDir, filename);

        var rt = new RenderTexture(CaptureWidth, CaptureHeight, 24);
        var tex = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);

        RenderTexture prevTarget = cam.targetTexture;
        RenderTexture prevActive = RenderTexture.active;

        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
        tex.Apply();

        cam.targetTexture = prevTarget;
        RenderTexture.active = prevActive;

        File.WriteAllBytes(path, tex.EncodeToPNG());
        Destroy(tex);
        Destroy(rt);
    }

    private static GameObject FireDartTracked(Vector3 velocity, float xOffset)
    {
        var launcher = FindAnyObjectByType<DartLauncher>();
        if (launcher != null)
        {
            var controller = launcher.SpawnAndLaunch(velocity);
            if (controller != null)
            {
                var rb = controller.GetComponent<Rigidbody>();
                rb.position = GameConstants.LAUNCH_POSITION + new Vector3(xOffset, 0f, 0f);
                return controller.gameObject;
            }
            return null;
        }

        return CreateSimpleDart(velocity, xOffset);
    }

    private static GameObject CreateSimpleDart(Vector3 velocity, float xOffset)
    {
        var dart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dart.name = "TestDart";
        dart.transform.position = GameConstants.LAUNCH_POSITION + new Vector3(xOffset, 0f, 0f);
        dart.transform.localScale = Vector3.one * 0.3f;

        var rb = dart.GetComponent<Rigidbody>();
        if (rb == null) rb = dart.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.mass = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = velocity;

        var gravity = dart.AddComponent<DartPhysicsTestGravity>();
        gravity.SetGravity(GameConstants.DART_GRAVITY);

        Destroy(dart, GameConstants.DEFAULT_DART_LIFETIME);
        return dart;
    }
}

/// <summary>
/// Applies custom gravity to a test dart each physics step.
/// </summary>
public class DartPhysicsTestGravity : MonoBehaviour
{
    private float _gravity;
    private Rigidbody _rigidbody;

    public void SetGravity(float gravity) => _gravity = gravity;
    private void Awake() => _rigidbody = GetComponent<Rigidbody>();

    private void FixedUpdate()
    {
        _rigidbody.AddForce(new Vector3(0f, _gravity, 0f), ForceMode.Acceleration);
    }
}
