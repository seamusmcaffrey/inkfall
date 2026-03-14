using UnityEngine;

/// <summary>
/// Slow-motion effect that scales intensity and duration with combo size.
/// Uses unscaled delta time for smooth ramp-up, hold, and ramp-down.
/// Driven by JuiceConfigSO — no magic numbers.
/// </summary>
[DisallowMultipleComponent]
public class SlowMotionController : MonoBehaviour
{
    private enum Phase { Idle, RampUp, Hold, RampDown }

    private JuiceConfigSO _config;
    private Phase _phase = Phase.Idle;
    private float _elapsed;
    private float _targetTimeScale;
    private float _holdDuration;
    private float _startTimeScale;

    public void SetConfig(JuiceConfigSO config)
    {
        _config = config;
    }

    /// <summary>
    /// Triggers slow motion with default intensity.
    /// </summary>
    public void Trigger(float durationMultiplier = 1f)
    {
        TriggerForCombo(0, durationMultiplier);
    }

    /// <summary>
    /// Triggers slow motion that scales with combo count.
    /// Higher combos produce slower time and longer hold.
    /// </summary>
    public void TriggerForCombo(int comboCount, float durationMultiplier = 1f)
    {
        EnsureConfig();

        int comboAboveThreshold = Mathf.Max(0, comboCount - _config.slowMotionComboThreshold);

        // Scale time deeper with higher combos, clamped to min
        float comboTimeScaleReduction = comboAboveThreshold * _config.slowMotionComboScalePerHit;
        float targetScale = Mathf.Max(
            _config.slowMotionMinTimeScale,
            _config.slowMotionTimeScale - comboTimeScaleReduction
        );

        // Extend duration with higher combos
        float comboDurationBonus = comboAboveThreshold * _config.slowMotionComboDurationPerHit;
        float holdDuration = (_config.slowMotionDuration + comboDurationBonus) * durationMultiplier;

        // If already in slo-mo, only override if this trigger is more intense
        if (_phase != Phase.Idle && targetScale >= _targetTimeScale)
        {
            return;
        }

        _targetTimeScale = targetScale;
        _holdDuration = holdDuration;
        _startTimeScale = Time.timeScale;
        _elapsed = 0f;
        _phase = Phase.RampUp;
    }

    private void Update()
    {
        if (_phase == Phase.Idle)
        {
            return;
        }

        _elapsed += Time.unscaledDeltaTime;

        switch (_phase)
        {
            case Phase.RampUp:
                UpdateRampUp();
                break;
            case Phase.Hold:
                UpdateHold();
                break;
            case Phase.RampDown:
                UpdateRampDown();
                break;
        }
    }

    private void UpdateRampUp()
    {
        float rampUpTime = _config.slowMotionRampUpTime;
        if (rampUpTime <= 0f || _elapsed >= rampUpTime)
        {
            Time.timeScale = _targetTimeScale;
            TransitionTo(Phase.Hold);
            return;
        }

        float t = _elapsed / rampUpTime;
        // Ease-out for snappy entry into slow motion
        float eased = 1f - (1f - t) * (1f - t);
        Time.timeScale = Mathf.Lerp(_startTimeScale, _targetTimeScale, eased);
    }

    private void UpdateHold()
    {
        Time.timeScale = _targetTimeScale;
        if (_elapsed >= _holdDuration)
        {
            _startTimeScale = _targetTimeScale;
            TransitionTo(Phase.RampDown);
        }
    }

    private void UpdateRampDown()
    {
        float rampDownTime = _config.slowMotionRampDownTime;
        if (rampDownTime <= 0f || _elapsed >= rampDownTime)
        {
            Finish();
            return;
        }

        float t = _elapsed / rampDownTime;
        // Ease-in for gradual return to normal speed
        float eased = t * t;
        Time.timeScale = Mathf.Lerp(_startTimeScale, 1f, eased);
    }

    private void TransitionTo(Phase newPhase)
    {
        _phase = newPhase;
        _elapsed = 0f;
    }

    private void Finish()
    {
        Time.timeScale = 1f;
        _phase = Phase.Idle;
    }

    private void EnsureConfig()
    {
        if (_config == null)
        {
            _config = JuiceConfigSO.Instance;
        }
    }

    private void OnDisable()
    {
        if (_phase != Phase.Idle)
        {
            Finish();
        }
    }
}
