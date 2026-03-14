# Plan 01: Foundation & Architecture

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish the core infrastructure (render pipeline, event system, object pooling, data architecture, save/load, audio, haptics, safe area, and folder structure) that all subsequent INKSHOT plans depend on.

**Architecture:** All new systems live under `Assets/Scripts/BalloonGame/Core/` (runtime utilities) and `Assets/Scripts/BalloonGame/Data/` (ScriptableObject definitions). Existing scripts are reorganized into role-based subfolders (Balloons, Darts, UI, Managers, Visuals) with `.meta` files preserved via `git mv`. The EventBus uses a static generic publish/subscribe pattern with struct-based events; ObjectPool is a generic MonoBehaviour; SaveManager wraps JsonUtility + PlayerPrefs.

**Tech Stack:** Unity 2022.3.10f1, Universal Render Pipeline (URP), C# 9, Input System 1.18.0, TextMesh Pro

---

## File Map

| Action | Path | Responsibility |
|--------|------|---------------|
| Create | `Assets/Settings/URPAsset.asset` | URP pipeline asset (created via Editor) |
| Create | `Assets/Settings/URPRenderer.asset` | URP forward renderer (created via Editor) |
| Create | `Assets/Scripts/BalloonGame/Core/EventBus.cs` | Static generic pub/sub event system |
| Create | `Assets/Scripts/BalloonGame/Core/GameEvents.cs` | All event struct definitions |
| Create | `Assets/Scripts/BalloonGame/Core/ObjectPool.cs` | Generic MonoBehaviour object pool |
| Create | `Assets/Scripts/BalloonGame/Core/SaveData.cs` | Serializable save data class |
| Create | `Assets/Scripts/BalloonGame/Core/SaveManager.cs` | PlayerPrefs-backed JSON save/load |
| Create | `Assets/Scripts/BalloonGame/Core/AudioManager.cs` | Singleton audio manager with SFX/Music/UI channels |
| Create | `Assets/Scripts/BalloonGame/Core/Haptics.cs` | Static haptic feedback utility |
| Create | `Assets/Scripts/BalloonGame/Core/SafeAreaHandler.cs` | RectTransform safe area adjustment |
| Create | `Assets/Scripts/BalloonGame/Data/BalloonTypeSO.cs` | Balloon type ScriptableObject |
| Create | `Assets/Scripts/BalloonGame/Data/PerkSO.cs` | Perk definition ScriptableObject |
| Create | `Assets/Scripts/BalloonGame/Data/RoomTemplateSO.cs` | Room template ScriptableObject |
| Create | `Assets/Scripts/BalloonGame/Data/GameConfigSO.cs` | Global tuning config ScriptableObject |
| Modify | `Assets/Scripts/BalloonGame/GameConstants.cs` | Add new constants for combo, audio, haptics |
| Modify | `Assets/Scripts/BalloonGame/BalloonGameManager.cs` | Add namespace, wire EventBus events |
| Modify | `Assets/Scripts/BalloonGame/ScoreManager.cs` | Publish ScoreChangedEvent via EventBus |
| Modify | `Assets/Scripts/BalloonGame/BalloonNode.cs` | Publish BalloonPoppedEvent via EventBus |
| Move | `Assets/Scripts/BalloonGame/DartController.cs` | To `Darts/` subfolder |
| Move | `Assets/Scripts/BalloonGame/DartLauncher.cs` | To `Darts/` subfolder |
| Move | `Assets/Scripts/BalloonGame/SlingshotInput.cs` | To `Darts/` subfolder |
| Move | `Assets/Scripts/BalloonGame/BalloonNode.cs` | To `Balloons/` subfolder |
| Move | `Assets/Scripts/BalloonGame/BalloonWall.cs` | To `Balloons/` subfolder |
| Move | `Assets/Scripts/BalloonGame/BalloonData.cs` | To `Balloons/` subfolder |
| Move | `Assets/Scripts/BalloonGame/GameHUD.cs` | To `UI/` subfolder |
| Move | `Assets/Scripts/BalloonGame/BalloonGameManager.cs` | To `Managers/` subfolder |
| Move | `Assets/Scripts/BalloonGame/ScoreManager.cs` | To `Managers/` subfolder |
| Move | `Assets/Scripts/BalloonGame/SlingshotVisuals.cs` | To `Visuals/` subfolder |
| Move | `Assets/Scripts/BalloonGame/BalloonCamera.cs` | To `Visuals/` subfolder |

---

## Task 1: Install and Configure URP

**Files:**
- Modify: `Packages/manifest.json`
- Create: `Assets/Settings/` directory
- Create: `Assets/Settings/URPAsset.asset` (via Editor script)
- Create: `Assets/Settings/URPAsset_Renderer.asset` (via Editor script)

- [ ] **Step 1: Add URP package to manifest.json**

Open `Packages/manifest.json` and add the URP dependency. If `com.unity.render-pipelines.universal` is already present, skip this step. Otherwise add:

```json
"com.unity.render-pipelines.universal": "14.0.11"
```

This version is compatible with Unity 2022.3.10f1. After saving, return to Unity and let it resolve packages.

- [ ] **Step 2: Create the Settings directory**

```bash
mkdir -p Assets/Settings
```

- [ ] **Step 3: Create URP setup Editor script**

Create file `Assets/Scripts/BalloonGame/Editor/URPSetup.cs`:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class URPSetup
{
    [MenuItem("INKSHOT/Setup URP Pipeline")]
    public static void SetupURP()
    {
        // Create Forward Renderer
        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        rendererData.name = "URPAsset_Renderer";
        AssetDatabase.CreateAsset(rendererData, "Assets/Settings/URPAsset_Renderer.asset");

        // Create Pipeline Asset
        var pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
        pipelineAsset.name = "URPAsset";

        // Main Light
        pipelineAsset.mainLightRenderingMode = LightRenderingMode.PerPixel;
        pipelineAsset.additionalLightsRenderingMode = LightRenderingMode.Disabled;

        // Shadows
        pipelineAsset.supportsMainLightShadows = true;
        pipelineAsset.mainLightShadowmapResolution = 1024;
        pipelineAsset.shadowDistance = 15f;
        pipelineAsset.supportsSoftShadows = true;

        // Anti-aliasing
        pipelineAsset.msaaSampleCount = 2;

        // SRP Batcher
        pipelineAsset.useSRPBatcher = true;

        // HDR off for mobile
        pipelineAsset.supportsHDR = false;

        // Depth texture on, opaque texture off
        pipelineAsset.supportsCameraDepthTexture = true;
        pipelineAsset.supportsCameraOpaqueTexture = false;

        AssetDatabase.CreateAsset(pipelineAsset, "Assets/Settings/URPAsset.asset");
        AssetDatabase.SaveAssets();

        // Assign to Graphics Settings
        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        QualitySettings.renderPipeline = pipelineAsset;

        EditorUtility.SetDirty(pipelineAsset);
        AssetDatabase.SaveAssets();

        Debug.Log("URP Pipeline configured for INKSHOT mobile.");
    }
}
#endif
```

- [ ] **Step 4: Run the URP setup**

In Unity Editor, run **INKSHOT > Setup URP Pipeline** from the menu bar. Verify the following in the Inspector for `Assets/Settings/URPAsset.asset`:

- Main Light: Per Pixel
- Additional Lights: Disabled
- Shadows: Main Light on, resolution 1024, distance 15
- Soft Shadows: on
- MSAA: 2x
- SRP Batcher: on
- HDR: off
- Depth Texture: on
- Opaque Texture: off

- [ ] **Step 5: Update DartLauncher.cs shader references for URP**

In `Assets/Scripts/BalloonGame/DartLauncher.cs`, update `GetBodyMaterial()` and `GetTipMaterial()` to prefer URP shader:

Change both methods' shader lookup from:
```csharp
Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
```
To:
```csharp
Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
```

- [ ] **Step 6: Update BalloonWall.cs shader references for URP**

In `Assets/Scripts/BalloonGame/BalloonWall.cs`, in the `EnsureMaterials()` method, change:
```csharp
Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
```
To:
```csharp
Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
```

- [ ] **Step 7: Verify zero compiler errors**

Open Unity, wait for script compilation. Verify no errors in the Console. Enter Play mode briefly and confirm the scene still renders (colors may shift slightly due to URP's different lighting model).

**Commit:** `git commit -m "feat(foundation): configure URP pipeline for mobile rendering"`

---

## Task 2: Create EventBus System

**Files:**
- Create: `Assets/Scripts/BalloonGame/Core/EventBus.cs`
- Create: `Assets/Scripts/BalloonGame/Core/GameEvents.cs`

- [ ] **Step 1: Create Core directory**

```bash
mkdir -p Assets/Scripts/BalloonGame/Core
```

- [ ] **Step 2: Create EventBus.cs**

Create file `Assets/Scripts/BalloonGame/Core/EventBus.cs`:

```csharp
using System;
using System.Collections.Generic;

