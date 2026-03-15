using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Width-locked, height-flexible orthographic camera.
/// Guarantees TARGET_WORLD_WIDTH always fills the screen width.
/// On taller devices, extra vertical space reveals more environment.
/// On wider devices (iPad), clamps to MIN_ORTHO_SIZE so the board stays visible.
/// </summary>
[RequireComponent(typeof(Camera))]
public class BalloonCamera : MonoBehaviour
{
    private Camera _camera;
    private Vector3 _basePosition;
    private float _lastAspect;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        ConfigureCamera();
        EnablePostProcessing();
        ApplyAdaptiveSize();
        _basePosition = transform.position;
    }

    private void OnValidate()
    {
        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }

        if (_camera != null)
        {
            ConfigureCamera();
            ApplyAdaptiveSize();
        }
    }

    private void EnablePostProcessing()
    {
        var urpData = _camera.GetUniversalAdditionalCameraData();
        if (urpData != null)
        {
            urpData.renderPostProcessing = true;
        }
    }

    private void ConfigureCamera()
    {
        _camera.orthographic = true;
        _camera.nearClipPlane = 0.1f;
        _camera.farClipPlane = 50f;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0.008f, 0.008f, 0.012f);
        _camera.rect = new Rect(0f, 0f, 1f, 1f);
        transform.position = new Vector3(0f, GameConstants.CAMERA_Y_CENTER, transform.position.z);
    }

    private void ApplyAdaptiveSize()
    {
        float screenAspect = (float)Screen.width / Screen.height;

        // Prevent division by zero on startup or in batch mode
        if (screenAspect <= 0.01f)
        {
            _camera.orthographicSize = GameConstants.CAMERA_ORTHO_SIZE;
            return;
        }

        // Width-locked: calculate ortho size to fit TARGET_WORLD_WIDTH exactly
        float requiredOrthoSize = (GameConstants.TARGET_WORLD_WIDTH / 2f) / screenAspect;

        // Clamp between min (wide screens like iPad) and max (ultra-tall screens)
        _camera.orthographicSize = Mathf.Clamp(
            requiredOrthoSize,
            GameConstants.MIN_ORTHO_SIZE,
            GameConstants.MAX_ORTHO_SIZE);
    }

    private void LateUpdate()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        if (Mathf.Abs(currentAspect - _lastAspect) > 0.001f)
        {
            _lastAspect = currentAspect;
            ApplyAdaptiveSize();
        }

        transform.position = _basePosition + new Vector3(
            Mathf.Sin(Time.time * 0.24f) * 0.06f,
            Mathf.Cos(Time.time * 0.18f) * 0.04f,
            0f);
    }
}
