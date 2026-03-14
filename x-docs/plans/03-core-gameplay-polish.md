# Plan 03: Core Gameplay Polish

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development or superpowers:executing-plans.

**Goal:** Transform INKSHOT from a single-mechanic prototype into a satisfying, crunchy gameplay loop with combo scoring, special balloon types, dart ricochet physics, balloon jiggle, piercing darts, floating score text, room difficulty scaling, and input feel polish.

**Architecture:** All new gameplay systems communicate through the EventBus established in Plan 01. Special balloon behavior is data-driven via BalloonTypeSO assets. Tuning constants live in GameConstants.cs or ScriptableObject fields -- zero magic numbers in MonoBehaviours. Spawned objects (floating text, currency tokens) use the ObjectPool from Plan 01.

**Tech Stack:** Unity 2022.3 LTS, C# 10, URP, New Input System, ScriptableObjects

**Depends on:** Plan 01 (EventBus, ObjectPool, BalloonTypeSO, GameConfigSO, SaveManager), Plan 02 (Visual balloon meshes/materials, dart mesh/material, environment)

---

## File Map

| File | Action | Task |
|------|--------|------|
| `Assets/Scripts/BalloonGame/GameConstants.cs` | Modify | 1, 2, 4, 5, 6, 7, 8, 9, 10 |
| `Assets/Scripts/BalloonGame/Events/GameplayEvents.cs` | Create | 1 |
| `Assets/Scripts/BalloonGame/Combo/ComboTracker.cs` | Create | 1 |
| `Assets/Scripts/BalloonGame/ScoreManager.cs` | Modify | 1, 8 |
| `Assets/ScriptableObjects/BalloonTypes/BalloonTypeSO.cs` | Create | 2 |
| `Assets/Scripts/BalloonGame/BalloonNode.cs` | Modify | 2, 4 |
| `Assets/Scripts/BalloonGame/Balloons/PaintExplosion.cs` | Create | 2 |
| `Assets/Scripts/BalloonGame/Balloons/CurrencyToken.cs` | Create | 2 |
| `Assets/Scripts/BalloonGame/BalloonWall.cs` | Modify | 3 |
| `Assets/ScriptableObjects/RoomTemplateSO.cs` | Create | 3, 9 |
| `Assets/Scripts/BalloonGame/Balloons/BalloonJiggle.cs` | Create | 4 |
| `Assets/Scripts/BalloonGame/DartController.cs` | Modify | 5, 6, 7 |
| `Assets/Scripts/BalloonGame/DartLauncher.cs` | Modify | 5 |
| `Assets/Scripts/BalloonGame/UI/FloatingScoreText.cs` | Create | 8 |
| `Assets/Scripts/BalloonGame/UI/ComboDisplay.cs` | Create | 8 |
| `Assets/Scripts/BalloonGame/BalloonGameManager.cs` | Modify | 9, 10 |
| `Assets/Scripts/BalloonGame/Feel/LaunchFeel.cs` | Create | 10 |

---

## Task 1: EventBus Events and Combo System

**Files:**
- Create: `Assets/Scripts/BalloonGame/Events/GameplayEvents.cs`
- Create: `Assets/Scripts/BalloonGame/Combo/ComboTracker.cs`
- Modify: `Assets/Scripts/BalloonGame/GameConstants.cs`
- Modify: `Assets/Scripts/BalloonGame/ScoreManager.cs`

- [ ] **Step 1: Add combo constants to GameConstants**

```csharp
// In Assets/Scripts/BalloonGame/GameConstants.cs
// ADD these lines after the existing SCORE_PER_BALLOON constant:

    public const float COMBO_WINDOW_SECONDS = 1.5f;
    public const float COMBO_MULTIPLIER_PER_STEP = 0.5f;
    public const int COMBO_MAX_STACK = 20;
```

- [ ] **Step 2: Create gameplay event structs**

```csharp
// Assets/Scripts/BalloonGame/Events/GameplayEvents.cs
using UnityEngine;

/// <summary>
/// Fired when any balloon is popped, carrying positional and type data.
/// </summary>
public struct BalloonPoppedEvent
{
    public Vector3 WorldPosition;
    public BalloonColor Color;
    public string BalloonTypeId;
    public int BasePoints;
    public int ComboCount;
    public float ComboMultiplier;
    public int FinalPoints;
}

/// <summary>
/// Fired when the combo counter changes (increment or reset).
/// </summary>
public struct ComboChangedEvent
{
    public int ComboCount;
    public float Multiplier;
    public bool WasReset;
}

/// <summary>
/// Fired when the total score changes.
/// </summary>
public struct ScoreChangedEvent
{
    public int CurrentScore;
    public int TargetScore;
    public int PointsJustAdded;
    public bool TargetReached;
}

/// <summary>
/// Fired when a dart completes its flight (hit, timeout, or out of bounds).
/// </summary>
public struct DartFinishedEvent
{
    public Vector3 FinalPosition;
    public string StopReason;
    public int BalloonsHitThisFlight;
}

/// <summary>
/// Fired when a dart is launched from the slingshot.
/// </summary>
public struct DartLaunchedEvent
{
    public Vector3 LaunchVelocity;
    public float PullStrength;
}

/// <summary>
/// Fired when a dart ricochets off a stuck dart.
/// </summary>
public struct DartRicochetEvent
{
    public Vector3 RicochetPosition;
    public Vector3 IncomingVelocity;
    public Vector3 OutgoingVelocity;
    public int RicochetNumber;
}

/// <summary>
/// Fired when a paint balloon explodes, triggering chain pops.
/// </summary>
public struct PaintExplosionEvent
{
    public Vector3 Center;
    public float Radius;
    public int BalloonsHit;
}

/// <summary>
/// Fired when a currency token is dropped from a Prize balloon.
/// </summary>
public struct CurrencyDropEvent
{
    public Vector3 SpawnPosition;
    public int Amount;
}

/// <summary>
/// Fired when the room state changes (intro, ready, cleared, failed).
/// </summary>
public struct RoomStateChangedEvent
{
    public BalloonGameManager.GameState NewState;
    public BalloonGameManager.GameState PreviousState;
    public int DartsRemaining;
}
```

- [ ] **Step 3: Create ComboTracker**

```csharp
// Assets/Scripts/BalloonGame/Combo/ComboTracker.cs
using UnityEngine;

/// <summary>
/// Tracks rapid successive balloon pops and calculates combo multipliers.
/// Each pop within COMBO_WINDOW_SECONDS of the last increments the combo counter.
/// Multiplier = 1 + (comboCount * COMBO_MULTIPLIER_PER_STEP).
/// </summary>
[DisallowMultipleComponent]
public class ComboTracker : MonoBehaviour
{
    /// <summary>Current number of consecutive pops in the active combo chain.</summary>
    public int ComboCount { get; private set; }

    /// <summary>Current score multiplier derived from combo count.</summary>
    public float Multiplier => 1f + ComboCount * GameConstants.COMBO_MULTIPLIER_PER_STEP;

    /// <summary>True if a combo is currently active (count >= 1).</summary>
    public bool IsComboActive => ComboCount > 0;

    private float _lastPopTime;
    private bool _hasActiveCombo;

    /// <summary>
    /// Call this when a balloon is popped. Returns the current multiplier
    /// after updating the combo state.
    /// </summary>
    public float RegisterPop()
    {
        float currentTime = Time.time;
        float elapsed = currentTime - _lastPopTime;

        if (_hasActiveCombo && elapsed <= GameConstants.COMBO_WINDOW_SECONDS)
        {
            ComboCount = Mathf.Min(ComboCount + 1, GameConstants.COMBO_MAX_STACK);
        }
        else
        {
            if (_hasActiveCombo && ComboCount > 0)
            {
                FireComboChanged(wasReset: true);
            }
            ComboCount = 0;
        }

        _lastPopTime = currentTime;
        _hasActiveCombo = true;

        FireComboChanged(wasReset: false);
        return Multiplier;
    }

    /// <summary>
    /// Forces the combo to reset immediately (e.g., on room transition).
    /// </summary>
    public void ResetCombo()
    {
        if (ComboCount > 0)
        {
            ComboCount = 0;
            _hasActiveCombo = false;
            FireComboChanged(wasReset: true);
        }
    }

    private void Update()
    {
        if (!_hasActiveCombo || ComboCount <= 0)
        {
            return;
        }

        float elapsed = Time.time - _lastPopTime;
        if (elapsed > GameConstants.COMBO_WINDOW_SECONDS)
        {
            int previousCount = ComboCount;
            ComboCount = 0;
            _hasActiveCombo = false;

            if (previousCount > 0)
            {
                FireComboChanged(wasReset: true);
            }
        }
    }

    private void FireComboChanged(bool wasReset)
    {
        EventBus.Publish(new ComboChangedEvent
        {
            ComboCount = ComboCount,
            Multiplier = Multiplier,
            WasReset = wasReset
        });
    }
}
```

- [ ] **Step 4: Update ScoreManager to use combo multiplier and fire events**

```csharp
// Assets/Scripts/BalloonGame/ScoreManager.cs
// REPLACE the entire file contents with:
using System;
using UnityEngine;

/// <summary>
/// Manages score accumulation, integrates with ComboTracker for multiplied scoring,
/// and publishes score events via EventBus.
/// </summary>
[DisallowMultipleComponent]
public class ScoreManager : MonoBehaviour
{
    public event Action<int> OnScoreChanged;
    public event Action<int> OnBalloonPopped;
    public event Action OnTargetReached;

    /// <summary>Current accumulated score for this room.</summary>
    public int CurrentScore { get; private set; }

    /// <summary>Score needed to clear the room.</summary>
    public int TargetScore { get; private set; }

    /// <summary>True if current score meets or exceeds target.</summary>
    public bool IsTargetReached => CurrentScore >= TargetScore;

    [SerializeField] private ComboTracker _comboTracker;

    /// <summary>
    /// Resets score to zero and sets new target. Call at room start.
    /// </summary>
    public void Initialize(int targetScore)
    {
        CurrentScore = 0;
        TargetScore = targetScore;
        OnScoreChanged?.Invoke(CurrentScore);

        if (_comboTracker != null)
        {
            _comboTracker.ResetCombo();
        }

        EventBus.Publish(new ScoreChangedEvent
        {
            CurrentScore = 0,
            TargetScore = targetScore,
            PointsJustAdded = 0,
            TargetReached = false
        });
    }

    private void OnEnable()
    {
        BalloonNode.OnAnyBalloonPopped += HandleBalloonPopped;
    }

    private void OnDisable()
    {
        BalloonNode.OnAnyBalloonPopped -= HandleBalloonPopped;
    }

    private void HandleBalloonPopped(BalloonNode balloon)
    {
        bool reachedBeforePop = IsTargetReached;

        int basePoints = balloon.PointValue;
        float multiplier = 1f;
        int comboCount = 0;

        if (_comboTracker != null)
        {
            multiplier = _comboTracker.RegisterPop();
            comboCount = _comboTracker.ComboCount;
        }

        int finalPoints = Mathf.RoundToInt(basePoints * multiplier);
        CurrentScore += finalPoints;

        OnBalloonPopped?.Invoke(finalPoints);
        OnScoreChanged?.Invoke(CurrentScore);

        string typeId = balloon.TypeId;

        EventBus.Publish(new BalloonPoppedEvent
        {
            WorldPosition = balloon.transform.position,
            Color = balloon.BalloonColor,
            BalloonTypeId = typeId,
            BasePoints = basePoints,
            ComboCount = comboCount,
            ComboMultiplier = multiplier,
            FinalPoints = finalPoints
        });

        EventBus.Publish(new ScoreChangedEvent
        {
            CurrentScore = CurrentScore,
            TargetScore = TargetScore,
            PointsJustAdded = finalPoints,
            TargetReached = IsTargetReached
        });

        Debug.Log($"POP! +{finalPoints} pts (base:{basePoints} x{multiplier:F1} combo:{comboCount}) | Score: {CurrentScore}/{TargetScore}");

        if (!reachedBeforePop && IsTargetReached)
        {
            OnTargetReached?.Invoke();
        }
    }
}
```

