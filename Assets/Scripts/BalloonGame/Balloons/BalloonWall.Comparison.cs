using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Temporary comparison system: assigns different Loafbrr balloon meshes per row
/// so we can visually evaluate which looks best with BalloonLit shader.
/// </summary>
public partial class BalloonWall
{
    private const string LoafbrrPrefabRoot = "Assets/LoafbrrAssets/Balloons/prefab/Baloons/";
    private const bool ComparisonModeEnabled = true;

    private static readonly string[] ComparisonPrefabNames =
    {
        "Balloon_Balloon",
        "Balloon_Heart",
        "Balloon_Star",
        "Balloon_Dog",
        "Balloon_Flower_A",
    };

    private GameObject CreateComparisonBalloon(int row)
    {
#if UNITY_EDITOR
        if (!ComparisonModeEnabled)
            return null;

        int prefabIndex = row % ComparisonPrefabNames.Length;
        string path = LoafbrrPrefabRoot + ComparisonPrefabNames[prefabIndex] + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            return null;

        GameObject balloon = Instantiate(prefab);
        balloon.name = "Balloon";

        if (balloon.GetComponent<SphereCollider>() == null)
            balloon.AddComponent<SphereCollider>();

        if (balloon.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = balloon.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (balloon.GetComponent<BalloonNode>() == null)
            balloon.AddComponent<BalloonNode>();

        if (balloon.GetComponent<BalloonJiggle>() == null)
            balloon.AddComponent<BalloonJiggle>();

        return balloon;
#else
        return null;
#endif
    }
}
