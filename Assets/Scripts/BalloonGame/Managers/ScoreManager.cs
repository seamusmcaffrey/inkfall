using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ComboTracker))]
public class ScoreManager : MonoBehaviour
{
    public event Action<int> OnScoreChanged;
    public event Action<int> OnBalloonPopped;
    public event Action OnTargetReached;

    public int CurrentScore { get; private set; }
    public int TargetScore { get; private set; }
    public bool IsTargetReached => CurrentScore >= TargetScore;

    [SerializeField] private ComboTracker _comboTracker;
    [SerializeField] private PerkManager _perkManager;
    [SerializeField] private RunSystemsManager _runSystemsManager;

    public void Initialize(int targetScore)
    {
        CurrentScore = 0;
        TargetScore = targetScore;
        OnScoreChanged?.Invoke(CurrentScore);
        EventBus.Publish(new ScoreChangedEvent
        {
            CurrentScore = CurrentScore,
            TargetScore = TargetScore,
            PointsJustAdded = 0,
            TargetReached = IsTargetReached,
        });
        _comboTracker?.ResetCombo(true);
    }

    private void Awake()
    {
        _comboTracker = ComponentUtility.EnsureComponent<ComboTracker>(gameObject);
        _perkManager = ComponentUtility.EnsureComponent<PerkManager>(gameObject);
        _runSystemsManager = ComponentUtility.EnsureComponent<RunSystemsManager>(gameObject);
    }

    public void SetPerkManager(PerkManager perkManager)
    {
        _perkManager = perkManager;
    }

    public void SetRunSystemsManager(RunSystemsManager runSystemsManager)
    {
        _runSystemsManager = runSystemsManager;
    }

    private void OnEnable()
    {
        BalloonNode.OnAnyBalloonPopped += HandleBalloonPopped;
    }

    private void OnDisable()
    {
        BalloonNode.OnAnyBalloonPopped -= HandleBalloonPopped;
    }

    private void HandleBalloonPopped(BalloonNode balloon)
    {
        bool reachedBeforePop = IsTargetReached;
        var comboData = _comboTracker != null ? _comboTracker.RegisterPop() : (1, 1f);
        BalloonScoreResult scoreResult = _runSystemsManager != null
            ? _runSystemsManager.CalculateBalloonScore(balloon, comboData.Item1, comboData.Item2)
            : new BalloonScoreResult { FinalPoints = Mathf.RoundToInt(balloon.PointValue * comboData.Item2) };
        int points = scoreResult.FinalPoints;
        CurrentScore = Mathf.Max(0, CurrentScore + points);
        OnBalloonPopped?.Invoke(points);
        OnScoreChanged?.Invoke(CurrentScore);
        _runSystemsManager?.RegisterBalloonResolved(balloon, scoreResult);
        EventBus.Publish(new BalloonScoredEvent
        {
            WorldPosition = balloon.transform.position,
            BalloonColor = balloon.BalloonColor,
            BalloonTypeId = balloon.BalloonTypeId,
            BasePoints = balloon.PointValue,
            ComboCount = comboData.Item1,
            ComboMultiplier = comboData.Item2,
            FinalPoints = points,
        });
        EventBus.Publish(new ScoreChangedEvent
        {
            CurrentScore = CurrentScore,
            TargetScore = TargetScore,
            PointsJustAdded = points,
            TargetReached = IsTargetReached,
        });

        if (!reachedBeforePop && IsTargetReached)
        {
            OnTargetReached?.Invoke();
        }
    }

    public void AddScoreDelta(int delta)
    {
        bool reachedBefore = IsTargetReached;
        CurrentScore = Mathf.Max(0, CurrentScore + delta);
        OnScoreChanged?.Invoke(CurrentScore);
        EventBus.Publish(new ScoreChangedEvent
        {
            CurrentScore = CurrentScore,
            TargetScore = TargetScore,
            PointsJustAdded = delta,
            TargetReached = IsTargetReached,
        });

        if (!reachedBefore && IsTargetReached)
        {
            OnTargetReached?.Invoke();
        }
    }
}
