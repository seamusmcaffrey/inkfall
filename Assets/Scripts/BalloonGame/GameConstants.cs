using UnityEngine;

public static class GameConstants
{
    public const float CAMERA_ORTHO_SIZE = 10f;

    public const int BOARD_COLUMNS = 8;
    public const int BOARD_ROWS = 9;
    public const int TOTAL_BALLOONS = BOARD_COLUMNS * BOARD_ROWS;

    public const float BOARD_LEFT = -4.0f;
    public const float BOARD_RIGHT = 4.0f;
    public const float BOARD_TOP = 8.5f;
    public const float BOARD_BOTTOM = -1.0f;
    public const float BOARD_WIDTH = BOARD_RIGHT - BOARD_LEFT;
    public const float BOARD_HEIGHT = BOARD_TOP - BOARD_BOTTOM;

    public const float LANE_TOP = -2.0f;
    public const float LANE_BOTTOM = -8.5f;

    public static readonly Vector3 LAUNCH_POSITION = new(0f, -5.5f, 0f);

    public const float BALLOON_MAX_WIDTH = 0.78f;
    public const float BALLOON_MAX_HEIGHT = 0.94f;
    public const float BALLOON_SLOT_RATIO_X = 0.9f;
    public const float BALLOON_SLOT_RATIO_Y = 0.95f;

    public const float PERSPECTIVE_MIN_SCALE = 0.72f;
    public const float PERSPECTIVE_SCALE_RANGE = 0.28f;
    public const float PERSPECTIVE_HORIZONTAL_PINCH = 0.12f;
    public const float PERSPECTIVE_VERTICAL_COMPRESSION = 0.14f;

    public const float MAX_PULL_DISTANCE = 3.0f;
    public const float PULL_SPEED_EXPONENT = 1.45f;
    public const float MIN_LAUNCH_SPEED = 10f;
    public const float MAX_LAUNCH_SPEED = 40f;
    public const float AIM_ACTIVATION_RADIUS = 4f;

    public const int BASE_TARGET_SCORE = 3000;
    public const int STARTING_DARTS = 4;
    public const int SCORE_PER_BALLOON = 100;

    public const float SIDE_WALL_WIDTH = 0.3f;
    public const float WALL_BOUNCINESS = 0.8f;
    public const float WALL_FRICTION = 0.1f;

    public const int LAYER_ENVIRONMENT = 3;
    public const int LAYER_PROJECTILES = 8;
    public const int LAYER_BALLOONS = 7;
}
