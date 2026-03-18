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
        _runSystemsManager?.BeginShot();
        _activeDart = _dartLauncher.SpawnAndLaunch(velocity);
        CurrentState = GameState.DartInFlight;
        UpdateHud();
        EventBus.Publish(new GameStateChangedEvent { State = CurrentState });
        EventBus.Publish(new DartLaunchedEvent { LaunchVelocity = velocity, PullStrength = 0f });
    }

    private void HandleDartFinished(DartController dart)
    {
        if (dart != _activeDart)
        {
            return;
        }

        _activeDart = null;
        _runSystemsManager?.ResolveShotEnd(_scoreManager);

        DartsRemaining--;
        if (dart.BalloonsHitThisFlight == 0 &&
            (dart.LastStopReason == "timeout" || dart.LastStopReason == "out_of_bounds") &&
            _perkManager != null &&
            (_perkManager.MissesReturnInsteadOfStick || Random.value < _perkManager.MissReturnChanceBonus))
        {
            DartsRemaining++;
        }

        EventBus.Publish(new DartsRemainingChangedEvent
        {
            DartsRemaining = DartsRemaining,
            StartingDarts = CurrentRoomConfig != null ? CurrentRoomConfig.startingDarts : GameConstants.STARTING_DARTS,
        });

        if (_scoreManager.IsTargetReached)
        {
            BankAndClearRoom();
            return;
        }

        if (_balloonWall != null && _balloonWall.ActiveCount() <= 0)
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
        UpdateHud();
    }

    private void HandleWallBounce(DartController dart, Collision _)
    {
        if (dart == _activeDart)
        {
            _runSystemsManager?.RegisterBounce();
        }
    }

    private void HandleBalloonPopped(BalloonNode _)
    {
        if (_balloonWall != null && _balloonWall.ActiveCount() <= 0 && CurrentState != GameState.RoomCleared)
        {
            HandleTargetReached();
        }
    }
}