**Commit:** `git commit -m "feat: combo system with EventBus integration and gameplay events"`

---

## Task 2: BalloonTypeSO and Special Balloon Types

**Files:**
- Create: `Assets/ScriptableObjects/BalloonTypes/BalloonTypeSO.cs`
- Modify: `Assets/Scripts/BalloonGame/BalloonNode.cs`
- Create: `Assets/Scripts/BalloonGame/Balloons/PaintExplosion.cs`
- Create: `Assets/Scripts/BalloonGame/Balloons/CurrencyToken.cs`
- Modify: `Assets/Scripts/BalloonGame/GameConstants.cs`

- [ ] **Step 1: Add special balloon constants to GameConstants**

```csharp
// In Assets/Scripts/BalloonGame/GameConstants.cs
// ADD these lines after the existing combo constants:

    // Special balloon tuning
    public const int GOLD_POINT_MULTIPLIER = 3;
    public const float PAINT_EXPLOSION_RADIUS = 1.5f;
    public const int PRIZE_CURRENCY_AMOUNT = 10;
    public const int HAZARD_DART_PENALTY = 1;
    public const int HAZARD_SCORE_PENALTY = 50;

    // Currency token
    public const float CURRENCY_TOKEN_FLOAT_SPEED = 2f;
    public const float CURRENCY_TOKEN_LIFETIME = 1.5f;

    // Layer for ricochet colliders on stuck darts
    public const int LAYER_RICOCHET = 9;
```

- [ ] **Step 2: Create BalloonTypeSO**

```csharp
// Assets/ScriptableObjects/BalloonTypes/BalloonTypeSO.cs
using UnityEngine;

/// <summary>
/// Defines a balloon type's behavior, visual overrides, and scoring properties.
/// Assign to BalloonNode to control per-type behavior.
/// </summary>
[CreateAssetMenu(fileName = "BalloonType_New", menuName = "INKSHOT/Balloon Type")]
public class BalloonTypeSO : ScriptableObject
{
    public enum BalloonCategory
    {
        Standard,
        Gold,
        Paint,
        Prize,
        Hazard
    }

    [Header("Identity")]
    [SerializeField] private string _typeId = "standard";
    [SerializeField] private BalloonCategory _category = BalloonCategory.Standard;
    [SerializeField] private string _displayName = "Standard";

    [Header("Scoring")]
    [SerializeField] private int _basePoints = 100;
    [SerializeField] private int _pointMultiplier = 1;

    [Header("Visual Overrides")]
    [SerializeField] private Color _colorOverride = Color.clear;
    [SerializeField] private bool _useColorOverride;
    [SerializeField] private Color _emissionColor = Color.clear;
    [SerializeField] private float _emissionIntensity;
    [SerializeField] private float _scaleMultiplier = 1f;

    [Header("Paint Balloon")]
    [SerializeField] private float _explosionRadius = 1.5f;

    [Header("Prize Balloon")]
    [SerializeField] private int _currencyDrop = 10;

    [Header("Hazard Balloon")]
    [SerializeField] private int _dartPenalty = 1;
    [SerializeField] private int _scorePenalty = 50;

    [Header("Spawn Weights")]
    [Tooltip("Higher weight = more likely to spawn. Standard is typically 80-90.")]
    [SerializeField] private float _spawnWeight = 85f;

    /// <summary>Unique identifier for this balloon type.</summary>
    public string TypeId => _typeId;

    /// <summary>What category of special behavior this balloon has.</summary>
    public BalloonCategory Category => _category;

    /// <summary>Human-readable name for UI display.</summary>
    public string DisplayName => _displayName;

    /// <summary>Base points awarded when this balloon is popped (before combo).</summary>
    public int BasePoints => _basePoints;

    /// <summary>Multiplier applied to base points (e.g., Gold = 3x).</summary>
    public int PointMultiplier => _pointMultiplier;

    /// <summary>Total point value for this balloon type before combo.</summary>
    public int PointValue => _basePoints * _pointMultiplier;

    /// <summary>If true, this balloon uses a color override instead of the grid color.</summary>
    public bool UseColorOverride => _useColorOverride;

    /// <summary>Color override for this balloon type's material.</summary>
    public Color ColorOverride => _colorOverride;

    /// <summary>Emission color for glowing balloon types (Gold, Hazard).</summary>
    public Color EmissionColor => _emissionColor;

    /// <summary>Emission intensity. 0 = no emission.</summary>
    public float EmissionIntensity => _emissionIntensity;

    /// <summary>Scale multiplier applied on top of grid-computed scale.</summary>
    public float ScaleMultiplier => _scaleMultiplier;

    /// <summary>Radius of paint explosion AoE (Paint type only).</summary>
    public float ExplosionRadius => _explosionRadius;

    /// <summary>Currency amount dropped (Prize type only).</summary>
    public int CurrencyDrop => _currencyDrop;

    /// <summary>Number of darts lost when a Hazard balloon is popped.</summary>
    public int DartPenalty => _dartPenalty;

    /// <summary>Score penalty when a Hazard balloon is popped.</summary>
    public int ScorePenalty => _scorePenalty;

    /// <summary>Relative weight for random spawn selection.</summary>
    public float SpawnWeight => _spawnWeight;
}
```

- [ ] **Step 3: Update BalloonNode to support BalloonTypeSO**

```csharp
// Assets/Scripts/BalloonGame/BalloonNode.cs
// REPLACE the entire file contents with:
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Individual balloon in the grid wall. Reads behavior from an assigned BalloonTypeSO.
/// Handles collision with darts, pop logic, and type-specific effects (paint AoE, prize drops, hazard penalties).
/// </summary>
[RequireComponent(typeof(SphereCollider))]
[DisallowMultipleComponent]
public class BalloonNode : MonoBehaviour
{
    public static event Action<BalloonNode> OnAnyBalloonPopped;

    /// <summary>Row index in the balloon grid.</summary>
    public int Row { get; private set; }

    /// <summary>Column index in the balloon grid.</summary>
    public int Column { get; private set; }

    /// <summary>Visual color assigned to this balloon.</summary>
    public BalloonColor BalloonColor { get; private set; }

    /// <summary>Whether this balloon has been popped.</summary>
    public bool IsPopped { get; private set; }

    /// <summary>The ScriptableObject defining this balloon's type and behavior.</summary>
    public BalloonTypeSO BalloonType { get; private set; }

    /// <summary>Point value for this balloon (from type SO, or default).</summary>
    public int PointValue => BalloonType != null ? BalloonType.PointValue : GameConstants.SCORE_PER_BALLOON;

    /// <summary>Type identifier string for event reporting.</summary>
    public string TypeId => BalloonType != null ? BalloonType.TypeId : "standard";

    /// <summary>The category of this balloon's type.</summary>
    public BalloonTypeSO.BalloonCategory Category =>
        BalloonType != null ? BalloonType.Category : BalloonTypeSO.BalloonCategory.Standard;

    // Jiggle state (driven by BalloonJiggle system)
    private Vector3 _baseLocalPosition;
    private Vector3 _jiggleOffset;
    private float _jiggleTime;
    private float _jiggleAmplitude;
    private bool _isJiggling;

    /// <summary>
    /// Initializes the balloon with grid coordinates, color, and optional type SO.
    /// </summary>
    public void Initialize(int row, int column, BalloonColor color, BalloonTypeSO balloonType = null)
    {
        Row = row;
        Column = column;
        BalloonColor = color;
        BalloonType = balloonType;
        IsPopped = false;
        _isJiggling = false;
        _jiggleOffset = Vector3.zero;
        gameObject.layer = GameConstants.LAYER_BALLOONS;

        _baseLocalPosition = transform.localPosition;

        ApplyTypeVisuals();
    }

    /// <summary>
    /// Pops this balloon, triggering type-specific effects and notifying listeners.
    /// </summary>
    public void Pop()
    {
        if (IsPopped)
        {
            return;
        }

        IsPopped = true;

        ExecuteTypeEffect();
        OnAnyBalloonPopped?.Invoke(this);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Starts a jiggle effect on this balloon (called by BalloonJiggle system).
    /// </summary>
    public void StartJiggle(float amplitude, Vector2 direction)
    {
        if (IsPopped || _isJiggling)
        {
            return;
        }

        _isJiggling = true;
        _jiggleAmplitude = amplitude;
        _jiggleTime = 0f;
        _jiggleOffset = new Vector3(direction.x, direction.y, 0f).normalized * amplitude;
    }

    private void Update()
    {
        if (!_isJiggling)
        {
            return;
        }

        _jiggleTime += Time.deltaTime;
        float duration = GameConstants.JIGGLE_DURATION;

        if (_jiggleTime >= duration)
        {
            _isJiggling = false;
            transform.localPosition = _baseLocalPosition;
            return;
        }

        float progress = _jiggleTime / duration;
        float decay = 1f - progress;
        float oscillation = Mathf.Sin(progress * GameConstants.JIGGLE_FREQUENCY * Mathf.PI * 2f);
        float displacement = oscillation * decay * _jiggleAmplitude;

        transform.localPosition = _baseLocalPosition + _jiggleOffset.normalized * displacement;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.rigidbody != null ? collision.rigidbody.gameObject : collision.gameObject;
        if (hitObject.layer != GameConstants.LAYER_PROJECTILES)
        {
            return;
        }

        var dartController = hitObject.GetComponent<DartController>();
        if (dartController == null)
        {
            return;
        }

        Pop();
        dartController.OnHitBalloon(this);
    }

    private void ApplyTypeVisuals()
    {
        if (BalloonType == null || !BalloonType.UseColorOverride)
        {
            return;
        }

        var renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.material.color = BalloonType.ColorOverride;

        if (BalloonType.EmissionIntensity > 0f)
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", BalloonType.EmissionColor * BalloonType.EmissionIntensity);
        }

        if (Mathf.Abs(BalloonType.ScaleMultiplier - 1f) > 0.01f)
        {
            transform.localScale *= BalloonType.ScaleMultiplier;
        }
    }

    private void ExecuteTypeEffect()
    {
        if (BalloonType == null)
        {
            return;
        }

        switch (BalloonType.Category)
        {
            case BalloonTypeSO.BalloonCategory.Paint:
                PaintExplosion.Execute(transform.position, BalloonType.ExplosionRadius, this);
                break;

            case BalloonTypeSO.BalloonCategory.Prize:
                CurrencyToken.Spawn(transform.position, BalloonType.CurrencyDrop);
                break;

            case BalloonTypeSO.BalloonCategory.Hazard:
                EventBus.Publish(new HazardPoppedEvent
                {
                    WorldPosition = transform.position,
                    DartPenalty = BalloonType.DartPenalty,
                    ScorePenalty = BalloonType.ScorePenalty
                });
                break;
        }
    }
}
```

