using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StarterLoadoutDefinition
{
    public string loadoutId = "needle-rig";
    public string displayName = "Needle Rig";
    [TextArea(1, 3)] public string description = "Precision opener.";
    [TextArea(1, 2)] public string tradeoff = "Smaller paint bursts.";
    public Color accentColor = Color.white;
    public string requiredMetaUpgradeId;
    public RunModifierBundle modifiers = new();
    public List<string> startingPerkIds = new();
}
