using UnityEngine;
using UnityEngine.InputSystem;

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

    private BalloonWall _balloonWall;
    private SlingshotInput _slingshotInput;
    private ScoreManager _scoreManager;
    private GameHUD _hud;
    private DartLauncher _dartLauncher;
    private DartController _activeDart;
    private float _introTimer;

    private void Awake()
    {
        _balloonWall = FindAnyObjectByType<BalloonWall>();
        _slingshotInput = FindAnyObjectByType<SlingshotInput>();
        _scoreManager = GetComponent<ScoreManager>();
        _hud = GetComponent<GameHUD>();
        ConfigureSceneCollisionLayers();

        var launcherObject = new GameObject("DartLauncher");
        launcherObject.transform.SetParent(transform, false);
        _dartLauncher = launcherObject.AddComponent<DartLauncher>();
    }

    private void Start()
    {
        if (_slingshotInput != null)
        {
            _slingshotInput.OnLaunch += HandleLaunch;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged += HandleScoreChanged;
            _scoreManager.OnTargetReached += HandleTargetReached;
        }

        DartController.OnDartFinished += HandleDartFinished;
        StartRoom();
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

    private void StartRoom()
    {
        CurrentState = GameState.RoomIntro;
        _introTimer = IntroDuration;
        DartsRemaining = GameConstants.STARTING_DARTS;
        _activeDart = null;

        _scoreManager?.Initialize(GameConstants.BASE_TARGET_SCORE);
        _balloonWall?.GenerateWall();
        _slingshotInput?.SetCanFire(false);
        UpdateHud();
    }

    private void RestartRoom()
    {
        _activeDart = null;

        foreach (DartController dart in FindObjectsByType<DartController>(FindObjectsSortMode.None))
        {
            Destroy(dart.gameObject);
        }

        StartRoom();
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
        if (DartsRemaining <= 0)
        {
            CurrentState = GameState.RoomFailed;
            _slingshotInput?.SetCanFire(false);
            UpdateHud();
            Debug.Log($"ROOM FAILED. Score: {_scoreManager.CurrentScore}/{_scoreManager.TargetScore}");
            return;
        }

        CurrentState = GameState.Ready;
        _slingshotInput?.SetCanFire(true);
        UpdateHud();
    }

    private void HandleScoreChanged(int score)
    {
        UpdateHud();
    }

    private void HandleTargetReached()
    {
        CurrentState = GameState.RoomCleared;
        _slingshotInput?.SetCanFire(false);
        UpdateHud();
        Debug.Log($"ROOM CLEARED! Score: {_scoreManager.CurrentScore}/{_scoreManager.TargetScore}");
    }

    private void UpdateHud()
    {
        _hud?.UpdateDisplay(
            _scoreManager != null ? _scoreManager.CurrentScore : 0,
            _scoreManager != null ? _scoreManager.TargetScore : GameConstants.BASE_TARGET_SCORE,
            DartsRemaining,
            CurrentState);
    }

    private static void ConfigureSceneCollisionLayers()
    {
        SetLayerIfFound("BackWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("LeftWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("RightWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("TopWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("LaneFloor", GameConstants.LAYER_ENVIRONMENT);
    }

    private static void SetLayerIfFound(string objectName, int layer)
    {
        GameObject sceneObject = GameObject.Find(objectName);
        if (sceneObject != null)
        {
            sceneObject.layer = layer;
        }
    }
}
