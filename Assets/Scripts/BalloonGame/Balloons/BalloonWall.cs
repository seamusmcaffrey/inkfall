using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public partial class BalloonWall : MonoBehaviour
{
    private readonly List<BalloonNode> _balloons = new();
    private readonly Dictionary<BalloonColor, Material> _materials = new();
    private readonly BalloonColor[] _colors = { BalloonColor.Red, BalloonColor.Blue, BalloonColor.Yellow, BalloonColor.Green, BalloonColor.Purple };

    [SerializeField] private RoomConfig _roomConfig;
    [SerializeField] private ObjectPool _balloonPool;

    private GameObject _balloonPrefab;
    private MaterialPropertyBlock _materialPropertyBlock;

    public IReadOnlyList<BalloonNode> Balloons => _balloons;

    [ContextMenu("Generate Wall")]
    public void GenerateWall()
    {
        GenerateWall(_roomConfig);
    }

    public void GenerateWall(RoomConfig roomConfig)
    {
        _roomConfig = roomConfig;
        ClearWall();
        EnsureMaterials();
        EnsurePropertyBlock();
        EnsurePool();

        int rows = roomConfig != null ? roomConfig.rows : GameConstants.BOARD_ROWS;
        int columns = roomConfig != null ? roomConfig.columns : GameConstants.BOARD_COLUMNS;
        float slotX = GameConstants.BOARD_WIDTH / columns;
        float slotY = GameConstants.BOARD_HEIGHT / rows;
        int specialCount = roomConfig != null ? Random.Range(roomConfig.minSpecials, roomConfig.maxSpecials + 1) : 0;
        int hazardCount = roomConfig != null ? Random.Range(roomConfig.minHazards, roomConfig.maxHazards + 1) : 0;
        List<Vector2Int> specialSlots = PickUniqueSlots(rows, columns, specialCount + hazardCount);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                BalloonTypeSO type = PickBalloonType(row, column, specialSlots, specialCount, hazardCount);
                float scale = PerspectiveScale(row, rows);
                Vector3 position = PerspectivePosition(row, column, rows, columns, slotX, slotY);

                GameObject balloon = GetBalloonInstance();
                if (balloon == null)
                {
                    continue;
                }

                balloon.name = $"Balloon_{row}_{column}";
                balloon.transform.SetParent(transform, false);
                balloon.transform.localPosition = position;
                float width = GameConstants.BALLOON_MAX_WIDTH * GameConstants.BALLOON_SLOT_RATIO_X * scale;
                float height = GameConstants.BALLOON_MAX_HEIGHT * GameConstants.BALLOON_SLOT_RATIO_Y * scale;
                balloon.transform.localScale = new Vector3(width, height, width);
                balloon.layer = GameConstants.LAYER_BALLOONS;

                var renderer = balloon.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = ResolveMaterial(type);
                ApplyAtmosphericFade(renderer, type, row, rows);

                var rigidbody = balloon.GetComponent<Rigidbody>();
                rigidbody.detectCollisions = true;
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;

                var node = balloon.GetComponent<BalloonNode>();
                BalloonJiggle jiggle = balloon.GetComponent<BalloonJiggle>();
                if (jiggle != null)
                {
                    jiggle.CaptureBaseScale();
                }

                var emblem = balloon.GetComponent<BalloonEmblem>();
                if (emblem != null)
                {
                    emblem.Configure(type != null ? type.specialType : BalloonSpecialType.Standard, type != null ? type.balloonColor : BalloonColor.Red);
                }

                node.Initialize(row, column, type);
                _balloons.Add(node);
            }
        }
    }

    [ContextMenu("Clear Wall")]
    public void ClearWall()
    {
        for (int index = _balloons.Count - 1; index >= 0; index--)
        {
            BalloonNode balloon = _balloons[index];
            if (balloon == null)
            {
                continue;
            }

            if (Application.isPlaying && _balloonPool != null)
            {
                _balloonPool.Return(balloon.gameObject);
            }
            else
            {
                DestroyImmediate(balloon.gameObject);
            }
        }

        _balloons.Clear();
    }

    public void TriggerPaintExplosion(BalloonNode source, float radius, DartController instigator)
    {
        float radiusSqr = radius * radius;
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon == null || balloon == source || balloon.IsPopped)
            {
                continue;
            }

            if ((balloon.transform.position - source.transform.position).sqrMagnitude <= radiusSqr)
            {
                balloon.Pop(instigator);
            }
        }
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

    public void ConfigureRoom(RoomConfig roomConfig)
    {
        _roomConfig = roomConfig;
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

    private void EnsurePropertyBlock()
    {
        _materialPropertyBlock ??= new MaterialPropertyBlock();
    }

    private void ApplyAtmosphericFade(Renderer renderer, BalloonTypeSO type, int row, int totalRows)
    {
        EnsurePropertyBlock();

        float rowRatio = (float)row / (totalRows - 1);
        float topBias = 1f - rowRatio;
        float fadeFactor = 1f - topBias * 0.08f;

        Color baseColor = ResolveDisplayColor(type);
        Color finalColor = baseColor * fadeFactor;
        finalColor.a = 1f;
        renderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetColor("_Color", finalColor);
        _materialPropertyBlock.SetColor("_BaseColor", finalColor);
        renderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