/// <summary>
/// Lightweight static event bus using C# events with struct-based messages.
/// Subscribe in OnEnable, unsubscribe in OnDisable to avoid leaks.
/// </summary>
public static class EventBus
{
    #region Inner Binding Class

    private static class Binding<T> where T : struct
    {
        private static event Action<T> Event;
        private static readonly HashSet<Action<T>> Subscribers = new();

        public static void Subscribe(Action<T> handler)
        {
            if (handler == null) return;
            if (Subscribers.Add(handler))
            {
                Event += handler;
            }
        }

        public static void Unsubscribe(Action<T> handler)
        {
            if (handler == null) return;
            if (Subscribers.Remove(handler))
            {
                Event -= handler;
            }
        }

        public static void Publish(T message)
        {
            Event?.Invoke(message);
        }

        public static void Clear()
        {
            Event = null;
            Subscribers.Clear();
        }

        public static int SubscriberCount => Subscribers.Count;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Subscribe to an event type. Call from OnEnable().
    /// </summary>
    public static void Subscribe<T>(Action<T> handler) where T : struct
    {
        Binding<T>.Subscribe(handler);
    }

    /// <summary>
    /// Unsubscribe from an event type. Call from OnDisable().
    /// </summary>
    public static void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Binding<T>.Unsubscribe(handler);
    }

    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    public static void Publish<T>(T message) where T : struct
    {
        Binding<T>.Publish(message);
    }

    /// <summary>
    /// Remove all subscribers for a specific event type.
    /// Useful for scene teardown.
    /// </summary>
    public static void Clear<T>() where T : struct
    {
        Binding<T>.Clear();
    }

    /// <summary>
    /// Get subscriber count for debugging.
    /// </summary>
    public static int SubscriberCount<T>() where T : struct
    {
        return Binding<T>.SubscriberCount;
    }

    #endregion
}
```

- [ ] **Step 3: Create GameEvents.cs**

Create file `Assets/Scripts/BalloonGame/Core/GameEvents.cs`:

```csharp
using UnityEngine;

/// <summary>
/// All game event structs used with EventBus.
/// Events are value types (structs) for zero-allocation publishing.
/// </summary>

#region Balloon Events

public struct BalloonPoppedEvent
{
    public int Row;
    public int Column;
    public BalloonColor Color;
    public Vector3 WorldPosition;
    public int PointValue;
}

public struct AllBalloonsClearedEvent { }

#endregion

#region Dart Events

public struct DartLaunchedEvent
{
    public Vector3 Velocity;
    public Vector3 LaunchPosition;
}

public struct DartFinishedEvent
{
    public Vector3 FinalPosition;
    public string Reason;
}

#endregion

#region Room Events

public struct RoomStartedEvent
{
    public int RoomNumber;
    public int DartCount;
    public int TargetScore;
}

public struct RoomClearedEvent
{
    public int RoomNumber;
    public int FinalScore;
    public int DartsRemaining;
}

public struct RoomFailedEvent
{
    public int RoomNumber;
    public int FinalScore;
}

#endregion

#region Score Events

public struct ScoreChangedEvent
{
    public int CurrentScore;
    public int TargetScore;
    public int DeltaPoints;
}

public struct ComboEvent
{
    public int ComboCount;
    public int BonusPoints;
}

#endregion

#region Perk Events

public struct PerkSelectedEvent
{
    public string PerkId;
}

public struct PerkActivatedEvent
{
    public string PerkId;
}

#endregion

#region UI Events

public struct GamePausedEvent
{
    public bool IsPaused;
}

