using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DartController : MonoBehaviour
{
    private static readonly System.Collections.Generic.HashSet<DartController> ActiveDarts = new();

    public enum DartState
    {
        Ready,
        Flying,
        Stopped
    }

    public static event System.Action<DartController> OnDartFinished;
    public static event System.Action<DartController, Collision> OnWallBounce;

    public DartState State { get; private set; } = DartState.Ready;

    private Rigidbody _rigidbody;
    private float _lifetime;
    private CapsuleCollider _capsuleCollider;
    private SphereCollider _ricochetCollider;
    private ObjectPool _owningPool;
    private int _balloonsHitThisFlight;
    private int _ricochetCount;
    private int _pierceRemaining;
    private int _maxRicochets;
    private bool _hasPassedPeak;
    private float _launchSpeed;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _ricochetCollider = GetComponentInChildren<SphereCollider>(true);
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        gameObject.layer = GameConstants.LAYER_PROJECTILES;
        if (_ricochetCollider != null) _ricochetCollider.enabled = false;
    }

    private void OnEnable()
    {
        ActiveDarts.Add(this);
    }

    private void OnDisable()
    {
        ActiveDarts.Remove(this);
    }

    public static DartController[] GetActiveDarts()
    {
        var result = new DartController[ActiveDarts.Count];
        ActiveDarts.CopyTo(result);
        return result;
    }

    public void Initialize(ObjectPool owningPool, int pierceBonus, int ricochetBonus)
    {
        _owningPool = owningPool;
        _pierceRemaining = Mathf.Max(1, GameConfigSO.Instance.defaultMaxPierce + pierceBonus);
        _maxRicochets = Mathf.Max(0, GameConfigSO.Instance.defaultMaxRicochets + ricochetBonus);
    }

    public void Launch(Vector3 velocity)
    {
        State = DartState.Flying;
        _balloonsHitThisFlight = 0;
        _ricochetCount = 0;
        _launchSpeed = velocity.magnitude;
        _hasPassedPeak = velocity.y <= GameConstants.DART_PEAK_VELOCITY_THRESHOLD;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = false;
        _rigidbody.linearVelocity = velocity;
        _lifetime = 0f;
        if (_capsuleCollider != null) _capsuleCollider.enabled = _hasPassedPeak;
        if (_ricochetCollider != null) _ricochetCollider.enabled = false;

        var trail = GetComponent<DartTrailVFX>();
        if (trail != null) trail.OnLaunch();
    }

    private void FixedUpdate()
    {
        if (State != DartState.Flying)
        {
            return;
        }

        _lifetime += Time.fixedDeltaTime;

        _rigidbody.AddForce(new Vector3(0f, GameConstants.DART_GRAVITY, 0f), ForceMode.Acceleration);

        if (!_hasPassedPeak && _rigidbody.linearVelocity.y <= GameConstants.DART_PEAK_VELOCITY_THRESHOLD)
        {
            _hasPassedPeak = true;
            if (_capsuleCollider != null) _capsuleCollider.enabled = true;
            CheckPeakOverlap();
        }

        if (_launchSpeed > 0.1f)
        {
            float nearPeak = 1f - Mathf.Clamp01(Mathf.Abs(_rigidbody.linearVelocity.y) / _launchSpeed);
            float arcScale = Mathf.Lerp(GameConstants.DART_ARC_SCALE_MIN, GameConstants.DART_ARC_SCALE_MAX, nearPeak);
            transform.localScale = Vector3.one * arcScale;
        }

        if (_rigidbody.linearVelocity.sqrMagnitude > 0.5f)
        {
            float angle = Mathf.Atan2(_rigidbody.linearVelocity.y, _rigidbody.linearVelocity.x) * Mathf.Rad2Deg;
            _rigidbody.MoveRotation(Quaternion.Euler(0f, 0f, angle));
        }

        if (_lifetime > GameConfigSO.Instance.dartLifetimeSeconds)
        {
            StopDart("timeout");
            return;
        }

        Vector3 position = transform.position;
        if (position.y < GameConstants.LANE_BOTTOM - 2f ||
            position.x < GameConstants.BOARD_LEFT - 5f ||
            position.x > GameConstants.BOARD_RIGHT + 5f ||
            position.y > GameConstants.BOARD_TOP + 5f)
        {
            StopDart("out_of_bounds");
        }
    }

    public void OnHitBalloon()
    {
        _balloonsHitThisFlight++;
        _pierceRemaining--;
        if (_pierceRemaining <= 0)
        {
            StopDart("balloon");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (State != DartState.Flying || other.gameObject.layer != GameConstants.LAYER_RICOCHET)
        {
            return;
        }

        if (_ricochetCount >= _maxRicochets)
        {
            return;
        }

        Vector3 incomingVelocity = _rigidbody.linearVelocity;
        if (incomingVelocity.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        _ricochetCount++;
        Vector3 reflected = Vector3.Reflect(incomingVelocity.normalized, Vector3.right * Mathf.Sign(incomingVelocity.x == 0 ? 1f : incomingVelocity.x));
        reflected = Quaternion.Euler(0f, 0f, Random.Range(-12f, 12f)) * reflected;
        _rigidbody.linearVelocity = reflected.normalized * incomingVelocity.magnitude;

        EventBus.Publish(new DartRicochetEvent
        {
            RicochetPosition = transform.position,
            IncomingVelocity = incomingVelocity,
            OutgoingVelocity = _rigidbody.linearVelocity,
            RicochetNumber = _ricochetCount,
        });
    }

    private void StopDart(string reason)
    {
        if (State == DartState.Stopped)
        {
            return;
        }

        State = DartState.Stopped;
        transform.localScale = Vector3.one;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;

        if (_capsuleCollider != null)
        {
            _capsuleCollider.enabled = false;
        }

        if (_ricochetCollider != null)
        {
            _ricochetCollider.enabled = true;
        }

        OnDartFinished?.Invoke(this);
        EventBus.Publish(new DartFinishedEvent
        {
            FinalPosition = transform.position,
            StopReason = reason,
            BalloonsHitThisFlight = _balloonsHitThisFlight,
        });

        if (reason == "timeout" || reason == "out_of_bounds")
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        if (_owningPool != null)
        {
            _owningPool.Return(gameObject);
            return;
        }

        gameObject.SetActive(false);
    }

    public void ForceRecycle()
    {
        StopAllCoroutines();
        transform.localScale = Vector3.one;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        State = DartState.Stopped;

        if (_capsuleCollider != null)
        {
            _capsuleCollider.enabled = false;
        }

        if (_ricochetCollider != null)
        {
            _ricochetCollider.enabled = false;
        }

        Deactivate();
    }

    private void CheckPeakOverlap()
    {
        if (State != DartState.Flying) return;
        var overlaps = Physics.OverlapSphere(
            transform.position, 0.5f, 1 << GameConstants.LAYER_BALLOONS);
        float closestDist = float.MaxValue;
        BalloonNode closestBalloon = null;
        foreach (var col in overlaps)
        {
            var balloon = col.GetComponent<BalloonNode>();
            if (balloon == null || balloon.IsPopped) continue;
            float dist = (balloon.transform.position - transform.position).sqrMagnitude;
            if (dist < closestDist) { closestDist = dist; closestBalloon = balloon; }
        }
        if (closestBalloon != null)
        {
            closestBalloon.Pop(this);
            OnHitBalloon();
        }
    }

    private void HandleCollision(Collision collision)
    {
        string hitName = collision.collider != null ? collision.collider.name : collision.gameObject.name;
        if (hitName == "LeftWall" || hitName == "RightWall")
        {
            OnWallBounce?.Invoke(this, collision);
            EventBus.Publish(new WallBounceEvent
            {
                Position = transform.position,
                Velocity = _rigidbody.linearVelocity,
                WallName = hitName,
            });
            return;
        }

        if (hitName == "TopWall")
        {
            if (_capsuleCollider != null && collision.collider != null)
                Physics.IgnoreCollision(_capsuleCollider, collision.collider);
            return;
        }
    }
}
