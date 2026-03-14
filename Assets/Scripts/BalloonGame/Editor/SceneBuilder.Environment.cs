#if UNITY_EDITOR
using UnityEngine;

public static partial class BalloonSceneBuilder
{
    private static void CreateBackWall(Material material)
    {
        float boardCenterY = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;

        var backWall = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backWall.name = "BackWall";
        backWall.layer = GameConstants.LAYER_ENVIRONMENT;
        backWall.transform.position = new Vector3(0f, boardCenterY, 1f);
        backWall.transform.localScale = new Vector3(GameConstants.BOARD_WIDTH + 1f, GameConstants.BOARD_HEIGHT + 1f, 1f);
        backWall.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(backWall.GetComponent<MeshCollider>());
    }

    private static void CreateFrameWalls(Material material, PhysicsMaterial bounceMaterial, PhysicsMaterial deadMaterial)
    {
        float boardCenterY = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;
        Vector3 wallScale = new(GameConstants.SIDE_WALL_WIDTH, GameConstants.BOARD_HEIGHT + 1f, 2f);

        var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.layer = GameConstants.LAYER_ENVIRONMENT;
        leftWall.transform.position = new Vector3(
            GameConstants.BOARD_LEFT - GameConstants.SIDE_WALL_WIDTH * 0.5f,
            boardCenterY,
            0.5f);
        leftWall.transform.localScale = wallScale;
        leftWall.GetComponent<Renderer>().sharedMaterial = material;
        leftWall.GetComponent<BoxCollider>().material = bounceMaterial;

        var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.layer = GameConstants.LAYER_ENVIRONMENT;
        rightWall.transform.position = new Vector3(
            GameConstants.BOARD_RIGHT + GameConstants.SIDE_WALL_WIDTH * 0.5f,
            boardCenterY,
            0.5f);
        rightWall.transform.localScale = wallScale;
        rightWall.GetComponent<Renderer>().sharedMaterial = material;
        rightWall.GetComponent<BoxCollider>().material = bounceMaterial;

        var topWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topWall.name = "TopWall";
        topWall.layer = GameConstants.LAYER_ENVIRONMENT;
        topWall.transform.position = new Vector3(
            0f,
            GameConstants.BOARD_TOP + GameConstants.SIDE_WALL_WIDTH * 0.5f,
            0.5f);
        topWall.transform.localScale = new Vector3(
            GameConstants.BOARD_WIDTH + GameConstants.SIDE_WALL_WIDTH * 2f + 1f,
            GameConstants.SIDE_WALL_WIDTH,
            2f);
        topWall.GetComponent<Renderer>().sharedMaterial = material;
        topWall.GetComponent<BoxCollider>().material = deadMaterial;
    }

    private static void CreateLane(Material material)
    {
        float laneCenterY = (GameConstants.LANE_TOP + GameConstants.LANE_BOTTOM) * 0.5f;

        var laneFloor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        laneFloor.name = "LaneFloor";
        laneFloor.layer = GameConstants.LAYER_ENVIRONMENT;
        laneFloor.transform.position = new Vector3(0f, laneCenterY, 1f);
        laneFloor.transform.localScale = new Vector3(8f, GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM, 1f);
        laneFloor.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(laneFloor.GetComponent<MeshCollider>());
    }

    private static void CreateLaunchOrigin()
    {
        var launchOrigin = new GameObject("LaunchOrigin");
        launchOrigin.transform.position = GameConstants.LAUNCH_POSITION;
    }
}
#endif
