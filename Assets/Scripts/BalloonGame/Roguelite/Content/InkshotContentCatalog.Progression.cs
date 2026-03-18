using UnityEngine;

public static partial class InkshotContentCatalog
{
    private static void BuildLoadouts()
    {
        CreateLoadout("scatter-string", "Static String", "Starts wide and reactive with extra mini-hits.", "Less focused bursts, more chain reactions.", UIColors.RoomPink, loadout =>
        {
            loadout.modifiers.miniHitCount = 1;
            loadout.modifiers.adjacentBurstCount = 1;
        });
        CreateLoadout("needle-rig", "Relay Rig", "Starts with pierce and cleaner bounce lanes.", "Paint bursts are smaller.", UIColors.ComboGold, loadout =>
        {
            loadout.modifiers.extraPierce = 1;
            loadout.modifiers.launchSpeedBonus = 0.08f;
            loadout.modifiers.paintRadiusBonus = -0.15f;
        });
        CreateLoadout("heavy-reel", "Flood Reel", "Starts chunkier with better overflow conversion.", "Launch speed slows down and finesse drops.", UIColors.HazardRed, loadout =>
        {
            loadout.requiredMetaUpgradeId = "heavy-reel-license";
            loadout.modifiers.scoreFlatBonus = 40;
            loadout.modifiers.launchSpeedBonus = -0.08f;
            loadout.modifiers.overflowTicketRateBonus = 0.1f;
        });
        CreateLoadout("trickline", "Stormline", "Starts with a ricochet and better anchor geometry.", "Raw score is lower until the build comes online.", UIColors.DartBlue, loadout =>
        {
            loadout.modifiers.extraRicochets = 1;
            loadout.modifiers.anchorRadiusBonus = 0.2f;
            loadout.modifiers.scoreMultiplierBonus = -0.05f;
        });
        CreateLoadout("bankers-draw", "Glimmer Draw", "Starts richer and converts overflow faster.", "Paint radius is tighter.", UIColors.ClearedGreen, loadout =>
        {
            loadout.modifiers.startingTickets = 4;
            loadout.modifiers.overflowTicketRateBonus = 0.2f;
            loadout.modifiers.paintRadiusBonus = -0.15f;
        });
        CreateLoadout("prism-kit", "Prism Kit", "Starts recipes and chaos balloons earlier.", "You enter the run with one fewer dart.", UIColors.InkCyan, loadout =>
        {
            loadout.requiredMetaUpgradeId = "prism-license";
            loadout.modifiers.chaosSpawnBonus = 0.08f;
            loadout.modifiers.recipeUnlockRoomDelta = -1;
            loadout.modifiers.extraDarts = -1;
        });
    }
}
