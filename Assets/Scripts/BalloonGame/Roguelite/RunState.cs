/// <summary>
/// Top-level run states.
/// </summary>
public enum RunState
{
    Idle,
    RoomIntro,
    InRoom,
    PerkDraft,
    RunEnd,
}

/// <summary>
/// Lightweight mutable run state snapshot.
/// </summary>
public class RunData
{
    public int currentRoomNumber;
    public int totalScore;
    public int totalInkEarned;
    public bool isActive;
}
