using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScoreManager))]
[RequireComponent(typeof(ComboTracker))]
public class BalloonGameManager : MonoBehaviour
{
    public enum GameState
    {
        RoomIntro,
        Ready,
        DartInFlight,
        RoomCleared,
        RoomFailed
    }

    private const float IntroDuration = 1f;

    public GameState CurrentState { get; private set; } = GameState.RoomIntro;
    public int DartsRemaining { get; private set; }
    public RoomConfig CurrentRoomConfig { get; private set; }
    public int CurrentRoomNumber => CurrentRoomConfig != null ? CurrentRoomConfig.roomNumber : 1;

    public event System.Action<RoomConfig> OnRoomStarted;
    public event System.Action<int> OnRoomCleared;
    public event System.Action<int> OnRoomFailed;

    [SerializeField] private BalloonWall _balloonWall = null;
    [SerializeField] private SlingshotInput _slingshotInput = null;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private DartLauncher _dartLauncher;
    [SerializeField] private PerkManager _perkManager;
    [SerializeField] private RoomGenerator _roomGenerator;

    private DartController _activeDart;
    private float _introTimer;

    private void Awake()
    {
        ResolveDependencies();
        _dartLauncher.SetPerkManager(_perkManager);
    }

    private void ResolveDependencies()
    {
        ComponentUtility.ResolveSceneReference(this, ref _balloonWall);
        ComponentUtility.ResolveSceneReference(this, ref _slingshotInput);
        _scoreManager = ComponentUtility.EnsureComponent<ScoreManager>(gameObject);
        _perkManager = ComponentUtility.EnsureComponent<PerkManager>(gameObject);
        _roomGenerator = ComponentUtility.EnsureComponent<RoomGenerator>(gameObject);
        _dartLauncher = ComponentUtility.EnsureComponent<DartLauncher>(gameObject);
    }

    /// <summary>
    /// Injects scene references created by the scene builder.
    /// </summary>
    public void SetDependencies(BalloonWall balloonWall, SlingshotInput slingshotInput, DartLauncher dartLauncher)
    {
        _balloonWall = balloonWall;
        _slingshotInput = slingshotInput;
        _dartLauncher = dartLauncher;
    }

    private void Start()
    {
        ResolveDependencies();
        if (_slingshotInput != null)
        {
            _slingshotInput.OnLaunch += HandleLaunch;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged += HandleScoreChanged;
            _scoreManager.OnTargetReached += HandleTargetReached;
            _scoreManager.SetPerkManager(_perkManager);
        }

        DartController.OnDartFinished += HandleDartFinished;
        BalloonNode.OnAnyBalloonPopped += HandleBalloonPopped;
        if (_roomGenerator == null)
        {
            StartRoom(new RoomConfig
            {
                roomNumber = 1,
                roomName = "Opening Booth",
                rows = GameConstants.BOARD_ROWS,
                columns = GameConstants.BOARD_COLUMNS,
                targetScore = GameConstants.BASE_TARGET_SCORE,
                startingDarts = GameConstants.STARTING_DARTS,
                roomType = RoomType.Normal,
                accentColor = Color.cyan,
            });
        }
    }

