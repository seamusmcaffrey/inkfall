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
    }

    public void SetPerkManager(PerkManager perkManager)
    {
        _perkManager = perkManager;
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
        float perkScoreMultiplier = _perkManager != null ? _perkManager.ScoreMultiplier : GameConfigSO.Instance.scoreMultiplier;
        int points = Mathf.RoundToInt(balloon.PointValue * comboData.Item2 * Mathf.Max(0.1f, perkScoreMultiplier));
        CurrentScore += points;
        OnBalloonPopped?.Invoke(points);
        OnScoreChanged?.Invoke(CurrentScore);
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
}
