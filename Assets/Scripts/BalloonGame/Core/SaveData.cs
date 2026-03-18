using System;
using System.Collections.Generic;

/// <summary>
/// Serializable save payload for INKSHOT.
/// </summary>
[Serializable]
public class SaveData
{
    public int version = 2;
    public int highScore;
    public int highestRoomReached;
    public int totalInk;
    public int totalRunsCompleted;
    public float masterVolume = GameConstants.DEFAULT_MASTER_VOLUME;
    public float sfxVolume = GameConstants.DEFAULT_SFX_VOLUME;
    public float musicVolume = GameConstants.DEFAULT_MUSIC_VOLUME;
    public float uiVolume = GameConstants.DEFAULT_UI_VOLUME;
    public bool hapticsEnabled = true;
    public bool hasCompletedTutorial;
    public string selectedLoadoutId = "scatter-string";
    public List<string> unlockedPerkIds = new();
    public List<string> purchasedMetaUpgradeIds = new();
    public List<RunHistoryRecord> runHistory = new();
}

[Serializable]
public class RunHistoryRecord
{
    public string runId;
    public string finishedAtIsoUtc;
    public int roomReached;
    public int score;
    public int inkEarned;
    public bool clearedRun;
}
