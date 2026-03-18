using UnityEngine;

public partial class PerkManager
{
    private void Recalculate()
    {
        ResetBundle();
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
        _colorFocusBonuses.Clear();
        _stickerFocusBonuses.Clear();

        ApplyLoadout(_starterLoadout);
        foreach (MetaUpgradeSO upgrade in _activeMetaUpgrades)
        {
            if (upgrade != null)
            {
                _bundle.Add(upgrade.modifiers);
            }
        }

        foreach (PerkSO perk in _activePerks)
        {
            ApplyPerk(perk);
        }

        foreach (PerkSO relic in _activeRelics)
        {
            ApplyPerk(relic);
        }

        LaunchSpeedMultiplier += _bundle.launchSpeedBonus;
        AdditionalPierce += _bundle.extraPierce;
        AdditionalRicochet += _bundle.extraRicochets;
        AdditionalDarts += _bundle.extraDarts;
        PaintRadiusMultiplier += _bundle.paintRadiusBonus;
        ScoreMultiplier += _bundle.scoreMultiplierBonus;
        GoldMultiplier += _bundle.goldMultiplierBonus;
    }

    public float GetColorFocusBonus(BalloonColor color)
    {
        return _colorFocusBonuses.TryGetValue(color, out float bonus) ? bonus : 0f;
    }

    public float GetStickerFocusBonus(StickerFamily family)
    {
        return _stickerFocusBonuses.TryGetValue(family, out float bonus) ? bonus : 0f;
    }

    private void ApplyLoadout(StarterLoadoutDefinition loadout)
    {
        if (loadout == null)
        {
            return;
        }

        _bundle.Add(loadout.modifiers);
        foreach (string perkId in loadout.startingPerkIds)
        {
            PerkSO perk = InkshotContentCatalog.GetPerkById(perkId);
            if (perk != null)
            {
                ApplyPerk(perk);
            }
        }
    }

    private void ApplyPerk(PerkSO perk)
    {
        if (perk == null)
        {
            return;
        }

        if (perk.hasColorFocus)
        {
            _colorFocusBonuses.TryGetValue(perk.colorFocus, out float currentColorBonus);
            _colorFocusBonuses[perk.colorFocus] = currentColorBonus + perk.modifiers.colorFocusBonus;
        }

        if (perk.hasStickerFocus)
        {
            _stickerFocusBonuses.TryGetValue(perk.stickerFocus, out float currentStickerBonus);
            _stickerFocusBonuses[perk.stickerFocus] = currentStickerBonus + perk.modifiers.stickerFocusBonus;
        }

        _bundle.Add(perk.modifiers);
        switch (perk.effectType)
        {
            case PerkEffectType.DartSpeed: LaunchSpeedMultiplier += perk.effectValue; break;
            case PerkEffectType.DartPierce: AdditionalPierce += Mathf.Max(1, perk.effectIntValue); break;
            case PerkEffectType.Ricochet: AdditionalRicochet += Mathf.Max(1, perk.effectIntValue); break;
            case PerkEffectType.ExtraDart: AdditionalDarts += perk.effectIntValue != 0 ? perk.effectIntValue : 1; break;
            case PerkEffectType.ComboExtend: ComboWindowBonus += perk.effectValue; break;
            case PerkEffectType.ComboBonus: ComboBonusPoints += perk.effectValue; break;
            case PerkEffectType.PaintRadius: PaintRadiusMultiplier += perk.effectValue; break;
            case PerkEffectType.ScoreMultiplier: ScoreMultiplier += perk.effectValue; break;
            case PerkEffectType.GoldMultiplier: GoldMultiplier += perk.effectValue; break;
            case PerkEffectType.HazardShield: HazardShieldArmed = true; break;
        }
    }

}
