using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BalloonWall : MonoBehaviour
{
    private readonly List<BalloonNode> _balloons = new();
    private readonly Dictionary<BalloonColor, Material> _materials = new();
    private readonly List<BalloonTypeSO> _runtimeTypes = new();
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

    private void ApplyAtmosphericFade(Renderer renderer, BalloonTypeSO type, int row, int totalRows)
    {
        EnsurePropertyBlock();

        float rowRatio = (float)row / (totalRows - 1);
        float topBias = 1f - rowRatio;
        float fade = 1f - topBias * 0.12f;

        Color baseColor = ResolveDisplayColor(type);
        Color darkenedColor = new(baseColor.r * 0.6f, baseColor.g * 0.6f, baseColor.b * 0.6f);
        Color finalColor = Color.Lerp(darkenedColor, baseColor, fade);
        renderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetColor("_Color", finalColor);
        _materialPropertyBlock.SetColor("_BaseColor", finalColor);
        renderer.SetPropertyBlock(_materialPropertyBlock);
    }

    private void EnsureMaterials()
    {
        if (_materials.Count > 0)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
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

    private BalloonTypeSO PickBalloonType(int row, int column, List<Vector2Int> specialSlots, int specialCount, int hazardCount)
    {
        List<BalloonTypeSO> availableTypes = GetAvailableTypes();
        Vector2Int slot = new(column, row);
        bool isSpecialSlot = specialSlots.Contains(slot);

        if (isSpecialSlot)
        {
            int slotIndex = specialSlots.IndexOf(slot);
            BalloonSpecialType requestedType = slotIndex < hazardCount ? BalloonSpecialType.Hazard : BalloonSpecialType.Paint;
            BalloonTypeSO specialType = availableTypes.Find(type => type.specialType == requestedType);
            if (specialType != null)
            {
                return specialType;
            }
        }

        BalloonColor color = _colors[Random.Range(0, _colors.Length)];
        BalloonTypeSO standardType = availableTypes.Find(type => type.specialType == BalloonSpecialType.Standard && type.balloonColor == color);
        return standardType ?? availableTypes[0];
    }

    private Material ResolveMaterial(BalloonTypeSO type)
    {
        if (type != null && type.materialOverride != null)
        {
            return type.materialOverride;
        }

        BalloonColor color = type != null ? type.balloonColor : BalloonColor.Red;
        return _materials[color];
    }

    private Color ResolveDisplayColor(BalloonTypeSO type)
    {
        if (type != null && type.materialOverride != null)
        {
            if (type.materialOverride.HasProperty("_BaseColor"))
            {
                return type.materialOverride.GetColor("_BaseColor");
            }

            if (type.materialOverride.HasProperty("_Color"))
            {
                return type.materialOverride.GetColor("_Color");
            }
        }

        return (type != null ? type.balloonColor : BalloonColor.Red).ToUnityColor();
    }

    private List<BalloonTypeSO> GetAvailableTypes()
    {
        if (GameConfigSO.Instance.balloonTypes != null && GameConfigSO.Instance.balloonTypes.Count > 0)
        {
            return GameConfigSO.Instance.balloonTypes;
        }

        if (_runtimeTypes.Count > 0)
        {
            return _runtimeTypes;
        }

        foreach (BalloonColor color in _colors)
        {
            BalloonTypeSO standard = ScriptableObject.CreateInstance<BalloonTypeSO>();
            standard.typeId = $"standard-{color.ToString().ToLowerInvariant()}";
            standard.displayName = $"{color.ToDisplayName()} Balloon";
            standard.balloonColor = color;
            standard.specialType = BalloonSpecialType.Standard;
            standard.basePoints = GameConstants.SCORE_PER_BALLOON;
            _runtimeTypes.Add(standard);
        }

        BalloonTypeSO paint = ScriptableObject.CreateInstance<BalloonTypeSO>();
        paint.typeId = "paint";
        paint.displayName = "Paint Balloon";
        paint.balloonColor = BalloonColor.Blue;
        paint.specialType = BalloonSpecialType.Paint;
        paint.basePoints = GameConstants.SCORE_PER_BALLOON + 25;
        paint.effectRadius = GameConfigSO.Instance.defaultPaintRadius;
        _runtimeTypes.Add(paint);

        BalloonTypeSO hazard = ScriptableObject.CreateInstance<BalloonTypeSO>();
        hazard.typeId = "hazard";
        hazard.displayName = "Hazard Balloon";
        hazard.balloonColor = BalloonColor.Purple;
        hazard.specialType = BalloonSpecialType.Hazard;
        hazard.basePoints = -GameConfigSO.Instance.hazardBalloonPenalty;
        hazard.hazardPenalty = GameConfigSO.Instance.hazardBalloonPenalty;
        _runtimeTypes.Add(hazard);

        BalloonTypeSO gold = ScriptableObject.CreateInstance<BalloonTypeSO>();
        gold.typeId = "gold";
        gold.displayName = "Gold Balloon";
        gold.balloonColor = BalloonColor.Yellow;
        gold.specialType = BalloonSpecialType.Gold;
        gold.basePoints = GameConstants.SCORE_PER_BALLOON + 50;
        gold.currencyReward = 3;
        _runtimeTypes.Add(gold);

        return _runtimeTypes;
    }

    private static List<Vector2Int> PickUniqueSlots(int rows, int columns, int count)
    {
        var slots = new List<Vector2Int>(count);
        int safety = rows * columns * 2;
        while (slots.Count < count && safety-- > 0)
        {
            Vector2Int candidate = new(Random.Range(0, columns), Random.Range(0, rows));
            if (!slots.Contains(candidate))
            {
                slots.Add(candidate);
            }
        }

        return slots;
    }

    private void EnsurePool()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (_balloonPool != null)
        {
            return;
        }

        _balloonPrefab = CreateBalloonObject();
        _balloonPrefab.SetActive(false);
        _balloonPool = ComponentUtility.EnsureComponent<ObjectPool>(gameObject);
        _balloonPool.Configure(_balloonPrefab, GetPoolSize());
    }

    private GameObject GetBalloonInstance()
    {
        if (Application.isPlaying && _balloonPool != null)
        {
            return _balloonPool.Get();
        }

        return CreateBalloonObject();
    }

    private GameObject CreateBalloonObject()
    {
        var balloon = new GameObject("Balloon");
        balloon.AddComponent<MeshFilter>().sharedMesh = BalloonMeshGenerator.GetSharedMesh();
        balloon.AddComponent<MeshRenderer>();
        balloon.AddComponent<SphereCollider>();
        var rigidbody = balloon.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        balloon.AddComponent<BalloonNode>();
        balloon.AddComponent<BalloonJiggle>();
        return balloon;
    }

    private int GetPoolSize()
    {
        int maxCount = GameConstants.TOTAL_BALLOONS;
        if (GameConfigSO.Instance.roomTemplates != null)
        {
            foreach (RoomTemplateSO template in GameConfigSO.Instance.roomTemplates)
            {
                if (template != null)
                {
                    maxCount = Mathf.Max(maxCount, template.rows * template.columns);
                }
            }
        }

        return Mathf.Max(GameConstants.TOTAL_BALLOONS, maxCount);
    }

    private void EnsurePropertyBlock()
    {
        _materialPropertyBlock ??= new MaterialPropertyBlock();
    }
}
