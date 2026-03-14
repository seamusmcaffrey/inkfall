using UnityEngine;

/// <summary>
/// Tracks balloon pop combos across a short timing window.
/// Combos intentionally become most useful once paint chains and pierce are online.
/// </summary>
[DisallowMultipleComponent]
public class ComboTracker : MonoBehaviour
{
    private float _comboTimer;

    public int CurrentCombo { get; private set; }
    public float CurrentMultiplier { get; private set; } = 1f;

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
        }
    }

    public (int comboCount, float multiplier) RegisterPop()
    {
        float comboWindow = GameConfigSO.Instance.comboWindowSeconds;
        if (_comboTimer > 0f)
        {
            CurrentCombo = Mathf.Min(CurrentCombo + 1, GameConfigSO.Instance.comboMaxStack);
        }
        else
        {
            CurrentCombo = 1;
        }

        _comboTimer = comboWindow;
        CurrentMultiplier = 1f + Mathf.Max(0, CurrentCombo - 1) * GameConfigSO.Instance.comboMultiplierPerStep;
        EventBus.Publish(new ComboChangedEvent
        {
            ComboCount = CurrentCombo,
            Multiplier = CurrentMultiplier,
            WasReset = false,
        });

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
        }
    }
}