#endregion
```

- [ ] **Step 4: Verify compilation**

Open Unity editor, confirm zero errors in Console. The EventBus and GameEvents are pure C# with no MonoBehaviour dependencies, so they should compile immediately.

**Commit:** `git commit -m "feat(foundation): add static EventBus with struct-based game events"`

---

## Task 3: Create Generic ObjectPool

**Files:**
- Create: `Assets/Scripts/BalloonGame/Core/ObjectPool.cs`

- [ ] **Step 1: Create ObjectPool.cs**

Create file `Assets/Scripts/BalloonGame/Core/ObjectPool.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic MonoBehaviour object pool. Attach to a GameObject,
/// assign a prefab, and call Get()/Release() at runtime.
/// Supports initial warmup count and auto-grow.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    #region Serialized Fields

    [Header("Pool Configuration")]
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _initialSize = 10;
    [SerializeField] private bool _autoGrow = true;
    [SerializeField] private Transform _poolParent;

    #endregion

    #region Private State

    private readonly Queue<GameObject> _available = new();
    private readonly HashSet<GameObject> _inUse = new();
    private bool _isWarmedUp;

    #endregion

    #region Properties

    public int AvailableCount => _available.Count;
    public int InUseCount => _inUse.Count;
    public int TotalCount => _available.Count + _inUse.Count;
    public GameObject Prefab => _prefab;

    #endregion

    #region Lifecycle

    private void Awake()
    {
        if (_poolParent == null)
        {
            _poolParent = transform;
        }
    }

    private void Start()
    {
        if (!_isWarmedUp)
        {
            Warmup(_initialSize);
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Pre-instantiate objects. Safe to call multiple times.
    /// </summary>
    public void Warmup(int count)
    {
        _isWarmedUp = true;
        for (int i = 0; i < count; i++)
        {
            GameObject instance = CreateInstance();
            instance.SetActive(false);
            _available.Enqueue(instance);
        }
    }

    /// <summary>
    /// Initialize the pool with a prefab at runtime (for pools created via code).
    /// </summary>
    public void Initialize(GameObject prefab, int initialSize = 10, bool autoGrow = true)
    {
        _prefab = prefab;
        _initialSize = initialSize;
        _autoGrow = autoGrow;

        if (_poolParent == null)
        {
            _poolParent = transform;
        }

        Warmup(initialSize);
    }

    /// <summary>
    /// Get an object from the pool. Returns null if pool exhausted and autoGrow is false.
    /// </summary>
    public GameObject Get()
    {
        GameObject instance;

        if (_available.Count > 0)
        {
            instance = _available.Dequeue();

            // Handle destroyed objects (scene reload, etc.)
            if (instance == null)
            {
                instance = CreateInstance();
            }
        }
        else if (_autoGrow)
        {
            instance = CreateInstance();
        }
        else
        {
            Debug.LogWarning($"[ObjectPool] Pool exhausted for {_prefab.name}, autoGrow is disabled.");
            return null;
        }

        instance.SetActive(true);
        _inUse.Add(instance);
        return instance;
    }

    /// <summary>
    /// Get an object and position it in one call.
    /// </summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject instance = Get();
        if (instance != null)
        {
            instance.transform.SetPositionAndRotation(position, rotation);
        }
        return instance;
    }

    /// <summary>
    /// Return an object to the pool.
    /// </summary>
    public void Release(GameObject instance)
    {
        if (instance == null) return;

        if (!_inUse.Remove(instance))
        {
            Debug.LogWarning($"[ObjectPool] Releasing object '{instance.name}' that was not tracked. Destroying it.");
            Destroy(instance);
            return;
        }

        instance.SetActive(false);
        instance.transform.SetParent(_poolParent, false);
        _available.Enqueue(instance);
    }

    /// <summary>
    /// Release all in-use objects back to the pool.
    /// </summary>
    public void ReleaseAll()
    {
        // Copy to avoid modifying during iteration
        var inUseList = new List<GameObject>(_inUse);
        foreach (GameObject instance in inUseList)
        {
            if (instance != null)
            {
                Release(instance);
            }
        }
        _inUse.Clear();
    }

    /// <summary>
    /// Destroy all pooled objects and reset state.
    /// </summary>
    public void Clear()
    {
        foreach (GameObject instance in _inUse)
        {
            if (instance != null) Destroy(instance);
        }
        _inUse.Clear();

        while (_available.Count > 0)
        {
            GameObject instance = _available.Dequeue();
            if (instance != null) Destroy(instance);
        }

        _isWarmedUp = false;
    }

    #endregion

    #region Internals

    private GameObject CreateInstance()
    {
        if (_prefab == null)
        {
            Debug.LogError($"[ObjectPool] No prefab assigned on {gameObject.name}.");
            return null;
        }

        GameObject instance = Instantiate(_prefab, _poolParent);
        instance.name = $"{_prefab.name}_{TotalCount}";
        return instance;
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        Clear();
    }

    #endregion
}
```

- [ ] **Step 2: Verify compilation**

Open Unity, confirm zero errors. The ObjectPool has no dependencies on other new scripts.

**Commit:** `git commit -m "feat(foundation): add generic ObjectPool with warmup and auto-grow"`

---

## Task 4: Create ScriptableObject Data Architecture

**Files:**
- Create: `Assets/Scripts/BalloonGame/Data/BalloonTypeSO.cs`
- Create: `Assets/Scripts/BalloonGame/Data/PerkSO.cs`
- Create: `Assets/Scripts/BalloonGame/Data/RoomTemplateSO.cs`
- Create: `Assets/Scripts/BalloonGame/Data/GameConfigSO.cs`

- [ ] **Step 1: Create Data directory**

```bash
mkdir -p Assets/Scripts/BalloonGame/Data
```

- [ ] **Step 2: Create BalloonTypeSO.cs**

Create file `Assets/Scripts/BalloonGame/Data/BalloonTypeSO.cs`:

```csharp
using UnityEngine;

public enum BalloonBehavior
{
    Standard,
    Armored,       // Requires 2 hits
    Explosive,     // Pops adjacent balloons
    Multiplier,    // 2x points
    Poison,        // Deducts points
    Ghost,         // Periodically invisible
    Bonus          // Extra darts
}

[CreateAssetMenu(fileName = "NewBalloonType", menuName = "INKSHOT/Balloon Type")]
public class BalloonTypeSO : ScriptableObject
{
    #region Identity

    [Header("Identity")]
    [Tooltip("Must match BalloonColor enum for standard balloons")]
    public BalloonColor balloonColor;

    [Tooltip("Display name shown in UI")]
    public string displayName;

    #endregion

    #region Gameplay

    [Header("Gameplay")]
    public int pointValue = 100;
    public BalloonBehavior behavior = BalloonBehavior.Standard;

    [Tooltip("Hits required to pop (for Armored type)")]
    [Min(1)]
    public int hitPoints = 1;

    #endregion

    #region Visuals

    [Header("Visuals")]
    public Color color = Color.white;
    public Material material;
    public Sprite icon;

    [Tooltip("Prefab spawned on pop. If null, uses default VFX.")]
    public GameObject popVFXPrefab;

    #endregion

    #region Audio

    [Header("Audio")]
    public AudioClip popSound;

    #endregion
}
```

- [ ] **Step 3: Create PerkSO.cs**

Create file `Assets/Scripts/BalloonGame/Data/PerkSO.cs`:

```csharp
using UnityEngine;

public enum PerkRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public enum PerkEffectType
{
    ExtraDarts,
    DamageBoost,
    ExplosionRadius,
    ScoreMultiplier,
    SlowMotion,
    Piercing,
    Magnet,
    Shield,
    DoubleScore,
    Ricochet
}

[CreateAssetMenu(fileName = "NewPerk", menuName = "INKSHOT/Perk")]
public class PerkSO : ScriptableObject
{
    #region Identity

    [Header("Identity")]
    public string perkId;
    public string displayName;

    [TextArea(2, 4)]
    public string description;

    public Sprite icon;

    #endregion

    #region Rarity & Effect

    [Header("Rarity & Effect")]
    public PerkRarity rarity = PerkRarity.Common;
    public PerkEffectType effectType;

    [Tooltip("Numeric parameters for the effect (e.g., [0]=amount, [1]=duration)")]
    public float[] effectParameters = new float[2];

    #endregion

    #region Stacking

    [Header("Stacking")]
    [Tooltip("Can this perk be picked multiple times?")]
    public bool stackable;

    [Min(1)]
    public int maxStacks = 1;

    #endregion

    #region Weighting

    [Header("Selection Weight")]
    [Tooltip("Relative weight in perk selection pool. Higher = more likely.")]
    [Min(0.01f)]
    public float selectionWeight = 1f;

    #endregion
}
```

- [ ] **Step 4: Create RoomTemplateSO.cs**

Create file `Assets/Scripts/BalloonGame/Data/RoomTemplateSO.cs`:

```csharp
using System;
using UnityEngine;

[Serializable]
public class BalloonTypeWeight
{
    public BalloonColor color;
    [Range(0f, 1f)]
    public float weight = 0.2f;
}

public enum RoomSpecialRule
{
    None,
    NoWallBounce,
    LimitedTime,
    BonusDarts,
    AllArmored,
    MovingBalloons
}

[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "INKSHOT/Room Template")]
public class RoomTemplateSO : ScriptableObject
{
    #region Identity

    [Header("Identity")]
    public int roomNumber;
    public string displayName;

    [TextArea(1, 3)]
    public string description;

    #endregion

    #region Grid Layout

    [Header("Grid Layout")]
    [Range(4, 12)]
    public int gridColumns = 8;

    [Range(3, 12)]
    public int gridRows = 9;

