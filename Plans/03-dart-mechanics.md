# Plan 03: Dart + Slingshot — Input, Launch & Rigidbody Flight

## Goal
Implement slingshot drag-to-aim input and dart launching using Rigidbody physics. Reuse existing projectile patterns from this codebase (ArrowMover, ArrowPositionPredicter, ArrowLaunchData). The dart is a real physics object — Unity handles gravity, drag, and collisions.

## Deliverables
- [ ] `Assets/Scripts/BalloonGame/SlingshotInput.cs` — drag-to-aim using Input System
- [ ] `Assets/Scripts/BalloonGame/SlingshotVisuals.cs` — rubber band + trajectory preview
- [ ] `Assets/Scripts/BalloonGame/DartController.cs` — dart Rigidbody behavior (adapted from ArrowMover)
- [ ] `Assets/Scripts/BalloonGame/DartLauncher.cs` — converts slingshot pull into launch velocity

---

## Existing Code We Build On

Read these first — they contain the patterns we adapt:

| File | What We Reuse |
|---|---|
| `Assets/Scripts/Weapons/Projectiles/ArrowMover.cs` | Rigidbody velocity assignment + rotation-to-velocity in FixedUpdate |
| `Assets/Scripts/Weapons/Projectiles/ArrowLaunchData.cs` | Launch data struct (position, velocity, gravity) |
| `Assets/Scripts/Weapons/Projectiles/ArrowPositionPredicter.cs` | SUVAT trajectory preview: `s = ut + 0.5at²` |
| `Assets/Scripts/Weapons/ProjectileLaunchers/HeightBasedProjectileLauncher.cs` | SUVAT equations for trajectory calculation |

---

## Step 1: Create DartController.cs

**File**: `Assets/Scripts/BalloonGame/DartController.cs`

Adapted from `ArrowMover.cs`. A Rigidbody projectile that rotates to face its velocity.

```csharp
using UnityEngine;

/// <summary>
/// Controls a dart's physics behavior after launch.
/// Adapted from ArrowMover — uses Rigidbody for flight, rotates to face velocity.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DartController : MonoBehaviour
{
    public enum DartState { Ready, Flying, Stopped }
    public DartState State { get; private set; } = DartState.Ready;

    private Rigidbody _rb;
    private float _lifetime;
    private const float MAX_LIFETIME = 5f;

    /// <summary>
    /// Event fired when dart finishes (hit, stopped, timed out).
    /// </summary>
    public static event System.Action<DartController> OnDartFinished;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false; // Enabled on launch
        _rb.isKinematic = true; // Until launched
        gameObject.layer = GameConstants.LAYER_PROJECTILES;
    }

    /// <summary>
    /// Launch the dart with the given velocity. Enables physics.
    /// </summary>
    public void Launch(Vector3 velocity)
    {
        State = DartState.Flying;
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.linearVelocity = velocity;
        _lifetime = 0f;
    }

    private void FixedUpdate()
    {
        if (State != DartState.Flying) return;

        _lifetime += Time.fixedDeltaTime;

        // Rotate to face velocity (from ArrowMover pattern)
        if (_rb.linearVelocity.sqrMagnitude > 0.5f)
        {
            // For 2D flight in XY plane, rotate around Z axis
            float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Timeout: if dart has been flying too long, stop it
        if (_lifetime > MAX_LIFETIME)
        {
            StopDart();
        }

        // Out of bounds check
        Vector3 pos = transform.position;
        if (pos.y < GameConstants.LANE_BOTTOM - 2f ||
            pos.x < GameConstants.BOARD_LEFT - 5f ||
            pos.x > GameConstants.BOARD_RIGHT + 5f ||
            pos.y > GameConstants.BOARD_TOP + 5f)
        {
            StopDart();
        }
    }

    /// <summary>
    /// Called by BalloonNode when dart hits a balloon.
    /// </summary>
    public void OnHitBalloon()
    {
        StopDart();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If we hit a wall (not a balloon — balloons handle their own collision)
        // Check if it's the top wall — stop the dart
        if (collision.gameObject.name == "TopWall")
        {
            StopDart();
        }
        // Side walls: dart bounces naturally via PhysicMaterial — no code needed
    }

    private void StopDart()
    {
        if (State == DartState.Stopped) return;
        State = DartState.Stopped;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = false;
        _rb.isKinematic = true;

        OnDartFinished?.Invoke(this);

        // Deactivate after short delay (let it be visible briefly)
        Invoke(nameof(Deactivate), 0.3f);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
```

