using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BalloonCamera : MonoBehaviour
{
    private const float TargetAspect = 9f / 16f;

    private Camera _camera;
    private Vector3 _basePosition;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        ConfigureCamera();
        EnforceAspect();
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
            EnforceAspect();
        }
    }

    private void ConfigureCamera()
    {
        _camera.orthographic = true;
        _camera.orthographicSize = GameConstants.CAMERA_ORTHO_SIZE;
        _camera.nearClipPlane = 0.1f;
        _camera.farClipPlane = 50f;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0.08f, 0.07f, 0.06f);
    }

    private void EnforceAspect()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect > TargetAspect)
        {
            float width = TargetAspect / currentAspect;
            _camera.rect = new Rect((1f - width) / 2f, 0f, width, 1f);
            return;
        }

        if (currentAspect < TargetAspect)
        {
            float height = currentAspect / TargetAspect;
            _camera.rect = new Rect(0f, (1f - height) / 2f, 1f, height);
            return;
        }

        _camera.rect = new Rect(0f, 0f, 1f, 1f);
    }

    private void LateUpdate()
    {
        transform.position = _basePosition + new Vector3(
            Mathf.Sin(Time.time * 0.24f) * 0.06f,
            Mathf.Cos(Time.time * 0.18f) * 0.04f,
            0f);
    }
}