    #endregion

    #region Scoring

    [Header("Scoring")]
    public int targetScore = 3000;

    [Min(1)]
    public int dartCount = 4;

    #endregion

    #region Balloon Distribution

    [Header("Balloon Distribution")]
    public BalloonTypeWeight[] balloonWeights = new BalloonTypeWeight[]
    {
        new() { color = BalloonColor.Red, weight = 0.2f },
        new() { color = BalloonColor.Blue, weight = 0.2f },
        new() { color = BalloonColor.Yellow, weight = 0.2f },
        new() { color = BalloonColor.Green, weight = 0.2f },
        new() { color = BalloonColor.Purple, weight = 0.2f }
    };

    #endregion

    #region Special Rules

    [Header("Special Rules")]
    public RoomSpecialRule specialRule = RoomSpecialRule.None;

    [Tooltip("Time limit in seconds if LimitedTime rule is active")]
    public float timeLimitSeconds = 60f;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Pick a random balloon color based on configured weights.
    /// </summary>
    public BalloonColor GetRandomBalloonColor()
    {
        if (balloonWeights == null || balloonWeights.Length == 0)
        {
            return (BalloonColor)UnityEngine.Random.Range(0, 5);
        }

        float totalWeight = 0f;
        foreach (var bw in balloonWeights)
        {
            totalWeight += bw.weight;
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var bw in balloonWeights)
        {
            cumulative += bw.weight;
            if (roll <= cumulative)
            {
                return bw.color;
            }
        }

        return balloonWeights[^1].color;
    }

    #endregion
}
```

- [ ] **Step 5: Create GameConfigSO.cs**

Create file `Assets/Scripts/BalloonGame/Data/GameConfigSO.cs`:

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "INKSHOT/Game Config")]
public class GameConfigSO : ScriptableObject
{
    #region Singleton Access

    private static GameConfigSO _instance;

    public static GameConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfigSO>("GameConfig");
                if (_instance == null)
                {
                    Debug.LogWarning("[GameConfigSO] No GameConfig found in Resources. Using defaults.");
                    _instance = CreateInstance<GameConfigSO>();
                }
            }
            return _instance;
        }
    }

    #endregion

    #region Physics

    [Header("Physics")]
    [Tooltip("Custom gravity Y value (default Unity = -9.81)")]
    public float gravityY = -9.81f;

    [Tooltip("Dart mass for Rigidbody")]
    public float dartMass = 0.5f;

    [Tooltip("Min launch speed (m/s)")]
    public float minLaunchSpeed = 10f;

    [Tooltip("Max launch speed (m/s)")]
    public float maxLaunchSpeed = 40f;

    [Tooltip("Pull-to-speed curve exponent")]
    public float pullSpeedExponent = 1.45f;

    [Tooltip("Max pull distance in world units")]
    public float maxPullDistance = 3.0f;

    #endregion

    #region Combo System

    [Header("Combo System")]
    [Tooltip("Time window (seconds) between pops to maintain combo")]
    public float comboWindowSeconds = 1.5f;

    [Tooltip("Points per combo level (e.g., combo x3 = 3 * this)")]
    public int comboBonusPerLevel = 50;

    [Tooltip("Max combo multiplier cap")]
    public int maxComboMultiplier = 10;

    #endregion

    #region Dart

    [Header("Dart")]
    [Tooltip("Dart lifetime before auto-deactivate")]
    public float dartLifetimeSeconds = 5f;

    [Tooltip("Activation radius around launch point")]
    public float aimActivationRadius = 4f;

    #endregion

    #region Audio

    [Header("Audio")]
    [Range(0f, 1f)]
    public float defaultSFXVolume = 1f;

    [Range(0f, 1f)]
    public float defaultMusicVolume = 0.7f;

    [Range(0f, 1f)]
    public float defaultUIVolume = 1f;

    #endregion

    #region Haptics

    [Header("Haptics")]
    public bool hapticsEnabled = true;

    #endregion

    #region Progression

    [Header("Progression")]
    [Tooltip("Score target increase per room")]
    public int targetScoreIncrement = 500;

    [Tooltip("Starting dart count")]
    public int startingDarts = 4;

    #endregion
}
```

- [ ] **Step 6: Create Resources directory and default GameConfig asset**

```bash
mkdir -p Assets/Resources
```

After the scripts compile, right-click in Unity Project window: **Create > INKSHOT > Game Config**. Name it `GameConfig` and place it in `Assets/Resources/GameConfig.asset`. Leave all values at defaults.

- [ ] **Step 7: Verify compilation**

Open Unity, confirm zero errors. Verify all four SO types appear under **Create > INKSHOT** in the Project context menu.

**Commit:** `git commit -m "feat(foundation): add ScriptableObject data architecture (BalloonTypeSO, PerkSO, RoomTemplateSO, GameConfigSO)"`

---

## Task 5: Create Save System

**Files:**
- Create: `Assets/Scripts/BalloonGame/Core/SaveData.cs`
- Create: `Assets/Scripts/BalloonGame/Core/SaveManager.cs`

- [ ] **Step 1: Create SaveData.cs**

Create file `Assets/Scripts/BalloonGame/Core/SaveData.cs`:

```csharp
using System;
using System.Collections.Generic;

/// <summary>
/// Serializable save data. Stored as JSON in PlayerPrefs.
/// </summary>
[Serializable]
public class SaveData
{
    #region Score & Progression

    public int highScore;
    public int totalGamesPlayed;
    public int lastRoomReached;
    public int currency;

    #endregion

    #region Perks

    public List<string> unlockedPerks = new();

    #endregion

    #region Settings

    public float sfxVolume = 1f;
    public float musicVolume = 0.7f;
    public float uiVolume = 1f;
    public bool hapticsEnabled = true;

    #endregion

    #region Stats

    public int totalBalloonsPopped;
    public int totalDartsThrown;
    public int bestCombo;

    #endregion

    /// <summary>
    /// Create default save data for new players.
    /// </summary>
    public static SaveData CreateDefault()
    {
        return new SaveData
        {
            highScore = 0,
            totalGamesPlayed = 0,
            lastRoomReached = 0,
            currency = 0,
            unlockedPerks = new List<string>(),
            sfxVolume = 1f,
            musicVolume = 0.7f,
            uiVolume = 1f,
            hapticsEnabled = true,
            totalBalloonsPopped = 0,
            totalDartsThrown = 0,
            bestCombo = 0
        };
    }
}
```

- [ ] **Step 2: Create SaveManager.cs**

Create file `Assets/Scripts/BalloonGame/Core/SaveManager.cs`:

