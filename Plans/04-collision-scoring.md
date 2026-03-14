# Plan 04: Collision & Scoring

## Goal
Wire up Unity physics collision between darts and balloons, handle balloon popping, and track score. No custom intersection math — Unity's collision system does the work.

## Deliverables
- [ ] `Assets/Scripts/BalloonGame/ScoreManager.cs`
- [ ] Collision wiring verified between dart (layer 8) and balloons (layer 7)

---

## Step 1: Create ScoreManager.cs

**File**: `Assets/Scripts/BalloonGame/ScoreManager.cs`

```csharp
using System;
using UnityEngine;

/// <summary>
/// Tracks score for the current room. Listens to BalloonNode.OnAnyBalloonPopped.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public event Action<int> OnScoreChanged;    // New total score
    public event Action<int> OnBalloonPopped;   // Points from this specific pop
    public event Action OnTargetReached;

    public int CurrentScore { get; private set; }
    public int TargetScore { get; private set; }
    public bool IsTargetReached => CurrentScore >= TargetScore;

    public void Initialize(int targetScore)
    {
        CurrentScore = 0;
        TargetScore = targetScore;
        OnScoreChanged?.Invoke(CurrentScore);
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
        int points = GameConstants.SCORE_PER_BALLOON;
        CurrentScore += points;

        OnBalloonPopped?.Invoke(points);
        OnScoreChanged?.Invoke(CurrentScore);

        if (IsTargetReached)
        {
            OnTargetReached?.Invoke();
        }
    }
}
```

---

## Step 2: Collision Architecture

The collision flow uses Unity's built-in physics:

```
Dart (Rigidbody, layer 8) flies through scene
    ↓
Hits Balloon (SphereCollider, kinematic Rigidbody, layer 7)
    ↓
BalloonNode.OnCollisionEnter fires
    ↓
BalloonNode.Pop() called
    → balloon.SetActive(false)
    → BalloonNode.OnAnyBalloonPopped event fires
    ↓
ScoreManager.HandleBalloonPopped listens
    → CurrentScore += 100
    → OnScoreChanged fires → HUD updates
    → If score >= target → OnTargetReached fires
    ↓
BalloonNode.OnCollisionEnter also calls DartController.OnHitBalloon()
    → Dart stops and deactivates
    → DartController.OnDartFinished event fires
    ↓
GameManager.HandleDartFinished listens
    → Decrement darts remaining
    → Check win/lose
    → Enable input for next dart (or end round)
```

### Why This Works Without Custom Collision Math

The original Inkshot uses segment-to-circle intersection because SpriteKit doesn't have real physics collision for manually-moved objects. In Unity, the dart IS a Rigidbody moving through the physics engine, so `OnCollisionEnter` fires automatically when the dart's collider overlaps a balloon's collider.

The dart uses `CollisionDetectionMode.Continuous` (set in DartController) which prevents tunneling — even fast darts won't pass through balloons.

---

## Step 3: Physics Layer Configuration

Verify in Project Settings → Physics → Layer Collision Matrix:

| | Layer 7 (Balloons) | Layer 8 (Projectiles) |
|---|---|---|
| Layer 7 (Balloons) | ✗ (balloons don't collide with each other) | ✓ (darts hit balloons) |
| Layer 8 (Projectiles) | ✓ (darts hit balloons) | ✗ (darts don't collide with each other) |
| Default (0) | ✓ (walls collide with balloons if needed) | ✓ (darts collide with walls) |

The existing project already has layers 7 and 8 defined. The collision matrix likely already enables 7↔8 collision (it was set up for arrows hitting enemies). If not, enable it in Physics settings.

---

## Step 4: Dart Behavior On Collision

The dart's collision responses:

| Hit Target | Dart Behavior | How |
|---|---|---|
| Balloon | Stops, deactivates | `DartController.OnHitBalloon()` called by `BalloonNode.OnCollisionEnter` |
| Side wall | Bounces | Automatic via PhysicMaterial (bounciness 0.8) |
| Top wall | Stops, embeds | `DartController.OnCollisionEnter` checks `collision.gameObject.name == "TopWall"` |
| Out of bounds | Deactivates | `DartController.FixedUpdate` bounds check |
| Timeout | Deactivates | `DartController.FixedUpdate` lifetime check (5s) |

---

## Step 5: Score Display

The `ScoreManager` fires events that `GameHUD` (Plan 05) subscribes to. For now, `Debug.Log` output confirms scoring works:

```
// In ScoreManager.HandleBalloonPopped:
Debug.Log($"POP! +{points} pts | Score: {CurrentScore}/{TargetScore}");
```

---

## Verification Checklist

- [ ] Dart collides with balloon → balloon disappears
- [ ] Score increases by 100 per balloon pop
- [ ] Dart stops after hitting a balloon
- [ ] Dart bounces off side walls (natural physics)
- [ ] Dart stops at top wall
- [ ] Console log shows score updates
- [ ] Multiple darts can be thrown, each popping one balloon
- [ ] No double-pops (IsPopped check)
- [ ] No tunneling (dart doesn't pass through balloons)
- [ ] Collision events fire reliably
- [ ] No console errors

## Files Created
1. `Assets/Scripts/BalloonGame/ScoreManager.cs`

## Files From Earlier Plans That Handle Collision
- `BalloonNode.cs` (Plan 02) — `OnCollisionEnter`, `Pop()`
- `DartController.cs` (Plan 03) — `OnCollisionEnter` for walls, `OnHitBalloon()`
