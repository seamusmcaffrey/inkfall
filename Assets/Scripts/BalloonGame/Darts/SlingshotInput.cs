using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[DisallowMultipleComponent]
public class SlingshotInput : MonoBehaviour
{
    public event Action<Vector3> OnLaunch;
    public event Action<Vector2> OnPullUpdate;
    public event Action OnPullCancel;
    public event Action OnPullStart;

    private Camera _camera;
    private bool _isAiming;
    private bool _isPullArmed;
    private bool _hasClearedDeadZone;
    private bool _canFire = true;
    private Vector2 _pullVector;
    private int _activeFingerIndex = -1;
    private AimAssist _aimAssist;

    public bool IsAiming => _isAiming;
    public bool IsPullArmed => _isPullArmed;

    public float PullStrength
    {
        get
        {
            float distance = _pullVector.magnitude;
            return Mathf.Pow(
                Mathf.Clamp01(distance / GameConstants.MAX_PULL_DISTANCE),
                GameConfigSO.Instance.pullSpeedExponent);
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
                GameConfigSO.Instance.pullSpeedExponent);
            float speed = GameConfigSO.Instance.minLaunchSpeed +
                          power * (GameConfigSO.Instance.maxLaunchSpeed - GameConfigSO.Instance.minLaunchSpeed);
            Vector3 velocity = new Vector3(direction.x, direction.y, 0f) * speed;
            velocity.z = GameConstants.DART_FORWARD_SPEED;
            return _aimAssist != null ? _aimAssist.ApplyAssist(velocity) : velocity;
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        ComponentUtility.ResolveLocalComponent(this, ref _aimAssist);
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (!_canFire)
        {
            return;
        }

        if (Touch.activeTouches.Count > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandlePointerInput();
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

    private void HandleTouchInput()
    {
        if (!_isAiming)
        {
            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    Vector2 worldPosition = ScreenToWorld(touch.screenPosition);
                    if (IsWithinActivationZone(worldPosition))
                    {
                        _activeFingerIndex = touch.finger.index;
                        BeginPullAtPosition(worldPosition);
                        break;
                    }
                }
            }

            return;
        }

        Touch? trackedTouch = null;
        foreach (var touch in Touch.activeTouches)
        {
            if (touch.finger.index == _activeFingerIndex)
            {
                trackedTouch = touch;
                break;
            }
        }

        if (trackedTouch == null)
        {
            CancelPull();
            return;
        }

        var tracked = trackedTouch.Value;
        if (tracked.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
            tracked.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            ReleasePull();
        }
        else if (tracked.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                 tracked.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
        {
            UpdatePullAtPosition(ScreenToWorld(tracked.screenPosition));
        }
    }

    private void HandlePointerInput()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }

        if (pointer.press.wasPressedThisFrame)
        {
            Vector2 worldPosition = ScreenToWorld(pointer.position.ReadValue());
            if (IsWithinActivationZone(worldPosition))
            {
                BeginPullAtPosition(worldPosition);
            }
        }
        else if (pointer.press.isPressed && _isAiming)
        {
            UpdatePullAtPosition(ScreenToWorld(pointer.position.ReadValue()));
        }
        else if (pointer.press.wasReleasedThisFrame && _isAiming)
        {
            ReleasePull();
        }
    }

    private void BeginPullAtPosition(Vector2 worldPosition)
    {
        _isAiming = true;
        _isPullArmed = false;
        _hasClearedDeadZone = false;
        _pullVector = Vector2.zero;
        OnPullStart?.Invoke();
        OnPullUpdate?.Invoke(_pullVector);
    }

    private void UpdatePullAtPosition(Vector2 worldPosition)
    {
        Vector2 launchPosition = new(GameConstants.LAUNCH_POSITION.x, GameConstants.LAUNCH_POSITION.y);
        Vector2 rawPull = worldPosition - launchPosition;
        float distance = Mathf.Min(rawPull.magnitude, GameConstants.MAX_PULL_DISTANCE);
        _pullVector = rawPull.sqrMagnitude > Mathf.Epsilon
            ? rawPull.normalized * distance
            : Vector2.zero;

        if (!_hasClearedDeadZone && _pullVector.magnitude >= GameConstants.PULL_DEAD_ZONE)
        {
            _hasClearedDeadZone = true;
        }

        if (_hasClearedDeadZone && !_isPullArmed && _pullVector.magnitude >= GameConstants.MIN_PULL_DISTANCE)
        {
            _isPullArmed = true;
        }

        OnPullUpdate?.Invoke(_pullVector);
    }

    private void ReleasePull()
    {
        _isAiming = false;
        Vector3 velocity = PreviewVelocity;

        if (_pullVector.magnitude <= GameConstants.PULL_CANCEL_RETURN_RADIUS)
        {
            CancelPull();
            return;
        }

        if (_isPullArmed && velocity.sqrMagnitude > GameConfigSO.Instance.minPreviewVelocity)
        {
            OnLaunch?.Invoke(velocity);
            EventBus.Publish(new DartLaunchedEvent
            {
                LaunchVelocity = velocity,
                PullStrength = PullStrength,
            });
        }
        else
        {
            OnPullCancel?.Invoke();
        }

        _pullVector = Vector2.zero;
        _isPullArmed = false;
        _hasClearedDeadZone = false;
        _activeFingerIndex = -1;
    }

    private void CancelPull()
    {
        _isAiming = false;
        _isPullArmed = false;
        _hasClearedDeadZone = false;
        _pullVector = Vector2.zero;
        _activeFingerIndex = -1;
        OnPullCancel?.Invoke();
    }

    private static bool IsWithinActivationZone(Vector2 worldPosition)
    {
        Vector2 launchPosition = new(GameConstants.LAUNCH_POSITION.x, GameConstants.LAUNCH_POSITION.y);
        return Vector2.Distance(worldPosition, launchPosition) <= GameConstants.AIM_ACTIVATION_RADIUS;
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