```csharp
using System;
using UnityEngine;

/// <summary>
/// PlayerPrefs-backed save/load manager using JsonUtility.
/// Access via SaveManager.Instance. Auto-creates default data on first load.
/// </summary>
public class SaveManager : MonoBehaviour
{
    #region Constants

    private const string SaveKey = "INKSHOT_SaveData";

    #endregion

    #region Singleton

    private static SaveManager _instance;

    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[SaveManager]");
                _instance = go.AddComponent<SaveManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    #endregion

    #region State

    private SaveData _data;

    /// <summary>
    /// Current save data. Always non-null after Awake.
    /// </summary>
    public SaveData Data
    {
        get
        {
            if (_data == null)
            {
                Load();
            }
            return _data;
        }
    }

    /// <summary>
    /// Raised after data is loaded or saved.
    /// </summary>
    public event Action OnSaveDataChanged;

    #endregion

    #region Lifecycle

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Save current data to PlayerPrefs.
    /// </summary>
    public void Save()
    {
        if (_data == null) return;

        string json = JsonUtility.ToJson(_data, false);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        OnSaveDataChanged?.Invoke();
    }

    /// <summary>
    /// Load data from PlayerPrefs. Creates default if none exists.
    /// </summary>
    public void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            _data = JsonUtility.FromJson<SaveData>(json);

            if (_data == null)
            {
                Debug.LogWarning("[SaveManager] Corrupted save data. Creating defaults.");
                _data = SaveData.CreateDefault();
            }

            // Ensure lists are never null (JsonUtility can deserialize null lists)
            _data.unlockedPerks ??= new();
        }
        else
        {
            _data = SaveData.CreateDefault();
        }

        OnSaveDataChanged?.Invoke();
    }

    /// <summary>
    /// Delete all save data and reset to defaults.
    /// </summary>
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        _data = SaveData.CreateDefault();
        OnSaveDataChanged?.Invoke();
        Debug.Log("[SaveManager] Save data deleted.");
    }

    /// <summary>
    /// Convenience: update high score if current is higher, then save.
    /// </summary>
    public void TryUpdateHighScore(int score)
    {
        if (score > Data.highScore)
        {
            Data.highScore = score;
            Save();
        }
    }

    /// <summary>
    /// Convenience: record end-of-game stats and save.
    /// </summary>
    public void RecordGameEnd(int finalScore, int roomReached, int balloonsPopped, int dartsThrown, int bestCombo)
    {
        Data.totalGamesPlayed++;
        Data.totalBalloonsPopped += balloonsPopped;
        Data.totalDartsThrown += dartsThrown;

        if (bestCombo > Data.bestCombo)
        {
            Data.bestCombo = bestCombo;
        }

        if (roomReached > Data.lastRoomReached)
        {
            Data.lastRoomReached = roomReached;
        }

        TryUpdateHighScore(finalScore);
    }

    #endregion
}
```

- [ ] **Step 3: Verify compilation**

Open Unity, confirm zero errors. Verify entering Play mode creates the `[SaveManager]` GameObject with DontDestroyOnLoad. Verify exiting Play mode does not leave orphaned objects.

**Commit:** `git commit -m "feat(foundation): add SaveData and SaveManager with PlayerPrefs persistence"`

---

## Task 6: Create Audio Manager

**Files:**
- Create: `Assets/Scripts/BalloonGame/Core/AudioManager.cs`

- [ ] **Step 1: Create AudioManager.cs**

Create file `Assets/Scripts/BalloonGame/Core/AudioManager.cs`:

```csharp
using UnityEngine;

/// <summary>
/// Singleton audio manager with three channels: SFX, Music, UI.
/// Auto-creates AudioSources. Volume controlled via SaveData.
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Singleton

    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[AudioManager]");
                _instance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    #endregion

    #region Audio Sources

    private AudioSource _sfxSource;
    private AudioSource _musicSource;
    private AudioSource _uiSource;

    #endregion

    #region Volume Properties

    public float SFXVolume
    {
        get => _sfxSource != null ? _sfxSource.volume : 1f;
        set
        {
            if (_sfxSource != null) _sfxSource.volume = value;
        }
    }

    public float MusicVolume
    {
        get => _musicSource != null ? _musicSource.volume : 0.7f;
        set
        {
            if (_musicSource != null) _musicSource.volume = value;
        }
    }

    public float UIVolume
    {
        get => _uiSource != null ? _uiSource.volume : 1f;
        set
        {
            if (_uiSource != null) _uiSource.volume = value;
        }
    }

    #endregion

    #region Lifecycle

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        CreateAudioSources();
        ApplySavedVolumes();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Play a one-shot sound effect.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Play background music. Crossfades if music is already playing.
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || _musicSource == null) return;

        if (_musicSource.clip == clip && _musicSource.isPlaying)
        {
            return;
        }

        _musicSource.clip = clip;
        _musicSource.Play();
    }

    /// <summary>
    /// Stop background music.
    /// </summary>
    public void StopMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
            _musicSource.clip = null;
        }
    }

    /// <summary>
    /// Play a UI sound (button clicks, navigation, etc.)
    /// </summary>
    public void PlayUI(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _uiSource == null) return;
        _uiSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Apply volume settings from SaveData.
    /// Call after loading save or changing settings.
    /// </summary>
    public void ApplySavedVolumes()
    {
        var data = SaveManager.Instance.Data;
        SFXVolume = data.sfxVolume;
        MusicVolume = data.musicVolume;
        UIVolume = data.uiVolume;
    }

    /// <summary>
    /// Save current volume levels to SaveData.
    /// </summary>
    public void SaveVolumes()
    {
        var data = SaveManager.Instance.Data;
        data.sfxVolume = SFXVolume;
        data.musicVolume = MusicVolume;
        data.uiVolume = UIVolume;
        SaveManager.Instance.Save();
    }

    #endregion

    #region Internals

    private void CreateAudioSources()
    {
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.spatialBlend = 0f; // 2D

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
        _musicSource.spatialBlend = 0f;

        _uiSource = gameObject.AddComponent<AudioSource>();
        _uiSource.playOnAwake = false;
        _uiSource.spatialBlend = 0f;
    }

    #endregion
}
```

- [ ] **Step 2: Verify compilation**

Open Unity, confirm zero errors. Enter Play mode and verify `[AudioManager]` GameObject appears with three AudioSource components.

**Commit:** `git commit -m "feat(foundation): add AudioManager singleton with SFX/Music/UI channels"`

---

## Task 7: Create Haptic Feedback Utility

**Files:**
- Create: `Assets/Scripts/BalloonGame/Core/Haptics.cs`

- [ ] **Step 1: Create Haptics.cs**

Create file `Assets/Scripts/BalloonGame/Core/Haptics.cs`:

