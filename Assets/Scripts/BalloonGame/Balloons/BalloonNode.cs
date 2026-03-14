using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider))]
public class BalloonNode : MonoBehaviour
{
    public static event Action<BalloonNode> OnAnyBalloonPopped;

    public int Row { get; private set; }
    public int Column { get; private set; }
    public BalloonColor BalloonColor { get; private set; }
    public string BalloonTypeId { get; private set; }
    public BalloonSpecialType SpecialType { get; private set; }
    public int PointValue { get; private set; }
    public bool IsPopped { get; private set; }

    private BalloonTypeSO _balloonType;
    private SphereCollider _collider;
    private Rigidbody _rigidbody;
    private BalloonWall _wall;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _wall = GetComponentInParent<BalloonWall>();
    }

    public void Initialize(int row, int column, BalloonTypeSO balloonType)
    {
        Row = row;
        Column = column;
        _balloonType = balloonType;
        BalloonColor = balloonType != null ? balloonType.balloonColor : BalloonColor.Red;
        BalloonTypeId = balloonType != null ? balloonType.typeId : "standard";
        SpecialType = balloonType != null ? balloonType.specialType : BalloonSpecialType.Standard;
        PointValue = ResolvePointValue(balloonType);
        IsPopped = false;
        gameObject.layer = GameConstants.LAYER_BALLOONS;

        if (_collider != null)
        {
            _collider.enabled = true;
            _collider.radius = 0.5f;
        }

        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
        }
    }

    public void Initialize(int row, int column, BalloonColor color)
    {
        var runtimeType = ScriptableObject.CreateInstance<BalloonTypeSO>();
        runtimeType.typeId = $"standard-{color.ToString().ToLowerInvariant()}";
        runtimeType.balloonColor = color;
        runtimeType.specialType = BalloonSpecialType.Standard;
        runtimeType.basePoints = GameConstants.SCORE_PER_BALLOON;
        Initialize(row, column, runtimeType);
    }

    public void Pop()
    {
        Pop(null);
    }

    public void Pop(DartController instigator)
    {
        if (IsPopped)
        {
            return;
        }

        IsPopped = true;
        if (_collider != null)
        {
            _collider.enabled = false;
        }

        if (_rigidbody != null)
        {
            _rigidbody.detectCollisions = false;
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
        }

        OnAnyBalloonPopped?.Invoke(this);
        EventBus.Publish(new BalloonPoppedEvent
        {
            WorldPosition = transform.position,
            BalloonColor = BalloonColor,
            BalloonTypeId = BalloonTypeId,
            SpecialType = SpecialType,
            BasePoints = PointValue,
        });

        if (SpecialType == BalloonSpecialType.Paint && _wall != null)
        {
            float radius = (_balloonType != null ? _balloonType.effectRadius : GameConfigSO.Instance.defaultPaintRadius);
            _wall.TriggerPaintExplosion(this, radius, instigator);
            EventBus.Publish(new PaintExplosionEvent
            {
                WorldPosition = transform.position,
                Radius = radius,
                SourceColor = BalloonColor,
            });
        }

        if (SpecialType == BalloonSpecialType.Gold && _balloonType != null && _balloonType.currencyReward > 0)
        {
            SaveManager.Instance.AddInk(_balloonType.currencyReward);
        }

        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.rigidbody != null ? collision.rigidbody.gameObject : collision.gameObject;
        if (hitObject.layer != GameConstants.LAYER_PROJECTILES)
        {
            return;
        }

        var dartController = hitObject.GetComponent<DartController>();
        Pop(dartController);
        dartController?.OnHitBalloon();
    }

    private int ResolvePointValue(BalloonTypeSO balloonType)
    {
        if (balloonType == null)
        {
            return GameConstants.SCORE_PER_BALLOON;
        }

        int points = balloonType.ResolvedPoints;
        if (balloonType.specialType == BalloonSpecialType.Gold)
        {
            points += GameConfigSO.Instance.goldBalloonBonus;
        }
        else if (balloonType.specialType == BalloonSpecialType.Hazard)
        {
            points = -Mathf.Max(balloonType.hazardPenalty, GameConfigSO.Instance.hazardBalloonPenalty);
        }

        return points;
    }
}
