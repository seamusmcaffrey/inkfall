using System.Collections.Generic;
using UnityEngine;

public partial class BalloonWall
{
    private readonly List<BalloonTypeSO> _runtimeTypes = new();

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

        _runtimeTypes.Add(CreateRuntimeType("paint", "Paint Balloon", BalloonColor.Blue,
            BalloonSpecialType.Paint, GameConstants.SCORE_PER_BALLOON + 25,
            effectRadius: GameConfigSO.Instance.defaultPaintRadius));
        _runtimeTypes.Add(CreateRuntimeType("hazard", "Hazard Balloon", BalloonColor.Purple,
            BalloonSpecialType.Hazard, -GameConfigSO.Instance.hazardBalloonPenalty,
            hazardPenalty: GameConfigSO.Instance.hazardBalloonPenalty));
        _runtimeTypes.Add(CreateRuntimeType("gold", "Gold Balloon", BalloonColor.Yellow,
            BalloonSpecialType.Gold, GameConstants.SCORE_PER_BALLOON + 50, currencyReward: 3));

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
        GameObject balloon;
        GameObject prefabSource = GameConfigSO.Instance.balloonPrefabOverride;

        if (prefabSource != null)
        {
            balloon = Instantiate(prefabSource);
            balloon.name = "Balloon";
        }
        else
        {
            balloon = new GameObject("Balloon");
            Mesh mesh = GameConfigSO.Instance.balloonMeshOverride != null
                ? GameConfigSO.Instance.balloonMeshOverride
                : BalloonMeshGenerator.GetSharedMesh();
            balloon.AddComponent<MeshFilter>().sharedMesh = mesh;
            balloon.AddComponent<MeshRenderer>();
        }

        if (balloon.GetComponent<SphereCollider>() == null)
            balloon.AddComponent<SphereCollider>();
        if (balloon.GetComponent<Rigidbody>() == null)
        {
            var rb = balloon.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (balloon.GetComponent<BalloonNode>() == null)
            balloon.AddComponent<BalloonNode>();
        if (balloon.GetComponent<BalloonJiggle>() == null)
            balloon.AddComponent<BalloonJiggle>();
        if (prefabSource == null && balloon.GetComponent<BalloonEmblem>() == null)
            balloon.AddComponent<BalloonEmblem>();

        return balloon;
    }

    private static BalloonTypeSO CreateRuntimeType(string id, string name, BalloonColor color,
        BalloonSpecialType special, int points, float effectRadius = 0f, int hazardPenalty = 0, int currencyReward = 0)
    {
        var t = ScriptableObject.CreateInstance<BalloonTypeSO>();
        t.typeId = id; t.displayName = name; t.balloonColor = color;
        t.specialType = special; t.basePoints = points;
        t.effectRadius = effectRadius; t.hazardPenalty = hazardPenalty; t.currencyReward = currencyReward;
        return t;
    }

    private int GetPoolSize()
    {
        int maxCount = GameConstants.TOTAL_BALLOONS;
        if (GameConfigSO.Instance.roomTemplates == null) return maxCount;
        foreach (RoomTemplateSO template in GameConfigSO.Instance.roomTemplates)
        {
            if (template != null)
            {
                maxCount = Mathf.Max(maxCount, template.rows * template.columns);
            }
        }
        return maxCount;
    }
}
