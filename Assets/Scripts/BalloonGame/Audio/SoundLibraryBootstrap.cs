using UnityEngine;

/// <summary>
/// Populates the sound library with procedural clips if assets are missing.
/// </summary>
public static class SoundLibraryBootstrap
{
    public static void EnsurePopulated(SoundLibrarySO library)
    {
        if (library == null)
        {
            return;
        }

        library.balloonPop ??= SoundGenerator.Mix("BalloonPop", SoundGenerator.Chirp(540f, 220f, 0.08f, 0.3f), SoundGenerator.Noise(0.06f, 0.1f));
        library.paintBurst ??= SoundGenerator.Mix("PaintBurst", SoundGenerator.Noise(0.18f, 0.18f), SoundGenerator.Chirp(220f, 90f, 0.16f, 0.2f));
        library.wallHit ??= SoundGenerator.Mix("WallHit", SoundGenerator.Noise(0.08f, 0.14f), SoundGenerator.Sine(180f, 0.05f, 0.08f));
        library.comboRise ??= SoundGenerator.Chirp(320f, 820f, 0.14f, 0.24f, "ComboRise");
        library.launch ??= SoundGenerator.Chirp(260f, 560f, 0.12f, 0.16f, "Launch");
        library.roomCleared ??= SoundGenerator.Mix("RoomCleared", SoundGenerator.Chirp(420f, 840f, 0.35f, 0.2f), SoundGenerator.Sine(280f, 0.3f, 0.1f));
        library.roomFailed ??= SoundGenerator.Chirp(260f, 120f, 0.3f, 0.18f, "RoomFailed");
    }
}
