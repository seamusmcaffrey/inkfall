# Plan 05: Game Loop — State Machine, HUD & Round Flow

## Goal
Wire everything into a playable game loop: state machine, dart spawning/lifecycle, HUD, room clear/fail, restart.

## Deliverables
- [ ] `Assets/Scripts/BalloonGame/BalloonGameManager.cs`
- [ ] `Assets/Scripts/BalloonGame/GameHUD.cs`
- [ ] Fully playable game loop

---

## Step 1: Create BalloonGameManager.cs

**File**: `Assets/Scripts/BalloonGame/BalloonGameManager.cs`

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central game state machine. Orchestrates dart spawning, input, scoring, and round flow.
///
/// States:
///   RoomIntro    → Brief pause before play
///   Ready        → Waiting for slingshot input
///   DartInFlight → Dart is flying, physics active
///   RoomCleared  → Score target reached
///   RoomFailed   → Out of darts without reaching target
/// </summary>
public class BalloonGameManager : MonoBehaviour
{
    public enum GameState { RoomIntro, Ready, DartInFlight, RoomCleared, RoomFailed }

    public GameState CurrentState { get; private set; } = GameState.RoomIntro;
    public int DartsRemaining { get; private set; }

    private BalloonWall _balloonWall;
    private SlingshotInput _slingshotInput;
    private ScoreManager _scoreManager;
    private GameHUD _hud;
    private DartLauncher _dartLauncher;

    private DartController _activeDart;
    private float _introTimer;

    private void Awake()
    {
        _balloonWall = FindAnyObjectByType<BalloonWall>();
        _slingshotInput = FindAnyObjectByType<SlingshotInput>();
        _scoreManager = FindAnyObjectByType<ScoreManager>();
        _hud = FindAnyObjectByType<GameHUD>();

        // Create DartLauncher as a child
        var launcherGO = new GameObject("DartLauncher");
        launcherGO.transform.SetParent(transform);
        _dartLauncher = launcherGO.AddComponent<DartLauncher>();
    }

    private void Start()
    {
        if (_slingshotInput != null)
            _slingshotInput.OnLaunch += OnLaunch;
        if (_scoreManager != null)
            _scoreManager.OnTargetReached += OnTargetReached;

        DartController.OnDartFinished += OnDartFinished;

        StartRoom();
    }

    private void OnDestroy()
    {
        if (_slingshotInput != null)
            _slingshotInput.OnLaunch -= OnLaunch;
        if (_scoreManager != null)
            _scoreManager.OnTargetReached -= OnTargetReached;

        DartController.OnDartFinished -= OnDartFinished;
    }

    // ── Room Lifecycle ──

    private void StartRoom()
    {
        CurrentState = GameState.RoomIntro;
        _introTimer = 1.0f;
        DartsRemaining = GameConstants.STARTING_DARTS;

        _scoreManager?.Initialize(GameConstants.BASE_TARGET_SCORE);
        _balloonWall?.GenerateWall();
        _slingshotInput?.SetCanFire(false);

        UpdateHUD();
    }

    public void RestartRoom()
    {
        // Clean up existing darts
        _activeDart = null;
        foreach (var dart in FindObjectsByType<DartController>(FindObjectsSortMode.None))
            Destroy(dart.gameObject);

        StartRoom();
    }

    // ── State Machine ──

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.RoomIntro:
                _introTimer -= Time.deltaTime;
                if (_introTimer <= 0f)
                {
                    CurrentState = GameState.Ready;
                    _slingshotInput?.SetCanFire(true);
                }
                break;

            case GameState.Ready:
                // Waiting for slingshot input
                break;

            case GameState.DartInFlight:
                // Physics handles dart — DartController.OnDartFinished fires when done
                break;