- [ ] **Step 4: Add HazardPoppedEvent to GameplayEvents.cs**

```csharp
// APPEND to Assets/Scripts/BalloonGame/Events/GameplayEvents.cs:

/// <summary>
/// Fired when a hazard balloon is popped, signaling dart/score penalties.
/// </summary>
public struct HazardPoppedEvent
{
    public Vector3 WorldPosition;
    public int DartPenalty;
    public int ScorePenalty;
}
```

- [ ] **Step 5: Add jiggle constants to GameConstants**

```csharp
// In Assets/Scripts/BalloonGame/GameConstants.cs
// ADD after the LAYER_RICOCHET constant:

    // Jiggle / shockwave
    public const float JIGGLE_RADIUS = 2f;
    public const float JIGGLE_MAX_AMPLITUDE = 0.15f;
    public const float JIGGLE_DURATION = 0.4f;
    public const float JIGGLE_FREQUENCY = 3f;
```

- [ ] **Step 6: Create PaintExplosion**

```csharp
// Assets/Scripts/BalloonGame/Balloons/PaintExplosion.cs
using UnityEngine;

/// <summary>
/// Static utility that executes a paint balloon's area-of-effect explosion.
/// Finds all active BalloonNodes within radius and pops them, enabling chain reactions.
/// </summary>
public static class PaintExplosion
{
    /// <summary>
    /// Pops all active balloons within the given radius of the center position.
    /// Chain-capable: if a Paint balloon is within radius, its Pop() will
    /// trigger another PaintExplosion via BalloonNode.ExecuteTypeEffect.
    /// </summary>
    public static void Execute(Vector3 center, float radius, BalloonNode source)
    {
        float radiusSqr = radius * radius;
        int hitCount = 0;

        BalloonWall wall = Object.FindAnyObjectByType<BalloonWall>();
        if (wall == null)
        {
            return;
        }

        // Snapshot the list to avoid mutation during iteration
        var balloons = wall.Balloons;
        var toPop = new System.Collections.Generic.List<BalloonNode>(8);

        for (int i = 0; i < balloons.Count; i++)
        {
            BalloonNode balloon = balloons[i];
            if (balloon == null || balloon.IsPopped || balloon == source)
            {
                continue;
            }

            float distSqr = (balloon.transform.position - center).sqrMagnitude;
            if (distSqr <= radiusSqr)
            {
                toPop.Add(balloon);
            }
        }

        for (int i = 0; i < toPop.Count; i++)
        {
            toPop[i].Pop();
            hitCount++;
        }

        EventBus.Publish(new PaintExplosionEvent
        {
            Center = center,
            Radius = radius,
            BalloonsHit = hitCount
        });
    }
}
```

- [ ] **Step 7: Create CurrencyToken**

```csharp
// Assets/Scripts/BalloonGame/Balloons/CurrencyToken.cs
using UnityEngine;

/// <summary>
/// Visual currency token that floats upward from a popped Prize balloon.
/// Fires a CurrencyDropEvent and self-deactivates after its lifetime expires.
/// </summary>
[DisallowMultipleComponent]
public class CurrencyToken : MonoBehaviour
{
    private int _amount;
    private float _elapsed;

    /// <summary>
    /// Spawns a currency token at the given position.
    /// Uses Object.Instantiate for now; will be replaced with ObjectPool in Plan 01 integration.
    /// </summary>
    public static void Spawn(Vector3 position, int amount)
    {
        var tokenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tokenObj.name = "CurrencyToken";
        tokenObj.transform.position = position;
        tokenObj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        // Remove default collider so it doesn't interact with physics
        var collider = tokenObj.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        var renderer = tokenObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            renderer.material = new Material(shader)
            {
                color = new Color(1f, 0.84f, 0f)
            };
        }

        var token = tokenObj.AddComponent<CurrencyToken>();
        token._amount = amount;
        token._elapsed = 0f;

        EventBus.Publish(new CurrencyDropEvent
        {
            SpawnPosition = position,
            Amount = amount
        });
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        // Float upward
        transform.position += Vector3.up * (GameConstants.CURRENCY_TOKEN_FLOAT_SPEED * Time.deltaTime);

        // Spin
        transform.Rotate(0f, 360f * Time.deltaTime, 0f);

        // Fade out via scale
        float progress = _elapsed / GameConstants.CURRENCY_TOKEN_LIFETIME;
        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        float scale = 0.2f * (1f - progress * 0.5f);
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
```

**Commit:** `git commit -m "feat: BalloonTypeSO, special balloon types (Gold/Paint/Prize/Hazard), and type effects"`

---

## Task 3: RoomTemplateSO and Balloon Placement Logic

**Files:**
- Create: `Assets/ScriptableObjects/RoomTemplateSO.cs`
- Modify: `Assets/Scripts/BalloonGame/BalloonWall.cs`

- [ ] **Step 1: Create RoomTemplateSO**

```csharp
// Assets/ScriptableObjects/RoomTemplateSO.cs
using UnityEngine;

/// <summary>
/// Defines a room's balloon layout, difficulty parameters, and type distribution.
/// Used by BalloonWall to generate varied room configurations.
/// </summary>
[CreateAssetMenu(fileName = "RoomTemplate_New", menuName = "INKSHOT/Room Template")]
public class RoomTemplateSO : ScriptableObject
{
    [Header("Grid Dimensions")]
    [SerializeField] [Range(4, 10)] private int _columns = 8;
    [SerializeField] [Range(3, 12)] private int _rows = 9;

    [Header("Difficulty")]
    [SerializeField] private int _targetScore = 3000;
    [SerializeField] [Range(2, 8)] private int _dartCount = 4;
    [SerializeField] [Range(1, 10)] private int _difficultyTier = 1;

    [Header("Balloon Type Weights")]
    [Tooltip("Relative spawn weight for standard balloons.")]
    [SerializeField] [Range(0f, 100f)] private float _standardWeight = 85f;
    [Tooltip("Relative spawn weight for gold balloons.")]
    [SerializeField] [Range(0f, 30f)] private float _goldWeight = 5f;
    [Tooltip("Relative spawn weight for paint balloons.")]
    [SerializeField] [Range(0f, 30f)] private float _paintWeight = 5f;
    [Tooltip("Relative spawn weight for prize balloons.")]
    [SerializeField] [Range(0f, 20f)] private float _prizeWeight = 3f;
    [Tooltip("Relative spawn weight for hazard balloons.")]
    [SerializeField] [Range(0f, 30f)] private float _hazardWeight = 2f;

    [Header("Guarantees")]
    [SerializeField] [Range(0, 5)] private int _minGold = 1;
    [SerializeField] [Range(0, 5)] private int _minPaint = 1;
    [SerializeField] [Range(0, 3)] private int _maxHazard = 2;

    [Header("Balloon Type Assets")]
    [SerializeField] private BalloonTypeSO _standardType;
    [SerializeField] private BalloonTypeSO _goldType;
    [SerializeField] private BalloonTypeSO _paintType;
    [SerializeField] private BalloonTypeSO _prizeType;
    [SerializeField] private BalloonTypeSO _hazardType;

    /// <summary>Number of columns in the balloon grid.</summary>
    public int Columns => _columns;

    /// <summary>Number of rows in the balloon grid.</summary>
    public int Rows => _rows;

    /// <summary>Total number of balloons in this room layout.</summary>
    public int TotalBalloons => _columns * _rows;

    /// <summary>Score required to clear this room.</summary>
    public int TargetScore => _targetScore;

    /// <summary>Number of darts the player gets for this room.</summary>
    public int DartCount => _dartCount;

    /// <summary>Difficulty tier (1 = easiest).</summary>
    public int DifficultyTier => _difficultyTier;

    /// <summary>Minimum guaranteed gold balloons.</summary>
    public int MinGold => _minGold;

    /// <summary>Minimum guaranteed paint balloons.</summary>
    public int MinPaint => _minPaint;

    /// <summary>Maximum allowed hazard balloons.</summary>
    public int MaxHazard => _maxHazard;

    /// <summary>
    /// Selects a BalloonTypeSO based on weighted random distribution.
    /// Returns null for standard type (BalloonNode will use default behavior).
    /// </summary>
    public BalloonTypeSO SelectRandomType()
    {
        float totalWeight = _standardWeight + _goldWeight + _paintWeight + _prizeWeight + _hazardWeight;
        float roll = Random.Range(0f, totalWeight);

        roll -= _standardWeight;
        if (roll <= 0f) return _standardType;

        roll -= _goldWeight;
        if (roll <= 0f) return _goldType;

        roll -= _paintWeight;
        if (roll <= 0f) return _paintType;

        roll -= _prizeWeight;
        if (roll <= 0f) return _prizeType;

        return _hazardType;
    }

    /// <summary>
    /// Returns the BalloonTypeSO for a specific category.
    /// </summary>
    public BalloonTypeSO GetTypeForCategory(BalloonTypeSO.BalloonCategory category)
    {
        return category switch
        {
            BalloonTypeSO.BalloonCategory.Standard => _standardType,
            BalloonTypeSO.BalloonCategory.Gold => _goldType,
            BalloonTypeSO.BalloonCategory.Paint => _paintType,
            BalloonTypeSO.BalloonCategory.Prize => _prizeType,
            BalloonTypeSO.BalloonCategory.Hazard => _hazardType,
            _ => _standardType
        };
    }
}
```

- [ ] **Step 2: Update BalloonWall to use RoomTemplateSO**