---

## Step 2: Create SlingshotInput.cs

**File**: `Assets/Scripts/BalloonGame/SlingshotInput.cs`

Uses the new Input System (Pointer.current) for drag-to-aim.

```csharp
using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Slingshot drag-to-aim input handler using Unity Input System.
///
/// Flow:
///   1. Pointer down near launch point → begin aiming
///   2. Drag away → pull vector (rubber band stretches)
///   3. Release → launch in opposite direction of pull
///
/// Pull-to-speed mapping (from Inkshot):
///   - normalizedPull = clamp(distance / MAX_PULL_DISTANCE, 0, 1)
///   - powerRatio = pow(normalizedPull, 1.45)
///   - speed = MIN_LAUNCH_SPEED + powerRatio * (MAX - MIN)
///   - direction = opposite of pull (normalized)
/// </summary>
public class SlingshotInput : MonoBehaviour
{
    public event Action<Vector3> OnLaunch;        // Launch velocity (3D)
    public event Action<Vector2> OnPullUpdate;    // Current pull vector (for visuals)
    public event Action OnPullCancel;

    private Camera _camera;
    private bool _isAiming;
    private Vector2 _pullVector;
    private bool _canFire = true;

    public bool IsAiming => _isAiming;

    public float PullStrength
    {
        get
        {
            float dist = _pullVector.magnitude;
            return Mathf.Pow(Mathf.Clamp01(dist / GameConstants.MAX_PULL_DISTANCE), GameConstants.PULL_SPEED_EXPONENT);
        }
    }

    /// <summary>
    /// Preview velocity for trajectory display.
    /// </summary>
    public Vector3 PreviewVelocity
    {
        get
        {
            float dist = _pullVector.magnitude;
            if (dist < 0.05f) return Vector3.zero;

            Vector2 dir = -_pullVector.normalized;
            float power = Mathf.Pow(Mathf.Clamp01(dist / GameConstants.MAX_PULL_DISTANCE), GameConstants.PULL_SPEED_EXPONENT);
            float speed = GameConstants.MIN_LAUNCH_SPEED + power * (GameConstants.MAX_LAUNCH_SPEED - GameConstants.MIN_LAUNCH_SPEED);
            return new Vector3(dir.x, dir.y, 0) * speed;
        }
    }

    public void SetCanFire(bool canFire)
    {
        _canFire = canFire;
        if (!canFire && _isAiming) CancelPull();
    }

    private void Update()
    {
        if (!_canFire) return;

        var pointer = Pointer.current;
        if (pointer == null) return;

        if (pointer.press.wasPressedThisFrame)
            BeginPull(pointer);
        else if (pointer.press.isPressed && _isAiming)
            UpdatePull(pointer);
        else if (pointer.press.wasReleasedThisFrame && _isAiming)
            ReleasePull();
    }

    private void BeginPull(Pointer pointer)
    {
        Vector2 worldPos = ScreenToWorld(pointer.position.ReadValue());
        Vector2 launchPos = new(GameConstants.LAUNCH_POSITION.x, GameConstants.LAUNCH_POSITION.y);

        if (Vector2.Distance(worldPos, launchPos) > GameConstants.AIM_ACTIVATION_RADIUS) return;

        _isAiming = true;
        _pullVector = Vector2.zero;
    }

    private void UpdatePull(Pointer pointer)
    {
        Vector2 worldPos = ScreenToWorld(pointer.position.ReadValue());
        Vector2 launchPos = new(GameConstants.LAUNCH_POSITION.x, GameConstants.LAUNCH_POSITION.y);

        Vector2 raw = worldPos - launchPos;
        float dist = Mathf.Min(raw.magnitude, GameConstants.MAX_PULL_DISTANCE);
        _pullVector = raw.normalized * dist;

        OnPullUpdate?.Invoke(_pullVector);
    }

    private void ReleasePull()
    {
        _isAiming = false;
        Vector3 velocity = PreviewVelocity;

        if (velocity.sqrMagnitude > 1f)
            OnLaunch?.Invoke(velocity);
        else
            OnPullCancel?.Invoke();

        _pullVector = Vector2.zero;
    }

    private void CancelPull()
    {
        _isAiming = false;
        _pullVector = Vector2.zero;
        OnPullCancel?.Invoke();
    }

    private Vector2 ScreenToWorld(Vector2 screenPos)
    {
        if (_camera == null) _camera = Camera.main;
        Vector3 worldPos = _camera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(_camera.transform.position.z)));
        return new Vector2(worldPos.x, worldPos.y);
    }
}
```

