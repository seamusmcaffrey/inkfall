using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BalloonNode : MonoBehaviour
{
    public static event Action<BalloonNode> OnAnyBalloonPopped;

    public int Row { get; private set; }
    public int Column { get; private set; }
    public BalloonColor BalloonColor { get; private set; }
    public bool IsPopped { get; private set; }

    public void Initialize(int row, int column, BalloonColor color)
    {
        Row = row;
        Column = column;
        BalloonColor = color;
        IsPopped = false;
        gameObject.layer = GameConstants.LAYER_BALLOONS;
    }

    public void Pop()
    {
        if (IsPopped)
        {
            return;
        }

        IsPopped = true;
        OnAnyBalloonPopped?.Invoke(this);
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.rigidbody != null ? collision.rigidbody.gameObject : collision.gameObject;
        if (hitObject.layer != GameConstants.LAYER_PROJECTILES)
        {
            return;
        }

        Pop();

        var dartController = hitObject.GetComponent<DartController>();
        dartController?.OnHitBalloon();
    }
}