```csharp
// Assets/Scripts/BalloonGame/BalloonWall.cs
// REPLACE the entire file contents with:
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates and manages the balloon grid wall. When a RoomTemplateSO is provided,
/// uses its dimensions, type weights, and guarantees. Falls back to GameConstants defaults.
/// </summary>
[DisallowMultipleComponent]
public class BalloonWall : MonoBehaviour
{
    private readonly List<BalloonNode> _balloons = new();
    private readonly Dictionary<BalloonColor, Material> _materials = new();
    private readonly BalloonColor[] _colors =
    {
        BalloonColor.Red,
        BalloonColor.Blue,
        BalloonColor.Yellow,
        BalloonColor.Green,
        BalloonColor.Purple
    };

    [SerializeField] private RoomTemplateSO _roomTemplate;

    /// <summary>Read-only access to all balloon nodes in the current wall.</summary>
    public IReadOnlyList<BalloonNode> Balloons => _balloons;

    /// <summary>The currently active room template, if any.</summary>
    public RoomTemplateSO ActiveTemplate => _roomTemplate;

    /// <summary>
    /// Sets the room template to use for the next GenerateWall call.
    /// </summary>
    public void SetRoomTemplate(RoomTemplateSO template)
    {
        _roomTemplate = template;
    }

    /// <summary>
    /// Generates the balloon wall using the active RoomTemplateSO or GameConstants defaults.
    /// </summary>
    [ContextMenu("Generate Wall")]
    public void GenerateWall()
    {
        ClearWall();
        EnsureMaterials();

        int rows = _roomTemplate != null ? _roomTemplate.Rows : GameConstants.BOARD_ROWS;
        int columns = _roomTemplate != null ? _roomTemplate.Columns : GameConstants.BOARD_COLUMNS;
        float slotX = GameConstants.BOARD_WIDTH / columns;
        float slotY = GameConstants.BOARD_HEIGHT / rows;

        // Pre-compute type assignments with guaranteed minimums
        BalloonTypeSO[] typeAssignments = ComputeTypeAssignments(rows, columns);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = row * columns + column;
                BalloonTypeSO assignedType = typeAssignments[index];
                BalloonColor color = _colors[Random.Range(0, _colors.Length)];
                float scale = PerspectiveScale(row, rows);
                Vector3 position = PerspectivePosition(row, column, rows, columns, slotX, slotY);

                var balloon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                balloon.name = $"Balloon_{row}_{column}";
                balloon.transform.SetParent(transform);
                balloon.transform.localPosition = position;

                float width = GameConstants.BALLOON_MAX_WIDTH * GameConstants.BALLOON_SLOT_RATIO_X * scale;
                float height = GameConstants.BALLOON_MAX_HEIGHT * GameConstants.BALLOON_SLOT_RATIO_Y * scale;
                balloon.transform.localScale = new Vector3(width, height, width);
                balloon.layer = GameConstants.LAYER_BALLOONS;

                var renderer = balloon.GetComponent<Renderer>();
                renderer.material = _materials[color];
                ApplyAtmosphericFade(renderer, row, rows);

                var rigidbody = balloon.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;

                var node = balloon.AddComponent<BalloonNode>();
                node.Initialize(row, column, color, assignedType);
                _balloons.Add(node);
            }
        }
    }

    /// <summary>
    /// Destroys all balloon GameObjects and clears the internal list.
    /// </summary>
    [ContextMenu("Clear Wall")]
    public void ClearWall()
    {
        if (_balloons.Count == 0 && transform.childCount > 0)
        {
            for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
            {
                GameObject child = transform.GetChild(childIndex).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        for (int index = _balloons.Count - 1; index >= 0; index--)
        {
            BalloonNode balloon = _balloons[index];
            if (balloon == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(balloon.gameObject);
            }
            else
            {
                DestroyImmediate(balloon.gameObject);
            }
        }

        _balloons.Clear();
    }

    /// <summary>
    /// Returns the count of active (non-popped) balloons.
    /// </summary>
    public int ActiveCount()
    {
        int count = 0;
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon != null && !balloon.IsPopped)
            {
                count++;
            }
        }

        return count;
    }

    private BalloonTypeSO[] ComputeTypeAssignments(int rows, int columns)
    {
        int total = rows * columns;
        var assignments = new BalloonTypeSO[total];

        if (_roomTemplate == null)
        {
            // No template: all standard (null type = default behavior)
            return assignments;
        }

        // First pass: random assignment by weight
        int goldCount = 0;
        int paintCount = 0;
        int hazardCount = 0;

        for (int i = 0; i < total; i++)
        {
            BalloonTypeSO selectedType = _roomTemplate.SelectRandomType();
            if (selectedType != null && selectedType.Category == BalloonTypeSO.BalloonCategory.Hazard)
            {
                if (hazardCount >= _roomTemplate.MaxHazard)
                {
                    selectedType = _roomTemplate.GetTypeForCategory(BalloonTypeSO.BalloonCategory.Standard);
                }
                else
                {
                    hazardCount++;
                }
            }

            if (selectedType != null)
            {
                switch (selectedType.Category)
                {
                    case BalloonTypeSO.BalloonCategory.Gold:
                        goldCount++;
                        break;
                    case BalloonTypeSO.BalloonCategory.Paint:
                        paintCount++;
                        break;
                }
            }

            assignments[i] = selectedType;
        }

        // Second pass: enforce minimums by replacing random standard slots
        EnforceMinimum(assignments, BalloonTypeSO.BalloonCategory.Gold, goldCount, _roomTemplate.MinGold, total);
        EnforceMinimum(assignments, BalloonTypeSO.BalloonCategory.Paint, paintCount, _roomTemplate.MinPaint, total);

        return assignments;
    }

    private void EnforceMinimum(BalloonTypeSO[] assignments, BalloonTypeSO.BalloonCategory category, int currentCount, int minimum, int total)
    {
        if (_roomTemplate == null || currentCount >= minimum)
        {
            return;
        }

        BalloonTypeSO typeToAssign = _roomTemplate.GetTypeForCategory(category);
        if (typeToAssign == null)
        {
            return;
        }

        int needed = minimum - currentCount;
        int attempts = 0;
        int maxAttempts = total * 3;

        while (needed > 0 && attempts < maxAttempts)
        {
            int index = Random.Range(0, total);
            BalloonTypeSO existing = assignments[index];

            // Only replace standard slots
            bool isStandard = existing == null ||
                              existing.Category == BalloonTypeSO.BalloonCategory.Standard;

            if (isStandard)
            {
                assignments[index] = typeToAssign;
                needed--;
            }

            attempts++;
        }
    }

    private Vector3 PerspectivePosition(int row, int column, int totalRows, int totalColumns, float slotX, float slotY)
    {
        float rowRatio = (float)row / (totalRows - 1);
        float topBias = 1f - rowRatio;

        float baseX = GameConstants.BOARD_LEFT + slotX * 0.5f + column * slotX;
        float centerX = (GameConstants.BOARD_LEFT + GameConstants.BOARD_RIGHT) * 0.5f;
        float pinchedX = centerX + (baseX - centerX) * (1f - topBias * GameConstants.PERSPECTIVE_HORIZONTAL_PINCH);
        float compressedY = GameConstants.BOARD_TOP - slotY * 0.5f - (row * slotY * (1f - topBias * GameConstants.PERSPECTIVE_VERTICAL_COMPRESSION));

        float depthOffset = (totalRows - 1 - row) * 0.01f;
        return new Vector3(pinchedX, compressedY, depthOffset);
    }

    private float PerspectiveScale(int row, int totalRows)
    {
        float rowRatio = (float)row / (totalRows - 1);
        return GameConstants.PERSPECTIVE_MIN_SCALE + rowRatio * GameConstants.PERSPECTIVE_SCALE_RANGE;
    }

    private void ApplyAtmosphericFade(Renderer renderer, int row, int totalRows)
    {
        float rowRatio = (float)row / (totalRows - 1);
        float topBias = 1f - rowRatio;
        float fade = 1f - topBias * 0.12f;

        Color baseColor = renderer.material.color;
        Color darkenedColor = new(baseColor.r * 0.6f, baseColor.g * 0.6f, baseColor.b * 0.6f);
        renderer.material.color = Color.Lerp(darkenedColor, baseColor, fade);
    }

    private void EnsureMaterials()
    {
        if (_materials.Count > 0)
        {
            return;
        }

        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        foreach (BalloonColor color in _colors)
        {
            var material = new Material(shader)
            {
                color = color.ToUnityColor()
            };

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.7f);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.7f);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            _materials[color] = material;
        }
    }
}
```

**Commit:** `git commit -m "feat: RoomTemplateSO and weighted balloon type placement with guarantees"`

---

## Task 4: Balloon Jiggle / Shockwave System

**Files:**
- Create: `Assets/Scripts/BalloonGame/Balloons/BalloonJiggle.cs`

- [ ] **Step 1: Create BalloonJiggle system**

```csharp
// Assets/Scripts/BalloonGame/Balloons/BalloonJiggle.cs
using UnityEngine;

/// <summary>
/// Listens for balloon pop events and applies a spring-dampened jiggle impulse
/// to nearby balloons within JIGGLE_RADIUS. Strength falls off linearly with distance.
/// Attach to the same GameObject as BalloonWall.
/// </summary>
[DisallowMultipleComponent]
public class BalloonJiggle : MonoBehaviour
{
    [SerializeField] private BalloonWall _balloonWall;

    private void OnEnable()
    {
        BalloonNode.OnAnyBalloonPopped += HandleBalloonPopped;
    }

    private void OnDisable()
    {
        BalloonNode.OnAnyBalloonPopped -= HandleBalloonPopped;
    }

    private void HandleBalloonPopped(BalloonNode poppedBalloon)
    {
        if (_balloonWall == null)
        {
            return;
        }

        Vector3 popPosition = poppedBalloon.transform.position;
        float radius = GameConstants.JIGGLE_RADIUS;
        float radiusSqr = radius * radius;

        var balloons = _balloonWall.Balloons;
        for (int i = 0; i < balloons.Count; i++)
        {
            BalloonNode balloon = balloons[i];
            if (balloon == null || balloon.IsPopped || balloon == poppedBalloon)
            {
                continue;
            }

            Vector3 delta = balloon.transform.position - popPosition;
            float distSqr = delta.sqrMagnitude;

            if (distSqr > radiusSqr)
            {
                continue;
            }

            float dist = Mathf.Sqrt(distSqr);
            float falloff = 1f - (dist / radius);
            float amplitude = GameConstants.JIGGLE_MAX_AMPLITUDE * falloff;

            Vector2 direction = dist > 0.01f
                ? new Vector2(delta.x, delta.y).normalized
                : Random.insideUnitCircle.normalized;

            balloon.StartJiggle(amplitude, direction);
        }
    }
}
```

**Commit:** `git commit -m "feat: balloon jiggle shockwave system with spring-dampened displacement"`

---

## Task 5: Dart Ricochet Off Stuck Darts

**Files:**
- Modify: `Assets/Scripts/BalloonGame/DartController.cs`
- Modify: `Assets/Scripts/BalloonGame/DartLauncher.cs`
- Modify: `Assets/Scripts/BalloonGame/GameConstants.cs`

