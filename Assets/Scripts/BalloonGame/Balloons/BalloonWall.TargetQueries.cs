using System.Collections.Generic;
using UnityEngine;

public partial class BalloonWall
{
    private BalloonNode FindNearestLiveBalloon(Vector3 origin, float maxRadius, BalloonNode exclude)
    {
        float bestDistance = maxRadius > 0f ? maxRadius * maxRadius : float.MaxValue;
        BalloonNode best = null;
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon == null || balloon.IsPopped || balloon == exclude)
            {
                continue;
            }

            float distance = (balloon.transform.position - origin).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = balloon;
            }
        }

        return best;
    }

    private BalloonNode FindBalloonBelow(BalloonNode source, int maxRowDistance)
    {
        if (source == null)
        {
            return null;
        }

        BalloonNode best = null;
        int bestRowDistance = int.MaxValue;
        int bestColumnDelta = int.MaxValue;
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon == null || balloon.IsPopped || balloon == source || balloon.Row <= source.Row)
            {
                continue;
            }

            int rowDistance = balloon.Row - source.Row;
            if (maxRowDistance > 0 && rowDistance > maxRowDistance)
            {
                continue;
            }

            int columnDelta = Mathf.Abs(balloon.Column - source.Column);
            if (rowDistance < bestRowDistance || (rowDistance == bestRowDistance && columnDelta < bestColumnDelta))
            {
                best = balloon;
                bestRowDistance = rowDistance;
                bestColumnDelta = columnDelta;
            }
        }

        return best;
    }

    private BalloonNode FindMirroredBalloon(BalloonNode source)
    {
        if (source == null)
        {
            return null;
        }

        int columns = _roomConfig != null ? _roomConfig.columns : GameConstants.BOARD_COLUMNS;
        int mirroredColumn = columns - 1 - source.Column;
        BalloonNode fallback = null;
        int fallbackDelta = int.MaxValue;
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon == null || balloon.IsPopped || balloon == source || balloon.Row != source.Row)
            {
                continue;
            }

            if (balloon.Column == mirroredColumn)
            {
                return balloon;
            }

            int delta = Mathf.Abs(balloon.Column - mirroredColumn);
            if (delta < fallbackDelta)
            {
                fallback = balloon;
                fallbackDelta = delta;
            }
        }

        return fallback;
    }

    private List<BalloonNode> GetLiveBalloonsInRow(int row, BalloonNode exclude)
    {
        var rowBalloons = new List<BalloonNode>();
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon != null && !balloon.IsPopped && balloon != exclude && balloon.Row == row)
            {
                rowBalloons.Add(balloon);
            }
        }

        return rowBalloons;
    }
}