    private void OnDestroy()
    {
        if (_slingshotInput != null)
        {
            _slingshotInput.OnLaunch -= HandleLaunch;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged -= HandleScoreChanged;
            _scoreManager.OnTargetReached -= HandleTargetReached;
        }

        DartController.OnDartFinished -= HandleDartFinished;
        BalloonNode.OnAnyBalloonPopped -= HandleBalloonPopped;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.RoomIntro:
                _introTimer -= Time.deltaTime;
                if (_introTimer <= 0f)
                {
                    CurrentState = GameState.Ready;
                    _slingshotInput?.SetCanFire(true);
                    UpdateHud();
                    EventBus.Publish(new GameStateChangedEvent { State = CurrentState });
                }
                break;

            case GameState.RoomCleared:
            case GameState.RoomFailed:
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                {
                    RestartRoom();
                }
                break;
        }
    }

    public void StartRoom(RoomConfig roomConfig)
    {
        ResolveDependencies();
        CurrentRoomConfig = roomConfig;
        CurrentState = GameState.RoomIntro;
        _introTimer = IntroDuration;
        DartsRemaining = roomConfig != null ? roomConfig.startingDarts : GameConstants.STARTING_DARTS;
        _activeDart = null;

        _scoreManager?.Initialize(roomConfig != null ? roomConfig.targetScore : GameConstants.BASE_TARGET_SCORE);
        _balloonWall?.GenerateWall(roomConfig);
        PoolManager.WarmupAll();
        _slingshotInput?.SetCanFire(false);
        UpdateHud();
        OnRoomStarted?.Invoke(roomConfig);
        EventBus.Publish(new RoomStartedEvent
        {
            RoomNumber = CurrentRoomNumber,
            RoomName = roomConfig != null ? roomConfig.roomName : "Opening Booth",
            TargetScore = _scoreManager != null ? _scoreManager.TargetScore : GameConstants.BASE_TARGET_SCORE,
            StartingDarts = DartsRemaining,
        });
        EventBus.Publish(new DartsRemainingChangedEvent
        {
            DartsRemaining = DartsRemaining,
            StartingDarts = DartsRemaining,
        });
    }

    private void RestartRoom()
    {
        _activeDart = null;

        foreach (DartController dart in DartController.GetActiveDarts())
        {
            dart.ForceRecycle();
        }

        if (CurrentRoomConfig != null)
        {
            StartRoom(CurrentRoomConfig);
        }
    }

    private void HandleLaunch(Vector3 velocity)
    {
        if (CurrentState != GameState.Ready || DartsRemaining <= 0)
        {
            return;
        }

        _slingshotInput?.SetCanFire(false);
        _activeDart = _dartLauncher.SpawnAndLaunch(velocity);
        CurrentState = GameState.DartInFlight;
        UpdateHud();
        EventBus.Publish(new GameStateChangedEvent { State = CurrentState });
    }

    private void HandleDartFinished(DartController dart)
    {
        if (dart != _activeDart)
        {
            return;
        }

        _activeDart = null;

        if (CurrentState == GameState.RoomCleared)
        {
            return;
        }

        DartsRemaining--;
        EventBus.Publish(new DartsRemainingChangedEvent
        {
            DartsRemaining = DartsRemaining,
            StartingDarts = CurrentRoomConfig != null ? CurrentRoomConfig.startingDarts : GameConstants.STARTING_DARTS,
        });
        if (DartsRemaining <= 0)
        {
            CurrentState = GameState.RoomFailed;
            _slingshotInput?.SetCanFire(false);
            UpdateHud();
            OnRoomFailed?.Invoke(_scoreManager.CurrentScore);
            EventBus.Publish(new RoomFailedEvent
            {
                RoomNumber = CurrentRoomNumber,
                FinalScore = _scoreManager.CurrentScore,
            });
            return;
        }

        CurrentState = GameState.Ready;
        _slingshotInput?.SetCanFire(true);
        UpdateHud();
        EventBus.Publish(new GameStateChangedEvent { State = CurrentState });
    }

    private void HandleScoreChanged(int score)
    {
        UpdateHud();
    }

    private void HandleTargetReached()
    {
        CurrentState = GameState.RoomCleared;
        _slingshotInput?.SetCanFire(false);
        int inkEarned = Mathf.Max(0, GameConfigSO.Instance.inkPerRoom + _scoreManager.CurrentScore / 1000 * GameConfigSO.Instance.inkPer1000Score);
        SaveManager.Instance.AddInk(inkEarned);
        UpdateHud();
        OnRoomCleared?.Invoke(_scoreManager.CurrentScore);
        EventBus.Publish(new RoomClearedEvent
        {
            RoomNumber = CurrentRoomNumber,
            FinalScore = _scoreManager.CurrentScore,
            InkEarned = inkEarned,
            WasFinalRoom = CurrentRoomNumber >= GameConfigSO.Instance.totalRooms,
        });
    }

    private void UpdateHud()
    {
    }

    private void HandleBalloonPopped(BalloonNode _)
    {
        if (_balloonWall != null && _balloonWall.ActiveCount() <= 0 && CurrentState != GameState.RoomCleared)
        {
            HandleTargetReached();
        }
    }
}