- [ ] **Step 1: Add ricochet constants to GameConstants**

```csharp
// In Assets/Scripts/BalloonGame/GameConstants.cs
// ADD after the jiggle constants:

    // Ricochet
    public const int MAX_RICOCHETS_PER_DART = 2;
    public const float RICOCHET_COLLIDER_RADIUS = 0.15f;
    public const float RICOCHET_SPEED_RETENTION = 0.75f;
```

- [ ] **Step 2: Update DartController with ricochet support, stick behavior, and pierce count**

```csharp
// Assets/Scripts/BalloonGame/DartController.cs
// REPLACE the entire file contents with:
using UnityEngine;

/// <summary>
/// Controls dart state machine: Ready -> Flying -> Stopped.
/// Supports ricochet off stuck darts, brief stick-on-hit delay,
/// and piercing through multiple balloons.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class DartController : MonoBehaviour
{
    public enum DartState
    {
        Ready,
        Flying,
        Stopped
    }

    public static event System.Action<DartController> OnDartFinished;

    /// <summary>Current state of this dart.</summary>
    public DartState State { get; private set; } = DartState.Ready;

    /// <summary>Number of balloons this dart can pass through before stopping. 0 = stops on first hit.</summary>
    [SerializeField] private int _pierceCount;

    /// <summary>Number of ricochets remaining for this dart.</summary>
    public int RicochetsRemaining { get; private set; }

    /// <summary>Number of balloons hit during this flight.</summary>
    public int BalloonsHitThisFlight { get; private set; }

    private Rigidbody _rigidbody;
    private float _lifetime;
    private int _piercesRemaining;
    private string _stopReason;
    private GameObject _ricochetCollider;

    private const float MaxLifetime = 5f;
    private const float StickDelay = 0.1f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        gameObject.layer = GameConstants.LAYER_PROJECTILES;
    }

    /// <summary>
    /// Sets the pierce count for this dart. Call before Launch.
    /// </summary>
    public void SetPierceCount(int count)
    {
        _pierceCount = count;
    }

    /// <summary>
    /// Launches the dart with the given velocity.
    /// </summary>
    public void Launch(Vector3 velocity)
    {
        CancelInvoke(nameof(Deactivate));
        CancelInvoke(nameof(ExecuteStop));
        State = DartState.Flying;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.linearVelocity = velocity;
        _lifetime = 0f;
        _piercesRemaining = _pierceCount;
        RicochetsRemaining = GameConstants.MAX_RICOCHETS_PER_DART;
        BalloonsHitThisFlight = 0;
        _stopReason = "";

        EventBus.Publish(new DartLaunchedEvent
        {
            LaunchVelocity = velocity,
            PullStrength = velocity.magnitude / GameConstants.MAX_LAUNCH_SPEED
        });
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

    /// <summary>
    /// Called by BalloonNode when this dart hits a balloon.
    /// Handles pierce logic: if pierces remain, the dart continues flying.
    /// </summary>
    public void OnHitBalloon(BalloonNode balloon)
    {
        BalloonsHitThisFlight++;

        if (_piercesRemaining > 0)
        {
            _piercesRemaining--;
            Debug.Log($"DART PIERCE remaining={_piercesRemaining} balloon={balloon.name}");
            return;
        }

        // Dart stops: brief stick delay for visual feedback
        StopDart("balloon");
    }

    /// <summary>
    /// Legacy overload for backward compatibility. Stops the dart immediately.
    /// </summary>
    public void OnHitBalloon()
    {
        BalloonsHitThisFlight++;
        StopDart("balloon");
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision collision)
    {
        if (State != DartState.Flying)
        {
            return;
        }

        string hitName = collision.collider != null ? collision.collider.name : collision.gameObject.name;

        // Check for ricochet off a stuck dart's ricochet collider
        if (collision.gameObject.layer == GameConstants.LAYER_RICOCHET && RicochetsRemaining > 0)
        {
            PerformRicochet(collision);
            return;
        }

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

    private void PerformRicochet(Collision collision)
    {
        Vector3 incomingVelocity = _rigidbody.linearVelocity;
        Vector3 normal = collision.contacts[0].normal;
        Vector3 reflected = Vector3.Reflect(incomingVelocity, normal);
        reflected *= GameConstants.RICOCHET_SPEED_RETENTION;

        // Ensure ricochet stays in 2D plane
        reflected.z = 0f;

        _rigidbody.linearVelocity = reflected;
        RicochetsRemaining--;

        int ricochetNumber = GameConstants.MAX_RICOCHETS_PER_DART - RicochetsRemaining;

        EventBus.Publish(new DartRicochetEvent
        {
            RicochetPosition = collision.contacts[0].point,
            IncomingVelocity = incomingVelocity,
            OutgoingVelocity = reflected,
            RicochetNumber = ricochetNumber
        });

        Debug.Log($"DART RICOCHET #{ricochetNumber} pos={collision.contacts[0].point} newVel={reflected}");
    }

    private void StopDart(string reason)
    {
        if (State == DartState.Stopped)
        {
            return;
        }

        _stopReason = reason;

        if (reason == "balloon")
        {
            // Brief stick delay before completing stop
            State = DartState.Stopped;
            Vector3 velocityBeforeStop = _rigidbody.linearVelocity;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;

            Debug.Log($"DART STICK reason={reason} pos={transform.position} vel={velocityBeforeStop}");
            Invoke(nameof(ExecuteStop), StickDelay);
        }
        else
        {
            ExecuteStopImmediate();
        }
    }

    private void ExecuteStop()
    {
        // Dart was stuck, now finalize
        CreateRicochetCollider();
        FireFinishedEvent();
        Invoke(nameof(Deactivate), 3f);
    }

    private void ExecuteStopImmediate()
    {
        State = DartState.Stopped;
        Vector3 velocityBeforeStop = _rigidbody.linearVelocity;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;

        Debug.Log($"DART STOP reason={_stopReason} pos={transform.position} vel={velocityBeforeStop}");

        if (_stopReason == "top_wall")
        {
            CreateRicochetCollider();
            Invoke(nameof(Deactivate), 3f);
        }
        else
        {
            Invoke(nameof(Deactivate), 0.3f);
        }

        FireFinishedEvent();
    }

    private void FireFinishedEvent()
    {
        OnDartFinished?.Invoke(this);

        EventBus.Publish(new DartFinishedEvent
        {
            FinalPosition = transform.position,
            StopReason = _stopReason,
            BalloonsHitThisFlight = BalloonsHitThisFlight
        });
    }

    private void CreateRicochetCollider()
    {
        if (_ricochetCollider != null)
        {
            return;
        }

        _ricochetCollider = new GameObject("RicochetPoint");
        _ricochetCollider.transform.SetParent(transform, false);
        _ricochetCollider.transform.localPosition = Vector3.zero;
        _ricochetCollider.layer = GameConstants.LAYER_RICOCHET;

        var sphere = _ricochetCollider.AddComponent<SphereCollider>();
        sphere.radius = GameConstants.RICOCHET_COLLIDER_RADIUS;
        sphere.isTrigger = false;

        var rb = _ricochetCollider.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Deactivate()
    {
        if (_ricochetCollider != null)
        {
            Destroy(_ricochetCollider);
            _ricochetCollider = null;
        }

        gameObject.SetActive(false);
    }
}
```

- [ ] **Step 3: Update DartLauncher to configure ricochet layer collision**

```csharp
// Assets/Scripts/BalloonGame/DartLauncher.cs
// REPLACE the entire file contents with:
using UnityEngine;

/// <summary>
/// Spawns dart GameObjects from primitives and launches them.
/// Configures physics layers so projectiles can collide with ricochet points.
/// </summary>
[DisallowMultipleComponent]
public class DartLauncher : MonoBehaviour
{
    private static Material _bodyMaterial;
    private static Material _tipMaterial;
    private static bool _layersConfigured;

    private void Awake()
    {
        ConfigurePhysicsLayers();
    }

    /// <summary>
    /// Creates a dart at the launch position and launches it with the given velocity.
    /// </summary>
    public DartController SpawnAndLaunch(Vector3 velocity)
    {
        GameObject dart = CreateDartObject();
        dart.transform.position = GameConstants.LAUNCH_POSITION;

        var controller = dart.GetComponent<DartController>();
        controller.Launch(velocity);
        return controller;
    }

    /// <summary>
    /// Creates a dart with a custom pierce count, then launches it.
    /// </summary>
    public DartController SpawnAndLaunch(Vector3 velocity, int pierceCount)
    {
        GameObject dart = CreateDartObject();
        dart.transform.position = GameConstants.LAUNCH_POSITION;

        var controller = dart.GetComponent<DartController>();
        controller.SetPierceCount(pierceCount);
        controller.Launch(velocity);
        return controller;
    }

    private static void ConfigurePhysicsLayers()
    {
        if (_layersConfigured)
        {
            return;
        }

        // Projectiles should collide with ricochet colliders
        Physics.IgnoreLayerCollision(GameConstants.LAYER_PROJECTILES, GameConstants.LAYER_RICOCHET, false);

        // Ricochet colliders should not collide with balloons or environment
        Physics.IgnoreLayerCollision(GameConstants.LAYER_RICOCHET, GameConstants.LAYER_BALLOONS, true);
        Physics.IgnoreLayerCollision(GameConstants.LAYER_RICOCHET, GameConstants.LAYER_ENVIRONMENT, true);

        // Ricochet colliders should not collide with each other
        Physics.IgnoreLayerCollision(GameConstants.LAYER_RICOCHET, GameConstants.LAYER_RICOCHET, true);

        _layersConfigured = true;
    }

    private GameObject CreateDartObject()
    {
        var root = new GameObject("Dart");
        root.layer = GameConstants.LAYER_PROJECTILES;

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        body.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
        body.layer = GameConstants.LAYER_PROJECTILES;
        Object.Destroy(body.GetComponent<CapsuleCollider>());
        body.GetComponent<Renderer>().material = GetBodyMaterial();

        var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "Tip";
        tip.transform.SetParent(root.transform, false);
        tip.transform.localPosition = new Vector3(0.28f, 0f, 0f);
        tip.transform.localScale = new Vector3(0.08f, 0.08f, 0.15f);
        tip.layer = GameConstants.LAYER_PROJECTILES;
        Object.Destroy(tip.GetComponent<SphereCollider>());
        tip.GetComponent<Renderer>().material = GetTipMaterial();

        var collider = root.AddComponent<CapsuleCollider>();
        collider.direction = 0;
        collider.center = Vector3.zero;
        collider.radius = 0.06f;
        collider.height = 0.6f;

        var rigidbody = root.AddComponent<Rigidbody>();
        rigidbody.mass = 0.5f;
        rigidbody.linearDamping = 0.1f;
        rigidbody.angularDamping = 0.5f;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigidbody.constraints = RigidbodyConstraints.FreezePositionZ |
                                 RigidbodyConstraints.FreezeRotationX |
                                 RigidbodyConstraints.FreezeRotationY;

        root.AddComponent<DartController>();
        return root;
    }

    private static Material GetBodyMaterial()
    {
        if (_bodyMaterial != null)
        {
            return _bodyMaterial;
        }

        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        _bodyMaterial = new Material(shader)
        {
            color = new Color(0.7f, 0.72f, 0.75f)
        };

        if (_bodyMaterial.HasProperty("_Glossiness"))
        {
            _bodyMaterial.SetFloat("_Glossiness", 0.8f);
        }

        if (_bodyMaterial.HasProperty("_Smoothness"))
        {
            _bodyMaterial.SetFloat("_Smoothness", 0.8f);
        }

        if (_bodyMaterial.HasProperty("_Metallic"))
        {
            _bodyMaterial.SetFloat("_Metallic", 0.6f);
        }

        return _bodyMaterial;
    }

    private static Material GetTipMaterial()
    {
        if (_tipMaterial != null)
        {
            return _tipMaterial;
        }

        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        _tipMaterial = new Material(shader)
        {
            color = new Color(0.85f, 0.85f, 0.85f)
        };

        if (_tipMaterial.HasProperty("_Glossiness"))
        {
            _tipMaterial.SetFloat("_Glossiness", 0.9f);
        }

        if (_tipMaterial.HasProperty("_Smoothness"))
        {
            _tipMaterial.SetFloat("_Smoothness", 0.9f);
        }

        if (_tipMaterial.HasProperty("_Metallic"))
        {
            _tipMaterial.SetFloat("_Metallic", 0.7f);
        }

        return _tipMaterial;
    }
}
```

