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
    /// Pull is downward-only: magnitude maps to board height (gentle = bottom,
    /// strong = top), X direction controls lateral aim (inverted, slingshot-style).
    /// Shared by runtime input and editor tools.
    /// </summary>
    public static Vector3 ComputeAimPoint(Vector2 pullVector)
    {
        // Enforce downward-only pull for any external caller
        Vector2 clamped = new(pullVector.x, Mathf.Min(pullVector.y, 0f));

        float pullMag = clamped.magnitude;
        float pullNorm = Mathf.Clamp01(pullMag / GameConstants.MAX_PULL_DISTANCE);
        float power = Mathf.Pow(pullNorm, GameConfigSO.Instance.pullSpeedExponent);

        // Y aim: pull strength maps through a virtual floor below the board,
        // giving gentle pulls a wide shelf that all aim at the bottom row
        float virtualBottom = GameConstants.BOARD_BOTTOM - GameConstants.AIM_VERTICAL_SHELF;
        float aimY = Mathf.Lerp(virtualBottom, GameConstants.BOARD_TOP, power);

        // X aim: lateral pull direction, inverted (pull left → aim right)
        float pullDirX = pullMag > 0.001f ? clamped.x / pullMag : 0f;
        float aimX = GameConstants.BOARD_CENTER_X + (-pullDirX) * power * GameConstants.AIM_RANGE_X;

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

            return ComputeBallisticVelocity(GameConstants.LAUNCH_POSITION, aim);
        }
    }

    /// <summary>
    /// Computes the launch velocity needed to reach a target point on the board,
    /// accounting for gravity along the parabolic arc. Enforces a minimum lob
    /// height so every shot visibly arcs upward before descending to its target.
    /// </summary>
    public static Vector3 ComputeBallisticVelocity(Vector3 origin, Vector3 target)
    {
        float zDistance = target.z - origin.z;
        if (zDistance < 0.01f)
        {
            return Vector3.zero;
        }

        float g = GameConstants.DART_GRAVITY;
        float absG = Mathf.Abs(g);

        // Natural solution: fixed forward speed, solve for vy
        float forwardSpeed = GameConstants.DART_FORWARD_SPEED;
        float flightTime = zDistance / forwardSpeed;
        float vy = (target.y - origin.y - 0.5f * g * flightTime * flightTime) / flightTime;

        // Enforce minimum lob arc — peak must be at least MIN_LOB_HEIGHT above origin
        float minVy = Mathf.Sqrt(2f * absG * GameConstants.MIN_LOB_HEIGHT);
        if (vy < minVy)
        {
            vy = minVy;
            // Recompute flight time so dart hits the target on the descending arc
            // Solve: 0.5*g*t² + vy*t + (origin.y - target.y) = 0
            float a = 0.5f * g;
            float c = origin.y - target.y;
            float discriminant = vy * vy - 4f * a * c;
            if (discriminant < 0f) discriminant = 0f;
            // Take the later root (descending arc hit)
            flightTime = (-vy - Mathf.Sqrt(discriminant)) / (2f * a);
            if (flightTime < 0.1f) flightTime = zDistance / GameConstants.DART_FORWARD_SPEED;
            forwardSpeed = zDistance / flightTime;
        }

        float vx = (target.x - origin.x) / flightTime;
        return new Vector3(vx, vy, forwardSpeed);
    }
}
