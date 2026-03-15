using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Width-locked perspective camera.
/// Adjusts Z distance so TARGET_WORLD_WIDTH always fills the screen width.
/// On taller devices, extra vertical space reveals more environment.
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
        ApplyAdaptiveDistance();
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
            ApplyAdaptiveDistance();
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
        _camera.orthographic = false;
        _camera.fieldOfView = GameConstants.CAMERA_FOV;
        _camera.nearClipPlane = 0.1f;
        _camera.farClipPlane = 100f;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0.008f, 0.008f, 0.012f);
        _camera.rect = new Rect(0f, 0f, 1f, 1f);
    }

    private void ApplyAdaptiveDistance()
    {
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect <= 0.01f)
        {
            transform.position = new Vector3(0f, GameConstants.CAMERA_Y_CENTER, GameConstants.CAMERA_DISTANCE);
            return;
        }

        // Calculate Z distance needed to frame TARGET_WORLD_WIDTH at the board plane (Z=0)
        float halfWidth = GameConstants.TARGET_WORLD_WIDTH / 2f;
        float halfFovRad = GameConstants.CAMERA_FOV * 0.5f * Mathf.Deg2Rad;
        // Horizontal half-FOV depends on aspect ratio
        float halfHFovRad = Mathf.Atan(Mathf.Tan(halfFovRad) * screenAspect);
        float requiredDistance = halfWidth / Mathf.Tan(halfHFovRad);

        transform.position = new Vector3(0f, GameConstants.CAMERA_Y_CENTER, -requiredDistance);
    }

    private void LateUpdate()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        if (Mathf.Abs(currentAspect - _lastAspect) > 0.001f)
        {
            _lastAspect = currentAspect;
            ApplyAdaptiveDistance();
            _basePosition = transform.position;
        }

        transform.position = _basePosition + new Vector3(
            Mathf.Sin(Time.time * 0.24f) * 0.06f,
            Mathf.Cos(Time.time * 0.18f) * 0.04f,
            0f);
    }
}
