using UnityEngine;

/// <summary>
/// Gently nudges throws toward nearby balloons to improve touch forgiveness.
/// Also applies a subtle highlight pulse to the currently targeted balloon.
/// </summary>
[DisallowMultipleComponent]
public class AimAssist : MonoBehaviour
{
    [SerializeField] private BalloonWall _balloonWall = null;

    // --- Highlight pulse settings ---
    private const float HIGHLIGHT_PULSE_SPEED = 4f;
    private const float HIGHLIGHT_PULSE_MAX = 1.15f;
    private const float HIGHLIGHT_LERP_IN = 8f;
    private const float HIGHLIGHT_LERP_OUT = 6f;

    // --- Highlight state ---
    private BalloonNode _currentTarget;
    private BalloonJiggle _currentTargetJiggle;
    private float _highlightT; // 0 = no highlight, 1 = fully highlighted

    private void Awake()
    {
        ComponentUtility.ResolveSceneReference(this, ref _balloonWall);
    }

    /// <summary>
    /// Injects the wall reference during scene composition.
    /// </summary>
    public void SetBalloonWall(BalloonWall balloonWall)
    {
        _balloonWall = balloonWall;
    }

    public Vector3 ApplyAssist(Vector3 rawVelocity)
    {
        ComponentUtility.ResolveSceneReference(this, ref _balloonWall);
        if (!GameConfigSO.Instance.aimAssistEnabled || _balloonWall == null || rawVelocity.sqrMagnitude <= Mathf.Epsilon)
        {
            SetHighlightTarget(null);
            return rawVelocity;
        }

        Vector3 origin = GameConstants.LAUNCH_POSITION;
        Vector3 direction = rawVelocity.normalized;
        BalloonNode bestBalloon = null;
        float bestScore = 0f;

        foreach (BalloonNode balloon in _balloonWall.Balloons)
        {
            if (balloon == null || balloon.IsPopped)
            {
                continue;
            }

            Vector3 toBalloon = balloon.transform.position - origin;
            float distance = toBalloon.magnitude;
            if (distance > GameConstants.AIM_ASSIST_MAX_DISTANCE)
            {
                continue;
            }

            float angle = Vector3.Angle(direction, toBalloon.normalized);
            if (angle > GameConstants.AIM_ASSIST_MAX_ANGLE)
            {
                continue;
            }

            float score = 1f - angle / GameConstants.AIM_ASSIST_MAX_ANGLE;
            if (score > bestScore)
            {
                bestScore = score;
                bestBalloon = balloon;
            }
        }

        // Update highlight target (may be null if nothing qualified)
        SetHighlightTarget(bestBalloon);

        if (bestBalloon == null)
        {
            return rawVelocity;
        }

        Vector3 assistedDirection = Vector3.Slerp(
            direction,
            (bestBalloon.transform.position - origin).normalized,
            GameConfigSO.Instance.aimAssistStrength * bestScore);
        return assistedDirection.normalized * rawVelocity.magnitude;
    }

    // ----------------------------------------------------------------
    // Highlight pulse
    // ----------------------------------------------------------------

    private void Update()
    {
        UpdateHighlightPulse();
    }

    private void SetHighlightTarget(BalloonNode newTarget)
    {
        if (newTarget == _currentTarget)
        {
            return;
        }

        // Reset previous target
        ClearCurrentHighlight();

        _currentTarget = newTarget;
        _currentTargetJiggle = newTarget != null ? newTarget.GetComponent<BalloonJiggle>() : null;

        if (newTarget == null)
        {
            _highlightT = 0f;
        }
    }

    private void ClearCurrentHighlight()
    {
        if (_currentTargetJiggle != null)
        {
            _currentTargetJiggle.HighlightMultiplier = 1f;
        }

        _currentTarget = null;
        _currentTargetJiggle = null;
        _highlightT = 0f;
    }

    private void UpdateHighlightPulse()
    {
        bool hasTarget = _currentTarget != null
                         && !_currentTarget.IsPopped
                         && _currentTargetJiggle != null;

        if (hasTarget)
        {
            // Ramp highlight in
            _highlightT = Mathf.MoveTowards(_highlightT, 1f, HIGHLIGHT_LERP_IN * Time.deltaTime);
        }
        else
        {
            // Ramp highlight out
            _highlightT = Mathf.MoveTowards(_highlightT, 0f, HIGHLIGHT_LERP_OUT * Time.deltaTime);

            if (_highlightT <= 0f)
            {
                ClearCurrentHighlight();
                return;
            }
        }

        if (_currentTargetJiggle != null)
        {
            // Sine-based pulse between 1.0 and HIGHLIGHT_PULSE_MAX
            float pulse = (Mathf.Sin(Time.time * HIGHLIGHT_PULSE_SPEED) + 1f) * 0.5f; // 0..1
            float multiplier = Mathf.Lerp(1f, Mathf.Lerp(1f, HIGHLIGHT_PULSE_MAX, pulse), _highlightT);
            _currentTargetJiggle.HighlightMultiplier = multiplier;
        }
    }

    private void OnDisable()
    {
        ClearCurrentHighlight();
    }
}