---

## Step 3: Create SlingshotVisuals.cs

**File**: `Assets/Scripts/BalloonGame/SlingshotVisuals.cs`

Trajectory preview using the same SUVAT math from `ArrowPositionPredicter.cs`: `s = ut + 0.5at²`

```csharp
using UnityEngine;

/// <summary>
/// Renders rubber band line and trajectory preview during slingshot aiming.
/// Trajectory uses SUVAT: position = start + velocity*t + 0.5*gravity*t²
/// (Same math as ArrowPositionPredicter in this project.)
/// </summary>
[RequireComponent(typeof(SlingshotInput))]
public class SlingshotVisuals : MonoBehaviour
{
    [SerializeField] private int _trajectoryPoints = 30;
    [SerializeField] private float _trajectoryDuration = 2.0f;

    private SlingshotInput _input;
    private LineRenderer _bandLine;
    private LineRenderer _trajectoryLine;

    private void Awake()
    {
        _input = GetComponent<SlingshotInput>();
        _input.OnPullUpdate += OnPullUpdate;
        _input.OnPullCancel += HideAll;

        _bandLine = CreateLine("RubberBand", new Color(0.8f, 0.3f, 0.2f, 0.9f), 0.08f, 0.04f);
        _trajectoryLine = CreateLine("Trajectory", new Color(1, 1, 1, 0.4f), 0.06f, 0.02f);
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.OnPullUpdate -= OnPullUpdate;
            _input.OnPullCancel -= HideAll;
        }
    }

    private void OnPullUpdate(Vector2 pullVector)
    {
        Vector3 origin = GameConstants.LAUNCH_POSITION;
        Vector3 pullEnd = origin + new Vector3(pullVector.x, pullVector.y, 0);

        // Rubber band
        _bandLine.positionCount = 2;
        _bandLine.SetPosition(0, origin + Vector3.back);
        _bandLine.SetPosition(1, pullEnd + Vector3.back);

        // Trajectory preview (SUVAT: s = ut + 0.5at²)
        Vector3 velocity = _input.PreviewVelocity;
        if (velocity.sqrMagnitude < 1f)
        {
            _trajectoryLine.positionCount = 0;
            return;
        }

        Vector3 gravity = Physics.gravity;
        _trajectoryLine.positionCount = _trajectoryPoints;
        for (int i = 0; i < _trajectoryPoints; i++)
        {
            float t = (float)i / (_trajectoryPoints - 1) * _trajectoryDuration;
            // s = u*t + 0.5*a*t² (from ArrowPositionPredicter)
            Vector3 pos = origin + velocity * t + 0.5f * gravity * t * t;
            _trajectoryLine.SetPosition(i, pos + Vector3.back);
        }
    }

    private void HideAll()
    {
        _bandLine.positionCount = 0;
        _trajectoryLine.positionCount = 0;
    }

    private void LateUpdate()
    {
        if (!_input.IsAiming) HideAll();
    }

    private LineRenderer CreateLine(string name, Color color, float startWidth, float endWidth)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var lr = go.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = new Color(color.r, color.g, color.b, color.a * 0.2f);
        lr.startWidth = startWidth;
        lr.endWidth = endWidth;
        lr.positionCount = 0;
        lr.useWorldSpace = true;
        return lr;
    }
}
```

---

## Step 4: Create DartLauncher.cs

**File**: `Assets/Scripts/BalloonGame/DartLauncher.cs`

Spawns darts and launches them. Kept separate from input for clean separation.

