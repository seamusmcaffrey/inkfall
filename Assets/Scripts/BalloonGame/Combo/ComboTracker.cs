using UnityEngine;

/// <summary>
/// Tracks balloon pop combos across a short timing window.
/// Combos intentionally become most useful once paint chains and pierce are online.
/// </summary>
[DisallowMultipleComponent]
public class ComboTracker : MonoBehaviour
{
    private float _comboTimer;
    [SerializeField] private PerkManager _perkManager;

    public int CurrentCombo { get; private set; }
    public float CurrentMultiplier { get; private set; } = 1f;

    private void Awake()
    {
        _perkManager = ComponentUtility.EnsureComponent<PerkManager>(gameObject);
    }

    private void Update()
    {
        if (CurrentCombo <= 0)
        {
            return;
        }

        _comboTimer -= Time.deltaTime;
        if (_comboTimer <= 0f)
        {
            ResetCombo(true);
            return;
        }

        PublishWindowState(true);
    }

    public (int comboCount, float multiplier) RegisterPop()
    {
        float comboWindow = GetComboWindowSeconds();
        if (_comboTimer > 0f)
        {
            CurrentCombo = Mathf.Min(CurrentCombo + 1, GameConfigSO.Instance.comboMaxStack);
        }
        else
        {
            CurrentCombo = 1;
        }

        _comboTimer = comboWindow;
        CurrentMultiplier = RunScoreMath.EvaluateComboMultiplier(CurrentCombo);
        EventBus.Publish(new ComboChangedEvent
        {
            ComboCount = CurrentCombo,
            Multiplier = CurrentMultiplier,
            WasReset = false,
        });
        PublishWindowState(true);

        return (CurrentCombo, CurrentMultiplier);
    }

    public void ResetCombo(bool publishEvent)
    {
        if (CurrentCombo == 0 && !publishEvent)
        {
            return;
        }

        CurrentCombo = 0;
        CurrentMultiplier = 1f;
        _comboTimer = 0f;

        if (publishEvent)
        {
            EventBus.Publish(new ComboChangedEvent
            {
                ComboCount = 0,
                Multiplier = 1f,
                WasReset = true,
            });
            PublishWindowState(false);
        }
    }

    private void PublishWindowState(bool isActive)
    {
        float maxSeconds = GetComboWindowSeconds();
        float remainingSeconds = isActive ? Mathf.Max(_comboTimer, 0f) : 0f;
        float normalizedRemaining = maxSeconds > 0f
            ? Mathf.Clamp01(remainingSeconds / maxSeconds)
            : 0f;

        EventBus.Publish(new ComboWindowStateEvent
        {
            ComboCount = CurrentCombo,
            RemainingSeconds = remainingSeconds,
            MaxSeconds = maxSeconds,
            NormalizedRemaining = normalizedRemaining,
            IsActive = isActive,
        });
    }

    private float GetComboWindowSeconds()
    {
        float bonus = _perkManager != null ? _perkManager.ComboWindowBonus : 0f;
        return Mathf.Max(0.2f, GameConfigSO.Instance.comboWindowSeconds + bonus);
    }
}