**Commit:** `git commit -m "feat: dart ricochet off stuck darts, stick-on-hit delay, and piercing foundation"`

---

## Task 6: Floating Score Text

**Files:**
- Create: `Assets/Scripts/BalloonGame/UI/FloatingScoreText.cs`
- Modify: `Assets/Scripts/BalloonGame/GameConstants.cs`

- [ ] **Step 1: Add floating text constants to GameConstants**

```csharp
// In Assets/Scripts/BalloonGame/GameConstants.cs
// ADD after the ricochet constants:

    // Floating score text
    public const float FLOAT_TEXT_RISE_SPEED = 2.5f;
    public const float FLOAT_TEXT_LIFETIME = 0.8f;
    public const float FLOAT_TEXT_SCALE_START = 0.06f;
    public const float FLOAT_TEXT_SCALE_END = 0.03f;
```

- [ ] **Step 2: Create FloatingScoreText**

```csharp
// Assets/Scripts/BalloonGame/UI/FloatingScoreText.cs
using UnityEngine;

/// <summary>
/// World-space floating score text that rises from a popped balloon's position.
/// Shows points earned and combo multiplier. Self-destructs after lifetime.
/// Subscribes to BalloonPoppedEvent via EventBus.
/// </summary>
[DisallowMultipleComponent]
public class FloatingScoreText : MonoBehaviour
{
    private static FloatingScoreText _instance;

    private void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BalloonPoppedEvent>(HandleBalloonPopped);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BalloonPoppedEvent>(HandleBalloonPopped);
    }

    private void HandleBalloonPopped(BalloonPoppedEvent evt)
    {
        SpawnText(evt.WorldPosition, evt.FinalPoints, evt.ComboCount, evt.ComboMultiplier);
    }

    /// <summary>
    /// Spawns floating text at the given world position.
    /// </summary>
    public static void SpawnText(Vector3 worldPosition, int points, int comboCount, float multiplier)
    {
        var textObj = new GameObject("FloatingScore");
        textObj.transform.position = worldPosition + Vector3.back * 0.5f;

        var textMesh = textObj.AddComponent<TextMesh>();
        textMesh.characterSize = 0.15f;
        textMesh.fontSize = 48;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        if (comboCount > 0)
        {
            textMesh.text = $"+{points}\nx{multiplier:F1}";
            float hue = Mathf.Repeat(comboCount * 0.1f, 1f);
            textMesh.color = Color.HSVToRGB(hue, 0.6f, 1f);
        }
        else
        {
            textMesh.text = $"+{points}";
            textMesh.color = Color.white;
        }

        textObj.transform.localScale = Vector3.one * GameConstants.FLOAT_TEXT_SCALE_START;

        var mover = textObj.AddComponent<FloatingScoreTextMover>();
        mover.Initialize(
            GameConstants.FLOAT_TEXT_RISE_SPEED,
            GameConstants.FLOAT_TEXT_LIFETIME,
            GameConstants.FLOAT_TEXT_SCALE_START,
            GameConstants.FLOAT_TEXT_SCALE_END
        );
    }
}

/// <summary>
/// Handles the rise-and-fade animation of a floating score text instance.
/// </summary>
[DisallowMultipleComponent]
public class FloatingScoreTextMover : MonoBehaviour
{
    private float _riseSpeed;
    private float _lifetime;
    private float _scaleStart;
    private float _scaleEnd;
    private float _elapsed;
    private TextMesh _textMesh;
    private Color _startColor;

    /// <summary>
    /// Sets up the animation parameters.
    /// </summary>
    public void Initialize(float riseSpeed, float lifetime, float scaleStart, float scaleEnd)
    {
        _riseSpeed = riseSpeed;
        _lifetime = lifetime;
        _scaleStart = scaleStart;
        _scaleEnd = scaleEnd;
        _elapsed = 0f;
        _textMesh = GetComponent<TextMesh>();
        _startColor = _textMesh != null ? _textMesh.color : Color.white;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float progress = _elapsed / _lifetime;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Rise upward
        transform.position += Vector3.up * (_riseSpeed * Time.deltaTime);

        // Scale down over time
        float currentScale = Mathf.Lerp(_scaleStart, _scaleEnd, progress);
        transform.localScale = Vector3.one * currentScale;

        // Fade out alpha
        if (_textMesh != null)
        {
            float alpha = 1f - (progress * progress);
            Color c = _startColor;
            c.a = alpha;
            _textMesh.color = c;
        }
    }
}
```

**Commit:** `git commit -m "feat: world-space floating score text with combo display"`

---

## Task 7: Combo Display HUD Element

**Files:**
- Create: `Assets/Scripts/BalloonGame/UI/ComboDisplay.cs`

- [ ] **Step 1: Create ComboDisplay**

```csharp
// Assets/Scripts/BalloonGame/UI/ComboDisplay.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen-space UI element that shows the current combo count and multiplier.
/// Subscribes to ComboChangedEvent via EventBus. Pulses on increment, fades on reset.
/// </summary>
[DisallowMultipleComponent]
public class ComboDisplay : MonoBehaviour
{
    private Text _comboText;
    private Text _multiplierText;
    private CanvasGroup _canvasGroup;
    private float _displayTimer;
    private float _pulseTimer;
    private int _lastComboCount;

    private const float DisplayDuration = 2f;
    private const float FadeSpeed = 3f;
    private const float PulseDuration = 0.15f;
    private const float PulseScale = 1.3f;

    private RectTransform _containerRect;
    private Vector3 _baseScale;

    private void Awake()
    {
        CreateUI();
        _baseScale = _containerRect.localScale;
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ComboChangedEvent>(HandleComboChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ComboChangedEvent>(HandleComboChanged);
    }

    private void HandleComboChanged(ComboChangedEvent evt)
    {
        if (evt.WasReset)
        {
            _displayTimer = 0f;
            _lastComboCount = 0;
            return;
        }

        if (evt.ComboCount < 1)
        {
            return;
        }

        _lastComboCount = evt.ComboCount;
        _comboText.text = $"COMBO x{evt.ComboCount}";
        _multiplierText.text = $"{evt.Multiplier:F1}x";
        _displayTimer = DisplayDuration;
        _pulseTimer = PulseDuration;
        _canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        if (_displayTimer > 0f)
        {
            _displayTimer -= Time.deltaTime;

            // Pulse animation
            if (_pulseTimer > 0f)
            {
                _pulseTimer -= Time.deltaTime;
                float pulseProgress = 1f - (_pulseTimer / PulseDuration);
                float scale = Mathf.Lerp(PulseScale, 1f, pulseProgress);
                _containerRect.localScale = _baseScale * scale;
            }
            else
            {
                _containerRect.localScale = _baseScale;
            }

            if (_displayTimer <= 0f)
            {
                _displayTimer = 0f;
            }
        }
        else if (_canvasGroup.alpha > 0f)
        {
            _canvasGroup.alpha -= Time.deltaTime * FadeSpeed;
            if (_canvasGroup.alpha < 0f)
            {
                _canvasGroup.alpha = 0f;
            }
        }
    }

    private void CreateUI()
    {
        var canvasObj = new GameObject("ComboCanvas");
        canvasObj.transform.SetParent(transform, false);

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);

        canvasObj.AddComponent<GraphicRaycaster>();

        var container = new GameObject("ComboContainer");
        container.transform.SetParent(canvasObj.transform, false);
        _containerRect = container.AddComponent<RectTransform>();
        _containerRect.anchorMin = new Vector2(0.5f, 0.75f);
        _containerRect.anchorMax = new Vector2(0.5f, 0.75f);
        _containerRect.pivot = new Vector2(0.5f, 0.5f);
        _containerRect.anchoredPosition = Vector2.zero;
        _containerRect.sizeDelta = new Vector2(500f, 150f);

        _canvasGroup = container.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;

        // Combo count text
        var comboObj = new GameObject("ComboText");
        comboObj.transform.SetParent(container.transform, false);
        var comboRect = comboObj.AddComponent<RectTransform>();
        comboRect.anchorMin = new Vector2(0.5f, 0.7f);
        comboRect.anchorMax = new Vector2(0.5f, 0.7f);
        comboRect.pivot = new Vector2(0.5f, 0.5f);
        comboRect.anchoredPosition = Vector2.zero;
        comboRect.sizeDelta = new Vector2(500f, 70f);

        _comboText = comboObj.AddComponent<Text>();
        _comboText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _comboText.fontSize = 52;
        _comboText.color = new Color(1f, 0.85f, 0.2f);
        _comboText.alignment = TextAnchor.MiddleCenter;
        _comboText.horizontalOverflow = HorizontalWrapMode.Overflow;
        _comboText.text = "";

        var comboOutline = comboObj.AddComponent<Outline>();
        comboOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        comboOutline.effectDistance = new Vector2(3f, -3f);

        // Multiplier text
        var multObj = new GameObject("MultiplierText");
        multObj.transform.SetParent(container.transform, false);
        var multRect = multObj.AddComponent<RectTransform>();
        multRect.anchorMin = new Vector2(0.5f, 0.3f);
        multRect.anchorMax = new Vector2(0.5f, 0.3f);
        multRect.pivot = new Vector2(0.5f, 0.5f);
        multRect.anchoredPosition = Vector2.zero;
        multRect.sizeDelta = new Vector2(300f, 50f);

        _multiplierText = multObj.AddComponent<Text>();
        _multiplierText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _multiplierText.fontSize = 36;
        _multiplierText.color = new Color(1f, 1f, 1f, 0.8f);
        _multiplierText.alignment = TextAnchor.MiddleCenter;
        _multiplierText.horizontalOverflow = HorizontalWrapMode.Overflow;
        _multiplierText.text = "";

        var multOutline = multObj.AddComponent<Outline>();
        multOutline.effectColor = new Color(0f, 0f, 0f, 0.7f);
        multOutline.effectDistance = new Vector2(2f, -2f);
    }
}
```