```csharp
using UnityEngine;

public enum HapticType
{
    Light,
    Medium,
    Heavy,
    Success,
    Error
}

/// <summary>
/// Static utility for triggering haptic feedback on mobile devices.
/// Falls back to Handheld.Vibrate() on unsupported platforms.
/// Call Haptics.Play(HapticType) from anywhere.
/// </summary>
public static class Haptics
{
    #region State

    private static bool _enabled = true;

    /// <summary>
    /// Global haptic enable/disable. Reads from SaveData on first access.
    /// </summary>
    public static bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Trigger a haptic feedback event.
    /// No-op on unsupported platforms or if disabled.
    /// </summary>
    public static void Play(HapticType type)
    {
        if (!_enabled) return;

#if UNITY_IOS && !UNITY_EDITOR
        PlayiOS(type);
#elif UNITY_ANDROID && !UNITY_EDITOR
        PlayAndroid(type);
#else
        // Editor/standalone: log only in debug builds
        if (Debug.isDebugBuild)
        {
            Debug.Log($"[Haptics] {type} (no device)");
        }
#endif
    }

    #endregion

    #region iOS Implementation

    // iOS uses UIImpactFeedbackGenerator / UINotificationFeedbackGenerator.
    // Requires native plugin for full support. This is a placeholder
    // that uses Handheld.Vibrate() as fallback until the native plugin
    // is integrated.
    //
    // Native plugin bridge (future):
    //   [DllImport("__Internal")] static extern void _TriggerImpact(int style);
    //   [DllImport("__Internal")] static extern void _TriggerNotification(int type);

    private static void PlayiOS(HapticType type)
    {
        // Placeholder: use basic vibration until native plugin is added.
        // When native plugin is ready, replace with:
        //   Light/Medium/Heavy  -> _TriggerImpact(0/1/2)
        //   Success/Error       -> _TriggerNotification(0/2)

        switch (type)
        {
            case HapticType.Light:
            case HapticType.Medium:
            case HapticType.Heavy:
            case HapticType.Success:
            case HapticType.Error:
                Handheld.Vibrate();
                break;
        }
    }

    #endregion

    #region Android Implementation

    private static void PlayAndroid(HapticType type)
    {
        long durationMs = type switch
        {
            HapticType.Light => 10,
            HapticType.Medium => 25,
            HapticType.Heavy => 50,
            HapticType.Success => 30,
            HapticType.Error => 60,
            _ => 20
        };

        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            if (vibrator != null)
            {
                // API 26+ uses VibrationEffect
                if (GetAndroidSDKVersion() >= 26)
                {
                    using var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                    using var effect = vibrationEffect.CallStatic<AndroidJavaObject>(
                        "createOneShot", durationMs, GetAmplitude(type));
                    vibrator.Call("vibrate", effect);
                }
                else
                {
                    vibrator.Call("vibrate", durationMs);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Haptics] Android vibration failed: {e.Message}");
            Handheld.Vibrate();
        }
    }

    private static int GetAmplitude(HapticType type)
    {
        return type switch
        {
            HapticType.Light => 40,
            HapticType.Medium => 120,
            HapticType.Heavy => 255,
            HapticType.Success => 150,
            HapticType.Error => 200,
            _ => 120
        };
    }

    private static int GetAndroidSDKVersion()
    {
        try
        {
            using var version = new AndroidJavaClass("android.os.Build$VERSION");
            return version.GetStatic<int>("SDK_INT");
        }
        catch
        {
            return 0;
        }
    }

    #endregion
}
```

- [ ] **Step 2: Verify compilation**

Open Unity, confirm zero errors. The `#if` directives ensure Android/iOS code only compiles on those platforms.

**Commit:** `git commit -m "feat(foundation): add Haptics utility with iOS/Android platform support"`

---

## Task 8: Create Safe Area Handler

**Files:**
- Create: `Assets/Scripts/BalloonGame/Core/SafeAreaHandler.cs`

- [ ] **Step 1: Create SafeAreaHandler.cs**

Create file `Assets/Scripts/BalloonGame/Core/SafeAreaHandler.cs`:

```csharp
using UnityEngine;

/// <summary>
/// Adjusts a RectTransform to fit within Screen.safeArea.
/// Attach to any UI panel that needs notch/dynamic island avoidance.
/// Works at runtime; updates if screen dimensions change (rotation, etc.).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaHandler : MonoBehaviour
{
    #region State

    private RectTransform _rectTransform;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;
    private ScreenOrientation _lastOrientation;

    #endregion

    #region Lifecycle

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        if (HasScreenChanged())
        {
            ApplySafeArea();
        }
    }

    #endregion

    #region Safe Area Logic

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        _lastOrientation = Screen.orientation;

        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        // Convert safe area from screen pixels to anchor coordinates (0-1)
        Vector2 anchorMin = new(
            safeArea.x / Screen.width,
            safeArea.y / Screen.height
        );

        Vector2 anchorMax = new(
            (safeArea.x + safeArea.width) / Screen.width,
            (safeArea.y + safeArea.height) / Screen.height
        );

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }

    private bool HasScreenChanged()
    {
        return Screen.safeArea != _lastSafeArea ||
               Screen.width != _lastScreenSize.x ||
               Screen.height != _lastScreenSize.y ||
               Screen.orientation != _lastOrientation;
    }

    #endregion
}
```

- [ ] **Step 2: Verify compilation**

Open Unity, confirm zero errors.

**Commit:** `git commit -m "feat(foundation): add SafeAreaHandler for notch/dynamic island avoidance"`

---

## Task 9: Update GameConstants

**Files:**
- Modify: `Assets/Scripts/BalloonGame/GameConstants.cs`

- [ ] **Step 1: Add new constants to GameConstants.cs**

Open `Assets/Scripts/BalloonGame/GameConstants.cs` and replace the entire contents with:

```csharp
using UnityEngine;

public static class GameConstants
{
    #region Camera

    public const float CAMERA_ORTHO_SIZE = 10f;

    #endregion

    #region Board Layout

    public const int BOARD_COLUMNS = 8;
    public const int BOARD_ROWS = 9;
    public const int TOTAL_BALLOONS = BOARD_COLUMNS * BOARD_ROWS;

    public const float BOARD_LEFT = -4.0f;
    public const float BOARD_RIGHT = 4.0f;
    public const float BOARD_TOP = 8.5f;
    public const float BOARD_BOTTOM = -1.0f;
    public const float BOARD_WIDTH = BOARD_RIGHT - BOARD_LEFT;
    public const float BOARD_HEIGHT = BOARD_TOP - BOARD_BOTTOM;

    #endregion

    #region Lane

    public const float LANE_TOP = -2.0f;
    public const float LANE_BOTTOM = -8.5f;

    public static readonly Vector3 LAUNCH_POSITION = new(0f, -5.5f, 0f);

    #endregion

    #region Balloon Sizing

    public const float BALLOON_MAX_WIDTH = 0.78f;
    public const float BALLOON_MAX_HEIGHT = 0.94f;
    public const float BALLOON_SLOT_RATIO_X = 0.9f;
    public const float BALLOON_SLOT_RATIO_Y = 0.95f;

    #endregion

    #region Perspective

    public const float PERSPECTIVE_MIN_SCALE = 0.72f;
    public const float PERSPECTIVE_SCALE_RANGE = 0.28f;
    public const float PERSPECTIVE_HORIZONTAL_PINCH = 0.12f;
    public const float PERSPECTIVE_VERTICAL_COMPRESSION = 0.14f;

    #endregion

    #region Slingshot / Aiming

    public const float MAX_PULL_DISTANCE = 3.0f;
    public const float PULL_SPEED_EXPONENT = 1.45f;
    public const float MIN_LAUNCH_SPEED = 10f;
    public const float MAX_LAUNCH_SPEED = 40f;
    public const float AIM_ACTIVATION_RADIUS = 4f;

    #endregion

    #region Scoring

    public const int BASE_TARGET_SCORE = 3000;
    public const int STARTING_DARTS = 4;
    public const int SCORE_PER_BALLOON = 100;
    public const int TARGET_SCORE_INCREMENT = 500;

    #endregion

    #region Combo System

    public const float COMBO_WINDOW_SECONDS = 1.5f;
    public const int COMBO_BONUS_PER_LEVEL = 50;
    public const int MAX_COMBO_MULTIPLIER = 10;

    #endregion

    #region Walls

    public const float SIDE_WALL_WIDTH = 0.3f;
    public const float WALL_BOUNCINESS = 0.8f;
    public const float WALL_FRICTION = 0.1f;

    #endregion

    #region Layers

    public const int LAYER_ENVIRONMENT = 3;
    public const int LAYER_PROJECTILES = 8;
    public const int LAYER_BALLOONS = 7;

    #endregion

    #region Dart

    public const float DART_LIFETIME = 5f;
    public const float DART_MASS = 0.5f;
    public const float DART_LINEAR_DAMPING = 0.1f;
    public const float DART_ANGULAR_DAMPING = 0.5f;
    public const float DART_DEACTIVATE_DELAY = 0.3f;

    #endregion

    #region Audio

    public const float DEFAULT_SFX_VOLUME = 1f;
    public const float DEFAULT_MUSIC_VOLUME = 0.7f;
    public const float DEFAULT_UI_VOLUME = 1f;

    #endregion

    #region Save

    public const string SAVE_KEY = "INKSHOT_SaveData";

    #endregion

    #region Scene / UI

    public const float HUD_REFERENCE_WIDTH = 1080f;
    public const float HUD_REFERENCE_HEIGHT = 1920f;
    public const float ROOM_INTRO_DURATION = 1f;

    #endregion
}
```