            case GameState.RoomCleared:
            case GameState.RoomFailed:
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                    RestartRoom();
                break;
        }
    }

    // ── Events ──

    private void OnLaunch(Vector3 velocity)
    {
        if (CurrentState != GameState.Ready || DartsRemaining <= 0) return;

        _slingshotInput.SetCanFire(false);
        _activeDart = _dartLauncher.SpawnAndLaunch(velocity);
        CurrentState = GameState.DartInFlight;
    }

    private void OnDartFinished(DartController dart)
    {
        if (dart != _activeDart) return;
        _activeDart = null;

        // If room already cleared by scoring event, don't do anything else
        if (CurrentState == GameState.RoomCleared) return;

        DartsRemaining--;
        UpdateHUD();

        if (DartsRemaining <= 0)
        {
            CurrentState = GameState.RoomFailed;
            _slingshotInput?.SetCanFire(false);
            UpdateHUD();
            Debug.Log($"ROOM FAILED. Score: {_scoreManager.CurrentScore}/{_scoreManager.TargetScore}");
        }
        else
        {
            CurrentState = GameState.Ready;
            _slingshotInput?.SetCanFire(true);
        }
    }

    private void OnTargetReached()
    {
        CurrentState = GameState.RoomCleared;
        _slingshotInput?.SetCanFire(false);
        UpdateHUD();
        Debug.Log($"ROOM CLEARED! Score: {_scoreManager.CurrentScore}/{_scoreManager.TargetScore}");
    }

    private void UpdateHUD()
    {
        _hud?.UpdateDisplay(
            _scoreManager?.CurrentScore ?? 0,
            _scoreManager?.TargetScore ?? GameConstants.BASE_TARGET_SCORE,
            DartsRemaining,
            CurrentState
        );
    }
}
```

---

## Step 2: Create GameHUD.cs

**File**: `Assets/Scripts/BalloonGame/GameHUD.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD overlay: score, target, darts remaining, game state messages.
/// </summary>
public class GameHUD : MonoBehaviour
{
    private Text _scoreText;
    private Text _targetText;
    private Text _dartsText;
    private Text _messageText;

    private void Awake()
    {
        CreateHUD();
    }

    private void CreateHUD()
    {
        var canvasGO = new GameObject("HUDCanvas");
        canvasGO.transform.SetParent(transform);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        // Score (top-left)
        _scoreText = MakeText(canvasGO.transform, "Score", TextAnchor.UpperLeft, 42, Color.white);
        var scoreRect = _scoreText.rectTransform;
        scoreRect.anchorMin = scoreRect.anchorMax = scoreRect.pivot = new Vector2(0, 1);
        scoreRect.anchoredPosition = new Vector2(30, -30);
        scoreRect.sizeDelta = new Vector2(400, 60);

        // Target (top-right)
        _targetText = MakeText(canvasGO.transform, "Target", TextAnchor.UpperRight, 32, new Color(0.7f, 0.7f, 0.7f));
        var targetRect = _targetText.rectTransform;
        targetRect.anchorMin = targetRect.anchorMax = targetRect.pivot = new Vector2(1, 1);
        targetRect.anchoredPosition = new Vector2(-30, -30);
        targetRect.sizeDelta = new Vector2(400, 60);

        // Darts (top-center)
        _dartsText = MakeText(canvasGO.transform, "Darts", TextAnchor.UpperCenter, 36, new Color(0.9f, 0.7f, 0.3f));
        var dartsRect = _dartsText.rectTransform;
        dartsRect.anchorMin = dartsRect.anchorMax = dartsRect.pivot = new Vector2(0.5f, 1);
        dartsRect.anchoredPosition = new Vector2(0, -30);
        dartsRect.sizeDelta = new Vector2(300, 60);

        // Center message
        _messageText = MakeText(canvasGO.transform, "Message", TextAnchor.MiddleCenter, 64, Color.white);
        var msgRect = _messageText.rectTransform;
        msgRect.anchorMin = msgRect.anchorMax = msgRect.pivot = new Vector2(0.5f, 0.5f);
        msgRect.anchoredPosition = Vector2.zero;
        msgRect.sizeDelta = new Vector2(800, 200);
        _messageText.gameObject.SetActive(false);
    }

    private Text MakeText(Transform parent, string name, TextAnchor anchor, int size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.AddComponent<RectTransform>();
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(2, -2);
        return text;
    }

