using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Singleton save manager backed by JSON on disk.
/// </summary>
[DisallowMultipleComponent]
public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "inkshot-save.json";
    private static SaveManager _instance;

    private string _savePath;

    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[SaveManager]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<SaveManager>();
            }

            return _instance;
        }
    }

    public SaveData Data { get; private set; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
        Load();
    }

    public void Load()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            Data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }
        else
        {
            Data = new SaveData();
            Save();
        }

        EventBus.Publish(new SaveLoadedEvent { Data = Data });
    }

    public void Save()
    {
        Data ??= new SaveData();
        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(_savePath, json);
        EventBus.Publish(new SaveSavedEvent { Data = Data });
    }

    public void RecordRun(int roomReached, int score, int inkEarned, bool clearedRun)
    {
        Data ??= new SaveData();
        Data.totalRunsCompleted += clearedRun ? 1 : 0;
        Data.highestRoomReached = Mathf.Max(Data.highestRoomReached, roomReached);
        Data.highScore = Mathf.Max(Data.highScore, score);
        Data.runHistory.Insert(0, new RunHistoryRecord
        {
            runId = Guid.NewGuid().ToString("N"),
            finishedAtIsoUtc = DateTime.UtcNow.ToString("o"),
            roomReached = roomReached,
            score = score,
            inkEarned = inkEarned,
            clearedRun = clearedRun,
        });

        while (Data.runHistory.Count > 20)
        {
            Data.runHistory.RemoveAt(Data.runHistory.Count - 1);
        }

        Save();
    }

    public void AddInk(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        Data.totalInk = Mathf.Max(0, Data.totalInk + amount);
        Save();
        EventBus.Publish(new CurrencyChangedEvent { TotalInk = Data.totalInk, Delta = amount });
    }

    public bool SpendInk(int amount)
    {
        if (amount <= 0 || Data.totalInk < amount)
        {
            return false;
        }

        Data.totalInk -= amount;
        Save();
        EventBus.Publish(new CurrencyChangedEvent { TotalInk = Data.totalInk, Delta = -amount });
        return true;
    }

    public void SetVolumes(float master, float sfx, float music, float ui)
    {
        Data.masterVolume = Mathf.Clamp01(master);
        Data.sfxVolume = Mathf.Clamp01(sfx);
        Data.musicVolume = Mathf.Clamp01(music);
        Data.uiVolume = Mathf.Clamp01(ui);
        Save();
    }

    public void SetHapticsEnabled(bool enabled)
    {
        Data.hapticsEnabled = enabled;
        Save();
    }

    public void SetTutorialCompleted()
    {
        Data.hasCompletedTutorial = true;
        Save();
    }
}
