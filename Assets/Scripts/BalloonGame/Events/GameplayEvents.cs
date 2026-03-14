using UnityEngine;

/// <summary>
/// Fired when any balloon is popped.
/// </summary>
public struct BalloonPoppedEvent
{
    public Vector3 WorldPosition;
    public BalloonColor BalloonColor;
    public string BalloonTypeId;
    public BalloonSpecialType SpecialType;
    public int BasePoints;
}

/// <summary>
/// Fired when a balloon pop is fully scored.
/// </summary>
public struct BalloonScoredEvent
{
    public Vector3 WorldPosition;
    public BalloonColor BalloonColor;
    public string BalloonTypeId;
    public int BasePoints;
    public int ComboCount;
    public float ComboMultiplier;
    public int FinalPoints;
}

/// <summary>
/// Fired when the combo changes.
/// </summary>
public struct ComboChangedEvent
{
    public int ComboCount;
    public float Multiplier;
    public bool WasReset;
}

/// <summary>
/// Fired when a dart completes its flight.
/// </summary>
public struct DartFinishedEvent
{
    public Vector3 FinalPosition;
    public string StopReason;
    public int BalloonsHitThisFlight;
}

/// <summary>
/// Fired when a dart is launched.
/// </summary>
public struct DartLaunchedEvent
{
    public Vector3 LaunchVelocity;
    public float PullStrength;
}

/// <summary>
/// Fired when a dart ricochets off a stuck dart.
/// </summary>
public struct DartRicochetEvent
{
    public Vector3 RicochetPosition;
    public Vector3 IncomingVelocity;
    public Vector3 OutgoingVelocity;
    public int RicochetNumber;
}

/// <summary>
/// Fired when a paint balloon explodes.
/// </summary>
public struct PaintExplosionEvent
{
    public Vector3 WorldPosition;
    public float Radius;
    public BalloonColor SourceColor;
}

/// <summary>
/// Fired when a dart bounces off a side wall.
/// </summary>
public struct WallBounceEvent
{
    public Vector3 Position;
    public Vector3 Velocity;
    public string WallName;
}
