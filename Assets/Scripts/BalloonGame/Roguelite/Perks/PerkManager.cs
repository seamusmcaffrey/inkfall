using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks active perks and exposes their aggregate gameplay modifiers.
/// </summary>
[DisallowMultipleComponent]
public class PerkManager : MonoBehaviour
{
    private readonly List<PerkSO> _activePerks = new();

    public IReadOnlyList<PerkSO> ActivePerks => _activePerks;
    public float LaunchSpeedMultiplier { get; private set; } = 1f;
    public int AdditionalPierce { get; private set; }
    public int AdditionalRicochet { get; private set; }
    public int AdditionalDarts { get; private set; }
    public float ComboWindowBonus { get; private set; }
    public float ComboBonusPoints { get; private set; }
    public float PaintRadiusMultiplier { get; private set; } = 1f;
    public float ScoreMultiplier { get; private set; } = 1f;
    public float GoldMultiplier { get; private set; } = 1f;
    public bool HazardShieldArmed { get; private set; }

    private void OnEnable()
    {
        EventBus.Subscribe<PerkSelectedEvent>(HandlePerkSelected);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PerkSelectedEvent>(HandlePerkSelected);
    }

    public void ResetRun()
    {
        _activePerks.Clear();
        Recalculate();
    }

    public void AddPerk(PerkSO perk)
    {
        if (perk == null)
        {
            return;
        }

        _activePerks.Add(perk);
        Recalculate();
    }

    public bool ConsumeHazardShield()
    {
        if (!HazardShieldArmed)
        {
            return false;
        }

        HazardShieldArmed = false;
        return true;
    }

    private void HandlePerkSelected(PerkSelectedEvent evt)
    {
        AddPerk(evt.Perk);
    }

    private void Recalculate()
    {
        LaunchSpeedMultiplier = 1f;
        AdditionalPierce = 0;
        AdditionalRicochet = 0;
        AdditionalDarts = 0;
        ComboWindowBonus = 0f;
        ComboBonusPoints = 0f;
        PaintRadiusMultiplier = 1f;
        ScoreMultiplier = 1f;
        GoldMultiplier = 1f;
        HazardShieldArmed = false;

        foreach (PerkSO perk in _activePerks)
        {
            switch (perk.effectType)
            {
                case PerkEffectType.DartSpeed:
                    LaunchSpeedMultiplier += perk.effectValue;
                    break;

                case PerkEffectType.DartPierce:
                    AdditionalPierce += Mathf.Max(1, perk.effectIntValue);
                    break;

                case PerkEffectType.Ricochet:
                    AdditionalRicochet += Mathf.Max(1, perk.effectIntValue);
                    break;

                case PerkEffectType.ExtraDart:
                    AdditionalDarts += Mathf.Max(1, perk.effectIntValue);
                    break;

                case PerkEffectType.ComboExtend:
                    ComboWindowBonus += perk.effectValue;
                    break;

                case PerkEffectType.ComboBonus:
                    ComboBonusPoints += perk.effectValue;
                    break;

                case PerkEffectType.PaintRadius:
                    PaintRadiusMultiplier += perk.effectValue;
                    break;

                case PerkEffectType.ScoreMultiplier:
                    ScoreMultiplier += perk.effectValue;
                    break;

                case PerkEffectType.GoldMultiplier:
                    GoldMultiplier += perk.effectValue;
                    break;

                case PerkEffectType.HazardShield:
                    HazardShieldArmed = true;
                    break;
            }
        }
    }
}
