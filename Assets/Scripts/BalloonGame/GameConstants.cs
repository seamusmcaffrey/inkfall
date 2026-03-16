using UnityEngine;

public static class GameConstants
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
    public const float DART_FORWARD_SPEED = 14f;
    public const int BOARD_COLUMNS = 5;
    public const int BOARD_ROWS = 5;
    public const int TOTAL_BALLOONS = BOARD_COLUMNS * BOARD_ROWS;

    public const float BOARD_LEFT = -4.0f;
    public const float BOARD_RIGHT = 4.0f;
    public const float BOARD_TOP = 8.0f;
    public const float BOARD_BOTTOM = 0.0f;
    public const float BOARD_WIDTH = BOARD_RIGHT - BOARD_LEFT;
    public const float BOARD_HEIGHT = BOARD_TOP - BOARD_BOTTOM;
    public const float BOARD_CENTER_X = (BOARD_LEFT + BOARD_RIGHT) * 0.5f;
    public const float BOARD_CENTER_Y = (BOARD_BOTTOM + BOARD_TOP) * 0.5f;

    public const float LANE_TOP = -0.8f;
    public const float LANE_BOTTOM = -3.5f;

    /// <summary>
    /// Launch position near floor level, well in front of the board.
    /// Player throws upward and forward, like a real carnival booth.
    /// 12 units from the board (BOARD_Z=4).
    /// </summary>
    public static readonly Vector3 LAUNCH_POSITION = new(0f, ROOM_FLOOR_Y + 1f, -8f);

    /// <summary>
    /// Where darts actually spawn and begin their flight — roughly screen center,
    /// closer to the board than the slingshot. Separated from LAUNCH_POSITION
    /// so the player pulls from the bottom of the screen but darts fire from mid-screen.
    /// </summary>
    public static readonly Vector3 FIRE_ORIGIN = new(0f, CAMERA_Y_CENTER, -4f);

    /// <summary>Balloon width as a fraction of slot width (>1 means overlap neighbors).</summary>
    public const float BALLOON_SLOT_FILL_X = 0.792f;
    /// <summary>Balloon height as a fraction of slot height.</summary>
    public const float BALLOON_SLOT_FILL_Y = 0.891f;

    public const float MAX_PULL_DISTANCE = 1.8f;
    public const float AIM_ACTIVATION_RADIUS = 2.5f;
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
    public const float DART_GRAVITY = -14f;
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
    public const float TRAJECTORY_DURATION = 1.2f;
    public const float TRAJECTORY_DOT_SPACING = 0.18f;
    public const float AIM_ASSIST_MAX_ANGLE = 8f;
    public const float AIM_ASSIST_MAX_DISTANCE = 20f;

    // Screen FX
    public const float SHAKE_PERLIN_SPEED = 25f;
    public const float SHAKE_DECAY_EXPONENT = 2.2f;
    public const float SHAKE_COMBO_ESCALATION_PER_HIT = 0.15f;
    public const int SHAKE_COMBO_ESCALATION_CAP = 10;
    public const float SLOMO_MIN_TIME_SCALE = 0.1f;
    public const float SLOMO_COMBO_SCALE_PER_HIT = 0.06f;
    public const float SLOMO_COMBO_DURATION_PER_HIT = 0.1f;
    public const int SLOMO_COMBO_THRESHOLD_DEFAULT = 3;
    public const float CHROMATIC_COMBO_SCALE_PER_HIT = 0.2f;
    public const float CHROMATIC_COMBO_SCALE_MAX = 2f;

    // Dart arc physics
    public const float DART_PEAK_VELOCITY_THRESHOLD = 0.5f;
    public const float DART_ARC_SCALE_MIN = 1.0f;
    public const float DART_ARC_SCALE_MAX = 1.3f;

    // Aim-point ballistic model
    /// <summary>Max aim offset from board center at full pull (X axis).</summary>
    public const float AIM_RANGE_X = 4.5f;
    /// <summary>Max aim offset from board center at full pull (Y axis).</summary>
    public const float AIM_RANGE_Y = 4.5f;
    /// <summary>World-space radius for aim-assist snap on the board plane.</summary>
    public const float AIM_ASSIST_SNAP_RADIUS = 1.5f;
    /// <summary>Overshoot margin beyond board edges for aim clamping.</summary>
    public const float AIM_CLAMP_MARGIN = 0.5f;

    // Room geometry
    public const float ROOM_BACK_Z = 6f;
    public const float ROOM_FRONT_Z = -2f;
    public const float ROOM_HALF_WIDTH = 7f;
    public const float ROOM_FLOOR_Y = -1.5f;
    public const float ROOM_CEILING_Y = 10.5f;
    public const float ROOM_DEPTH = ROOM_BACK_Z - ROOM_FRONT_Z;

    // Decals / VFX
    public const float DECAL_Z_OFFSET = -0.05f;
    public const float DEFAULT_SCREEN_FADE_DURATION = 0.3f;
    public const int MAX_PAINT_DRIPS = 6;

    // Persistent splatters
    public const int MAX_PERSISTENT_SPLATTERS = 50;
    public const int SPLATTER_POOL_SIZE = 55;
    public const float SPLATTER_Z_OFFSET = -0.03f;
    public const float SPLATTER_MIN_SCALE = 0.35f;
    public const float SPLATTER_MAX_SCALE = 0.75f;
    public const float SPLATTER_NEON_BOOST = 0.55f;
    public const float SPLATTER_BASE_ALPHA = 0.92f;
    public const float SPLATTER_FADE_DURATION = 1.2f;

    // Combo Flash VFX
    public const float COMBO_FLASH_EDGE_THICKNESS = 0.18f;
    public const float COMBO_FLASH_ALPHA_PER_COMBO = 0.06f;
    public const float COMBO_FLASH_ALPHA_CAP = 0.55f;
    public const int COMBO_FLASH_EDGE_COUNT = 4;

    // Impact Spark VFX
    public const float SPARK_SIZE_MIN = 0.03f;
    public const float SPARK_SIZE_MAX = 0.08f;
    public const float SPARK_VELOCITY_STRETCH = 0.25f;
    public const float SPARK_GRAVITY = 1.5f;

    // VFX particle sizes
    public const float POP_PARTICLE_BASE_SIZE = 0.1f;
    public const float POP_PARTICLE_SIZE_VARIATION = 0.04f;
    public const float PAINT_PARTICLE_BASE_SIZE = 0.18f;
    public const float PAINT_PARTICLE_SIZE_STRETCH_X = 0.12f;
    public const float PAINT_PARTICLE_SIZE_STRETCH_Y = 0.28f;
    public const float PAINT_PARTICLE_SIZE_STRETCH_Z = 0.12f;
    public const float PAINT_SIZE_OVER_LIFETIME_END = 0.3f;
    public const float PAINT_EXTRA_GRAVITY = 0.4f;
    public const float DEFAULT_PARTICLE_SIZE = 0.08f;
}
