using UnityEngine;

/// <summary>
/// Aim-point ballistic model: pull direction/strength maps to a target on the
/// board, then we solve for the launch velocity that reaches it under gravity.
/// </summary>
public partial class SlingshotInput
{
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

    /// <summary>
    /// Board-plane aim point derived from pull direction and strength.
    /// </summary>
    public Vector3 AimPoint => ComputeAimPoint(_pullVector);

    /// <summary>
    /// Converts a pull vector into a board-plane aim point.
    /// Shared by runtime input and editor tools.
    /// </summary>
    public static Vector3 ComputeAimPoint(Vector2 pullVector)
    {
        float pullMag = pullVector.magnitude;
        float pullNorm = Mathf.Clamp01(pullMag / GameConstants.MAX_PULL_DISTANCE);
        float power = Mathf.Pow(pullNorm, GameConfigSO.Instance.pullSpeedExponent);
        Vector2 pullDir = pullMag > 0.001f ? pullVector / pullMag : Vector2.zero;

        float aimX = GameConstants.BOARD_CENTER_X + (-pullDir.x) * power * GameConstants.AIM_RANGE_X;
        float aimY = GameConstants.BOARD_CENTER_Y + (-pullDir.y) * power * GameConstants.AIM_RANGE_Y;

        aimX = Mathf.Clamp(aimX, GameConstants.BOARD_LEFT - GameConstants.AIM_CLAMP_MARGIN,
            GameConstants.BOARD_RIGHT + GameConstants.AIM_CLAMP_MARGIN);
        aimY = Mathf.Clamp(aimY, GameConstants.BOARD_BOTTOM - GameConstants.AIM_CLAMP_MARGIN,
            GameConstants.BOARD_TOP + GameConstants.AIM_CLAMP_MARGIN);

        return new Vector3(aimX, aimY, GameConstants.BOARD_Z);
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

            Vector3 aim = AimPoint;
            if (_aimAssist != null)
            {
                aim = _aimAssist.AdjustAimPoint(aim);
            }

            return ComputeBallisticVelocity(GameConstants.FIRE_ORIGIN, aim);
        }
    }

    /// <summary>
    /// Computes the launch velocity needed to reach a target point on the board,
    /// accounting for gravity along the parabolic arc.
    /// </summary>
    public static Vector3 ComputeBallisticVelocity(Vector3 origin, Vector3 target)
    {
        float zDistance = target.z - origin.z;
        if (zDistance < 0.01f)
        {
            return Vector3.zero;
        }

        float forwardSpeed = GameConstants.DART_FORWARD_SPEED;
        float flightTime = zDistance / forwardSpeed;

        float vx = (target.x - origin.x) / flightTime;
        float vy = (target.y - origin.y - 0.5f * GameConstants.DART_GRAVITY * flightTime * flightTime) / flightTime;

        return new Vector3(vx, vy, forwardSpeed);
    }
}
