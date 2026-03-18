public static partial class GameConstants
{
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

    public const float DART_PEAK_VELOCITY_THRESHOLD = 0.5f;
    public const float DART_ARC_SCALE_MIN = 1.0f;
    public const float DART_ARC_SCALE_MAX = 1.3f;
    public const float MIN_LOB_HEIGHT = 1.5f;
    public const float AIM_RANGE_X = 4.5f;
    public const float AIM_RANGE_Y = 4.5f;
    public const float AIM_ASSIST_SNAP_RADIUS = 1.5f;
    public const float AIM_VERTICAL_SHELF = 4f;
    public const float AIM_CLAMP_MARGIN = 0.5f;

    public const float ROOM_BACK_Z = 6f;
    public const float ROOM_FRONT_Z = -2f;
    public const float ROOM_HALF_WIDTH = 7f;
    public const float ROOM_FLOOR_Y = -1.5f;
    public const float ROOM_CEILING_Y = 10.5f;
    public const float ROOM_DEPTH = ROOM_BACK_Z - ROOM_FRONT_Z;

    public const float DECAL_Z_OFFSET = -0.05f;
    public const float DEFAULT_SCREEN_FADE_DURATION = 0.3f;
    public const int MAX_PAINT_DRIPS = 6;
    public const int MAX_PERSISTENT_SPLATTERS = 50;
    public const int SPLATTER_POOL_SIZE = 55;
    public const float SPLATTER_Z_OFFSET = -0.03f;
    public const float SPLATTER_MIN_SCALE = 0.35f;
    public const float SPLATTER_MAX_SCALE = 0.75f;
    public const float SPLATTER_NEON_BOOST = 0.55f;
    public const float SPLATTER_BASE_ALPHA = 0.92f;
    public const float SPLATTER_FADE_DURATION = 1.2f;

    public const float HAPTIC_PULL_THRESHOLD = 0.15f;
    public const float HAPTIC_TICK_INTERVAL_MIN_PULL = 0.30f;
    public const float HAPTIC_TICK_INTERVAL_MAX_PULL = 0.07f;
    public const float HAPTIC_MEDIUM_THRESHOLD = 0.45f;
    public const float HAPTIC_HEAVY_THRESHOLD = 0.80f;

    public const float COMBO_FLASH_EDGE_THICKNESS = 0.18f;
    public const float COMBO_FLASH_ALPHA_PER_COMBO = 0.06f;
    public const float COMBO_FLASH_ALPHA_CAP = 0.55f;
    public const int COMBO_FLASH_EDGE_COUNT = 4;
    public const float SPARK_SIZE_MIN = 0.03f;
    public const float SPARK_SIZE_MAX = 0.08f;
    public const float SPARK_VELOCITY_STRETCH = 0.25f;
    public const float SPARK_GRAVITY = 1.5f;
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
