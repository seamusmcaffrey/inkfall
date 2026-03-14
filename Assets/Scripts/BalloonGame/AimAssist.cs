using UnityEngine;

/// <summary>
/// Gently nudges throws toward nearby balloons to improve touch forgiveness.
/// </summary>
[DisallowMultipleComponent]
public class AimAssist : MonoBehaviour
{
    [SerializeField] private BalloonWall _balloonWall = null;

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
}