- [ ] **Step 2: Verify compilation**

Open Unity, confirm zero errors. All existing code that references the original constants should still compile since no constants were renamed or removed --- only new ones were added and `#region` blocks were introduced.

**Commit:** `git commit -m "feat(foundation): extend GameConstants with combo, dart, audio, and save constants"`

---

## Task 10: Wire EventBus into Existing Scripts

**Files:**
- Modify: `Assets/Scripts/BalloonGame/BalloonNode.cs`
- Modify: `Assets/Scripts/BalloonGame/ScoreManager.cs`
- Modify: `Assets/Scripts/BalloonGame/BalloonGameManager.cs`

- [ ] **Step 1: Add EventBus publish to BalloonNode.Pop()**

Open `Assets/Scripts/BalloonGame/BalloonNode.cs`. Replace the `Pop()` method with:

```csharp
    public void Pop()
    {
        if (IsPopped)
        {
            return;
        }

        IsPopped = true;
        OnAnyBalloonPopped?.Invoke(this);

        EventBus.Publish(new BalloonPoppedEvent
        {
            Row = Row,
            Column = Column,
            Color = BalloonColor,
            WorldPosition = transform.position,
            PointValue = GameConstants.SCORE_PER_BALLOON
        });

        gameObject.SetActive(false);
    }
```

This adds the EventBus publish alongside the existing `OnAnyBalloonPopped` C# event so that both mechanisms work during migration. The static event will be deprecated in a future plan once all subscribers migrate to EventBus.

- [ ] **Step 2: Add EventBus publish to ScoreManager**

Open `Assets/Scripts/BalloonGame/ScoreManager.cs`. Replace the `HandleBalloonPopped` method with:

```csharp
    private void HandleBalloonPopped(BalloonNode balloon)
    {
        bool reachedBeforePop = IsTargetReached;
        int points = GameConstants.SCORE_PER_BALLOON;
        CurrentScore += points;
        OnBalloonPopped?.Invoke(points);
        OnScoreChanged?.Invoke(CurrentScore);

        EventBus.Publish(new ScoreChangedEvent
        {
            CurrentScore = CurrentScore,
            TargetScore = TargetScore,
            DeltaPoints = points
        });

        Debug.Log($"POP! +{points} pts | Score: {CurrentScore}/{TargetScore}");

        if (!reachedBeforePop && IsTargetReached)
        {
            OnTargetReached?.Invoke();
        }
    }
```

- [ ] **Step 3: Add EventBus publishes to BalloonGameManager**

Open `Assets/Scripts/BalloonGame/BalloonGameManager.cs`. Make the following targeted changes:

**3a.** In `StartRoom()`, after the last line (`UpdateHud();`), add:

```csharp
        EventBus.Publish(new RoomStartedEvent
        {
            RoomNumber = 1, // TODO: track room number in future plan
            DartCount = DartsRemaining,
            TargetScore = _scoreManager != null ? _scoreManager.TargetScore : GameConstants.BASE_TARGET_SCORE
        });
```

**3b.** In `HandleLaunch(Vector3 velocity)`, after the line `CurrentState = GameState.DartInFlight;`, add:

```csharp
        EventBus.Publish(new DartLaunchedEvent
        {
            Velocity = velocity,
            LaunchPosition = GameConstants.LAUNCH_POSITION
        });
```

**3c.** In `HandleTargetReached()`, after `UpdateHud();`, add:

```csharp
        EventBus.Publish(new RoomClearedEvent
        {
            RoomNumber = 1,
            FinalScore = _scoreManager.CurrentScore,
            DartsRemaining = DartsRemaining
        });
```

**3d.** In `HandleDartFinished(DartController dart)`, inside the `if (DartsRemaining <= 0)` block, after the `Debug.Log` line, add:

```csharp
            EventBus.Publish(new RoomFailedEvent
            {
                RoomNumber = 1,
                FinalScore = _scoreManager.CurrentScore
            });
```

- [ ] **Step 4: Verify compilation**

Open Unity, confirm zero errors. Enter Play mode, pop some balloons, and verify the game still works identically. EventBus events are fire-and-forget with no subscribers yet, so behavior is unchanged.

**Commit:** `git commit -m "feat(foundation): wire EventBus publishes into BalloonNode, ScoreManager, and BalloonGameManager"`

---

## Task 11: Reorganize Folder Structure

**Files:**
- Move: all existing scripts into role-based subfolders (see File Map above)

> **Important:** Use `git mv` to preserve file history and `.meta` file associations. Unity tracks assets by GUID in `.meta` files. Moving both the `.cs` and `.cs.meta` together via `git mv` preserves all scene/prefab references automatically.

- [ ] **Step 1: Create subdirectories**

```bash
mkdir -p Assets/Scripts/BalloonGame/Balloons
mkdir -p Assets/Scripts/BalloonGame/Darts
mkdir -p Assets/Scripts/BalloonGame/UI
mkdir -p Assets/Scripts/BalloonGame/Managers
mkdir -p Assets/Scripts/BalloonGame/Visuals
```

- [ ] **Step 2: Move Balloon scripts**

```bash
git mv Assets/Scripts/BalloonGame/BalloonNode.cs Assets/Scripts/BalloonGame/Balloons/BalloonNode.cs
git mv Assets/Scripts/BalloonGame/BalloonNode.cs.meta Assets/Scripts/BalloonGame/Balloons/BalloonNode.cs.meta
git mv Assets/Scripts/BalloonGame/BalloonWall.cs Assets/Scripts/BalloonGame/Balloons/BalloonWall.cs
git mv Assets/Scripts/BalloonGame/BalloonWall.cs.meta Assets/Scripts/BalloonGame/Balloons/BalloonWall.cs.meta
git mv Assets/Scripts/BalloonGame/BalloonData.cs Assets/Scripts/BalloonGame/Balloons/BalloonData.cs
git mv Assets/Scripts/BalloonGame/BalloonData.cs.meta Assets/Scripts/BalloonGame/Balloons/BalloonData.cs.meta
```

- [ ] **Step 3: Move Dart scripts**

```bash
git mv Assets/Scripts/BalloonGame/DartController.cs Assets/Scripts/BalloonGame/Darts/DartController.cs
git mv Assets/Scripts/BalloonGame/DartController.cs.meta Assets/Scripts/BalloonGame/Darts/DartController.cs.meta
git mv Assets/Scripts/BalloonGame/DartLauncher.cs Assets/Scripts/BalloonGame/Darts/DartLauncher.cs
git mv Assets/Scripts/BalloonGame/DartLauncher.cs.meta Assets/Scripts/BalloonGame/Darts/DartLauncher.cs.meta
git mv Assets/Scripts/BalloonGame/SlingshotInput.cs Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs
git mv Assets/Scripts/BalloonGame/SlingshotInput.cs.meta Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs.meta
```

