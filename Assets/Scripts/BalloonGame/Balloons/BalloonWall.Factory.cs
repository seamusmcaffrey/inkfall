using UnityEngine;

public partial class BalloonWall
{
    private void EnsurePool()
    {
        if (!Application.isPlaying || _balloonPool != null)
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
        {
            balloon.AddComponent<SphereCollider>();
        }

        if (balloon.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = balloon.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (balloon.GetComponent<BalloonNode>() == null)
        {
            balloon.AddComponent<BalloonNode>();
        }

        if (balloon.GetComponent<BalloonJiggle>() == null)
        {
            balloon.AddComponent<BalloonJiggle>();
        }

        if (prefabSource == null && balloon.GetComponent<BalloonEmblem>() == null)
        {
            balloon.AddComponent<BalloonEmblem>();
        }

        return balloon;
    }

    private int GetPoolSize()
    {
        int maxCount = GameConstants.TOTAL_BALLOONS;
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
