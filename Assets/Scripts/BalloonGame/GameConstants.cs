using UnityEngine;

public static partial class GameConstants
{
    public const bool DEV_SKIP_INTRO = true;

    public const float CAMERA_FOV = 60f;
    public const float CAMERA_Y_CENTER = 2.5f;
    public const float CAMERA_DISTANCE = -18f;
    public const float TARGET_WORLD_WIDTH = 8.6f;

    /// <summary>Visible half-height at Z=0 from default camera distance (for environment sizing).</summary>
    public const float CAMERA_VISIBLE_HALF_HEIGHT = 10.4f;

    /// <summary>Z depth where the balloon board sits. Darts fly from Z=0 toward this plane.</summary>
    public const float BOARD_Z = 4f;

    /// <summary>Forward Z velocity added to all darts so they fly into the scene.</summary>
    public const float DART_FORWARD_SPEED = 12f;
    public const int BOARD_COLUMNS = 5;
    public const int BOARD_ROWS = 5;
    public const int TOTAL_BALLOONS = BOARD_COLUMNS * BOARD_ROWS;

    public const float BOARD_LEFT = -4.0f;
    public const float BOARD_RIGHT = 4.0f;
    public const float BOARD_TOP = 10.0f;
    public const float BOARD_BOTTOM = 2.0f;
    public const float BOARD_WIDTH = BOARD_RIGHT - BOARD_LEFT;
    public const float BOARD_HEIGHT = BOARD_TOP - BOARD_BOTTOM;
    public const float BOARD_CENTER_X = (BOARD_LEFT + BOARD_RIGHT) * 0.5f;
    public const float BOARD_CENTER_Y = (BOARD_BOTTOM + BOARD_TOP) * 0.5f;

    public const float LANE_TOP = -0.8f;
    public const float LANE_BOTTOM = -3.5f;

    /// <summary>
    /// Unified slingshot + dart origin. Player pulls back here and darts fire from here.
    /// Near floor level, 12 units from the board (BOARD_Z=4), like a real carnival booth.
    /// </summary>
    public static readonly Vector3 LAUNCH_POSITION = new(0f, ROOM_FLOOR_Y + 2f, -8f);

    /// <summary>Balloon width as a fraction of slot width (>1 means overlap neighbors).</summary>
    public const float BALLOON_SLOT_FILL_X = 0.88f;
    /// <summary>Balloon height as a fraction of slot height.</summary>
    public const float BALLOON_SLOT_FILL_Y = 0.95f;

    public const float MAX_PULL_DISTANCE = 1.8f;
    public const float AIM_ACTIVATION_RADIUS = 2.5f;
    public const float MIN_PULL_DISTANCE = 0.5f;
    public const float PULL_CANCEL_RETURN_RADIUS = 0.3f;
    public const float PULL_DEAD_ZONE = 0.2f;
    /// <summary>How fast the aim drifts toward the finger position (units per second).
    /// Lower = smoother/slower aim movement.</summary>
    public const float AIM_SMOOTH_SPEED = 5f;
    public const int STARTING_DARTS = 4;

    public const int OPENING_ROOM_TARGET_SCORE = 450;
    public const int BASE_TARGET_SCORE = 650;
    public const int SCORE_PER_BALLOON = 100;
    public const float COMBO_WINDOW_SECONDS = 1.5f;
    public const float COMBO_MULTIPLIER_PER_STEP = 0.28f;
    public const int COMBO_MAX_STACK = 20;

    // Physics
    public const float DART_GRAVITY = -18f;
    public const float SIDE_WALL_WIDTH = 0.3f;
    public const float WALL_BOUNCINESS = 0.8f;
    public const float WALL_FRICTION = 0.1f;
    public const float DEFAULT_DART_LIFETIME = 5f;
    public const float DEFAULT_FIXED_TIMESTEP_HIGH = 0.02f;
    public const float DEFAULT_FIXED_TIMESTEP_LOW = 0.025f;
    public const int DEFAULT_MAX_RICOCHETS = 1;
    public const int DEFAULT_MAX_PIERCE = 1;
    public const float DEFAULT_PAINT_RADIUS = 1.5f;

    public const int LAYER_ENVIRONMENT = 3;
    public const int LAYER_PROJECTILES = 8;
    public const int LAYER_BALLOONS = 7;
    public const int LAYER_RICOCHET = 9;
    public const int LAYER_UI_WORLD = 10;

    // Pooling and performance
    public const int DEFAULT_POOL_SIZE = 8;
    public const int AUDIO_SOURCE_POOL_SIZE = 12;
    public const int VFX_POOL_INITIAL_SIZE = 8;
    public const int MAX_ACTIVE_PARTICLES = 300;

    // Audio
    public const int AUDIO_SAMPLE_RATE = 44100;
    public const float DEFAULT_MASTER_VOLUME = 0.85f;
    public const float DEFAULT_SFX_VOLUME = 1f;
    public const float DEFAULT_MUSIC_VOLUME = 0.55f;
    public const float DEFAULT_UI_VOLUME = 0.9f;

    // UI
    public static readonly Vector2 UI_REFERENCE_RESOLUTION = new(1080f, 1920f);
    public const float SAFE_AREA_PADDING = 24f;
    public const float HUD_TOP_MARGIN = 8f;
    public const float HUD_SIDE_MARGIN = 10f;
    public const float HUD_PROGRESS_HEIGHT = 18f;
    public const float FLOATING_SCORE_Z_OFFSET = -1.5f;
    public const float MESSAGE_FONT_SIZE = 36f;

    // Touch / aim polish
    public const int TRAJECTORY_POINT_COUNT = 30;
    public const float TRAJECTORY_DURATION = 0.7f;
    public const float TRAJECTORY_DOT_SPACING = 0.18f;
    public const float AIM_ASSIST_MAX_ANGLE = 8f;
    public const float AIM_ASSIST_MAX_DISTANCE = 20f;

}
