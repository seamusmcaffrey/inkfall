using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public event Action<int> OnScoreChanged;
    public event Action<int> OnBalloonPopped;
    public event Action OnTargetReached;

    public int CurrentScore { get; private set; }
    public int TargetScore { get; private set; }
    public bool IsTargetReached => CurrentScore >= TargetScore;

    public void Initialize(int targetScore)
    {
        CurrentScore = 0;
        TargetScore = targetScore;
        OnScoreChanged?.Invoke(CurrentScore);
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
        int points = GameConstants.SCORE_PER_BALLOON;
        CurrentScore += points;
        OnBalloonPopped?.Invoke(points);
        OnScoreChanged?.Invoke(CurrentScore);
        Debug.Log($"POP! +{points} pts | Score: {CurrentScore}/{TargetScore}");

        if (!reachedBeforePop && IsTargetReached)
        {
            OnTargetReached?.Invoke();
        }
    }
}