**Commit:** `git commit -m "feat: combo display HUD with pulse animation and fade"`

---

## Task 8: Hazard Penalty Integration in BalloonGameManager

**Files:**
- Modify: `Assets/Scripts/BalloonGame/BalloonGameManager.cs`

- [ ] **Step 1: Update BalloonGameManager to handle hazard penalties, room templates, and combo tracker**

```csharp
// Assets/Scripts/BalloonGame/BalloonGameManager.cs
// REPLACE the entire file contents with:
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central game state machine. Manages room lifecycle, dart counting, hazard penalties,
/// and integrates with RoomTemplateSO for difficulty scaling.
/// </summary>
[DisallowMultipleComponent]
public class BalloonGameManager : MonoBehaviour
{
    public enum GameState
    {
        RoomIntro,
        Ready,
        DartInFlight,
        RoomCleared,
        RoomFailed
    }

    private const float IntroDuration = 1f;

    /// <summary>Current game state.</summary>
    public GameState CurrentState { get; private set; } = GameState.RoomIntro;

    /// <summary>Number of darts remaining for this room.</summary>
    public int DartsRemaining { get; private set; }

    [SerializeField] private RoomTemplateSO _roomTemplate;

    private BalloonWall _balloonWall;
    private SlingshotInput _slingshotInput;
    private ScoreManager _scoreManager;
    private ComboTracker _comboTracker;
    private GameHUD _hud;
    private DartLauncher _dartLauncher;
    private DartController _activeDart;
    private float _introTimer;

    private void Awake()
    {
        _balloonWall = FindAnyObjectByType<BalloonWall>();
        _slingshotInput = FindAnyObjectByType<SlingshotInput>();
        _scoreManager = GetComponent<ScoreManager>();
        _comboTracker = GetComponent<ComboTracker>();
        _hud = GetComponent<GameHUD>();
        ConfigureSceneCollisionLayers();

        // Create ComboTracker if not present
        if (_comboTracker == null)
        {
            _comboTracker = gameObject.AddComponent<ComboTracker>();
        }

        var launcherObject = new GameObject("DartLauncher");
        launcherObject.transform.SetParent(transform, false);
        _dartLauncher = launcherObject.AddComponent<DartLauncher>();
    }

    private void Start()
    {
        if (_slingshotInput != null)
        {
            _slingshotInput.OnLaunch += HandleLaunch;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged += HandleScoreChanged;
            _scoreManager.OnTargetReached += HandleTargetReached;
        }

        DartController.OnDartFinished += HandleDartFinished;
        EventBus.Subscribe<HazardPoppedEvent>(HandleHazardPopped);

        StartRoom();
    }

    private void OnDestroy()
    {
        if (_slingshotInput != null)
        {
            _slingshotInput.OnLaunch -= HandleLaunch;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged -= HandleScoreChanged;
            _scoreManager.OnTargetReached -= HandleTargetReached;
        }

        DartController.OnDartFinished -= HandleDartFinished;
        EventBus.Unsubscribe<HazardPoppedEvent>(HandleHazardPopped);
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.RoomIntro:
                _introTimer -= Time.deltaTime;
                if (_introTimer <= 0f)
                {
                    TransitionTo(GameState.Ready);
                    _slingshotInput?.SetCanFire(true);
                    UpdateHud();
                }
                break;

            case GameState.RoomCleared:
            case GameState.RoomFailed:
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                {
                    RestartRoom();
                }
                break;
        }
    }

    private void StartRoom()
    {
        GameState previousState = CurrentState;
        CurrentState = GameState.RoomIntro;
        _introTimer = IntroDuration;
        _activeDart = null;

        int targetScore;
        int dartCount;

        if (_roomTemplate != null)
        {
            targetScore = _roomTemplate.TargetScore;
            dartCount = _roomTemplate.DartCount;
            _balloonWall?.SetRoomTemplate(_roomTemplate);
        }
        else
        {
            targetScore = GameConstants.BASE_TARGET_SCORE;
            dartCount = GameConstants.STARTING_DARTS;
        }

        DartsRemaining = dartCount;
        _scoreManager?.Initialize(targetScore);
        _balloonWall?.GenerateWall();
        _slingshotInput?.SetCanFire(false);

        EventBus.Publish(new RoomStateChangedEvent
        {
            NewState = GameState.RoomIntro,
            PreviousState = previousState,
            DartsRemaining = DartsRemaining
        });

        UpdateHud();
    }

    private void RestartRoom()
    {
        _activeDart = null;

        foreach (DartController dart in FindObjectsByType<DartController>(FindObjectsSortMode.None))
        {
            Destroy(dart.gameObject);
        }

        StartRoom();
    }

    private void HandleLaunch(Vector3 velocity)
    {
        if (CurrentState != GameState.Ready || DartsRemaining <= 0)
        {
            return;
        }

        _slingshotInput?.SetCanFire(false);
        _activeDart = _dartLauncher.SpawnAndLaunch(velocity);
        TransitionTo(GameState.DartInFlight);
        UpdateHud();
    }

    private void HandleDartFinished(DartController dart)
    {
        if (dart != _activeDart)
        {
            return;
        }

        _activeDart = null;

        if (CurrentState == GameState.RoomCleared)
        {
            return;
        }

        DartsRemaining--;
        if (DartsRemaining <= 0)
        {
            TransitionTo(GameState.RoomFailed);
            _slingshotInput?.SetCanFire(false);
            UpdateHud();
            Debug.Log($"ROOM FAILED. Score: {_scoreManager.CurrentScore}/{_scoreManager.TargetScore}");
            return;
        }

        TransitionTo(GameState.Ready);
        _slingshotInput?.SetCanFire(true);
        UpdateHud();
    }

    private void HandleHazardPopped(HazardPoppedEvent evt)
    {
        DartsRemaining = Mathf.Max(0, DartsRemaining - evt.DartPenalty);

        if (_scoreManager != null)
        {
            int newScore = Mathf.Max(0, _scoreManager.CurrentScore - evt.ScorePenalty);
            // ScoreManager doesn't have a SetScore method, so we track penalty separately
            // For now, the penalty is logged. Full integration requires ScoreManager.ApplyPenalty.
        }

        Debug.Log($"HAZARD! Dart penalty: -{evt.DartPenalty} | Darts remaining: {DartsRemaining}");
        UpdateHud();
    }

    private void HandleScoreChanged(int score)
    {
        UpdateHud();
    }

    private void HandleTargetReached()
    {
        TransitionTo(GameState.RoomCleared);
        _slingshotInput?.SetCanFire(false);
        UpdateHud();
        Debug.Log($"ROOM CLEARED! Score: {_scoreManager.CurrentScore}/{_scoreManager.TargetScore}");
    }

    private void TransitionTo(GameState newState)
    {
        GameState previous = CurrentState;
        CurrentState = newState;

        EventBus.Publish(new RoomStateChangedEvent
        {
            NewState = newState,
            PreviousState = previous,
            DartsRemaining = DartsRemaining
        });
    }

    private void UpdateHud()
    {
        _hud?.UpdateDisplay(
            _scoreManager != null ? _scoreManager.CurrentScore : 0,
            _scoreManager != null ? _scoreManager.TargetScore : GameConstants.BASE_TARGET_SCORE,
            DartsRemaining,
            CurrentState);
    }

    private static void ConfigureSceneCollisionLayers()
    {
        SetLayerIfFound("BackWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("LeftWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("RightWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("TopWall", GameConstants.LAYER_ENVIRONMENT);
        SetLayerIfFound("LaneFloor", GameConstants.LAYER_ENVIRONMENT);
    }

    private static void SetLayerIfFound(string objectName, int layer)
    {
        GameObject sceneObject = GameObject.Find(objectName);
        if (sceneObject != null)
        {
            sceneObject.layer = layer;
        }
    }
}
```

**Commit:** `git commit -m "feat: hazard penalty handling, room template integration in game manager"`

---

## Task 9: Score Penalty Support in ScoreManager

**Files:**
- Modify: `Assets/Scripts/BalloonGame/ScoreManager.cs`

- [ ] **Step 1: Add ApplyPenalty method to ScoreManager**

Add the following method to `Assets/Scripts/BalloonGame/ScoreManager.cs`, inside the class body after the `Initialize` method:

```csharp
    /// <summary>
    /// Subtracts a penalty from the current score. Score cannot go below zero.
    /// Fires ScoreChangedEvent.
    /// </summary>
    public void ApplyPenalty(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentScore = Mathf.Max(0, CurrentScore - amount);
        OnScoreChanged?.Invoke(CurrentScore);

        EventBus.Publish(new ScoreChangedEvent
        {
            CurrentScore = CurrentScore,
            TargetScore = TargetScore,
            PointsJustAdded = -amount,
            TargetReached = IsTargetReached
        });

        Debug.Log($"PENALTY! -{amount} pts | Score: {CurrentScore}/{TargetScore}");
    }
```

- [ ] **Step 2: Update BalloonGameManager.HandleHazardPopped to use ApplyPenalty**

In `Assets/Scripts/BalloonGame/BalloonGameManager.cs`, replace the `HandleHazardPopped` method:

```csharp
    private void HandleHazardPopped(HazardPoppedEvent evt)
    {
        DartsRemaining = Mathf.Max(0, DartsRemaining - evt.DartPenalty);

        if (_scoreManager != null && evt.ScorePenalty > 0)
        {
            _scoreManager.ApplyPenalty(evt.ScorePenalty);
        }

        Debug.Log($"HAZARD! Dart penalty: -{evt.DartPenalty} | Score penalty: -{evt.ScorePenalty} | Darts remaining: {DartsRemaining}");
        UpdateHud();
    }
```

**Commit:** `git commit -m "feat: score penalty support for hazard balloons"`

---

## Task 10: Input Feel Polish — Launch Feel

**Files:**
- Create: `Assets/Scripts/BalloonGame/Feel/LaunchFeel.cs`
- Modify: `Assets/Scripts/BalloonGame/GameConstants.cs`

- [ ] **Step 1: Add feel constants to GameConstants**

