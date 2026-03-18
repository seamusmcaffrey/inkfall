using System.Collections.Generic;
using UnityEngine;

public partial class BalloonWall
{
    public void PopRandomNearby(Vector3 origin, float radius, int count, DartController instigator, BalloonNode exclude)
    {
        List<BalloonNode> candidates = GetLiveBalloonsWithin(origin, radius, exclude);
        PopFromCandidates(candidates, count, instigator);
    }

    public void PopRandomAnywhere(int count, DartController instigator)
    {
        var candidates = new List<BalloonNode>();
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon != null && !balloon.IsPopped)
            {
                candidates.Add(balloon);
            }
        }

        PopFromCandidates(candidates, count, instigator);
    }

    public void PopHazardsFirst(int count, DartController instigator)
    {
        var hazards = new List<BalloonNode>();
        var fallback = new List<BalloonNode>();
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon == null || balloon.IsPopped)
            {
                continue;
            }

            if (balloon.SpecialType == BalloonSpecialType.Hazard)
            {
                hazards.Add(balloon);
            }
            else
            {
                fallback.Add(balloon);
            }
        }

        PopFromCandidates(hazards, count, instigator);
        if (hazards.Count < count)
        {
            PopFromCandidates(fallback, count - hazards.Count, instigator);
        }
    }

    private static void PopFromCandidates(List<BalloonNode> candidates, int count, DartController instigator)
    {
        int remaining = Mathf.Min(count, candidates.Count);
        while (remaining-- > 0 && candidates.Count > 0)
        {
            int index = Random.Range(0, candidates.Count);
            BalloonNode balloon = candidates[index];
            candidates.RemoveAt(index);
            balloon?.Pop(instigator);
        }
    }

    private List<BalloonNode> GetLiveBalloonsWithin(Vector3 origin, float radius, BalloonNode exclude)
    {
        float radiusSqr = radius * radius;
        var balloons = new List<BalloonNode>();
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon == null || balloon == exclude || balloon.IsPopped)
            {
                continue;
            }

            if ((balloon.transform.position - origin).sqrMagnitude <= radiusSqr)
            {
                balloons.Add(balloon);
            }
        }

        return balloons;
    }
}