```csharp
using UnityEngine;

/// <summary>
/// Spawns and launches dart projectiles.
/// Called by GameManager when SlingshotInput fires.
/// </summary>
public class DartLauncher : MonoBehaviour
{
    /// <summary>
    /// Create a dart at the launch point, launch it with the given velocity.
    /// Returns the DartController for tracking.
    /// </summary>
    public DartController SpawnAndLaunch(Vector3 velocity)
    {
        // Create dart from primitives (no prefab dependency)
        var dart = CreateDartObject();
        dart.transform.position = GameConstants.LAUNCH_POSITION + Vector3.back * 0.5f;

        var controller = dart.GetComponent<DartController>();
        controller.Launch(velocity);

        return controller;
    }

    /// <summary>
    /// Creates a dart GameObject from primitives.
    /// Shaped as an elongated capsule (body) with a pointed tip.
    /// </summary>
    private GameObject CreateDartObject()
    {
        var root = new GameObject("Dart");
        root.layer = GameConstants.LAYER_PROJECTILES;

        // Body (capsule rotated to point right along X)
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localRotation = Quaternion.Euler(0, 0, 90);
        body.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
        body.layer = GameConstants.LAYER_PROJECTILES;

        // Material
        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.7f, 0.72f, 0.75f);
        mat.SetFloat("_Glossiness", 0.8f);
        mat.SetFloat("_Metallic", 0.6f);
        body.GetComponent<Renderer>().material = mat;

        // Tip (small sphere)
        var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "Tip";
        tip.transform.SetParent(root.transform);
        tip.transform.localPosition = new Vector3(0.28f, 0, 0);
        tip.transform.localScale = new Vector3(0.08f, 0.08f, 0.15f);
        tip.layer = GameConstants.LAYER_PROJECTILES;
        Object.Destroy(tip.GetComponent<SphereCollider>()); // Use body's collider only

        var tipMat = new Material(Shader.Find("Standard"));
        tipMat.color = new Color(0.85f, 0.85f, 0.85f);
        tipMat.SetFloat("_Glossiness", 0.9f);
        tipMat.SetFloat("_Metallic", 0.7f);
        tip.GetComponent<Renderer>().material = tipMat;

        // Rigidbody on root
        var rb = root.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.5f;
        rb.useGravity = false;    // Enabled on launch
        rb.isKinematic = true;     // Until launched
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Prevent tunneling

        // Use the capsule's collider (already on body child)
        // The CapsuleCollider on the child will detect collisions for the parent Rigidbody

        // DartController
        root.AddComponent<DartController>();

        return root;
    }
}
```

---

## Step 5: Dart Prefab (Alternative to Runtime Creation)

Optionally, create a prefab via editor script for better workflow:

Add to `SceneBuilder.cs`:
```csharp
[MenuItem("BalloonGame/Create Dart Prefab")]
public static void CreateDartPrefab()
{
    // Same logic as DartLauncher.CreateDartObject() but saves as prefab
    // Save to Assets/Prefabs/DartPrefab.prefab
}
```

For MVP, runtime creation in `DartLauncher` works fine without a prefab.

---

## How This Connects to Existing Code

The existing `ArrowMover.cs` does this:
```csharp
// Set velocity on launch
_rigidbody.linearVelocity = _arrowLaunchData.InitialVelocity;

// Rotate to face velocity each frame
transform.rotation = Quaternion.LookRotation(_rigidbody.linearVelocity);
```

Our `DartController` does the same thing, but for 2D (XY plane):
```csharp
// Set velocity on launch
_rb.linearVelocity = velocity;

// Rotate to face velocity each frame (2D: Z-axis rotation)
float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
transform.rotation = Quaternion.Euler(0, 0, angle);
```

The existing `ArrowPositionPredicter.cs` uses SUVAT:
```csharp
displacement = initialVelocity * time + (acceleration * time * time) / 2;
```

Our `SlingshotVisuals` uses the same equation:
```csharp
pos = origin + velocity * t + 0.5f * gravity * t * t;
```

---

## Verification Checklist

- [ ] Clicking/dragging near launch point shows rubber band line
- [ ] Trajectory preview arc visible during drag
- [ ] Release spawns a dart that flies in opposite direction of pull
- [ ] Dart follows a natural parabolic arc (Unity gravity)
- [ ] Dart rotates to face its velocity during flight
- [ ] Bigger pull = faster dart
- [ ] Dart bounces off side walls (bouncy PhysicMaterial)
- [ ] Dart stops at top wall
- [ ] Dart deactivates after going out of bounds
- [ ] No console errors
- [ ] Input uses Pointer.current (new Input System)

## Files Created
1. `Assets/Scripts/BalloonGame/DartController.cs`
2. `Assets/Scripts/BalloonGame/SlingshotInput.cs`
3. `Assets/Scripts/BalloonGame/SlingshotVisuals.cs`
4. `Assets/Scripts/BalloonGame/DartLauncher.cs`
