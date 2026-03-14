/// <summary>
/// Abstract perk effect definition used for gameplay modifier wiring.
/// </summary>
public abstract class PerkEffect
{
    public abstract PerkEffectType Type { get; }

    public virtual void Apply(PerkManager manager, PerkSO perk)
    {
        manager.AddPerk(perk);
    }
}
