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
    [SerializeField] private RunSystemsManager _runSystemsManager;
    [SerializeField] private PerkManager _perkManager;

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
        ComponentUtility.ResolveSceneReference(this, ref _runSystemsManager);
        ComponentUtility.ResolveSceneReference(this, ref _perkManager);
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
        if (_perkManager != null)
        {
            specialCount = Mathf.Max(0, specialCount + _perkManager.ExtraSpecialSpawns);
            hazardCount = Mathf.Max(0, hazardCount + _perkManager.ExtraHazardSpawns);
        }
        List<Vector2Int> specialSlots = PickUniqueSlots(rows, columns, specialCount + hazardCount);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                BalloonTypeSO type = PickBalloonType(row, column, specialSlots, specialCount, hazardCount);
                StickerFamily stickerFamily = ResolveStickerFamily(type);
                float scale = PerspectiveScale(row, rows);
                Vector3 position = PerspectivePosition(row, column, rows, columns, slotX, slotY);

                GameObject balloon = ComparisonModeEnabled
                    ? CreateComparisonBalloon(row) ?? GetBalloonInstance()
                    : GetBalloonInstance();
                if (balloon == null)
                {
                    continue;
                }

                balloon.name = $"Balloon_{row}_{column}";
                balloon.transform.SetParent(transform, false);
                balloon.transform.localPosition = position;
                ApplyBalloonScale(balloon, slotX, slotY, scale);
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

                if (!ComparisonModeEnabled)
                {
                    var emblem = balloon.GetComponent<BalloonEmblem>();
                    if (emblem != null)
                    {
                        emblem.Configure(
                            type != null ? type.specialType : BalloonSpecialType.Standard,
                            type != null ? type.balloonColor : BalloonColor.Red,
                            stickerFamily);
                    }
                }

                node.Initialize(row, column, type, stickerFamily);
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
        float baseX = GameConstants.BOARD_LEFT + slotX * 0.5f + column * slotX;
        float baseY = GameConstants.BOARD_TOP - slotY * 0.5f - row * slotY;
        return new Vector3(baseX, baseY, GameConstants.BOARD_Z);
    }

    private float PerspectiveScale(int row, int totalRows)
    {
        return 1f;
    }

    private void EnsurePropertyBlock()
    {
        _materialPropertyBlock ??= new MaterialPropertyBlock();
    }

    private static void ApplyBalloonScale(GameObject balloon, float slotX, float slotY, float scale)
    {
        float desiredWidth = slotX * GameConstants.BALLOON_SLOT_FILL_X * scale;
        float desiredHeight = slotY * GameConstants.BALLOON_SLOT_FILL_Y * scale;

        var filter = balloon.GetComponentInChildren<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
        {
            Vector3 meshSize = filter.sharedMesh.bounds.size;
            float meshWidth = Mathf.Max(meshSize.x, meshSize.z, 0.001f);
            float meshHeight = Mathf.Max(meshSize.y, 0.001f);
            balloon.transform.localScale = new Vector3(
                desiredWidth / meshWidth,
                desiredHeight / meshHeight,
                desiredWidth / meshWidth);
        }
        else
        {
            balloon.transform.localScale = new Vector3(desiredWidth, desiredHeight, desiredWidth);
        }
    }

    private void ApplyAtmosphericFade(Renderer renderer, BalloonTypeSO type, int row, int totalRows)
    {
        if (type != null && type.materialOverride != null)
            return;

        EnsurePropertyBlock();

        Color baseColor = ResolveDisplayColor(type);
        renderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetColor("_Color", baseColor);
        _materialPropertyBlock.SetColor("_BaseColor", baseColor);
        renderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
