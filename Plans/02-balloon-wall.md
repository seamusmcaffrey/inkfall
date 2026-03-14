# Plan 02: Balloon Wall Generation & Layout

## Goal
Generate an 8×9 grid of balloon GameObjects (spheres with SphereColliders) with perspective scaling. Balloons use Unity's collision system — no custom intersection math.

## Deliverables
- [ ] `Assets/Scripts/BalloonGame/BalloonData.cs`
- [ ] `Assets/Scripts/BalloonGame/BalloonNode.cs`
- [ ] `Assets/Scripts/BalloonGame/BalloonWall.cs`
- [ ] 5 balloon materials (Red, Blue, Yellow, Green, Purple)

---

## Step 1: Create BalloonData.cs

**File**: `Assets/Scripts/BalloonGame/BalloonData.cs`

```csharp
using UnityEngine;

public enum BalloonColor
{
    Red,
    Blue,
    Yellow,
    Green,
    Purple
}

public static class BalloonColorExtensions
{
    public static Color ToUnityColor(this BalloonColor color)
    {
        return color switch
        {
            BalloonColor.Red    => new Color(0.902f, 0.224f, 0.275f), // #E63946
            BalloonColor.Blue   => new Color(0.271f, 0.482f, 0.616f), // #457B9D
            BalloonColor.Yellow => new Color(0.945f, 0.980f, 0.933f), // #F1FAEE
            BalloonColor.Green  => new Color(0.165f, 0.616f, 0.561f), // #2A9D8F
            BalloonColor.Purple => new Color(0.608f, 0.349f, 0.714f), // #9B59B6
            _ => Color.white
        };
    }
}
```

---

## Step 2: Create BalloonNode.cs

**File**: `Assets/Scripts/BalloonGame/BalloonNode.cs`

Each balloon is a sphere with a SphereCollider. When hit by a dart, it pops.

```csharp
using UnityEngine;

/// <summary>
/// Individual balloon behavior. Handles collision with darts and popping.
/// Must be on layer 7 (Enemies/Balloons) so darts (layer 8) collide with it.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class BalloonNode : MonoBehaviour
{
    public int Row { get; private set; }
    public int Column { get; private set; }
    public BalloonColor BalloonColor { get; private set; }
    public bool IsPopped { get; private set; }

    /// <summary>
    /// Event fired when this balloon is popped. Subscribers (ScoreManager) handle scoring.
    /// </summary>
    public static event System.Action<BalloonNode> OnAnyBalloonPopped;

    public void Initialize(int row, int column, BalloonColor color)
    {
        Row = row;
        Column = column;
        BalloonColor = color;
        IsPopped = false;
        gameObject.layer = GameConstants.LAYER_BALLOONS;
    }

    /// <summary>
    /// Pop this balloon. Called when a dart collides with it.
    /// </summary>
    public void Pop()
    {
        if (IsPopped) return;
        IsPopped = true;
        OnAnyBalloonPopped?.Invoke(this);
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if hit by a dart (layer 8 = Projectiles)
        if (collision.gameObject.layer == GameConstants.LAYER_PROJECTILES)
        {
            Pop();

            // Also notify the dart that it hit something
            var dartController = collision.gameObject.GetComponent<DartController>();
            if (dartController != null)
            {
                dartController.OnHitBalloon();
            }
        }
    }
}
```

---

## Step 3: Create BalloonWall.cs

**File**: `Assets/Scripts/BalloonGame/BalloonWall.cs`

Generates the 8×9 grid with perspective layout. Uses primitive spheres at runtime.

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates and manages the 8×9 balloon grid with perspective layout.
/// </summary>
public class BalloonWall : MonoBehaviour
{
    private readonly List<BalloonNode> _balloons = new();
    private readonly BalloonColor[] _colors = {
        BalloonColor.Red, BalloonColor.Blue, BalloonColor.Yellow,
        BalloonColor.Green, BalloonColor.Purple
    };

    private readonly Dictionary<BalloonColor, Material> _materials = new();

    public IReadOnlyList<BalloonNode> Balloons => _balloons;

