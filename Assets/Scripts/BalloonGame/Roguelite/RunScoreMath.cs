using System;
using UnityEngine;

public static class RunScoreMath
{
    public static float EvaluateComboMultiplier(int comboCount)
    {
        GameConfigSO config = GameConfigSO.Instance;
        if (comboCount <= 1)
        {
            return 1f;
        }

        int earlyStacks = Mathf.Min(comboCount - 1, Mathf.Max(0, config.comboSoftCap - 1));
        int lateStacks = Mathf.Max(0, comboCount - config.comboSoftCap);
        return 1f + earlyStacks * config.comboMultiplierPerStep + lateStacks * config.comboMultiplierLateStep;
    }

    public static float EstimateComboPayoutFactor(float productivePops)
    {
        int fullPops = Mathf.FloorToInt(productivePops);
        float remainder = productivePops - fullPops;
        float total = 0f;
        for (int combo = 1; combo <= fullPops; combo++)
        {
            total += EvaluateComboMultiplier(combo);
        }

        if (remainder > 0f)
        {
            total += remainder * EvaluateComboMultiplier(fullPops + 1);
        }

        return total;
    }

    public static int ResolveBalloonValue(BalloonTypeSO balloonType, GameConfigSO config)
    {
        if (balloonType == null)
        {
            return config != null ? config.baseScorePerBalloon : GameConstants.SCORE_PER_BALLOON;
        }

        int points = balloonType.ResolvedPoints;
        if (config == null)
        {
            return points;
        }

        if (balloonType.specialType == BalloonSpecialType.Gold)
        {
            points += config.goldBalloonBonus;
        }
        else if (balloonType.specialType == BalloonSpecialType.Hazard)
        {
            points = -Mathf.Max(balloonType.hazardPenalty, config.hazardBalloonPenalty);
        }

        return points;
    }

    public static int EstimateRoomTarget(RoomTemplateSO template, int roomNumber, GameConfigSO config)
    {
        int rows = template != null ? template.rows : GameConstants.BOARD_ROWS;
        int columns = template != null ? template.columns : GameConstants.BOARD_COLUMNS;
        int boardSlots = Mathf.Max(1, rows * columns);
        int startingDarts = template != null ? template.baseDarts : GameConstants.STARTING_DARTS;
        float expectedSpecials = Average(template != null ? template.minSpecials : 0, template != null ? template.maxSpecials : 0);
        float expectedHazards = Average(template != null ? template.minHazards : 0, template != null ? template.maxHazards : 0);
        bool isBonusRoom = template != null && template.roomType == RoomType.Bonus;
        float productivePops = startingDarts * (0.92f + expectedSpecials * 0.05f) +
            0.75f +
            (1f + Mathf.Max(0, roomNumber - 1) * config.targetScoreScaling) +
            (isBonusRoom ? 0.5f : 0f);
        productivePops = Mathf.Clamp(productivePops, 2.5f, boardSlots - expectedHazards * 0.25f);

        float specialCaptureRate = 0.45f + Mathf.Min(0.25f, Mathf.Max(0, roomNumber - 1) * 0.015f) + (isBonusRoom ? 0.1f : 0f);
        float expectedSpecialHits = Mathf.Min(expectedSpecials, productivePops * specialCaptureRate);
        float hazardContactRate = Mathf.Max(0.08f, 0.16f - Mathf.Max(0, roomNumber - 1) * 0.004f);
        float expectedHazardHits = Mathf.Min(expectedHazards, productivePops * hazardContactRate);
        float expectedStandardHits = Mathf.Max(0f, productivePops - expectedSpecialHits - expectedHazardHits);

        float depthBonusMultiplier = 1f + Mathf.Max(0, roomNumber - 1) * config.roomDepthScoreBonusPerRoom;
        float standardValue = WeightedAverageBalloonValue(config, roomNumber, type => type.specialType == BalloonSpecialType.Standard);
        float specialValue = WeightedAverageBalloonValue(config, roomNumber,
            type => type.specialType != BalloonSpecialType.Standard &&
                type.specialType != BalloonSpecialType.Hazard);
        float hazardPenalty = WeightedAverageBalloonPenalty(config, roomNumber);

        float averagePayoutPerPop =
            (expectedStandardHits * standardValue * depthBonusMultiplier +
            expectedSpecialHits * specialValue * depthBonusMultiplier -
            expectedHazardHits * hazardPenalty) /
            Mathf.Max(1f, productivePops);
        averagePayoutPerPop = Mathf.Max(config.baseScorePerBalloon * 0.65f, averagePayoutPerPop);

        float modeledScore = EstimateComboPayoutFactor(productivePops) * averagePayoutPerPop;
        float normalizedDepth = config.totalRooms > 1
            ? Mathf.Clamp01((roomNumber - 1f) / (config.totalRooms - 1f))
            : 0f;
        float pressure = (template != null ? template.targetPressure : 0.5f) +
            Mathf.Max(0, roomNumber - 1) * 0.008f +
            (template != null ? template.depthPressureBonus : 0.1f) * normalizedDepth;
        int flatBonus = template != null ? template.flatTargetBonus : 0;
        int depthFloor = Mathf.RoundToInt(Mathf.Max(0, roomNumber - 1) * 35f);
        int minimumTarget = template != null
            ? template.baseTargetScore
            : (roomNumber <= 1 ? GameConstants.OPENING_ROOM_TARGET_SCORE : GameConstants.BASE_TARGET_SCORE);

        int rawTarget = Mathf.Max(minimumTarget, Mathf.RoundToInt(modeledScore * pressure + flatBonus + depthFloor));
        return Mathf.Max(100, Mathf.RoundToInt(rawTarget * Mathf.Max(0.1f, config.roomTargetMultiplier)));
    }

    private static float WeightedAverageBalloonValue(GameConfigSO config, int roomNumber, Func<BalloonTypeSO, bool> predicate)
    {
        float weightedValue = 0f;
        float totalWeight = 0f;
        foreach (BalloonTypeSO type in config.balloonTypes)
        {
            if (type == null || !type.canSpawn || type.roomUnlock > roomNumber || !string.IsNullOrEmpty(type.requiredMetaUpgradeId) || !predicate(type))
            {
                continue;
            }

            float weight = Mathf.Max(0.01f, type.spawnWeight);
            weightedValue += ResolveBalloonValue(type, config) * weight;
            totalWeight += weight;
        }

        return totalWeight > 0f ? weightedValue / totalWeight : config.baseScorePerBalloon;
    }

    private static float WeightedAverageBalloonPenalty(GameConfigSO config, int roomNumber)
    {
        float weightedPenalty = 0f;
        float totalWeight = 0f;
        foreach (BalloonTypeSO type in config.balloonTypes)
        {
            if (type == null || !type.canSpawn || type.roomUnlock > roomNumber || !string.IsNullOrEmpty(type.requiredMetaUpgradeId) || type.specialType != BalloonSpecialType.Hazard)
            {
                continue;
            }

            float weight = Mathf.Max(0.01f, type.spawnWeight);
            weightedPenalty += Mathf.Abs(ResolveBalloonValue(type, config)) * weight;
            totalWeight += weight;
        }

        return totalWeight > 0f ? weightedPenalty / totalWeight : config.hazardBalloonPenalty;
    }

    private static float Average(int minValue, int maxValue)
    {
        return (minValue + maxValue) * 0.5f;
    }
}