    public void UpdateDisplay(int score, int target, int darts, BalloonGameManager.GameState state)
    {
        _scoreText.text = score.ToString();
        _targetText.text = $"Target: {target}";
        _dartsText.text = $"Darts: {darts}";
        _scoreText.color = score >= target ? new Color(0.3f, 0.9f, 0.3f) : Color.white;

        switch (state)
        {
            case BalloonGameManager.GameState.RoomCleared:
                _messageText.gameObject.SetActive(true);
                _messageText.text = "CLEARED!\nPress R to restart";
                _messageText.color = new Color(0.3f, 0.9f, 0.3f);
                break;
            case BalloonGameManager.GameState.RoomFailed:
                _messageText.gameObject.SetActive(true);
                _messageText.text = "FAILED\nPress R to restart";
                _messageText.color = new Color(0.9f, 0.3f, 0.3f);
                break;
            default:
                _messageText.gameObject.SetActive(false);
                break;
        }
    }
}
```

---

## Scene Hierarchy (Final)

```
InkshotScene
├── Main Camera (Camera, BalloonCamera)
├── Directional Light
├── BackWall (Quad)
├── LeftWall (Cube, BoxCollider + bouncy PhysicMaterial)
├── RightWall (Cube, BoxCollider + bouncy PhysicMaterial)
├── TopWall (Cube, BoxCollider + dead PhysicMaterial)
├── LaneFloor (Quad)
├── LaunchOrigin (empty marker)
├── BalloonWall (BalloonWall script)
│   └── [72 balloon spheres, spawned at runtime]
├── SlingshotController (SlingshotInput, SlingshotVisuals)
│   ├── RubberBand (LineRenderer, runtime child)
│   └── Trajectory (LineRenderer, runtime child)
├── GameManager (BalloonGameManager, ScoreManager, GameHUD)
│   ├── DartLauncher (runtime child)
│   └── HUDCanvas (runtime child)
└── [Darts spawned at runtime, destroyed after use]
```

---

## Full Gameplay Flow

### 1. Room Start
```
GameManager.StartRoom()
→ ScoreManager.Initialize(target=3000, score=0)
→ BalloonWall.GenerateWall() → 72 balloons
→ DartsRemaining = 4
→ State = RoomIntro (1s pause)
→ HUD: "0 | Target: 3000 | Darts: 4"
```

### 2. Ready
```
After 1 second → State = Ready
→ SlingshotInput.SetCanFire(true)
→ Player can drag to aim
```

### 3. Aim & Launch
```
Player drags → rubber band + trajectory preview shown
Player releases → SlingshotInput.OnLaunch fires
→ GameManager.OnLaunch receives velocity
→ DartLauncher.SpawnAndLaunch(velocity) → dart Rigidbody launched
→ State = DartInFlight
```

### 4. Flight & Collision
```
Unity physics moves dart, applies gravity
→ Dart hits balloon:
    BalloonNode.OnCollisionEnter → Pop() → SetActive(false)
    → BalloonNode.OnAnyBalloonPopped fires
    → ScoreManager adds 100 pts
    → DartController.OnHitBalloon() → dart stops
    → DartController.OnDartFinished fires
→ Dart hits side wall: bounces (PhysicMaterial)
→ Dart hits top wall: stops (DartController.OnCollisionEnter)
→ Dart out of bounds: stops (DartController.FixedUpdate)
```

### 5. Next Dart
```
GameManager.OnDartFinished()
→ If room cleared: already handled by OnTargetReached
→ DartsRemaining--
→ If darts > 0: State = Ready, enable input
→ If darts == 0: State = RoomFailed
```

### 6. Room End
```
CLEARED: "CLEARED! Press R to restart" (green)
FAILED: "FAILED Press R to restart" (red)
→ Press R → RestartRoom() → loop back to step 1
```

---

## Final Verification Checklist (All Plans Combined)

### Scene
- [ ] InkshotScene loads, no errors
- [ ] Portrait orthographic camera
- [ ] Dark wall, frame walls, launch lane visible

### Balloons
- [ ] 72 colored spheres in 8×9 grid
- [ ] Top row smaller, bottom row larger (perspective)
- [ ] Each has SphereCollider + kinematic Rigidbody on layer 7

### Slingshot
- [ ] Click/drag near bottom shows rubber band + trajectory
- [ ] Release launches dart in opposite direction
- [ ] Bigger pull = faster dart

### Dart Physics
- [ ] Dart arcs naturally under Unity gravity
- [ ] Dart rotates to face velocity
- [ ] Bounces off side walls
- [ ] Stops at top wall

### Collision
- [ ] Dart hits balloon → balloon disappears
- [ ] Score increases by 100
- [ ] Dart stops after hitting balloon

### Game Loop
- [ ] 4 darts per room
- [ ] Score ≥ 3000 → "CLEARED!"
- [ ] 0 darts left, score < 3000 → "FAILED"
- [ ] Press R to restart
- [ ] HUD displays score, target, darts remaining

### No Errors
- [ ] Zero compile errors
- [ ] Zero runtime errors
- [ ] No null references
- [ ] No physics glitches

## Files Created
1. `Assets/Scripts/BalloonGame/BalloonGameManager.cs`
2. `Assets/Scripts/BalloonGame/GameHUD.cs`
