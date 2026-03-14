using System;
using UnityEngine;

/// <summary>
/// Fired when the overall room state changes.
/// </summary>
public struct GameStateChangedEvent
{
    public BalloonGameManager.GameState State;
}

/// <summary>
/// Fired when a room starts.
/// </summary>
public struct RoomStartedEvent
{
    public int RoomNumber;
    public string RoomName;
    public int TargetScore;
    public int StartingDarts;
}

/// <summary>
/// Fired when the current room is cleared.
/// </summary>
public struct RoomClearedEvent
{
    public int RoomNumber;
    public int FinalScore;
    public int InkEarned;
    public bool WasFinalRoom;
}

/// <summary>
/// Fired when the player fails the current room.
/// </summary>
public struct RoomFailedEvent
{
    public int RoomNumber;
    public int FinalScore;
}

/// <summary>
/// Fired when the dart count changes.
/// </summary>
public struct DartsRemainingChangedEvent
{
    public int DartsRemaining;
    public int StartingDarts;
}

/// <summary>
/// Fired when score changes.
/// </summary>
public struct ScoreChangedEvent
{
    public int CurrentScore;
    public int TargetScore;
    public int PointsJustAdded;
    public bool TargetReached;
}

/// <summary>
/// Fired when currency changes.
/// </summary>
public struct CurrencyChangedEvent
{
    public int TotalInk;
    public int Delta;
}

/// <summary>
/// Fired after save data is loaded.
/// </summary>
public struct SaveLoadedEvent
{
    public SaveData Data;
}

/// <summary>
/// Fired after save data is written.
/// </summary>
public struct SaveSavedEvent
{
    public SaveData Data;
}

/// <summary>
/// Fired when a perk is selected.
/// </summary>
public struct PerkSelectedEvent
{
    public PerkSO Perk;
}

/// <summary>
/// Fired when a run state changes.
/// </summary>
public struct RunStateChangedEvent
{
    public RunState State;
    public int RoomNumber;
}

/// <summary>
/// Fired when a transient UI message should be shown.
/// </summary>
public struct HudMessageEvent
{
    public string Message;
    public Color Color;
    public float Duration;
}

/// <summary>
/// Fired when a sound should be played.
/// </summary>
public struct AudioCueEvent
{
    public AudioClip Clip;
    public float Volume;
    public bool UseUiChannel;
}

/// <summary>
/// Fired when a simple haptic pulse should be played.
/// </summary>
public struct HapticEvent
{
    public Haptics.HapticType Type;
}
