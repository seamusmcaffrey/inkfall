public static partial class InkshotContentCatalog
{
    private static void BuildPerks()
    {
        BuildThrowAndPaintPerks();
        BuildControlAndEconomyPerks();
        BuildFutureAndRecoveryPerks();
    }

    private static void BuildRecipes()
    {
        CreateRecipe("afterburner", "Afterburner", "Red then blue bursts into nearby ricochet pops.", BalloonColor.Red, BalloonColor.Blue, PaintRecipeEffectType.AdjacentBurst, 2);
        CreateRecipe("jackpot-bank", "Relay Bank", "Blue then yellow turns angle play into tickets.", BalloonColor.Blue, BalloonColor.Yellow, PaintRecipeEffectType.TicketBurst, 2);
        CreateRecipe("safety-dividend", "Safety Dividend", "Yellow then green banks safer overflow.", BalloonColor.Yellow, BalloonColor.Green, PaintRecipeEffectType.SafetyDividend, 2);
        CreateRecipe("clean-splice", "Clean Splice", "Green then purple clones a cleaned-up pop.", BalloonColor.Green, BalloonColor.Purple, PaintRecipeEffectType.CloneBurst, 1);
        CreateRecipe("hexburst", "Hexburst", "Purple then red spikes score and chaos at once.", BalloonColor.Purple, BalloonColor.Red, PaintRecipeEffectType.ScoreSpike, 1);
        CreateRecipe("hot-streak", "Hot Streak", "Red then yellow ignites a jackpot finisher shove.", BalloonColor.Red, BalloonColor.Yellow, PaintRecipeEffectType.ScoreSpike, 2);
        CreateRecipe("anchor-wash", "Anchor Wash", "Blue then green pulses the nearest anchor.", BalloonColor.Blue, BalloonColor.Green, PaintRecipeEffectType.AnchorPulse, 2);
        CreateRecipe("gilded-glitch", "Glimmer Glitch", "Yellow then purple twists jackpots toward weirdness.", BalloonColor.Yellow, BalloonColor.Purple, PaintRecipeEffectType.JackpotShift, 2);
        CreateRecipe("primer-bomb", "Primer Bomb", "Green then red primes a larger adjacent burst.", BalloonColor.Green, BalloonColor.Red, PaintRecipeEffectType.PrimerMark, 2);
        CreateRecipe("mirror-echo", "Mirror Echo", "Purple then blue mirrors the last bounce into more pops.", BalloonColor.Purple, BalloonColor.Blue, PaintRecipeEffectType.RicochetEcho, 2);
    }

    private static void BuildFinishers()
    {
        CreateFinisher("ink-wave", "Ink Wave", "Flood a cluster with paint.", FinisherEffectType.RadialPop, 5, 500, 1, 1, 1f, 2, 4, 2.2f);
        CreateFinisher("dart-rain", "Dart Rain", "Rain extra hits across the wall.", FinisherEffectType.DartRain, 6, 650, 1, 0, 1.2f, 3, 4, 2f);
        CreateFinisher("radial-spray", "Radial Spray", "Explode outward from the best pop.", FinisherEffectType.RadialPop, 4, 700, 0, 2, 1f, 2, 5, 2.6f);
        CreateFinisher("chain-surge", "Chain Surge", "Turn the shot into a ticket burst.", FinisherEffectType.GoldRush, 5, 850, 1, 1, 1.1f, 3, 4, 2f);
        CreateFinisher("clone-storm", "Clone Storm", "Duplicate the best hit into nearby balloons.", FinisherEffectType.CloneStorm, 6, 700, 1, 2, 1.2f, 3, 4, 2f);
        CreateFinisher("echo-burst", "Echo Burst", "Anchors and ricochets echo one more time.", FinisherEffectType.EchoBurst, 4, 600, 1, 1, 1f, 2, 3, 2.1f);
        CreateFinisher("cleanup-sweep-finisher", "Cleanup Sweep", "Clean the remaining mess into points and safety.", FinisherEffectType.CleanupSweep, 5, 550, 0, 3, 1f, 2, 3, 2f);
    }

    private static void BuildMetaUpgrades()
    {
        CreateMetaUpgrade("scouted-prizes", "Field Notes", "Start each run with +2 Prize Tickets.", 20, "Flow", upgrade =>
        {
            upgrade.modifiers.startingTickets = 2;
            upgrade.unlockedPerkIds.Add("gold-glimmer");
        });
        CreateMetaUpgrade("paintbook-i", "Reaction Primer I", "Ordered paint recipes unlock one room earlier.", 25, "Paint", upgrade =>
        {
            upgrade.modifiers.recipeUnlockRoomDelta = -1;
        });
        CreateMetaUpgrade("sticker-crate-i", "Sticker Crate I", "Unlock Crown, Star, and Clover stickers.", 25, "Stickers", upgrade =>
        {
            upgrade.unlockedStickerFamilies.Add(StickerFamily.Crown);
            upgrade.unlockedStickerFamilies.Add(StickerFamily.Star);
            upgrade.unlockedStickerFamilies.Add(StickerFamily.Clover);
        });
        CreateMetaUpgrade("needle-license", "Relay License", "Sharpen all runs with a little more launch speed.", 30, "Loadouts", upgrade =>
        {
            upgrade.modifiers.launchSpeedBonus = 0.05f;
        });
        CreateMetaUpgrade("throw-mods-ledger", "Reaction Ledger", "Unlock deeper throw-reactive perks.", 35, "Perks", upgrade =>
        {
            upgrade.unlockedPerkIds.Add("static-surge");
            upgrade.unlockedPerkIds.Add("violet-relay");
            upgrade.unlockedPerkIds.Add("stormfront");
        });
        CreateMetaUpgrade("banker-license", "Glimmer License", "Start every run with +2 extra Prize Tickets.", 35, "Loadouts", upgrade =>
        {
            upgrade.prerequisiteIds.Add("scouted-prizes");
            upgrade.modifiers.startingTickets = 2;
        });
        CreateMetaUpgrade("chaos-permit", "Storm Permit", "Unlock the wave and swarm balloon set.", 40, "Chaos", upgrade =>
        {
        });
        CreateMetaUpgrade("relic-shelves", "Relic Shelves", "Unlock the first keystone relic shelf.", 45, "Relics", upgrade =>
        {
            upgrade.unlockedRelicIds.Add("chain-parade");
            upgrade.unlockedRelicIds.Add("monsoon-reel");
        });
        CreateMetaUpgrade("trickline-license", "Stormline License", "Improve bounce score on every run.", 45, "Loadouts", upgrade =>
        {
            upgrade.prerequisiteIds.Add("throw-mods-ledger");
            upgrade.modifiers.bounceScoreBonus = 0.1f;
        });
        CreateMetaUpgrade("sticker-crate-ii", "Sticker Crate II", "Unlock Bolt and Target stickers.", 45, "Stickers", upgrade =>
        {
            upgrade.prerequisiteIds.Add("sticker-crate-i");
            upgrade.unlockedStickerFamilies.Add(StickerFamily.Bolt);
            upgrade.unlockedStickerFamilies.Add(StickerFamily.Target);
        });
        CreateMetaUpgrade("overflow-meter", "Overflow Meter", "Banked overflow converts harder.", 50, "Flow", upgrade =>
        {
            upgrade.prerequisiteIds.Add("scouted-prizes");
            upgrade.modifiers.overflowTicketRateBonus = 0.12f;
        });
        CreateMetaUpgrade("economy-ledger", "Jackpot Ledger", "Unlock deeper jackpot and shop perks.", 55, "Perks", upgrade =>
        {
            upgrade.prerequisiteIds.Add("overflow-meter");
            upgrade.unlockedPerkIds.Add("jackpot-lane");
        });
        CreateMetaUpgrade("heavy-reel-license", "Flood Reel License", "Unlock the Flood Reel loadout.", 55, "Loadouts", upgrade =>
        {
            upgrade.unlockedLoadoutIds.Add("heavy-reel");
        });
        CreateMetaUpgrade("paint-lab", "Caustic Lab", "Unlock Invert and Rainbow balloons plus deeper paint perks.", 60, "Chaos", upgrade =>
        {
            upgrade.prerequisiteIds.Add("chaos-permit");
            upgrade.unlockedPerkIds.Add("acid-drip");
            upgrade.unlockedPerkIds.Add("caustic-flood");
        });
        CreateMetaUpgrade("skull-pack", "Burrow Pack", "Unlock Skull stickers and rat-synergy perks.", 60, "Stickers", upgrade =>
        {
            upgrade.prerequisiteIds.Add("sticker-crate-i");
            upgrade.unlockedStickerFamilies.Add(StickerFamily.Skull);
            upgrade.unlockedPerkIds.Add("sewer-rats");
            upgrade.unlockedPerkIds.Add("rat-king");
        });
        CreateMetaUpgrade("prism-license", "Prism License", "Unlock the Prism Kit loadout.", 65, "Loadouts", upgrade =>
        {
            upgrade.prerequisiteIds.Add("paint-lab");
            upgrade.unlockedLoadoutIds.Add("prism-kit");
        });
        CreateMetaUpgrade("recipe-atlas", "Recipe Atlas", "Unlock deeper recipe perks and +1 finisher charge on run start.", 70, "Paint", upgrade =>
        {
            upgrade.prerequisiteIds.Add("paintbook-i");
            upgrade.modifiers.finisherFlatCharge = 1;
            upgrade.unlockedPerkIds.Add("curtain-call");
        });
        CreateMetaUpgrade("keystone-annex", "Keystone Annex", "Unlock the full relic shelf.", 80, "Relics", upgrade =>
        {
            upgrade.prerequisiteIds.Add("relic-shelves");
            upgrade.unlockedRelicIds.Add("burrow-crown");
            upgrade.unlockedRelicIds.Add("prism-sweep");
        });
    }
}