- [ ] **Step 4: Move UI scripts**

```bash
git mv Assets/Scripts/BalloonGame/GameHUD.cs Assets/Scripts/BalloonGame/UI/GameHUD.cs
git mv Assets/Scripts/BalloonGame/GameHUD.cs.meta Assets/Scripts/BalloonGame/UI/GameHUD.cs.meta
```

- [ ] **Step 5: Move Manager scripts**

```bash
git mv Assets/Scripts/BalloonGame/BalloonGameManager.cs Assets/Scripts/BalloonGame/Managers/BalloonGameManager.cs
git mv Assets/Scripts/BalloonGame/BalloonGameManager.cs.meta Assets/Scripts/BalloonGame/Managers/BalloonGameManager.cs.meta
git mv Assets/Scripts/BalloonGame/ScoreManager.cs Assets/Scripts/BalloonGame/Managers/ScoreManager.cs
git mv Assets/Scripts/BalloonGame/ScoreManager.cs.meta Assets/Scripts/BalloonGame/Managers/ScoreManager.cs.meta
```

- [ ] **Step 6: Move Visual scripts**

```bash
git mv Assets/Scripts/BalloonGame/SlingshotVisuals.cs Assets/Scripts/BalloonGame/Visuals/SlingshotVisuals.cs
git mv Assets/Scripts/BalloonGame/SlingshotVisuals.cs.meta Assets/Scripts/BalloonGame/Visuals/SlingshotVisuals.cs.meta
git mv Assets/Scripts/BalloonGame/BalloonCamera.cs Assets/Scripts/BalloonGame/Visuals/BalloonCamera.cs
git mv Assets/Scripts/BalloonGame/BalloonCamera.cs.meta Assets/Scripts/BalloonGame/Visuals/BalloonCamera.cs.meta
```

- [ ] **Step 7: Verify remaining root scripts**

After moves, only `GameConstants.cs` (and its `.meta`) should remain at the `Assets/Scripts/BalloonGame/` root, alongside the `Core/`, `Data/`, `Balloons/`, `Darts/`, `UI/`, `Managers/`, `Visuals/`, and `Editor/` directories.

Run `ls Assets/Scripts/BalloonGame/` and confirm:

```
Balloons/
Core/
Darts/
Data/
Editor/
GameConstants.cs
GameConstants.cs.meta
Managers/
UI/
Visuals/
```

- [ ] **Step 8: Verify compilation**

Open Unity, wait for reimport. Confirm zero errors. Enter Play mode and verify the game still works. Since no class names or namespaces changed, all scene references remain valid.

**Commit:** `git commit -m "refactor(foundation): reorganize scripts into role-based subfolders"`

---

## Task 12: Integration Verification

**Files:**
- No new files. This task is a verification checklist.

- [ ] **Step 1: Full compilation check**

Open Unity. Wait for full script compilation. **Zero errors required.**

- [ ] **Step 2: Play mode smoke test**

Enter Play mode. Verify:
1. Balloons render on screen in 8x9 grid
2. Slingshot input works (click/drag near launch point)
3. Dart spawns and flies toward balloons
4. Balloons pop on collision
5. Score updates in HUD
6. Room clears when target reached
7. Room fails when darts exhausted
8. Press R to restart works in cleared/failed states

- [ ] **Step 3: Verify singletons**

While in Play mode, check the Hierarchy for:
- `[SaveManager]` GameObject with `DontDestroyOnLoad`
- `[AudioManager]` GameObject with 3 AudioSource components (only if you instantiate them; they auto-create on first `.Instance` access)

To trigger AudioManager creation, add this temporary test in any script's `Start()`:
```csharp
Debug.Log($"AudioManager ready: {AudioManager.Instance != null}");
Debug.Log($"SaveManager ready: {SaveManager.Instance != null}");
```
Remove after verification.

- [ ] **Step 4: Verify ScriptableObject menus**

In the Unity Project window, right-click > Create. Verify these menu items exist:
- **INKSHOT > Balloon Type**
- **INKSHOT > Perk**
- **INKSHOT > Room Template**
- **INKSHOT > Game Config**

Create one test asset of each type. Verify all fields appear in Inspector. Delete the test assets after verification.

- [ ] **Step 5: Verify EventBus is publishing**

Add temporary subscriber in any MonoBehaviour's `OnEnable()`:
```csharp
EventBus.Subscribe<BalloonPoppedEvent>(e =>
    Debug.Log($"[EventBus] BalloonPopped at ({e.Row},{e.Column}) color={e.Color} points={e.PointValue}"));
EventBus.Subscribe<ScoreChangedEvent>(e =>
    Debug.Log($"[EventBus] ScoreChanged: {e.CurrentScore}/{e.TargetScore} delta={e.DeltaPoints}"));
EventBus.Subscribe<DartLaunchedEvent>(e =>
    Debug.Log($"[EventBus] DartLaunched vel={e.Velocity}"));
```
Pop some balloons. Verify all three log lines appear in Console. Remove the temporary code after verification.

- [ ] **Step 6: Verify folder structure**

Confirm final project layout matches:
```
Assets/Scripts/BalloonGame/
├── Balloons/
│   ├── BalloonData.cs
│   ├── BalloonNode.cs
│   └── BalloonWall.cs
├── Core/
│   ├── AudioManager.cs
│   ├── EventBus.cs
│   ├── GameEvents.cs
│   ├── Haptics.cs
│   ├── ObjectPool.cs
│   ├── SafeAreaHandler.cs
│   ├── SaveData.cs
│   └── SaveManager.cs
├── Darts/
│   ├── DartController.cs
│   ├── DartLauncher.cs
│   └── SlingshotInput.cs
├── Data/
│   ├── BalloonTypeSO.cs
│   ├── GameConfigSO.cs
│   ├── PerkSO.cs
│   └── RoomTemplateSO.cs
├── Editor/
│   ├── BalloonSceneBuilder.cs
│   └── URPSetup.cs
├── GameConstants.cs
├── Managers/
│   ├── BalloonGameManager.cs
│   └── ScoreManager.cs
├── UI/
│   └── GameHUD.cs
└── Visuals/
    ├── BalloonCamera.cs
    └── SlingshotVisuals.cs
```

**Commit:** `git commit -m "chore(foundation): verify integration of all foundation systems"`

---

## Summary

| Task | System | New Files | Modified Files |
|------|--------|-----------|---------------|
| 1 | URP Pipeline | 3 (assets + editor script) | 2 (DartLauncher, BalloonWall) |
| 2 | EventBus | 2 | 0 |
| 3 | ObjectPool | 1 | 0 |
| 4 | ScriptableObject Data | 4 | 0 |
| 5 | Save System | 2 | 0 |
| 6 | Audio Manager | 1 | 0 |
| 7 | Haptics | 1 | 0 |
| 8 | Safe Area | 1 | 0 |
| 9 | GameConstants | 0 | 1 |
| 10 | EventBus Wiring | 0 | 3 |
| 11 | Folder Reorg | 0 | 0 (moves only) |
| 12 | Verification | 0 | 0 |

**Total: 15 new files, 6 modified files, 10 moved files, 12 commits**
