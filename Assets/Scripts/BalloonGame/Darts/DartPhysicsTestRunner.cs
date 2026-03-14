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
    private const float FlightDuration = 2.0f;
    private const string OutputDir = "Logs/agent-feedback/screenshots";
    private const int CaptureWidth = 1080;
    private const int CaptureHeight = 1920;

    public static bool IsComplete { get; set; }

    private static readonly float[] PullLevels = { 0.3f, 0.6f, 1.0f };
    private static readonly string[] PullLabels = { "pull_30", "pull_60", "pull_100" };

    public IEnumerator FireSequenceWithScreenshots()
    {
        var config = GameConfigSO.Instance;
        float minSpeed = config.minLaunchSpeed;
        float maxSpeed = config.maxLaunchSpeed;
        float exponent = config.pullSpeedExponent;

        // Force shader compilation before first capture to avoid magenta frames
        Shader.WarmupAllShaders();
        yield return null; // Wait one frame for compilation to complete

        yield return new WaitForEndOfFrame();
        RenderScreenshot("dart_test_before");

        for (int i = 0; i < PullLevels.Length; i++)
        {
            float pull = PullLevels[i];
            float pullStrength = Mathf.Pow(pull, exponent);
            float speed = minSpeed + pullStrength * (maxSpeed - minSpeed);
            Vector3 velocity = Vector3.up * speed;

            yield return new WaitForSeconds(DelayBeforeFire);
            FireDart(velocity);
            yield return new WaitForSeconds(FlightDuration);
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

    private static void FireDart(Vector3 velocity)
    {
        var launcher = FindAnyObjectByType<DartLauncher>();
        if (launcher != null)
        {
            launcher.SpawnAndLaunch(velocity);
            return;
        }

        CreateSimpleDart(velocity);
    }

    private static void CreateSimpleDart(Vector3 velocity)
    {
        var dart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dart.name = "TestDart";
        dart.transform.position = GameConstants.LAUNCH_POSITION;
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
