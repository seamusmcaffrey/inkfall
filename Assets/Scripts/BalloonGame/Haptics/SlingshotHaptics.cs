using UnityEngine;

/// <summary>
/// Continuous haptic feedback during slingshot pull. Ticks increase in
/// frequency and intensity as pull strength grows. Attach to the same
/// GameObject as SlingshotInput.
/// </summary>
[DisallowMultipleComponent]
public class SlingshotHaptics : MonoBehaviour
{
    private SlingshotInput _input;
    private float _tickTimer;
    private float _currentPullNormalized;
    private bool _wasAiming;

    private void OnEnable()
    {
        ComponentUtility.ResolveLocalComponent(this, ref _input);
        _input.OnPullUpdate += HandlePullUpdate;
        _input.OnPullCancel += HandlePullEnd;
        _input.OnLaunch += HandleLaunch;
    }

    private void OnDisable()
    {
        if (_input != null)
        {
            _input.OnPullUpdate -= HandlePullUpdate;
            _input.OnPullCancel -= HandlePullEnd;
            _input.OnLaunch -= HandleLaunch;
        }
    }

    private void Update()
    {
        if (!_input.IsAiming)
        {
            return;
        }

        if (_currentPullNormalized < GameConstants.HAPTIC_PULL_THRESHOLD)
        {
            return;
        }

        _tickTimer -= Time.deltaTime;
        if (_tickTimer <= 0f)
        {
            PlayTickForPull(_currentPullNormalized);
            _tickTimer = ComputeTickInterval(_currentPullNormalized);
        }
    }

    private void HandlePullUpdate(Vector2 pullVector)
    {
        float pullMagnitude = pullVector.magnitude;
        _currentPullNormalized = Mathf.Clamp01(pullMagnitude / GameConstants.MAX_PULL_DISTANCE);

        if (!_wasAiming)
        {
            _wasAiming = true;
            _tickTimer = 0f;
        }
    }

    private void HandlePullEnd()
    {
        _wasAiming = false;
        _currentPullNormalized = 0f;
    }

    private void HandleLaunch(Vector3 velocity)
    {
        _wasAiming = false;
        _currentPullNormalized = 0f;
        HapticsUtility.Medium();
    }

    private static float ComputeTickInterval(float pullNormalized)
    {
        return Mathf.Lerp(
            GameConstants.HAPTIC_TICK_INTERVAL_MIN_PULL,
            GameConstants.HAPTIC_TICK_INTERVAL_MAX_PULL,
            pullNormalized);
    }

    private static void PlayTickForPull(float pullNormalized)
    {
        if (pullNormalized < GameConstants.HAPTIC_MEDIUM_THRESHOLD)
        {
            HapticsUtility.Light();
        }
        else if (pullNormalized < GameConstants.HAPTIC_HEAVY_THRESHOLD)
        {
            HapticsUtility.Medium();
        }
        else
        {
            HapticsUtility.Heavy();
        }
    }
}
