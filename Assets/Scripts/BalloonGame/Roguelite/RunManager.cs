using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RoomGenerator))]
[RequireComponent(typeof(PerkManager))]
[RequireComponent(typeof(RunSystemsManager))]
public partial class RunManager : MonoBehaviour
{
    [SerializeField] private BalloonGameManager _balloonGameManager = null;
    [SerializeField] private RoomGenerator _roomGenerator;
    [SerializeField] private PerkManager _perkManager;
    [SerializeField] private RunSystemsManager _runSystemsManager;
    [SerializeField] private PerkSelectionScreen _perkSelectionScreen = null;
    [SerializeField] private RunChoiceScreen _choiceScreen = null;
    [SerializeField] private RoomIntroScreen _roomIntroScreen = null;
    [SerializeField] private RunEndScreen _runEndScreen = null;
    [SerializeField] private RunHUD _runHud;
    [SerializeField] private FadeOverlay _fadeOverlay = null;
    [SerializeField] private bool _autoStart = false;

    private readonly RunData _runData = new();

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
        if (_autoStart || GameConstants.DEV_SKIP_INTRO)
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

        _runData.currentRoomNumber = 1;
        _runData.totalInkEarned = 0;
        _runData.totalScore = 0;
        _runData.isActive = true;
        _runEndScreen?.Hide();
        _runSystemsManager.InitializeRun(_runSystemsManager.ResolveSelectedLoadout());
        EnterRoom(_runData.currentRoomNumber);
    }

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

    public void OpenLoadoutSelection()
    {
        if (CurrentState == RunState.Idle)
        {
            StartCoroutine(PromptLoadoutSelection());
        }
    }

    private void EnterRoom(int roomNumber)
    {
        ResolveDependencies();
        CurrentState = RunState.RoomIntro;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = roomNumber });
        RoomConfig room = _roomGenerator.Generate(roomNumber);
        room.startingDarts = Mathf.Max(1, room.startingDarts + _perkManager.AdditionalDarts);
        if (_roomIntroScreen != null && !GameConstants.DEV_SKIP_INTRO)
        {
            _roomIntroScreen.Show(room);
        }

        StartCoroutine(BeginRoomAfterIntro(room));
    }

    private IEnumerator BeginRoomAfterIntro(RoomConfig room)
    {
        if (!GameConstants.DEV_SKIP_INTRO)
        {
            if (_fadeOverlay != null)
            {
                yield return _fadeOverlay.FadeTo(0f, 0.2f);
            }

            yield return new WaitForSecondsRealtime(0.8f);
        }

        CurrentState = RunState.InRoom;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = room.roomNumber });
        _balloonGameManager.StartRoom(room);
    }

    private void HandleRoomCleared(int finalScore)
    {
        _runData.totalScore += finalScore;
        _runData.totalInkEarned = _runSystemsManager.PrizeTickets;
        if (_runData.currentRoomNumber >= GameConfigSO.Instance.totalRooms)
        {
            EndRun(true, finalScore);
            return;
        }

        StartCoroutine(BetweenRoomFlow());
    }

    private void HandleRoomFailed(int finalScore)
    {
        _runData.totalScore += finalScore;
        EndRun(false, finalScore);
    }

    private void EndRun(bool clearedRun, int latestRoomScore)
    {
        CurrentState = RunState.RunEnd;
        EventBus.Publish(new RunStateChangedEvent { State = CurrentState, RoomNumber = _runData.currentRoomNumber });
        int ticketsEarned = _runSystemsManager.PrizeTickets;
        _runSystemsManager.CommitRunRewards();
        SaveManager.Instance.RecordRun(_runData.currentRoomNumber, _runData.totalScore, ticketsEarned, clearedRun);
        _runData.isActive = false;
        _runEndScreen?.Show(new RunEndData
        {
            cleared = clearedRun,
            roomsReached = _runData.currentRoomNumber,
            totalScore = _runData.totalScore,
            lastRoomScore = latestRoomScore,
            totalInk = SaveManager.Instance.Data.totalInk,
        }, RestartRun);
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
        _runSystemsManager = ComponentUtility.EnsureComponent<RunSystemsManager>(gameObject);
        ComponentUtility.ResolveSceneReference(this, ref _balloonGameManager);
        ComponentUtility.ResolveSceneReference(this, ref _perkSelectionScreen);
        ComponentUtility.ResolveSceneReference(this, ref _choiceScreen);
        ComponentUtility.ResolveSceneReference(this, ref _roomIntroScreen);
        ComponentUtility.ResolveSceneReference(this, ref _runEndScreen);
        ComponentUtility.ResolveSceneReference(this, ref _runHud);
        ComponentUtility.ResolveSceneReference(this, ref _fadeOverlay);
        if (_choiceScreen == null)
        {
            _choiceScreen = new GameObject("RunChoiceScreen").AddComponent<RunChoiceScreen>();
        }
    }

    private void AwardPerk(PerkSO perk)
    {
        _perkManager.AddPerk(perk);
        EventBus.Publish(new PerkSelectedEvent { Perk = perk });
    }

    private static PerkSO CreateRuntimePerk(string id, string name, string description, PerkEffectType type, float effectValue, int effectIntValue)
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
