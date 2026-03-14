using UnityEngine;

public static class GameConstants
{
    // Layout
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
    public const float AIM_ACTIVATION_RADIUS = 4f;
    public const float MIN_PULL_DISTANCE = 0.5f;
    public const float PULL_CANCEL_RETURN_RADIUS = 0.3f;
    public const float PULL_DEAD_ZONE = 0.2f;
    public const int STARTING_DARTS = 4;

    public const int BASE_TARGET_SCORE = 3000;
    public const int SCORE_PER_BALLOON = 100;
    public const float COMBO_WINDOW_SECONDS = 1.5f;
    public const float COMBO_MULTIPLIER_PER_STEP = 0.5f;
    public const int COMBO_MAX_STACK = 20;

    // Physics
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
    public const float HUD_TOP_MARGIN = 48f;
    public const float HUD_SIDE_MARGIN = 36f;
    public const float HUD_PROGRESS_HEIGHT = 18f;
    public const float FLOATING_SCORE_Z_OFFSET = -1.5f;
    public const float MESSAGE_FONT_SIZE = 72f;

    // Touch / aim polish
    public const int TRAJECTORY_POINT_COUNT = 30;
    public const float TRAJECTORY_DURATION = 2f;
    public const float TRAJECTORY_DOT_SPACING = 0.18f;
    public const float AIM_ASSIST_MAX_ANGLE = 8f;
    public const float AIM_ASSIST_MAX_DISTANCE = 5.5f;

    // Decals / VFX
    public const float DECAL_Z_OFFSET = -0.05f;
    public const float DEFAULT_SCREEN_FADE_DURATION = 0.3f;
    public const int MAX_PAINT_DRIPS = 6;
}
