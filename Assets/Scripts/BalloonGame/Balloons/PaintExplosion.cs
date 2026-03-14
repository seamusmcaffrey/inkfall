using UnityEngine;

/// <summary>
/// Helper component for paint-balloon chain reactions.
/// </summary>
[DisallowMultipleComponent]
public class PaintExplosion : MonoBehaviour
{
    public void Explode(BalloonNode origin, BalloonWall wall, DartController instigator, float radius)
    {
        if (origin == null || wall == null)
        {
            return;
        }

        wall.TriggerPaintExplosion(origin, radius, instigator);
    }
}