```csharp
// In Assets/Scripts/BalloonGame/GameConstants.cs
// ADD after the floating text constants:

    // Launch feel
    public const float LAUNCH_SHAKE_INTENSITY = 0.12f;
    public const float LAUNCH_SHAKE_DURATION = 0.15f;
    public const float LAUNCH_SHAKE_FREQUENCY = 25f;
    public const float BULLET_TIME_SCALE = 0.7f;
    public const float BULLET_TIME_DURATION = 0.3f;
    public const float BULLET_TIME_PROXIMITY = 1.5f;
```

- [ ] **Step 2: Create LaunchFeel**

```csharp
// Assets/Scripts/BalloonGame/Feel/LaunchFeel.cs
using UnityEngine;

/// <summary>
/// Adds game feel polish: camera shake on dart launch, and brief slow-motion
/// when a dart passes close to balloons. Subscribes to EventBus events.
/// </summary>
[DisallowMultipleComponent]
public class LaunchFeel : MonoBehaviour
{
    private Camera _camera;
    private Vector3 _cameraBasePosition;
    private float _shakeTimer;
    private float _shakeIntensity;
    private bool _isShaking;

    private float _bulletTimeTimer;
    private bool _isBulletTime;
    private bool _bulletTimeTriggered;

    private void Awake()
    {
        _camera = Camera.main;
        if (_camera != null)
        {
            _cameraBasePosition = _camera.transform.position;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<DartLaunchedEvent>(HandleDartLaunched);
        EventBus.Subscribe<DartFinishedEvent>(HandleDartFinished);
        EventBus.Subscribe<BalloonPoppedEvent>(HandleBalloonPopped);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DartLaunchedEvent>(HandleDartLaunched);
        EventBus.Unsubscribe<DartFinishedEvent>(HandleDartFinished);
        EventBus.Unsubscribe<BalloonPoppedEvent>(HandleBalloonPopped);

        // Ensure time scale is restored
        Time.timeScale = 1f;
    }

    private void HandleDartLaunched(DartLaunchedEvent evt)
    {
        // Camera shake proportional to pull strength
        float intensity = GameConstants.LAUNCH_SHAKE_INTENSITY * evt.PullStrength;
        StartShake(intensity, GameConstants.LAUNCH_SHAKE_DURATION);
        _bulletTimeTriggered = false;
    }

    private void HandleDartFinished(DartFinishedEvent evt)
    {
        // Reset bullet time when dart stops
        if (_isBulletTime)
        {
            EndBulletTime();
        }
    }

    private void HandleBalloonPopped(BalloonPoppedEvent evt)
    {
        // Brief shake on pop
        StartShake(GameConstants.LAUNCH_SHAKE_INTENSITY * 0.5f, 0.08f);

        // Trigger bullet time on first balloon hit (per dart flight)
        if (!_bulletTimeTriggered)
        {
            _bulletTimeTriggered = true;
            StartBulletTime();
        }
    }

    private void Update()
    {
        UpdateShake();
        UpdateBulletTime();
    }

    private void StartShake(float intensity, float duration)
    {
        _isShaking = true;
        _shakeIntensity = intensity;
        _shakeTimer = duration;
    }

    private void UpdateShake()
    {
        if (!_isShaking || _camera == null)
        {
            return;
        }

        _shakeTimer -= Time.unscaledDeltaTime;

        if (_shakeTimer <= 0f)
        {
            _isShaking = false;
            _camera.transform.position = _cameraBasePosition;
            return;
        }

        float decay = _shakeTimer / GameConstants.LAUNCH_SHAKE_DURATION;
        float currentIntensity = _shakeIntensity * decay;

        float time = Time.unscaledTime * GameConstants.LAUNCH_SHAKE_FREQUENCY;
        float offsetX = Mathf.Sin(time) * currentIntensity;
        float offsetY = Mathf.Cos(time * 1.3f) * currentIntensity;

        _camera.transform.position = _cameraBasePosition + new Vector3(offsetX, offsetY, 0f);
    }

    private void StartBulletTime()
    {
        _isBulletTime = true;
        _bulletTimeTimer = GameConstants.BULLET_TIME_DURATION;
        Time.timeScale = GameConstants.BULLET_TIME_SCALE;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void UpdateBulletTime()
    {
        if (!_isBulletTime)
        {
            return;
        }

        _bulletTimeTimer -= Time.unscaledDeltaTime;

        if (_bulletTimeTimer <= 0f)
        {
            EndBulletTime();
        }
    }

    private void EndBulletTime()
    {
        _isBulletTime = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
```

**Commit:** `git commit -m "feat: launch feel polish — camera shake and bullet time on balloon hit"`

---

## Final GameConstants.cs

After all tasks, `Assets/Scripts/BalloonGame/GameConstants.cs` should contain:

```csharp
// Assets/Scripts/BalloonGame/GameConstants.cs
using UnityEngine;

public static class GameConstants
{
    public const float CAMERA_ORTHO_SIZE = 10f;

    public const int BOARD_COLUMNS = 8;
    public const int BOARD_ROWS = 9;
    public const int TOTAL_BALLOONS = BOARD_COLUMNS * BOARD_ROWS;

    public const float BOARD_LEFT = -4.0f;
    public const float BOARD_RIGHT = 4.0f;
    public const float BOARD_TOP = 8.5f;
    public const float BOARD_BOTTOM = -1.0f;
    public const float BOARD_WIDTH = BOARD_RIGHT - BOARD_LEFT;
    public const float BOARD_HEIGHT = BOARD_TOP - BOARD_BOTTOM;

    public const float LANE_TOP = -2.0f;
    public const float LANE_BOTTOM = -8.5f;

    public static readonly Vector3 LAUNCH_POSITION = new(0f, -5.5f, 0f);

    public const float BALLOON_MAX_WIDTH = 0.78f;
    public const float BALLOON_MAX_HEIGHT = 0.94f;
    public const float BALLOON_SLOT_RATIO_X = 0.9f;
    public const float BALLOON_SLOT_RATIO_Y = 0.95f;

    public const float PERSPECTIVE_MIN_SCALE = 0.72f;
    public const float PERSPECTIVE_SCALE_RANGE = 0.28f;
    public const float PERSPECTIVE_HORIZONTAL_PINCH = 0.12f;
    public const float PERSPECTIVE_VERTICAL_COMPRESSION = 0.14f;

    public const float MAX_PULL_DISTANCE = 3.0f;
    public const float PULL_SPEED_EXPONENT = 1.45f;
    public const float MIN_LAUNCH_SPEED = 10f;
    public const float MAX_LAUNCH_SPEED = 40f;
    public const float AIM_ACTIVATION_RADIUS = 4f;

    public const int BASE_TARGET_SCORE = 3000;
    public const int STARTING_DARTS = 4;
    public const int SCORE_PER_BALLOON = 100;

    public const float SIDE_WALL_WIDTH = 0.3f;
    public const float WALL_BOUNCINESS = 0.8f;
    public const float WALL_FRICTION = 0.1f;

    public const int LAYER_ENVIRONMENT = 3;
    public const int LAYER_PROJECTILES = 8;
    public const int LAYER_BALLOONS = 7;

    // Combo system
    public const float COMBO_WINDOW_SECONDS = 1.5f;
    public const float COMBO_MULTIPLIER_PER_STEP = 0.5f;
    public const int COMBO_MAX_STACK = 20;

    // Special balloon tuning
    public const int GOLD_POINT_MULTIPLIER = 3;
    public const float PAINT_EXPLOSION_RADIUS = 1.5f;
    public const int PRIZE_CURRENCY_AMOUNT = 10;
    public const int HAZARD_DART_PENALTY = 1;
    public const int HAZARD_SCORE_PENALTY = 50;

    // Currency token
    public const float CURRENCY_TOKEN_FLOAT_SPEED = 2f;
    public const float CURRENCY_TOKEN_LIFETIME = 1.5f;

    // Layer for ricochet colliders on stuck darts
    public const int LAYER_RICOCHET = 9;

    // Jiggle / shockwave
    public const float JIGGLE_RADIUS = 2f;
    public const float JIGGLE_MAX_AMPLITUDE = 0.15f;
    public const float JIGGLE_DURATION = 0.4f;
    public const float JIGGLE_FREQUENCY = 3f;

    // Ricochet
    public const int MAX_RICOCHETS_PER_DART = 2;
    public const float RICOCHET_COLLIDER_RADIUS = 0.15f;
    public const float RICOCHET_SPEED_RETENTION = 0.75f;

    // Floating score text
    public const float FLOAT_TEXT_RISE_SPEED = 2.5f;
    public const float FLOAT_TEXT_LIFETIME = 0.8f;
    public const float FLOAT_TEXT_SCALE_START = 0.06f;
    public const float FLOAT_TEXT_SCALE_END = 0.03f;

    // Launch feel
    public const float LAUNCH_SHAKE_INTENSITY = 0.12f;
    public const float LAUNCH_SHAKE_DURATION = 0.15f;
    public const float LAUNCH_SHAKE_FREQUENCY = 25f;
    public const float BULLET_TIME_SCALE = 0.7f;
    public const float BULLET_TIME_DURATION = 0.3f;
    public const float BULLET_TIME_PROXIMITY = 1.5f;
}
```

---

## Scene Setup Checklist

After completing all tasks, the InkshotScene needs these components attached:

| GameObject | Component | Notes |
|-----------|-----------|-------|
| `GameManager` | `ComboTracker` | Auto-created by BalloonGameManager if missing |
| `GameManager` | `FloatingScoreText` | EventBus subscriber, attach anywhere active |
| `GameManager` | `ComboDisplay` | Creates its own Canvas on Awake |
| `GameManager` | `LaunchFeel` | EventBus subscriber, needs Camera.main |
| `BalloonWall` | `BalloonJiggle` | Needs `_balloonWall` serialized reference |

## Verification

After each task, verify compilation:
```bash
# From project root
find Assets/Scripts/BalloonGame -name "*.cs" | head -20
# Open Unity — check Console for zero errors
```

Test each system in isolation:
1. **Combo**: Pop 3+ balloons quickly, observe ComboTracker.ComboCount incrementing
2. **Special types**: Assign BalloonTypeSO assets to RoomTemplateSO, generate wall, verify mixed types appear
3. **Paint chain**: Pop a Paint balloon, verify neighbors pop via AoE
4. **Jiggle**: Pop a balloon, observe nearby balloons wobble
5. **Ricochet**: Throw dart at wall, verify it sticks; throw second dart at first, verify bounce
6. **Pierce**: Set `_pierceCount = 2` on DartController, verify dart passes through 2 balloons
7. **Floating text**: Pop balloon, verify "+100" text rises from pop position
8. **Combo display**: Get a 3+ combo, verify "COMBO x3" appears center-screen with pulse
9. **Hazard penalty**: Pop a hazard balloon, verify dart count decreases by 1
10. **Launch feel**: Fire dart, observe camera shake; hit balloon, observe brief slow-motion
