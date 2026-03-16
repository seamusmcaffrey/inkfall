using UnityEngine;

/// <summary>
/// Builds the 3D carnival booth room: back wall, side walls, floor, ceiling.
/// Creates an enclosed space that frames the balloon board.
/// </summary>
public partial class EnvironmentBuilder
{
    private const float WallTiling = 2f;
    private const float FloorTiling = 3f;
    private const float WallSmoothness = 0.15f;
    private const float WallMetallic = 0.0f;
    private const float FloorSmoothness = 0.35f;
    private const float FloorMetallic = 0.05f;
    private const float CeilingSmoothness = 0.1f;
    private const float TrimHeight = 0.15f;
    private const float TrimDepth = 0.06f;

    private static readonly Color TrimColor = new(0.08f, 0.065f, 0.05f);

    private void BuildRoom()
    {
        float backZ = GameConstants.ROOM_BACK_Z;
        float frontZ = GameConstants.ROOM_FRONT_Z;
        float halfW = GameConstants.ROOM_HALF_WIDTH;
        float floorY = GameConstants.ROOM_FLOOR_Y;
        float ceilY = GameConstants.ROOM_CEILING_Y;
        float depth = GameConstants.ROOM_DEPTH;
        float roomHeight = ceilY - floorY;
        float roomWidth = halfW * 2f;
        float centerY = (ceilY + floorY) * 0.5f;
        float centerZ = (backZ + frontZ) * 0.5f;

        Texture2D wallTex = ProceduralTextures.GenerateWallTexture(RoomTextureSize, RoomTextureSize);
        Material wallMat = CreateTexturedMaterial(wallTex, Color.white,
            smoothness: WallSmoothness, metallic: WallMetallic, tiling: WallTiling);

        BuildBackWall(backZ, floorY, ceilY, roomWidth, roomHeight, wallMat);
        BuildSideWalls(halfW, centerY, centerZ, depth, roomHeight, wallMat);
        BuildRoomFloor(floorY, centerZ, depth, roomWidth);
        BuildCeiling(ceilY, centerZ, depth, roomWidth);
        BuildFloorTrim(floorY, backZ, frontZ, halfW);
    }

    private void BuildBackWall(float backZ, float floorY, float ceilY,
        float roomWidth, float roomHeight, Material wallMat)
    {
        SetPanel("RoomBackWall", PrimitiveType.Quad,
            new Vector3(0f, (ceilY + floorY) * 0.5f, backZ),
            new Vector3(roomWidth, roomHeight, 1f), wallMat);
    }

    private void BuildSideWalls(float halfW,
        float centerY, float centerZ, float depth, float roomHeight, Material wallMat)
    {
        // Left wall faces +X (inward)
        SetPanel("RoomLeftWall", PrimitiveType.Quad,
            new Vector3(-halfW, centerY, centerZ),
            Quaternion.Euler(0f, 90f, 0f),
            new Vector3(depth, roomHeight, 1f), wallMat);

        // Right wall faces -X (inward)
        SetPanel("RoomRightWall", PrimitiveType.Quad,
            new Vector3(halfW, centerY, centerZ),
            Quaternion.Euler(0f, -90f, 0f),
            new Vector3(depth, roomHeight, 1f), wallMat);
    }

    private void BuildRoomFloor(float floorY, float centerZ,
        float depth, float roomWidth)
    {
        Texture2D floorTex = ProceduralTextures.GenerateFloorTexture(RoomTextureSize, RoomTextureSize);
        Material floorMat = CreateTexturedMaterial(floorTex, FloorTint,
            smoothness: FloorSmoothness, metallic: FloorMetallic, tiling: FloorTiling);

        // Floor faces +Y (visible from above)
        SetPanel("RoomFloor", PrimitiveType.Quad,
            new Vector3(0f, floorY, centerZ),
            Quaternion.Euler(90f, 0f, 0f),
            new Vector3(roomWidth, depth, 1f), floorMat);
    }

    private void BuildCeiling(float ceilY, float centerZ,
        float depth, float roomWidth)
    {
        Material ceilMat = CreateMaterial(CeilingColor, smoothness: CeilingSmoothness);

        // Ceiling faces -Y (visible from below)
        SetPanel("RoomCeiling", PrimitiveType.Quad,
            new Vector3(0f, ceilY, centerZ),
            Quaternion.Euler(-90f, 0f, 0f),
            new Vector3(roomWidth, depth, 1f), ceilMat);
    }

    private void BuildFloorTrim(float floorY, float backZ, float frontZ, float halfW)
    {
        Material trimMat = CreateMaterial(TrimColor, smoothness: 0.3f, metallic: 0.1f);

        // Baseboard trim along the back wall
        float roomWidth = halfW * 2f;
        SetPanel("TrimBack", PrimitiveType.Cube,
            new Vector3(0f, floorY + TrimHeight * 0.5f, backZ - TrimDepth * 0.5f),
            new Vector3(roomWidth, TrimHeight, TrimDepth), trimMat);

        // Baseboard trim along side walls
        float depth = backZ - frontZ;
        float centerZ = (backZ + frontZ) * 0.5f;
        SetPanel("TrimLeft", PrimitiveType.Cube,
            new Vector3(-halfW + TrimDepth * 0.5f, floorY + TrimHeight * 0.5f, centerZ),
            new Vector3(TrimDepth, TrimHeight, depth), trimMat);
        SetPanel("TrimRight", PrimitiveType.Cube,
            new Vector3(halfW - TrimDepth * 0.5f, floorY + TrimHeight * 0.5f, centerZ),
            new Vector3(TrimDepth, TrimHeight, depth), trimMat);
    }
}
