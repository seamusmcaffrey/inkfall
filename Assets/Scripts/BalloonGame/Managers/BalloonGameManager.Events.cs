using UnityEngine;

public partial class BalloonGameManager
{
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

    private void HandleBalloonPopped(BalloonNode _)
    {
        if (_balloonWall != null && _balloonWall.ActiveCount() <= 0 && CurrentState != GameState.RoomCleared)
        {
            HandleTargetReached();
        }
    }
}
