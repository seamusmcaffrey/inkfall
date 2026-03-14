using System.Collections.Generic;
using UnityEngine;

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

    public IReadOnlyList<BalloonNode> Balloons => _balloons;

    [ContextMenu("Generate Wall")]
    public void GenerateWall()
    {
        ClearWall();
        EnsureMaterials();

        int rows = GameConstants.BOARD_ROWS;
        int columns = GameConstants.BOARD_COLUMNS;
        float slotX = GameConstants.BOARD_WIDTH / columns;
        float slotY = GameConstants.BOARD_HEIGHT / rows;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
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
                node.Initialize(row, column, color);
                _balloons.Add(node);
            }
        }
    }

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
