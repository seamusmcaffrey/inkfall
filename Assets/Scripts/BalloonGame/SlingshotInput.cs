using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlingshotInput : MonoBehaviour
{
    public event Action<Vector3> OnLaunch;
    public event Action<Vector2> OnPullUpdate;
    public event Action OnPullCancel;

    private Camera _camera;
    private bool _isAiming;
    private bool _canFire = true;
    private Vector2 _pullVector;

    public bool IsAiming => _isAiming;

    public float PullStrength
    {
        get
        {
            float distance = _pullVector.magnitude;
            return Mathf.Pow(
                Mathf.Clamp01(distance / GameConstants.MAX_PULL_DISTANCE),
                GameConstants.PULL_SPEED_EXPONENT);
        }
    }

    public Vector3 PreviewVelocity
    {
        get
        {
            float distance = _pullVector.magnitude;
            if (distance < 0.05f)
            {
                return Vector3.zero;
            }

            Vector2 direction = -_pullVector.normalized;
            float power = Mathf.Pow(
                Mathf.Clamp01(distance / GameConstants.MAX_PULL_DISTANCE),
                GameConstants.PULL_SPEED_EXPONENT);
            float speed = GameConstants.MIN_LAUNCH_SPEED +
                          power * (GameConstants.MAX_LAUNCH_SPEED - GameConstants.MIN_LAUNCH_SPEED);
            return new Vector3(direction.x, direction.y, 0f) * speed;
        }
    }

    private void Update()
    {
        if (!_canFire)
        {
            return;
        }

        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }

        if (pointer.press.wasPressedThisFrame)
        {
            BeginPull(pointer);
        }
        else if (pointer.press.isPressed && _isAiming)
        {
            UpdatePull(pointer);
        }
        else if (pointer.press.wasReleasedThisFrame && _isAiming)
        {
            ReleasePull();
        }
    }

    public void SetCanFire(bool canFire)
    {
        _canFire = canFire;
        if (!canFire && _isAiming)
        {
            CancelPull();
        }
    }

    private void BeginPull(Pointer pointer)
    {
        Vector2 worldPosition = ScreenToWorld(pointer.position.ReadValue());
        Vector2 launchPosition = new(GameConstants.LAUNCH_POSITION.x, GameConstants.LAUNCH_POSITION.y);

        if (Vector2.Distance(worldPosition, launchPosition) > GameConstants.AIM_ACTIVATION_RADIUS)
        {
            return;
        }

        _isAiming = true;
        _pullVector = Vector2.zero;
        OnPullUpdate?.Invoke(_pullVector);
    }

    private void UpdatePull(Pointer pointer)
    {
        Vector2 worldPosition = ScreenToWorld(pointer.position.ReadValue());
        Vector2 launchPosition = new(GameConstants.LAUNCH_POSITION.x, GameConstants.LAUNCH_POSITION.y);
        Vector2 rawPull = worldPosition - launchPosition;
        float distance = Mathf.Min(rawPull.magnitude, GameConstants.MAX_PULL_DISTANCE);
        _pullVector = rawPull.sqrMagnitude > Mathf.Epsilon
            ? rawPull.normalized * distance
            : Vector2.zero;

        OnPullUpdate?.Invoke(_pullVector);
    }

    private void ReleasePull()
    {
        _isAiming = false;
        Vector3 velocity = PreviewVelocity;

        if (velocity.sqrMagnitude > 1f)
        {
            OnLaunch?.Invoke(velocity);
        }
        else
        {
            OnPullCancel?.Invoke();
        }

        _pullVector = Vector2.zero;
    }

    private void CancelPull()
    {
        _isAiming = false;
        _pullVector = Vector2.zero;
        OnPullCancel?.Invoke();
    }

    private Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        Vector3 worldPosition = _camera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(_camera.transform.position.z)));
        return new Vector2(worldPosition.x, worldPosition.y);
    }
}