    public void GenerateWall()
    {
        ClearWall();
        EnsureMaterials();

        int rows = GameConstants.BOARD_ROWS;
        int cols = GameConstants.BOARD_COLUMNS;
        float slotX = GameConstants.BOARD_WIDTH / (cols - 1);
        float slotY = GameConstants.BOARD_HEIGHT / (rows - 1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var color = _colors[Random.Range(0, _colors.Length)];
                float scale = PerspectiveScale(row, rows);
                Vector3 position = PerspectivePosition(row, col, rows, cols, slotX, slotY);

                // Create sphere
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"Balloon_{row}_{col}";
                go.transform.SetParent(transform);
                go.transform.position = position;

                // Scale balloon with perspective
                float w = GameConstants.BALLOON_MAX_WIDTH * GameConstants.BALLOON_SLOT_RATIO_X * scale;
                float h = GameConstants.BALLOON_MAX_HEIGHT * GameConstants.BALLOON_SLOT_RATIO_Y * scale;
                go.transform.localScale = new Vector3(w, h, w);

                // Set layer for collision
                go.layer = GameConstants.LAYER_BALLOONS;

                // Apply material
                var renderer = go.GetComponent<Renderer>();
                renderer.material = _materials[color];

                // Atmospheric darkening for top rows
                float rowRatio = (float)row / (rows - 1);
                float topBias = 1f - rowRatio;
                float fade = 1f - topBias * 0.12f;
                var mat = renderer.material;
                mat.color = Color.Lerp(
                    new Color(mat.color.r * 0.6f, mat.color.g * 0.6f, mat.color.b * 0.6f),
                    mat.color, fade);

                // SphereCollider already added by CreatePrimitive
                // Make it a trigger OR keep as collider — keep as collider for physics bounce
                // The collider is already there and sized to the sphere

                // Make balloons kinematic (they don't move from physics forces)
                var rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;

                // Add BalloonNode
                var node = go.AddComponent<BalloonNode>();
                node.Initialize(row, col, color);
                _balloons.Add(node);
            }
        }
    }

    public void ClearWall()
    {
        foreach (var b in _balloons)
            if (b != null) Destroy(b.gameObject);
        _balloons.Clear();
    }

    public int ActiveCount()
    {
        int count = 0;
        foreach (var b in _balloons)
            if (b != null && !b.IsPopped) count++;
        return count;
    }

    // ── Perspective Math (from Inkshot) ──

    private Vector3 PerspectivePosition(int row, int col, int totalRows, int totalCols, float slotX, float slotY)
    {
        float rowRatio = (float)row / (totalRows - 1);
        float topBias = 1f - rowRatio;

        // Base grid position
        float baseX = GameConstants.BOARD_LEFT + col * slotX;

        // Horizontal pinch: converge toward center at top
        float centerX = (GameConstants.BOARD_LEFT + GameConstants.BOARD_RIGHT) / 2f;
        float pinchedX = centerX + (baseX - centerX) * (1f - topBias * GameConstants.PERSPECTIVE_HORIZONTAL_PINCH);

        // Vertical compression: compress rows at top
        float compressedY = GameConstants.BOARD_TOP - (row * slotY * (1f - topBias * GameConstants.PERSPECTIVE_VERTICAL_COMPRESSION));

        // Z: slight depth offset for render ordering (top rows behind)
        float z = row * 0.01f;

        return new Vector3(pinchedX, compressedY, z);
    }

    private float PerspectiveScale(int row, int totalRows)
    {
        float rowRatio = (float)row / (totalRows - 1);
        return GameConstants.PERSPECTIVE_MIN_SCALE + rowRatio * GameConstants.PERSPECTIVE_SCALE_RANGE;
    }

    private void EnsureMaterials()
    {
        if (_materials.Count > 0) return;
        foreach (var color in _colors)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color.ToUnityColor();
            mat.SetFloat("_Glossiness", 0.7f);
            mat.SetFloat("_Metallic", 0.0f);
            _materials[color] = mat;
        }
    }
}
```

---

## Step 4: Balloon Materials (Pre-created for Editor)

Create these in `Assets/Materials/` via editor script or manually:

| Material | Color Hex | RGB |
|---|---|---|
| BalloonRed.mat | #E63946 | (0.902, 0.224, 0.275) |
| BalloonBlue.mat | #457B9D | (0.271, 0.482, 0.616) |
| BalloonYellow.mat | #F1FAEE | (0.945, 0.980, 0.933) |
| BalloonGreen.mat | #2A9D8F | (0.165, 0.616, 0.561) |
| BalloonPurple.mat | #9B59B6 | (0.608, 0.349, 0.714) |

All: Smoothness 0.7, Metallic 0.0, Standard shader.

---

## Important: Collision Setup

Balloons need:
1. **SphereCollider** (comes free from `CreatePrimitive(Sphere)`)
2. **Rigidbody** with `isKinematic = true` (balloons don't move, but Unity needs a Rigidbody on at least one side of a collision pair for `OnCollisionEnter` to fire)
3. **Layer 7** (`Enemies`) — the physics collision matrix already allows Projectiles (8) to collide with Enemies (7)

Darts need:
1. **Collider** (on the prefab)
2. **Rigidbody** with `isKinematic = false` (darts move via physics)
3. **Layer 8** (`Projectiles`)

---

## Verification Checklist

- [ ] 72 colored spheres visible in the scene
- [ ] 8 columns × 9 rows grid pattern
- [ ] Top row balloons noticeably smaller than bottom row
- [ ] Columns pinch toward center at top
- [ ] Colors randomly assigned from 5 options
- [ ] Each balloon has a SphereCollider and kinematic Rigidbody
- [ ] All balloons on layer 7
- [ ] No console errors

## Files Created
1. `Assets/Scripts/BalloonGame/BalloonData.cs`
2. `Assets/Scripts/BalloonGame/BalloonNode.cs`
3. `Assets/Scripts/BalloonGame/BalloonWall.cs`
