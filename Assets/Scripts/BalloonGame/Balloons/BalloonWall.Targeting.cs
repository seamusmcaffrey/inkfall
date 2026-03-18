using System.Collections.Generic;
using UnityEngine;

public partial class BalloonWall
{
    public bool TryPopNearest(Vector3 origin, float maxRadius, BalloonNode exclude, DartController instigator, out BalloonNode popped)
    {
        popped = FindNearestLiveBalloon(origin, maxRadius, exclude);
        if (popped == null)
        {
            return false;
        }

        popped.Pop(instigator);
        return true;
    }

    public bool TryPopBelow(BalloonNode source, int maxRowDistance, DartController instigator, out BalloonNode popped)
    {
        popped = FindBalloonBelow(source, maxRowDistance);
        if (popped == null)
        {
            return false;
        }

        popped.Pop(instigator);
        return true;
    }

    public bool TryPopMirrored(BalloonNode source, DartController instigator, out BalloonNode popped)
    {
        popped = FindMirroredBalloon(source);
        if (popped == null)
        {
            return false;
        }

        popped.Pop(instigator);
        return true;
    }

    public List<BalloonNode> PopRow(int row, DartController instigator, BalloonNode exclude = null)
    {
        var popped = GetLiveBalloonsInRow(row, exclude);
        foreach (BalloonNode balloon in popped)
        {
            balloon.Pop(instigator);
        }

        return popped;
    }

    public List<BalloonNode> PopAdjacentInRow(BalloonNode source, int columnRadius, DartController instigator)
    {
        var popped = new List<BalloonNode>();
        if (source == null)
        {
            return popped;
        }

        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon == null ||
                balloon.IsPopped ||
                balloon == source ||
                balloon.Row != source.Row ||
                Mathf.Abs(balloon.Column - source.Column) > columnRadius)
            {
                continue;
            }

            popped.Add(balloon);
        }

        foreach (BalloonNode balloon in popped)
        {
            balloon.Pop(instigator);
        }

        return popped;
    }

    public int GetHighestActiveRow()
    {
        int row = int.MaxValue;
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon != null && !balloon.IsPopped)
            {
                row = Mathf.Min(row, balloon.Row);
            }
        }

        return row == int.MaxValue ? -1 : row;
    }

    public int GetLowestActiveRow()
    {
        int row = -1;
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon != null && !balloon.IsPopped)
            {
                row = Mathf.Max(row, balloon.Row);
            }
        }

        return row;
    }

    public Vector3 GetRowCenter(int row)
    {
        Vector3 total = Vector3.zero;
        int count = 0;
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon == null || balloon.IsPopped || balloon.Row != row)
            {
                continue;
            }

            total += balloon.transform.position;
            count++;
        }

        if (count > 0)
        {
            return total / count;
        }

        int rows = _roomConfig != null ? _roomConfig.rows : GameConstants.BOARD_ROWS;
        int columns = _roomConfig != null ? _roomConfig.columns : GameConstants.BOARD_COLUMNS;
        float slotX = GameConstants.BOARD_WIDTH / columns;
        float slotY = GameConstants.BOARD_HEIGHT / rows;
        return PerspectivePosition(row, Mathf.Max(0, columns / 2), rows, columns, slotX, slotY);
    }
}
