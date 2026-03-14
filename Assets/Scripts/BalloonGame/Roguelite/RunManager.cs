using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Top-level run orchestrator.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RoomGenerator))]
[RequireComponent(typeof(PerkManager))]
public class RunManager : MonoBehaviour
{
    [SerializeField] private BalloonGameManager _balloonGameManager = null;
    [SerializeField] private RoomGenerator _roomGenerator;
    [SerializeField] private PerkManager _perkManager;
    [SerializeField] private PerkSelectionScreen _perkSelectionScreen = null;
    [SerializeField] private RoomIntroScreen _roomIntroScreen = null;
    [SerializeField] private RunEndScreen _runEndScreen = null;
    [SerializeField] private RunHUD _runHud;
    [SerializeField] private FadeOverlay _fadeOverlay = null;
    [SerializeField] private bool _autoStart = false;

    private readonly RunData _runData = new();
    private List<PerkSO> _fallbackPerks;

    public RunState CurrentState { get; private set; } = RunState.Idle;

    private void Awake()
    {
        ResolveDependencies();
    }

    private void OnEnable()
    {
        ResolveDependencies();
        if (_balloonGameManager != null)
        {
            _balloonGameManager.OnRoomCleared += HandleRoomCleared;
            _balloonGameManager.OnRoomFailed += HandleRoomFailed;
        }
    }

    private void Start()
    {
        if (_autoStart)
        {
            StartNewRun();
        }
    }

    private void OnDisable()
    {
        if (_balloonGameManager != null)
        {
            _balloonGameManager.OnRoomCleared -= HandleRoomCleared;
            _balloonGameManager.OnRoomFailed -= HandleRoomFailed;
        }
    }

    public void StartNewRun()
    {
        ResolveDependencies();
        if (CurrentState != RunState.Idle)
        {
            return;
        }

        if (_balloonGameManager == null)
        {
            Debug.LogError("RunManager could not find BalloonGameManager in the active scene.");
            return;
        }

        _runData.currentRoomNumber = 1;
        _runData.totalInkEarned = 0;
        _runData.totalScore = 0;
        _runData.isActive = true;
        _perkManager.ResetRun();
        if (_runEndScreen != null)
        {
            _runEndScreen.Hide();
        }

        EnterRoom(_runData.currentRoomNumber);
    }

    /// <summary>
    /// Injects scene references created by the scene builder.
    /// </summary>
    public void SetDependencies(
        BalloonGameManager balloonGameManager,
        PerkSelectionScreen perkSelectionScreen,
        RoomIntroScreen roomIntroScreen,
        RunEndScreen runEndScreen,
        RunHUD runHud,
        FadeOverlay fadeOverlay)
    {
        _balloonGameManager = balloonGameManager;
        _perkSelectionScreen = perkSelectionScreen;
        _roomIntroScreen = roomIntroScreen;
        _runEndScreen = runEndScreen;
        _runHud = runHud;
        _fadeOverlay = fadeOverlay;
    }

