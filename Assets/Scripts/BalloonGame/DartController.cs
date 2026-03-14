using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DartController : MonoBehaviour
{
    public enum DartState
    {
        Ready,
        Flying,
        Stopped
    }

    public static event System.Action<DartController> OnDartFinished;

    public DartState State { get; private set; } = DartState.Ready;

    private const float MaxLifetime = 5f;

    private Rigidbody _rigidbody;
    private float _lifetime;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        gameObject.layer = GameConstants.LAYER_PROJECTILES;
    }

    public void Launch(Vector3 velocity)
    {
        CancelInvoke(nameof(Deactivate));
        State = DartState.Flying;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.linearVelocity = velocity;
        _lifetime = 0f;
    }

    private void FixedUpdate()
    {
        if (State != DartState.Flying)
        {
            return;
        }

        _lifetime += Time.fixedDeltaTime;

        if (_rigidbody.linearVelocity.sqrMagnitude > 0.5f)
        {
            float angle = Mathf.Atan2(_rigidbody.linearVelocity.y, _rigidbody.linearVelocity.x) * Mathf.Rad2Deg;
            _rigidbody.MoveRotation(Quaternion.Euler(0f, 0f, angle));
        }

        if (_lifetime > MaxLifetime)
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
        StopDart("balloon");
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void StopDart(string reason)
    {
        if (State == DartState.Stopped)
        {
            return;
        }

        State = DartState.Stopped;
        Vector3 velocityBeforeStop = _rigidbody.linearVelocity;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;

        Debug.Log($"DART STOP reason={reason} pos={transform.position} vel={velocityBeforeStop}");
        OnDartFinished?.Invoke(this);
        Invoke(nameof(Deactivate), 0.3f);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void HandleCollision(Collision collision)
    {
        string hitName = collision.collider != null ? collision.collider.name : collision.gameObject.name;
        if (hitName == "LeftWall" || hitName == "RightWall")
        {
            Debug.Log($"DART WALL BOUNCE wall={hitName} pos={transform.position} vel={_rigidbody.linearVelocity}");
            return;
        }

        if (hitName == "TopWall")
        {
            Debug.Log($"DART TOP HIT pos={transform.position} vel={_rigidbody.linearVelocity}");
            StopDart("top_wall");
        }
    }
}