    private void EnterRoom(int roomNumber)
    {
        ResolveDependencies();
        CurrentState = RunState.RoomIntro;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = roomNumber });
        RoomConfig room = _roomGenerator.Generate(roomNumber);
        room.startingDarts += _perkManager.AdditionalDarts;
        if (_roomIntroScreen != null)
        {
            _roomIntroScreen.Show(room);
        }

        StartCoroutine(BeginRoomAfterIntro(room));
    }

    private IEnumerator BeginRoomAfterIntro(RoomConfig room)
    {
        if (_fadeOverlay != null)
        {
            yield return _fadeOverlay.FadeTo(0f, 0.2f);
        }

        yield return new WaitForSecondsRealtime(0.8f);
        CurrentState = RunState.InRoom;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = room.roomNumber });
        _balloonGameManager.StartRoom(room);
    }

    private void HandleRoomCleared(int finalScore)
    {
        _runData.totalScore += finalScore;
        _runData.totalInkEarned = SaveManager.Instance.Data.totalInk;
        if (_runData.currentRoomNumber >= GameConfigSO.Instance.totalRooms)
        {
            EndRun(true, finalScore);
            return;
        }

        StartCoroutine(DraftThenContinue());
    }

    private void HandleRoomFailed(int finalScore)
    {
        _runData.totalScore += finalScore;
        EndRun(false, finalScore);
    }

    private IEnumerator DraftThenContinue()
    {
        CurrentState = RunState.PerkDraft;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = _runData.currentRoomNumber });
        List<PerkSO> choices = GetPerkChoices();

        if (_perkSelectionScreen == null || choices.Count == 0)
        {
            if (choices.Count > 0)
            {
                _perkManager.AddPerk(choices[0]);
                EventBus.Publish(new PerkSelectedEvent { Perk = choices[0] });
            }
        }
        else
        {
            bool selectionMade = false;
            _perkSelectionScreen.Show(choices, perk =>
            {
                _perkManager.AddPerk(perk);
                selectionMade = true;
            });

            while (!selectionMade)
            {
                yield return null;
            }
        }

        _runData.currentRoomNumber++;
        EnterRoom(_runData.currentRoomNumber);
    }

    private void EndRun(bool clearedRun, int latestRoomScore)
    {
        CurrentState = RunState.RunEnd;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = _runData.currentRoomNumber });
        SaveManager.Instance.RecordRun(_runData.currentRoomNumber, _runData.totalScore, _runData.totalInkEarned, clearedRun);
        _runData.isActive = false;
        string summary = clearedRun
            ? $"RUN CLEARED\nRooms: {_runData.currentRoomNumber}\nScore: {_runData.totalScore}\nInk: {SaveManager.Instance.Data.totalInk}"
            : $"RUN OVER\nRoom: {_runData.currentRoomNumber}\nScore: {_runData.totalScore}\nLast Room Score: {latestRoomScore}";
        _runEndScreen?.Show(summary, RestartRun);
    }

    private void RestartRun()
    {
        CurrentState = RunState.Idle;
        StartNewRun();
    }

    private void ResolveDependencies()
    {
        _roomGenerator = ComponentUtility.EnsureComponent<RoomGenerator>(gameObject);
        _perkManager = ComponentUtility.EnsureComponent<PerkManager>(gameObject);
        ComponentUtility.ResolveSceneReference(this, ref _balloonGameManager);
        ComponentUtility.ResolveSceneReference(this, ref _perkSelectionScreen);
        ComponentUtility.ResolveSceneReference(this, ref _roomIntroScreen);
        ComponentUtility.ResolveSceneReference(this, ref _runEndScreen);
        ComponentUtility.ResolveSceneReference(this, ref _runHud);
        ComponentUtility.ResolveSceneReference(this, ref _fadeOverlay);
    }

    private List<PerkSO> GetPerkChoices()
    {
        List<PerkSO> pool = GameConfigSO.Instance.perkPool;
        if (pool == null || pool.Count == 0)
        {
            pool = GetFallbackPerks();
        }

        int count = Mathf.Min(GameConfigSO.Instance.perkChoicesPerDraft, pool.Count);
        var choices = new List<PerkSO>(count);
        for (int index = 0; index < count; index++)
        {
            choices.Add(pool[(index + _runData.currentRoomNumber) % pool.Count]);
        }

        return choices;
    }

    private List<PerkSO> GetFallbackPerks()
    {
        if (_fallbackPerks != null)
        {
            return _fallbackPerks;
        }

        _fallbackPerks = new List<PerkSO>();
        _fallbackPerks.Add(CreatePerk("swift-hands", "Swift Hands", "Launch darts faster.", PerkEffectType.DartSpeed, 0.15f, 0));
        _fallbackPerks.Add(CreatePerk("needle-thread", "Needle Thread", "+1 dart pierce.", PerkEffectType.DartPierce, 0f, 1));
        _fallbackPerks.Add(CreatePerk("wet-wall", "Wet Wall", "Paint explosions reach farther.", PerkEffectType.PaintRadius, 0.35f, 0));
        _fallbackPerks.Add(CreatePerk("double-down", "Double Down", "Score multiplier up.", PerkEffectType.ScoreMultiplier, 0.2f, 0));
        _fallbackPerks.Add(CreatePerk("insurance", "Insurance", "Gain a hazard shield each room.", PerkEffectType.HazardShield, 1f, 0));
        return _fallbackPerks;
    }

    private static PerkSO CreatePerk(string id, string name, string description, PerkEffectType type, float effectValue, int effectIntValue)
    {
        PerkSO perk = ScriptableObject.CreateInstance<PerkSO>();
        perk.perkId = id;
        perk.perkName = name;
        perk.description = description;
        perk.effectType = type;
        perk.effectValue = effectValue;
        perk.effectIntValue = effectIntValue;
        return perk;
    }
}
