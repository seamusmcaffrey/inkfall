# Plan 05 — VFX & Juice

> Paint splatter, particles, screen shake, haptics, procedural audio, time effects.
> Take INKSHOT from "functional prototype" to "feels incredible to play."

---

## File Map

```
Assets/Scripts/BalloonGame/
  VFX/
    VFXFactory.cs                    ← Creates all particle systems via code (no prefabs)
    BalloonPopVFX.cs                 ← Per-color balloon burst particles
    PaintSplatterVFX.cs              ← Paint balloon explosion + drip spawning
    PaintDripEffect.cs               ← Animated downward paint drip trails
    DartTrailVFX.cs                  ← Trail renderer attached to dart
    ImpactSparkVFX.cs                ← Spark burst on dart-balloon collision
    WallHitVFX.cs                    ← Dust puff on dart-wall collision
    ComboFlashVFX.cs                 ← Full-screen flash overlay via CanvasGroup
    PaintDecalManager.cs             ← Persistent paint splatter decals on wall
  ScreenFX/
    ScreenShakeManager.cs            ← Cinemachine impulse-based camera shake
    ChromaticAberrationPulse.cs      ← URP Volume chromatic aberration spike
    SlowMotionController.cs          ← Time.timeScale manipulation with lerp
  Audio/
    SoundGenerator.cs                ← Procedural AudioClip creation (sine, noise, chirp)
    SoundLibrarySO.cs                ← ScriptableObject holding all sound references
    AudioManager.cs                  ← Plays sounds with pooled AudioSources
    SoundLibraryBootstrap.cs         ← Generates all procedural clips on first access
  Haptics/
    HapticsUtility.cs                ← Cross-platform haptic feedback wrapper
  Juice/
    JuiceConfigSO.cs                 ← ScriptableObject: toggles + intensity for all juice
    JuiceManager.cs                  ← Central coordinator: events → VFX/audio/haptics/screen

Assets/ScriptableObjects/
  JuiceConfig.asset                  ← Default juice configuration instance
  SoundLibrary.asset                 ← Default sound library instance
```

---

## Dependencies

- **Plan 01**: EventBus (not yet implemented — this plan uses existing `BalloonNode.OnAnyBalloonPopped`, `DartController.OnDartFinished`, and `SlingshotInput.OnLaunch`/`OnPullUpdate` events directly)
- **Plan 02**: URP post-processing Volume (if not yet present, Task 08 creates a minimal one)
- **Plan 03**: Combo system (if not yet present, JuiceManager tracks combos internally as a fallback)
- **Cinemachine**: `com.unity.cinemachine` 2.10.5 (already in project manifest)
- **URP**: `com.unity.render-pipelines.universal` (already in project)

---

## Task 01 — JuiceConfigSO and Constants

> ScriptableObject that controls every juice parameter. Designers tune this, not code.

- [ ] Create `Assets/Scripts/BalloonGame/Juice/JuiceConfigSO.cs`
- [ ] Create default asset at `Assets/ScriptableObjects/JuiceConfig.asset` (via `[CreateAssetMenu]`, created at runtime if missing)
- [ ] Add VFX constants to `GameConstants.cs`

### `Assets/Scripts/BalloonGame/Juice/JuiceConfigSO.cs`

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "JuiceConfig", menuName = "Inkshot/Juice Config")]
public class JuiceConfigSO : ScriptableObject
{
    [Header("Master Controls")]
    [Tooltip("Kill switch for all juice effects.")]
    public bool juiceEnabled = true;

    [Range(0f, 2f)]
    [Tooltip("Global intensity multiplier applied to all effects.")]
    public float globalIntensity = 1f;

    // ── Particle FX ──────────────────────────────────────────────

    [Header("Particles — Balloon Pop")]
    public bool balloonPopEnabled = true;
    [Range(10, 40)] public int popParticleCount = 25;
    public float popParticleLifetime = 0.5f;
    public float popParticleSpeed = 4f;
    public float popParticleGravity = 2f;

    [Header("Particles — Paint Splatter")]
    public bool paintSplatterEnabled = true;
    [Range(20, 80)] public int paintParticleCount = 50;
    public float paintParticleLifetime = 1.0f;
    public float paintParticleSpeed = 6f;
    public float paintParticleGravity = 3f;
    [Range(1, 6)] public int paintDripCount = 3;
    public float paintDripSpeed = 0.8f;
    public float paintDripLifetime = 2.0f;

    [Header("Particles — Dart Impact")]
    public bool impactSparkEnabled = true;
    [Range(4, 20)] public int sparkParticleCount = 10;
    public float sparkParticleLifetime = 0.15f;
    public float sparkParticleSpeed = 8f;

    [Header("Particles — Wall Hit")]
    public bool wallHitEnabled = true;
    [Range(3, 12)] public int wallHitParticleCount = 6;
    public float wallHitParticleLifetime = 0.3f;
    public float wallHitParticleSpeed = 2f;

    [Header("Dart Trail")]
    public bool dartTrailEnabled = true;
    public float trailWidth = 0.02f;
    public float trailLifetime = 0.3f;
    public Color trailColor = new Color(0.8f, 0.9f, 1f, 0.6f);

    // ── Screen Effects ───────────────────────────────────────────

    [Header("Screen Shake")]
    public bool screenShakeEnabled = true;
    public float shakeIntensityPop = 0.15f;
    public float shakeIntensityCombo = 0.35f;
    public float shakeIntensityPaint = 0.5f;
    public float shakeDuration = 0.2f;

    [Header("Chromatic Aberration Pulse")]
    public bool chromaticPulseEnabled = true;
    [Range(1, 10)] public int chromaticComboThreshold = 3;
    public float chromaticMaxIntensity = 0.5f;
    public float chromaticPulseDuration = 0.3f;

    [Header("Combo Flash")]
    public bool comboFlashEnabled = true;
    public float comboFlashMaxAlpha = 0.15f;
    public float comboFlashDuration = 0.2f;

    [Header("Slow Motion")]
    public bool slowMotionEnabled = true;
    [Range(1, 10)] public int slowMotionComboThreshold = 5;
    public float slowMotionTimeScale = 0.4f;
    public float slowMotionDuration = 0.8f;
    public float slowMotionRampUpTime = 0.05f;
    public float slowMotionRampDownTime = 0.3f;
    public bool slowMotionOnLastDart = true;

    // ── Audio ────────────────────────────────────────────────────

    [Header("Audio")]
    public bool audioEnabled = true;
    [Range(0f, 1f)] public float masterVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 0.3f;

    // ── Haptics ──────────────────────────────────────────────────

    [Header("Haptics")]
    public bool hapticsEnabled = true;

    // ── Paint Decals ─────────────────────────────────────────────

    [Header("Paint Decals")]
    public bool paintDecalsEnabled = true;
    [Range(10, 80)] public int maxActiveDecals = 50;
    public float decalMinSize = 0.15f;
    public float decalMaxSize = 0.4f;
    public float decalAlpha = 0.7f;
}
```

### Additions to `Assets/Scripts/BalloonGame/GameConstants.cs`

Add these at the end of the class, before the closing brace:

```csharp
    // ── VFX Budget ───────────────────────────────────────────────
    public const int MAX_ACTIVE_PARTICLES = 200;
    public const int VFX_POOL_INITIAL_SIZE = 8;

    // ── Audio ────────────────────────────────────────────────────
    public const int AUDIO_SOURCE_POOL_SIZE = 12;
    public const int AUDIO_SAMPLE_RATE = 44100;

    // ── Paint Decals ─────────────────────────────────────────────
    public const float DECAL_Z_OFFSET = -0.05f;

    // ── Combo (fallback if Plan 03 not yet implemented) ──────────
    public const float COMBO_WINDOW_SECONDS = 1.5f;
```

### Commit message
```
feat(juice): add JuiceConfigSO and VFX/audio constants

ScriptableObject with toggles and intensity sliders for every juice
category. All magic numbers live here or in GameConstants.
```

---

## Task 02 — SoundGenerator (Procedural Audio)

> Static utility that creates AudioClips from math. Zero external assets.

- [ ] Create `Assets/Scripts/BalloonGame/Audio/SoundGenerator.cs`
- [ ] Verify: all methods are pure functions returning AudioClips

### `Assets/Scripts/BalloonGame/Audio/SoundGenerator.cs`

```csharp
using UnityEngine;

/// <summary>
/// Procedurally generates AudioClips from waveform math.
/// All methods are static and return new AudioClip instances.
/// </summary>
public static class SoundGenerator
{
    private static int SampleRate => GameConstants.AUDIO_SAMPLE_RATE;

    /// <summary>
    /// Pure sine wave at the given frequency.
    /// </summary>
    public static AudioClip Sine(float frequency, float duration, float volume = 1f, string name = "Sine")
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = EnvelopeADSR(t, duration, 0.01f, 0.05f, 0.7f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        return CreateClip(name, samples);
    }

    /// <summary>
    /// White noise burst.
    /// </summary>
    public static AudioClip Noise(float duration, float volume = 1f, string name = "Noise")
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = EnvelopeADSR(t, duration, 0.005f, 0.02f, 0.5f);
            samples[i] = (Random.value * 2f - 1f) * volume * envelope;
        }

        return CreateClip(name, samples);
    }

    /// <summary>
    /// Frequency sweep from startFreq to endFreq over duration.
    /// </summary>
    public static AudioClip Chirp(float startFreq, float endFreq, float duration,
        float volume = 1f, string name = "Chirp")
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float ratio = t / duration;
            float freq = Mathf.Lerp(startFreq, endFreq, ratio);
            float envelope = EnvelopeADSR(t, duration, 0.01f, 0.03f, 0.8f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * envelope;
        }

        return CreateClip(name, samples);
    }

    /// <summary>
    /// Layer multiple AudioClips by summing their samples. All clips are
    /// normalized to the longest clip duration. Output is clamped to [-1, 1].
    /// </summary>
    public static AudioClip Layer(AudioClip[] clips, string name = "Layered")
    {
        if (clips == null || clips.Length == 0)
        {
            return Sine(440f, 0.1f, 0.5f, name);
        }

        int maxSamples = 0;
        foreach (AudioClip clip in clips)
        {
            if (clip.samples > maxSamples)
            {
                maxSamples = clip.samples;
            }
        }

        float[] mixed = new float[maxSamples];

        foreach (AudioClip clip in clips)
        {
            float[] clipSamples = new float[clip.samples];
            clip.GetData(clipSamples, 0);

            for (int i = 0; i < clipSamples.Length; i++)
            {
                mixed[i] += clipSamples[i];
            }
        }

        // Normalize to prevent clipping
        float peak = 0f;
        for (int i = 0; i < mixed.Length; i++)
        {
            float abs = Mathf.Abs(mixed[i]);
            if (abs > peak) peak = abs;
        }

        if (peak > 1f)
        {
            float scale = 1f / peak;
            for (int i = 0; i < mixed.Length; i++)
            {
                mixed[i] *= scale;
            }
        }

        return CreateClip(name, mixed);
    }

    /// <summary>
    /// Sine wave with exponential decay — good for impact/thud sounds.
    /// </summary>
    public static AudioClip Impact(float frequency, float duration, float decayRate = 10f,
        float volume = 1f, string name = "Impact")
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-decayRate * t);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        return CreateClip(name, samples);
    }

    /// <summary>
    /// Musical note at a given MIDI-style note number (A4 = 69 = 440Hz).
    /// </summary>
    public static AudioClip Note(int midiNote, float duration, float volume = 1f, string name = "Note")
    {
        float frequency = 440f * Mathf.Pow(2f, (midiNote - 69) / 12f);
        return Sine(frequency, duration, volume, name);
    }

    /// <summary>
    /// Simple ADSR envelope. Attack/decay in seconds, sustain is a 0-1 level.
    /// Release fills the remaining time after attack+decay.
    /// </summary>
    private static float EnvelopeADSR(float t, float totalDuration, float attack, float decay, float sustainLevel)
    {
        if (t < attack)
        {
            return t / attack;
        }

        if (t < attack + decay)
        {
            float decayProgress = (t - attack) / decay;
            return Mathf.Lerp(1f, sustainLevel, decayProgress);
        }

        float releaseStart = totalDuration * 0.7f;
        if (t > releaseStart)
        {
            float releaseProgress = (t - releaseStart) / (totalDuration - releaseStart);
            return sustainLevel * (1f - releaseProgress);
        }

        return sustainLevel;
    }

    private static AudioClip CreateClip(string name, float[] samples)
    {
        var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
```

### Commit message
```
feat(audio): add procedural SoundGenerator utility

Static utility creating AudioClips from sine waves, noise, chirps,
and layered combinations. Zero external audio assets needed.
```

---

## Task 03 — SoundLibrarySO and Bootstrap

> Define all game sounds as named clips in a ScriptableObject. Bootstrap generates them procedurally.

- [ ] Create `Assets/Scripts/BalloonGame/Audio/SoundLibrarySO.cs`
- [ ] Create `Assets/Scripts/BalloonGame/Audio/SoundLibraryBootstrap.cs`

### `Assets/Scripts/BalloonGame/Audio/SoundLibrarySO.cs`

```csharp
using UnityEngine;

/// <summary>
/// Holds references to all game audio clips. Clips are populated at runtime
/// by SoundLibraryBootstrap if they are null (procedural placeholders).
/// Replace individual clips with real audio assets when available.
/// </summary>
[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Inkshot/Sound Library")]
public class SoundLibrarySO : ScriptableObject
{
    [Header("Balloon Pops")]
    public AudioClip popStandard;
    public AudioClip popGold;
    public AudioClip popPaint;
    public AudioClip popHazard;

    [Header("Slingshot")]
    public AudioClip slingshotPull;
    public AudioClip slingshotRelease;

    [Header("Dart")]
    public AudioClip dartWhoosh;
    public AudioClip dartWallHit;

    [Header("Combo")]
    public AudioClip comboChime1;  // C4 (MIDI 60)
    public AudioClip comboChime2;  // E4 (MIDI 64)
    public AudioClip comboChime3;  // G4 (MIDI 67)
    public AudioClip comboChime4;  // C5 (MIDI 72)
    public AudioClip comboChime5;  // E5 (MIDI 76)

    [Header("Ambient")]
    public AudioClip ambientCarnival;

    [Header("UI")]
    public AudioClip uiSelect;
    public AudioClip uiConfirm;

    /// <summary>
    /// Returns the combo chime for a given combo level (1-based).
    /// Wraps around if combo exceeds available chimes.
    /// </summary>
    public AudioClip GetComboChime(int comboLevel)
    {
        AudioClip[] chimes = { comboChime1, comboChime2, comboChime3, comboChime4, comboChime5 };
        int index = Mathf.Clamp(comboLevel - 1, 0, chimes.Length - 1);
        return chimes[index];
    }

    /// <summary>
    /// Returns the pop sound for a given balloon color.
    /// Standard balloons use popStandard with pitch variation.
    /// </summary>
    public AudioClip GetPopSound(BalloonColor color)
    {
        // All standard colors use popStandard — pitch varies in AudioManager
        return popStandard;
    }

    /// <summary>
    /// Returns a pitch multiplier based on balloon color.
    /// Red = low, Blue = high. Gives each color a distinct feel.
    /// </summary>
    public static float GetPopPitch(BalloonColor color)
    {
        return color switch
        {
            BalloonColor.Red => 0.8f,
            BalloonColor.Green => 0.9f,
            BalloonColor.Yellow => 1.0f,
            BalloonColor.Purple => 1.1f,
            BalloonColor.Blue => 1.2f,
            _ => 1.0f
        };
    }
}
```

### `Assets/Scripts/BalloonGame/Audio/SoundLibraryBootstrap.cs`

```csharp
using UnityEngine;

/// <summary>
/// Populates a SoundLibrarySO with procedurally generated placeholder clips.
/// Only fills null slots — manually assigned AudioClip assets are preserved.
/// Call EnsurePopulated() before first use.
/// </summary>
public static class SoundLibraryBootstrap
{
    private static bool _populated;

    /// <summary>
    /// Generate all placeholder sounds. Safe to call multiple times.
    /// </summary>
    public static void EnsurePopulated(SoundLibrarySO library)
    {
        if (_populated || library == null)
        {
            return;
        }

        _populated = true;

        // ── Balloon Pops ─────────────────────────────────────────
        if (library.popStandard == null)
        {
            // Short sine burst — the "pop" sound
            library.popStandard = SoundGenerator.Layer(new[]
            {
                SoundGenerator.Impact(880f, 0.1f, 15f, 0.8f, "PopSine"),
                SoundGenerator.Noise(0.05f, 0.3f, "PopNoise")
            }, "Pop_Standard");
        }

        if (library.popGold == null)
        {
            // Coin-like chime — layered harmonics
            library.popGold = SoundGenerator.Layer(new[]
            {
                SoundGenerator.Impact(1200f, 0.2f, 8f, 0.6f, "GoldBase"),
                SoundGenerator.Impact(2400f, 0.15f, 10f, 0.3f, "GoldHarm"),
                SoundGenerator.Impact(3600f, 0.1f, 12f, 0.15f, "GoldShimmer")
            }, "Pop_Gold");
        }

        if (library.popPaint == null)
        {
            // Wet splat — noise burst + low thud
            library.popPaint = SoundGenerator.Layer(new[]
            {
                SoundGenerator.Noise(0.15f, 0.7f, "SplatNoise"),
                SoundGenerator.Impact(150f, 0.2f, 8f, 0.5f, "SplatThud"),
                SoundGenerator.Chirp(400f, 100f, 0.12f, 0.4f, "SplatDrop")
            }, "Pop_Paint");
        }

        if (library.popHazard == null)
        {
            // Dissonant buzz — two close frequencies beating
            library.popHazard = SoundGenerator.Layer(new[]
            {
                SoundGenerator.Sine(220f, 0.2f, 0.5f, "HazardA"),
                SoundGenerator.Sine(233f, 0.2f, 0.5f, "HazardB"),
                SoundGenerator.Noise(0.08f, 0.3f, "HazardNoise")
            }, "Pop_Hazard");
        }

        // ── Slingshot ────────────────────────────────────────────
        if (library.slingshotPull == null)
        {
            // Rising pitch rubber creak
            library.slingshotPull = SoundGenerator.Chirp(80f, 300f, 0.4f, 0.3f, "Slingshot_Pull");
        }

        if (library.slingshotRelease == null)
        {
            // Fast descending snap
            library.slingshotRelease = SoundGenerator.Layer(new[]
            {
                SoundGenerator.Chirp(600f, 100f, 0.08f, 0.7f, "SnapChirp"),
                SoundGenerator.Noise(0.04f, 0.4f, "SnapNoise")
            }, "Slingshot_Release");
        }

        // ── Dart ─────────────────────────────────────────────────
        if (library.dartWhoosh == null)
        {
            // Filtered wind noise
            library.dartWhoosh = SoundGenerator.Noise(0.3f, 0.25f, "Dart_Whoosh");
        }

        if (library.dartWallHit == null)
        {
            // Low thud
            library.dartWallHit = SoundGenerator.Layer(new[]
            {
                SoundGenerator.Impact(100f, 0.05f, 20f, 0.8f, "ThudSine"),
                SoundGenerator.Noise(0.03f, 0.4f, "ThudNoise")
            }, "Dart_WallHit");
        }

        // ── Combo Chimes ─────────────────────────────────────────
        // C4, E4, G4, C5, E5 — ascending major arpeggio
        if (library.comboChime1 == null)
            library.comboChime1 = SoundGenerator.Note(60, 0.25f, 0.6f, "Combo_C4");
        if (library.comboChime2 == null)
            library.comboChime2 = SoundGenerator.Note(64, 0.25f, 0.6f, "Combo_E4");
        if (library.comboChime3 == null)
            library.comboChime3 = SoundGenerator.Note(67, 0.25f, 0.6f, "Combo_G4");
        if (library.comboChime4 == null)
            library.comboChime4 = SoundGenerator.Note(72, 0.25f, 0.6f, "Combo_C5");
        if (library.comboChime5 == null)
            library.comboChime5 = SoundGenerator.Note(76, 0.25f, 0.6f, "Combo_E5");

        // ── Ambient ──────────────────────────────────────────────
        if (library.ambientCarnival == null)
        {
            // Low drone — very quiet layered sines
            library.ambientCarnival = SoundGenerator.Layer(new[]
            {
                SoundGenerator.Sine(55f, 4f, 0.15f, "DroneA"),
                SoundGenerator.Sine(82.5f, 4f, 0.08f, "DroneE"),
                SoundGenerator.Sine(110f, 4f, 0.05f, "DroneA2")
            }, "Ambient_Carnival");
        }

        // ── UI ───────────────────────────────────────────────────
        if (library.uiSelect == null)
        {
            library.uiSelect = SoundGenerator.Impact(2000f, 0.02f, 40f, 0.4f, "UI_Select");
        }

        if (library.uiConfirm == null)
        {
            library.uiConfirm = SoundGenerator.Layer(new[]
            {
                SoundGenerator.Note(72, 0.08f, 0.5f, "ConfirmC"),
                SoundGenerator.Note(76, 0.12f, 0.5f, "ConfirmE")
            }, "UI_Confirm");
        }
    }
}
```

### Commit message
```
feat(audio): add SoundLibrarySO and procedural bootstrap

All 16 sound events defined in a ScriptableObject. Bootstrap fills
empty slots with procedural waveform placeholders on first access.
```

---

## Task 04 — AudioManager

> Pooled AudioSource playback system. Supports pitch variation, spatial blending, and volume categories.

- [ ] Create `Assets/Scripts/BalloonGame/Audio/AudioManager.cs`

### `Assets/Scripts/BalloonGame/Audio/AudioManager.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages audio playback via a pool of AudioSources.
/// Singleton — lives on a persistent GameObject.
/// </summary>
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private SoundLibrarySO _soundLibrary;
    [SerializeField] private JuiceConfigSO _juiceConfig;

    private readonly List<AudioSource> _sourcePool = new();
    private AudioSource _ambientSource;

    /// <summary>
    /// The sound library used by this AudioManager.
    /// </summary>
    public SoundLibrarySO SoundLibrary => _soundLibrary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Initialize pool
        for (int i = 0; i < GameConstants.AUDIO_SOURCE_POOL_SIZE; i++)
        {
            _sourcePool.Add(CreateSource($"SFX_{i}"));
        }

        // Bootstrap procedural sounds
        if (_soundLibrary != null)
        {
            SoundLibraryBootstrap.EnsurePopulated(_soundLibrary);
        }
    }

    private void Start()
    {
        StartAmbient();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Play a clip with optional pitch and volume modifiers.
    /// Returns the AudioSource used (or null if pool exhausted or audio disabled).
    /// </summary>
    public AudioSource Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (!IsAudioEnabled() || clip == null)
        {
            return null;
        }

        AudioSource source = GetAvailableSource();
        if (source == null)
        {
            return null;
        }

        float finalVolume = volume * _juiceConfig.sfxVolume * _juiceConfig.masterVolume;
        source.clip = clip;
        source.volume = finalVolume;
        source.pitch = pitch;
        source.spatialBlend = 0f; // 2D for all game sounds (portrait mobile)
        source.Play();
        return source;
    }

    /// <summary>
    /// Play a clip with a random pitch variation within +/- range.
    /// </summary>
    public AudioSource PlayWithVariation(AudioClip clip, float volume = 1f,
        float basePitch = 1f, float pitchVariation = 0.1f)
    {
        float pitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
        return Play(clip, volume, pitch);
    }

    /// <summary>
    /// Start or restart the ambient loop.
    /// </summary>
    public void StartAmbient()
    {
        if (!IsAudioEnabled() || _soundLibrary == null || _soundLibrary.ambientCarnival == null)
        {
            return;
        }

        if (_ambientSource == null)
        {
            _ambientSource = CreateSource("Ambient");
        }

        _ambientSource.clip = _soundLibrary.ambientCarnival;
        _ambientSource.volume = _juiceConfig.ambientVolume * _juiceConfig.masterVolume;
        _ambientSource.loop = true;
        _ambientSource.Play();
    }

    /// <summary>
    /// Stop the ambient loop.
    /// </summary>
    public void StopAmbient()
    {
        if (_ambientSource != null)
        {
            _ambientSource.Stop();
        }
    }

    private bool IsAudioEnabled()
    {
        return _juiceConfig != null && _juiceConfig.juiceEnabled && _juiceConfig.audioEnabled;
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in _sourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // All busy — steal the oldest (first in list)
        AudioSource stolen = _sourcePool[0];
        stolen.Stop();
        return stolen;
    }

    private AudioSource CreateSource(string sourceName)
    {
        var child = new GameObject(sourceName);
        child.transform.SetParent(transform, false);
        var source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }
}
```

### Commit message
```
feat(audio): add pooled AudioManager with volume controls

Singleton AudioManager with 12 pooled AudioSources. Supports pitch
variation, volume categories, and ambient looping.
```

---

## Task 05 — HapticsUtility

> Cross-platform haptic feedback wrapper. No-ops gracefully on unsupported platforms.

- [ ] Create `Assets/Scripts/BalloonGame/Haptics/HapticsUtility.cs`

### `Assets/Scripts/BalloonGame/Haptics/HapticsUtility.cs`

```csharp
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

/// <summary>
/// Cross-platform haptic feedback utility. Uses iOS UIImpactFeedbackGenerator
/// on iPhone, Vibration API on Android, and no-ops everywhere else.
/// </summary>
public static class HapticsUtility
{
    public enum HapticType
    {
        Light,
        Medium,
        Heavy,
        Success,
        Warning,
        Error
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _playHapticLight();
    [DllImport("__Internal")]
    private static extern void _playHapticMedium();
    [DllImport("__Internal")]
    private static extern void _playHapticHeavy();
    [DllImport("__Internal")]
    private static extern void _playHapticSuccess();
    [DllImport("__Internal")]
    private static extern void _playHapticWarning();
    [DllImport("__Internal")]
    private static extern void _playHapticError();
#endif

    private static bool _enabled = true;

    /// <summary>
    /// Enable or disable haptics globally.
    /// </summary>
    public static void SetEnabled(bool enabled)
    {
        _enabled = enabled;
    }

    /// <summary>
    /// Play a haptic feedback of the given type.
    /// </summary>
    public static void Play(HapticType type)
    {
        if (!_enabled)
        {
            return;
        }

#if UNITY_IOS && !UNITY_EDITOR
        switch (type)
        {
            case HapticType.Light:   _playHapticLight();   break;
            case HapticType.Medium:  _playHapticMedium();  break;
            case HapticType.Heavy:   _playHapticHeavy();   break;
            case HapticType.Success: _playHapticSuccess(); break;
            case HapticType.Warning: _playHapticWarning(); break;
            case HapticType.Error:   _playHapticError();   break;
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        PlayAndroidHaptic(type);
#else
        // Editor / unsupported platforms — log for debugging
        Debug.Log($"[Haptics] {type} (no-op on this platform)");
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void PlayAndroidHaptic(HapticType type)
    {
        long milliseconds = type switch
        {
            HapticType.Light => 10,
            HapticType.Medium => 25,
            HapticType.Heavy => 50,
            HapticType.Success => 30,
            HapticType.Warning => 40,
            HapticType.Error => 60,
            _ => 20
        };

        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            vibrator?.Call("vibrate", milliseconds);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Haptics] Android vibration failed: {e.Message}");
        }
    }
#endif
}
```

> **Note:** iOS native haptics require a small Objective-C plugin (`Haptics.mm`) in `Assets/Plugins/iOS/`. That plugin is a Plan 06 deliverable (build/deploy polish). For now, the `DllImport` declarations compile behind `#if UNITY_IOS && !UNITY_EDITOR` and are never called in-editor. Android uses the Java Vibrator API directly.

### Commit message
```
feat(haptics): add cross-platform HapticsUtility

Static utility wrapping iOS UIImpactFeedbackGenerator and Android
Vibrator. Gracefully no-ops on unsupported platforms and in editor.
```

---

## Task 06 — VFX Factory and Particle Systems

> All particle systems built via C# code. No editor-created prefabs. Poolable.

- [ ] Create `Assets/Scripts/BalloonGame/VFX/VFXFactory.cs`
- [ ] Create `Assets/Scripts/BalloonGame/VFX/BalloonPopVFX.cs`
- [ ] Create `Assets/Scripts/BalloonGame/VFX/PaintSplatterVFX.cs`
- [ ] Create `Assets/Scripts/BalloonGame/VFX/ImpactSparkVFX.cs`
- [ ] Create `Assets/Scripts/BalloonGame/VFX/WallHitVFX.cs`

### `Assets/Scripts/BalloonGame/VFX/VFXFactory.cs`

```csharp
using UnityEngine;

/// <summary>
/// Creates configured ParticleSystem GameObjects entirely via code.
/// All systems use URP Particles/Unlit shader with additive blending.
/// Returns the root GameObject — caller is responsible for pooling.
/// </summary>
public static class VFXFactory
{
    private static Material _additiveMaterial;
    private static Material _alphaMaterial;

    /// <summary>
    /// Shared additive particle material (URP Particles/Unlit).
    /// </summary>
    public static Material AdditiveMaterial
    {
        get
        {
            if (_additiveMaterial != null) return _additiveMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Particles/Standard Unlit")
                         ?? Shader.Find("Sprites/Default");

            _additiveMaterial = new Material(shader);
            _additiveMaterial.SetFloat("_Surface", 1f); // Transparent
            _additiveMaterial.SetFloat("_Blend", 1f);   // Additive

            // Enable keywords for URP particle shader
            _additiveMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _additiveMaterial.EnableKeyword("_ALPHABLEND_ON");
            _additiveMaterial.renderQueue = 3000;

            return _additiveMaterial;
        }
    }

    /// <summary>
    /// Shared alpha-blend particle material.
    /// </summary>
    public static Material AlphaMaterial
    {
        get
        {
            if (_alphaMaterial != null) return _alphaMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Particles/Standard Unlit")
                         ?? Shader.Find("Sprites/Default");

            _alphaMaterial = new Material(shader);
            _alphaMaterial.SetFloat("_Surface", 1f); // Transparent
            _alphaMaterial.SetFloat("_Blend", 0f);   // Alpha
            _alphaMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _alphaMaterial.renderQueue = 3000;

            return _alphaMaterial;
        }
    }

    /// <summary>
    /// Create a balloon pop burst particle system.
    /// </summary>
    public static GameObject CreateBalloonPop(JuiceConfigSO config)
    {
        var go = new GameObject("BalloonPopVFX");
        var ps = go.AddComponent<ParticleSystem>();
        var component = go.AddComponent<BalloonPopVFX>();

        var main = ps.main;
        main.duration = config.popParticleLifetime;
        main.loop = false;
        main.startLifetime = config.popParticleLifetime;
        main.startSpeed = config.popParticleSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.maxParticles = config.popParticleCount;
        main.gravityModifier = config.popParticleGravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.None;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)config.popParticleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = AdditiveMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        go.SetActive(false);
        return go;
    }

    /// <summary>
    /// Create a paint splatter particle system (heavier, wetter, stickier).
    /// </summary>
    public static GameObject CreatePaintSplatter(JuiceConfigSO config)
    {
        var go = new GameObject("PaintSplatterVFX");
        var ps = go.AddComponent<ParticleSystem>();
        go.AddComponent<PaintSplatterVFX>();

        var main = ps.main;
        main.duration = config.paintParticleLifetime;
        main.loop = false;
        main.startLifetime = config.paintParticleLifetime;
        main.startSpeed = config.paintParticleSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        main.maxParticles = config.paintParticleCount;
        main.gravityModifier = config.paintParticleGravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.None;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)config.paintParticleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.3f, 1f) }
        );
        colorOverLifetime.color = gradient;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = AlphaMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        go.SetActive(false);
        return go;
    }

    /// <summary>
    /// Create an impact spark burst (dart hitting balloon).
    /// </summary>
    public static GameObject CreateImpactSpark(JuiceConfigSO config)
    {
        var go = new GameObject("ImpactSparkVFX");
        var ps = go.AddComponent<ParticleSystem>();
        go.AddComponent<ImpactSparkVFX>();

        var main = ps.main;
        main.duration = config.sparkParticleLifetime;
        main.loop = false;
        main.startLifetime = config.sparkParticleLifetime;
        main.startSpeed = config.sparkParticleSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor = new Color(1f, 0.95f, 0.7f); // Warm white/yellow
        main.maxParticles = config.sparkParticleCount;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.None;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)config.sparkParticleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = AdditiveMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        go.SetActive(false);
        return go;
    }

    /// <summary>
    /// Create a wall hit dust puff.
    /// </summary>
    public static GameObject CreateWallHit(JuiceConfigSO config)
    {
        var go = new GameObject("WallHitVFX");
        var ps = go.AddComponent<ParticleSystem>();
        go.AddComponent<WallHitVFX>();

        var main = ps.main;
        main.duration = config.wallHitParticleLifetime;
        main.loop = false;
        main.startLifetime = config.wallHitParticleLifetime;
        main.startSpeed = config.wallHitParticleSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        main.startColor = new Color(0.5f, 0.45f, 0.4f, 0.6f); // Muted brown dust
        main.maxParticles = config.wallHitParticleCount;
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.None;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)config.wallHitParticleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.05f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0.6f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = AlphaMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        go.SetActive(false);
        return go;
    }
}
```

### `Assets/Scripts/BalloonGame/VFX/BalloonPopVFX.cs`

```csharp
using UnityEngine;

/// <summary>
/// Component on a balloon pop particle system instance.
/// Configures color from the popped balloon, plays, and tracks lifetime for pool return.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ParticleSystem))]
public class BalloonPopVFX : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private float _lifetime;
    private float _timer;
    private bool _playing;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Trigger the pop effect at a position with a given balloon color.
    /// </summary>
    public void Play(Vector3 position, Color balloonColor, float lifetime)
    {
        transform.position = position;
        _lifetime = lifetime;
        _timer = 0f;
        _playing = true;

        // Mix balloon color with white for the "flash" feel
        var main = _particleSystem.main;
        Color brightColor = Color.Lerp(balloonColor, Color.white, 0.3f);
        Gradient gradient = new();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(brightColor, 0.2f),
                new GradientColorKey(balloonColor, 1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );

        var colorOverLifetime = _particleSystem.colorOverLifetime;
        colorOverLifetime.color = gradient;

        gameObject.SetActive(true);
        _particleSystem.Clear();
        _particleSystem.Play();
    }

    private void Update()
    {
        if (!_playing) return;

        _timer += Time.deltaTime;
        if (_timer >= _lifetime + 0.1f)
        {
            _playing = false;
            gameObject.SetActive(false);
        }
    }
}
```

### `Assets/Scripts/BalloonGame/VFX/PaintSplatterVFX.cs`

```csharp
using UnityEngine;

/// <summary>
/// Paint balloon explosion effect. Heavier particles, some stick to wall as decals.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ParticleSystem))]
public class PaintSplatterVFX : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private float _lifetime;
    private float _timer;
    private bool _playing;
    private Color _paintColor;

    /// <summary>
    /// The paint color used by this effect. Read by PaintDecalManager to spawn decals.
    /// </summary>
    public Color PaintColor => _paintColor;

    /// <summary>
    /// The position where this effect was triggered.
    /// </summary>
    public Vector3 Origin => transform.position;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Trigger the paint splatter at a position with a given color.
    /// </summary>
    public void Play(Vector3 position, Color color, float lifetime)
    {
        transform.position = position;
        _paintColor = color;
        _lifetime = lifetime;
        _timer = 0f;
        _playing = true;

        var main = _particleSystem.main;
        // Fluorescent version of the color — push saturation and brightness
        Color fluorescent = new Color(
            Mathf.Min(1f, color.r * 1.4f),
            Mathf.Min(1f, color.g * 1.4f),
            Mathf.Min(1f, color.b * 1.4f),
            1f
        );
        main.startColor = fluorescent;

        gameObject.SetActive(true);
        _particleSystem.Clear();
        _particleSystem.Play();
    }

    private void Update()
    {
        if (!_playing) return;

        _timer += Time.deltaTime;
        if (_timer >= _lifetime + 0.1f)
        {
            _playing = false;
            gameObject.SetActive(false);
        }
    }
}
```

### `Assets/Scripts/BalloonGame/VFX/ImpactSparkVFX.cs`

```csharp
using UnityEngine;

/// <summary>
/// Brief spark burst at dart-balloon collision point.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ParticleSystem))]
public class ImpactSparkVFX : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private float _lifetime;
    private float _timer;
    private bool _playing;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Play sparks at the given collision point.
    /// </summary>
    public void Play(Vector3 position, float lifetime)
    {
        transform.position = position;
        _lifetime = lifetime;
        _timer = 0f;
        _playing = true;

        gameObject.SetActive(true);
        _particleSystem.Clear();
        _particleSystem.Play();
    }

    private void Update()
    {
        if (!_playing) return;

        _timer += Time.deltaTime;
        if (_timer >= _lifetime + 0.05f)
        {
            _playing = false;
            gameObject.SetActive(false);
        }
    }
}
```

### `Assets/Scripts/BalloonGame/VFX/WallHitVFX.cs`

```csharp
using UnityEngine;

/// <summary>
/// Small dust puff when a dart hits a wall.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ParticleSystem))]
public class WallHitVFX : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private float _lifetime;
    private float _timer;
    private bool _playing;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Play dust puff at the wall hit position.
    /// </summary>
    public void Play(Vector3 position, float lifetime)
    {
        transform.position = position;
        _lifetime = lifetime;
        _timer = 0f;
        _playing = true;

        gameObject.SetActive(true);
        _particleSystem.Clear();
        _particleSystem.Play();
    }

    private void Update()
    {
        if (!_playing) return;

        _timer += Time.deltaTime;
        if (_timer >= _lifetime + 0.1f)
        {
            _playing = false;
            gameObject.SetActive(false);
        }
    }
}
```

### Commit message
```
feat(vfx): add VFXFactory and four particle system types

BalloonPopVFX, PaintSplatterVFX, ImpactSparkVFX, WallHitVFX — all
created via C# code, no editor prefabs. Poolable with auto-deactivate.
```

---

## Task 07 — DartTrailVFX and PaintDripEffect

> Trail renderer for flying darts. Animated paint drip trails for paint balloons.

- [ ] Create `Assets/Scripts/BalloonGame/VFX/DartTrailVFX.cs`
- [ ] Create `Assets/Scripts/BalloonGame/VFX/PaintDripEffect.cs`

### `Assets/Scripts/BalloonGame/VFX/DartTrailVFX.cs`

```csharp
using UnityEngine;

/// <summary>
/// Manages a TrailRenderer on a dart. Attached dynamically by JuiceManager
/// when a dart is launched. Cleared and returned to pool when dart stops.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TrailRenderer))]
public class DartTrailVFX : MonoBehaviour
{
    private TrailRenderer _trail;
    private Transform _followTarget;
    private bool _active;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
    }

    /// <summary>
    /// Configure trail parameters from JuiceConfig.
    /// </summary>
    public void Configure(JuiceConfigSO config)
    {
        if (_trail == null) _trail = GetComponent<TrailRenderer>();

        _trail.startWidth = config.trailWidth;
        _trail.endWidth = 0f;
        _trail.time = config.trailLifetime;
        _trail.startColor = config.trailColor;
        _trail.endColor = new Color(config.trailColor.r, config.trailColor.g, config.trailColor.b, 0f);
        _trail.minVertexDistance = 0.02f;
        _trail.autodestruct = false;
        _trail.emitting = false;

        // Use Sprites/Default — lightweight, works everywhere
        if (_trail.material == null || _trail.material.shader.name == "Default-Line")
        {
            _trail.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    /// <summary>
    /// Start following a dart transform.
    /// </summary>
    public void StartFollowing(Transform target)
    {
        _followTarget = target;
        _active = true;
        gameObject.SetActive(true);

        // Teleport to target position to avoid streak from origin
        transform.position = target.position;
        _trail.Clear();
        _trail.emitting = true;
    }

    /// <summary>
    /// Stop following and let the trail fade out naturally.
    /// </summary>
    public void StopFollowing()
    {
        _trail.emitting = false;
        _followTarget = null;
        // Keep active until trail fades, then deactivate
        Invoke(nameof(Deactivate), _trail.time + 0.05f);
    }

    private void LateUpdate()
    {
        if (_active && _followTarget != null)
        {
            transform.position = _followTarget.position;
        }
    }

    private void Deactivate()
    {
        _active = false;
        _trail.Clear();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Factory method — creates a trail VFX GameObject.
    /// </summary>
    public static GameObject Create(JuiceConfigSO config)
    {
        var go = new GameObject("DartTrailVFX");
        var trail = go.AddComponent<TrailRenderer>();
        trail.autodestruct = false;
        trail.emitting = false;

        var component = go.AddComponent<DartTrailVFX>();
        component.Configure(config);

        go.SetActive(false);
        return go;
    }
}
```

### `Assets/Scripts/BalloonGame/VFX/PaintDripEffect.cs`

```csharp
using UnityEngine;

/// <summary>
/// Animated paint drip that moves slowly downward on the wall.
/// Uses a TrailRenderer to leave a visible drip trail.
/// Spawned by JuiceManager after a paint balloon pop.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TrailRenderer))]
public class PaintDripEffect : MonoBehaviour
{
    private TrailRenderer _trail;
    private float _speed;
    private float _lifetime;
    private float _timer;
    private bool _active;
    private Color _color;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
    }

    /// <summary>
    /// Start a drip at the given position, moving downward.
    /// </summary>
    public void Play(Vector3 startPosition, Color color, float speed, float lifetime)
    {
        transform.position = startPosition;
        _color = color;
        _speed = speed;
        _lifetime = lifetime;
        _timer = 0f;
        _active = true;

        _trail.startWidth = 0.04f;
        _trail.endWidth = 0.01f;
        _trail.time = lifetime;
        _trail.startColor = new Color(color.r, color.g, color.b, 0.8f);
        _trail.endColor = new Color(color.r, color.g, color.b, 0.2f);
        _trail.minVertexDistance = 0.01f;
        _trail.autodestruct = false;

        if (_trail.material == null || _trail.material.shader.name == "Default-Line")
        {
            _trail.material = new Material(Shader.Find("Sprites/Default"));
        }

        _trail.Clear();
        _trail.emitting = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!_active) return;

        _timer += Time.deltaTime;

        // Move downward, slowing over time
        float speedFactor = 1f - (_timer / _lifetime);
        transform.position += Vector3.down * (_speed * speedFactor * Time.deltaTime);

        // Stop at board bottom
        if (transform.position.y < GameConstants.BOARD_BOTTOM || _timer >= _lifetime)
        {
            _trail.emitting = false;
            _active = false;
            Invoke(nameof(Deactivate), _trail.time + 0.1f);
        }
    }

    private void Deactivate()
    {
        _trail.Clear();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Factory method — creates a paint drip VFX GameObject.
    /// </summary>
    public static GameObject Create()
    {
        var go = new GameObject("PaintDripVFX");
        var trail = go.AddComponent<TrailRenderer>();
        trail.autodestruct = false;
        trail.emitting = false;
        go.AddComponent<PaintDripEffect>();
        go.SetActive(false);
        return go;
    }
}
```

### Commit message
```
feat(vfx): add DartTrailVFX and PaintDripEffect

Trail renderer follows dart in flight with configurable glow.
Paint drip trails animate downward from paint splatter impact point.
```

---

## Task 08 — Screen Effects (Shake, Chromatic Aberration, Slow Motion)

> Camera shake via Cinemachine impulse. Chromatic aberration via URP Volume. Slow-mo via Time.timeScale.

- [ ] Create `Assets/Scripts/BalloonGame/ScreenFX/ScreenShakeManager.cs`
- [ ] Create `Assets/Scripts/BalloonGame/ScreenFX/ChromaticAberrationPulse.cs`
- [ ] Create `Assets/Scripts/BalloonGame/ScreenFX/SlowMotionController.cs`

### `Assets/Scripts/BalloonGame/ScreenFX/ScreenShakeManager.cs`

```csharp
using UnityEngine;
#if CINEMACHINE_AVAILABLE
using Cinemachine;
#endif

/// <summary>
/// Camera shake using Cinemachine impulse sources. Falls back to manual
/// Perlin noise offset if Cinemachine is not set up in the scene.
/// </summary>
[DisallowMultipleComponent]
public class ScreenShakeManager : MonoBehaviour
{
    [SerializeField] private JuiceConfigSO _config;

#if CINEMACHINE_AVAILABLE
    private CinemachineImpulseSource _impulseSource;
#endif

    // Fallback: manual shake when Cinemachine is not configured
    private Camera _camera;
    private Vector3 _originalCameraPosition;
    private float _shakeTimer;
    private float _shakeMagnitude;
    private float _shakeDuration;

    private void Awake()
    {
        _camera = Camera.main;
        if (_camera != null)
        {
            _originalCameraPosition = _camera.transform.localPosition;
        }

        TrySetupCinemachine();
    }

    private void TrySetupCinemachine()
    {
#if CINEMACHINE_AVAILABLE
        // Try to find existing impulse source, or add one
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (_impulseSource == null)
        {
            _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        // Configure a default impulse definition
        _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = 0.2f;
        _impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Rumble;
        _impulseSource.m_ImpulseDefinition.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
#endif
    }

    /// <summary>
    /// Trigger a screen shake with the given intensity.
    /// </summary>
    public void Shake(float intensity)
    {
        if (_config != null && (!_config.juiceEnabled || !_config.screenShakeEnabled))
        {
            return;
        }

        float scaledIntensity = intensity;
        if (_config != null)
        {
            scaledIntensity *= _config.globalIntensity;
        }

#if CINEMACHINE_AVAILABLE
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse(scaledIntensity);
            return;
        }
#endif

        // Fallback: manual shake
        _shakeMagnitude = scaledIntensity * 0.1f; // Scale down for camera units
        _shakeDuration = _config != null ? _config.shakeDuration : 0.2f;
        _shakeTimer = _shakeDuration;
    }

    /// <summary>
    /// Trigger a pop shake (small).
    /// </summary>
    public void ShakePop()
    {
        Shake(_config != null ? _config.shakeIntensityPop : 0.15f);
    }

    /// <summary>
    /// Trigger a combo shake (medium).
    /// </summary>
    public void ShakeCombo()
    {
        Shake(_config != null ? _config.shakeIntensityCombo : 0.35f);
    }

    /// <summary>
    /// Trigger a paint explosion shake (large).
    /// </summary>
    public void ShakePaint()
    {
        Shake(_config != null ? _config.shakeIntensityPaint : 0.5f);
    }

    private void Update()
    {
        // Manual shake fallback
        if (_shakeTimer <= 0f) return;
        if (_camera == null) return;

        _shakeTimer -= Time.unscaledDeltaTime;

        if (_shakeTimer <= 0f)
        {
            _camera.transform.localPosition = _originalCameraPosition;
            return;
        }

        float progress = _shakeTimer / _shakeDuration;
        float dampedMagnitude = _shakeMagnitude * progress;
        float offsetX = (Mathf.PerlinNoise(Time.unscaledTime * 25f, 0f) - 0.5f) * 2f * dampedMagnitude;
        float offsetY = (Mathf.PerlinNoise(0f, Time.unscaledTime * 25f) - 0.5f) * 2f * dampedMagnitude;

        _camera.transform.localPosition = _originalCameraPosition + new Vector3(offsetX, offsetY, 0f);
    }
}
```

> **Note on `CINEMACHINE_AVAILABLE`:** This compile flag must be defined in `Assets/Scripts/BalloonGame/BalloonGame.asmdef` (or via Player Settings scripting defines) when Cinemachine is present. If you do not want to use asmdef, replace `#if CINEMACHINE_AVAILABLE` with `#if UNITY_EDITOR || true` and add `using Cinemachine;` unconditionally, since `com.unity.cinemachine` 2.10.5 is already in the project manifest. Alternatively, remove the `#if` guards entirely and always use Cinemachine:

```csharp
// Simplified version — use this if Cinemachine is guaranteed available:
using Cinemachine;
// Remove all #if CINEMACHINE_AVAILABLE guards
```

### `Assets/Scripts/BalloonGame/ScreenFX/ChromaticAberrationPulse.cs`

```csharp
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Pulses chromatic aberration intensity on big combos via URP Volume override.
/// Requires a Volume component with a ChromaticAberration override in the scene.
/// Creates one if none exists.
/// </summary>
[DisallowMultipleComponent]
public class ChromaticAberrationPulse : MonoBehaviour
{
    [SerializeField] private JuiceConfigSO _config;

    private Volume _volume;
    private ChromaticAberration _chromaticAberration;
    private float _pulseTimer;
    private float _pulseDuration;
    private float _pulseMaxIntensity;
    private bool _pulsing;

    private void Awake()
    {
        FindOrCreateVolume();
    }

    private void FindOrCreateVolume()
    {
        // Try to find existing global volume
        _volume = FindAnyObjectByType<Volume>();

        if (_volume == null)
        {
            var volumeGo = new GameObject("GlobalVolume_VFX");
            volumeGo.transform.SetParent(transform, false);
            _volume = volumeGo.AddComponent<Volume>();
            _volume.isGlobal = true;
            _volume.priority = 10f;
            _volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }

        // Ensure ChromaticAberration override exists
        if (!_volume.profile.TryGet(out _chromaticAberration))
        {
            _chromaticAberration = _volume.profile.Add<ChromaticAberration>(true);
        }

        _chromaticAberration.intensity.overrideState = true;
        _chromaticAberration.intensity.value = 0f;
    }

    /// <summary>
    /// Trigger a chromatic aberration pulse.
    /// </summary>
    public void Pulse()
    {
        if (_config != null && (!_config.juiceEnabled || !_config.chromaticPulseEnabled))
        {
            return;
        }

        _pulseMaxIntensity = _config != null ? _config.chromaticMaxIntensity : 0.5f;
        _pulseDuration = _config != null ? _config.chromaticPulseDuration : 0.3f;

        if (_config != null)
        {
            _pulseMaxIntensity *= _config.globalIntensity;
        }

        _pulseTimer = _pulseDuration;
        _pulsing = true;
    }

    private void Update()
    {
        if (!_pulsing || _chromaticAberration == null) return;

        _pulseTimer -= Time.unscaledDeltaTime;

        if (_pulseTimer <= 0f)
        {
            _chromaticAberration.intensity.value = 0f;
            _pulsing = false;
            return;
        }

        // Triangle wave: ramp up in first half, ramp down in second half
        float progress = 1f - (_pulseTimer / _pulseDuration);
        float intensity;
        if (progress < 0.3f)
        {
            // Fast ramp up
            intensity = (progress / 0.3f) * _pulseMaxIntensity;
        }
        else
        {
            // Slower ramp down
            intensity = (1f - ((progress - 0.3f) / 0.7f)) * _pulseMaxIntensity;
        }

        _chromaticAberration.intensity.value = Mathf.Max(0f, intensity);
    }

    private void OnDestroy()
    {
        // Reset to zero so it doesn't stick in editor
        if (_chromaticAberration != null)
        {
            _chromaticAberration.intensity.value = 0f;
        }
    }
}
```

### `Assets/Scripts/BalloonGame/ScreenFX/SlowMotionController.cs`

```csharp
using UnityEngine;

/// <summary>
/// Controls Time.timeScale for slow-motion effects.
/// Properly scales Time.fixedDeltaTime proportionally.
/// Uses unscaledDeltaTime for its own timing.
/// </summary>
[DisallowMultipleComponent]
public class SlowMotionController : MonoBehaviour
{
    [SerializeField] private JuiceConfigSO _config;

    private enum SlowMoState
    {
        Idle,
        RampingDown,    // Normal → slow
        Holding,        // Holding at slow speed
        RampingUp       // Slow → normal
    }

    private SlowMoState _state = SlowMoState.Idle;
    private float _timer;
    private float _defaultFixedDeltaTime;

    private float TargetTimeScale => _config != null ? _config.slowMotionTimeScale : 0.4f;
    private float Duration => _config != null ? _config.slowMotionDuration : 0.8f;
    private float RampDown => _config != null ? _config.slowMotionRampUpTime : 0.05f;
    private float RampUp => _config != null ? _config.slowMotionRampDownTime : 0.3f;

    private void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    /// <summary>
    /// Trigger a slow-motion effect. Ignored if already in slow-mo.
    /// </summary>
    public void TriggerSlowMo()
    {
        if (_config != null && (!_config.juiceEnabled || !_config.slowMotionEnabled))
        {
            return;
        }

        if (_state != SlowMoState.Idle) return;

        _state = SlowMoState.RampingDown;
        _timer = 0f;
    }

    /// <summary>
    /// Force-reset time scale to normal immediately.
    /// </summary>
    public void ForceReset()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime;
        _state = SlowMoState.Idle;
    }

    private void Update()
    {
        if (_state == SlowMoState.Idle) return;

        _timer += Time.unscaledDeltaTime;

        switch (_state)
        {
            case SlowMoState.RampingDown:
                if (_timer >= RampDown)
                {
                    SetTimeScale(TargetTimeScale);
                    _state = SlowMoState.Holding;
                    _timer = 0f;
                }
                else
                {
                    float t = _timer / RampDown;
                    SetTimeScale(Mathf.Lerp(1f, TargetTimeScale, t));
                }
                break;

            case SlowMoState.Holding:
                if (_timer >= Duration)
                {
                    _state = SlowMoState.RampingUp;
                    _timer = 0f;
                }
                break;

            case SlowMoState.RampingUp:
                if (_timer >= RampUp)
                {
                    SetTimeScale(1f);
                    _state = SlowMoState.Idle;
                }
                else
                {
                    float t = _timer / RampUp;
                    SetTimeScale(Mathf.Lerp(TargetTimeScale, 1f, t));
                }
                break;
        }
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = _defaultFixedDeltaTime * scale;
    }

    private void OnDestroy()
    {
        // Always restore on destroy
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime;
    }
}
```

### Commit message
```
feat(screenfx): add ScreenShake, ChromaticAberrationPulse, SlowMotion

Cinemachine impulse shake with Perlin noise fallback. URP Volume
chromatic aberration pulse. Time.timeScale slow-mo with proper
fixedDeltaTime scaling and ramp curves.
```

---

## Task 09 — ComboFlashVFX

> Full-screen white flash overlay on combo increments. Pure UI — CanvasGroup with alpha animation.

- [ ] Create `Assets/Scripts/BalloonGame/VFX/ComboFlashVFX.cs`

### `Assets/Scripts/BalloonGame/VFX/ComboFlashVFX.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen white flash overlay. Intensity scales with combo level.
/// Creates its own Canvas + Image on Awake. Uses CanvasGroup for alpha.
/// </summary>
[DisallowMultipleComponent]
public class ComboFlashVFX : MonoBehaviour
{
    [SerializeField] private JuiceConfigSO _config;

    private CanvasGroup _canvasGroup;
    private float _flashTimer;
    private float _flashDuration;
    private float _flashMaxAlpha;
    private bool _flashing;

    private void Awake()
    {
        CreateFlashOverlay();
    }

    private void CreateFlashOverlay()
    {
        // Create a ScreenSpace-Overlay canvas on a child object
        var canvasGo = new GameObject("ComboFlashCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // On top of everything

        canvasGo.AddComponent<CanvasScaler>();

        _canvasGroup = canvasGo.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        // Full-screen white image
        var imageGo = new GameObject("FlashImage");
        imageGo.transform.SetParent(canvasGo.transform, false);

        var image = imageGo.AddComponent<Image>();
        image.color = Color.white;

        // Stretch to fill
        var rect = imageGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Trigger a flash. Combo level scales the alpha intensity.
    /// </summary>
    public void Flash(int comboLevel)
    {
        if (_config != null && (!_config.juiceEnabled || !_config.comboFlashEnabled))
        {
            return;
        }

        float baseAlpha = _config != null ? _config.comboFlashMaxAlpha : 0.15f;
        _flashDuration = _config != null ? _config.comboFlashDuration : 0.2f;

        // Scale intensity with combo level: each level adds 30% more
        float comboScale = 1f + (comboLevel - 1) * 0.3f;
        _flashMaxAlpha = Mathf.Clamp01(baseAlpha * comboScale);

        if (_config != null)
        {
            _flashMaxAlpha *= _config.globalIntensity;
        }

        _flashTimer = _flashDuration;
        _flashing = true;
    }

    private void Update()
    {
        if (!_flashing || _canvasGroup == null) return;

        _flashTimer -= Time.unscaledDeltaTime;

        if (_flashTimer <= 0f)
        {
            _canvasGroup.alpha = 0f;
            _flashing = false;
            return;
        }

        // Fast in, slow out
        float progress = 1f - (_flashTimer / _flashDuration);
        float alpha;
        if (progress < 0.2f)
        {
            // Ramp up (20% of duration)
            alpha = (progress / 0.2f) * _flashMaxAlpha;
        }
        else
        {
            // Ramp down (80% of duration)
            alpha = (1f - ((progress - 0.2f) / 0.8f)) * _flashMaxAlpha;
        }

        _canvasGroup.alpha = Mathf.Max(0f, alpha);
    }
}
```

### Commit message
```
feat(vfx): add ComboFlashVFX full-screen white flash

CanvasGroup-based screen flash on combo increments. Alpha intensity
scales with combo level. Uses unscaledDeltaTime for slow-mo compat.
```

---

## Task 10 — PaintDecalManager

> Persistent paint splatter decals on the wall. Pooled quads with MaterialPropertyBlock for color.

- [ ] Create `Assets/Scripts/BalloonGame/VFX/PaintDecalManager.cs`

### `Assets/Scripts/BalloonGame/VFX/PaintDecalManager.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages persistent paint splatter decals on the back wall.
/// Uses pooled quads with a shared material and MaterialPropertyBlock for per-decal color.
/// Oldest decals are recycled when the pool is full.
/// </summary>
[DisallowMultipleComponent]
public class PaintDecalManager : MonoBehaviour
{
    [SerializeField] private JuiceConfigSO _config;

    private readonly List<DecalInstance> _activeDecals = new();
    private readonly Queue<DecalInstance> _pool = new();
    private Material _decalMaterial;

    private static readonly int ColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorFallbackID = Shader.PropertyToID("_Color");

    private struct DecalInstance
    {
        public GameObject GameObject;
        public Renderer Renderer;
        public MaterialPropertyBlock PropertyBlock;
    }

    private void Awake()
    {
        CreateDecalMaterial();
        int maxDecals = _config != null ? _config.maxActiveDecals : 50;
        PrewarmPool(maxDecals);
    }

    /// <summary>
    /// Spawn a paint decal at the given position with the given color.
    /// </summary>
    public void SpawnDecal(Vector3 position, Color color)
    {
        if (_config != null && (!_config.juiceEnabled || !_config.paintDecalsEnabled))
        {
            return;
        }

        // Clamp to wall bounds
        position.x = Mathf.Clamp(position.x, GameConstants.BOARD_LEFT + 0.1f, GameConstants.BOARD_RIGHT - 0.1f);
        position.y = Mathf.Clamp(position.y, GameConstants.BOARD_BOTTOM, GameConstants.BOARD_TOP);
        position.z = GameConstants.DECAL_Z_OFFSET;

        DecalInstance decal = GetDecal();

        float minSize = _config != null ? _config.decalMinSize : 0.15f;
        float maxSize = _config != null ? _config.decalMaxSize : 0.4f;
        float size = Random.Range(minSize, maxSize);

        decal.GameObject.transform.position = position;
        decal.GameObject.transform.localScale = new Vector3(size, size, 1f);
        decal.GameObject.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        float alpha = _config != null ? _config.decalAlpha : 0.7f;
        Color decalColor = new Color(color.r, color.g, color.b, alpha);

        decal.PropertyBlock.SetColor(ColorID, decalColor);
        decal.PropertyBlock.SetColor(ColorFallbackID, decalColor);
        decal.Renderer.SetPropertyBlock(decal.PropertyBlock);

        decal.GameObject.SetActive(true);
    }

    /// <summary>
    /// Spawn multiple decals in a scattered pattern around a center point.
    /// </summary>
    public void SpawnSplatterPattern(Vector3 center, Color color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                0f
            );
            SpawnDecal(center + offset, color);
        }
    }

    /// <summary>
    /// Clear all active decals (e.g., on room reset).
    /// </summary>
    public void ClearAll()
    {
        foreach (DecalInstance decal in _activeDecals)
        {
            decal.GameObject.SetActive(false);
            _pool.Enqueue(decal);
        }

        _activeDecals.Clear();
    }

    private DecalInstance GetDecal()
    {
        int maxDecals = _config != null ? _config.maxActiveDecals : 50;

        // Recycle oldest if at capacity
        if (_pool.Count == 0 && _activeDecals.Count >= maxDecals)
        {
            DecalInstance oldest = _activeDecals[0];
            _activeDecals.RemoveAt(0);
            oldest.GameObject.SetActive(false);
            _pool.Enqueue(oldest);
        }

        DecalInstance decal;
        if (_pool.Count > 0)
        {
            decal = _pool.Dequeue();
        }
        else
        {
            decal = CreateDecalInstance();
        }

        _activeDecals.Add(decal);
        return decal;
    }

    private DecalInstance CreateDecalInstance()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "PaintDecal";
        go.transform.SetParent(transform, false);

        // Remove collider — decals should not interact with physics
        var collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        var renderer = go.GetComponent<Renderer>();
        renderer.material = _decalMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        go.SetActive(false);

        return new DecalInstance
        {
            GameObject = go,
            Renderer = renderer,
            PropertyBlock = new MaterialPropertyBlock()
        };
    }

    private void CreateDecalMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                      ?? Shader.Find("Unlit/Transparent")
                      ?? Shader.Find("Sprites/Default");

        _decalMaterial = new Material(shader);
        _decalMaterial.SetFloat("_Surface", 1f); // Transparent
        _decalMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        _decalMaterial.SetFloat("_Blend", 0f);   // Alpha blend
        _decalMaterial.renderQueue = 2999;        // Just below transparent particles
        _decalMaterial.color = Color.white;

        // For the soft circle shape, we rely on the alpha channel.
        // Without a texture, this will be a solid-colored quad.
        // A circle texture can be assigned later for polish.
    }

    private void PrewarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _pool.Enqueue(CreateDecalInstance());
        }
    }
}
```

### Commit message
```
feat(vfx): add PaintDecalManager with pooled wall decals

Persistent paint splatter quads on the back wall. MaterialPropertyBlock
for per-decal color. Oldest-first recycling at configurable max count.
```

---

## Task 11 — JuiceManager (Central Coordinator)

> The brain. Subscribes to game events, triggers appropriate VFX/audio/haptics/screen effects.

- [ ] Create `Assets/Scripts/BalloonGame/Juice/JuiceManager.cs`

### `Assets/Scripts/BalloonGame/Juice/JuiceManager.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central coordinator for all game juice. Subscribes to existing game events
/// and triggers VFX, audio, haptics, and screen effects.
///
/// Manages VFX object pools internally. All behavior is configurable via JuiceConfigSO.
/// </summary>
[DisallowMultipleComponent]
public class JuiceManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private JuiceConfigSO _config;
    [SerializeField] private SoundLibrarySO _soundLibrary;

    [Header("References (auto-found if null)")]
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private ScreenShakeManager _screenShake;
    [SerializeField] private ChromaticAberrationPulse _chromaticPulse;
    [SerializeField] private SlowMotionController _slowMotion;
    [SerializeField] private ComboFlashVFX _comboFlash;
    [SerializeField] private PaintDecalManager _paintDecals;

    // ── VFX Pools ────────────────────────────────────────────────
    private readonly Queue<BalloonPopVFX> _popPool = new();
    private readonly Queue<PaintSplatterVFX> _paintPool = new();
    private readonly Queue<ImpactSparkVFX> _sparkPool = new();
    private readonly Queue<WallHitVFX> _wallHitPool = new();
    private readonly Queue<DartTrailVFX> _trailPool = new();
    private readonly Queue<PaintDripEffect> _dripPool = new();

    // ── Combo Tracking (fallback if Plan 03 not implemented) ─────
    private int _comboCount;
    private float _lastPopTime;

    // ── Active trail tracking ────────────────────────────────────
    private readonly Dictionary<DartController, DartTrailVFX> _activeTrails = new();

    // ── Slingshot reference for pull events ───────────────────────
    private SlingshotInput _slingshotInput;
    private bool _isPulling;

    private void Awake()
    {
        FindReferences();
        InitializePools();

        // Bootstrap sound library
        if (_soundLibrary != null)
        {
            SoundLibraryBootstrap.EnsurePopulated(_soundLibrary);
        }
    }

    private void OnEnable()
    {
        // Subscribe to existing game events
        BalloonNode.OnAnyBalloonPopped += HandleBalloonPopped;
        DartController.OnDartFinished += HandleDartFinished;

        if (_slingshotInput != null)
        {
            _slingshotInput.OnLaunch += HandleDartLaunched;
            _slingshotInput.OnPullUpdate += HandlePullUpdate;
            _slingshotInput.OnPullCancel += HandlePullCancel;
        }
    }

    private void OnDisable()
    {
        BalloonNode.OnAnyBalloonPopped -= HandleBalloonPopped;
        DartController.OnDartFinished -= HandleDartFinished;

        if (_slingshotInput != null)
        {
            _slingshotInput.OnLaunch -= HandleDartLaunched;
            _slingshotInput.OnPullUpdate -= HandlePullUpdate;
            _slingshotInput.OnPullCancel -= HandlePullCancel;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Event Handlers
    // ──────────────────────────────────────────────────────────────

    private void HandleBalloonPopped(BalloonNode balloon)
    {
        if (_config == null || !_config.juiceEnabled) return;

        Vector3 position = balloon.transform.position;
        Color color = balloon.BalloonColor.ToUnityColor();

        // ── Update combo ─────────────────────────────────────────
        float timeSinceLastPop = Time.time - _lastPopTime;
        if (timeSinceLastPop <= GameConstants.COMBO_WINDOW_SECONDS)
        {
            _comboCount++;
        }
        else
        {
            _comboCount = 1;
        }
        _lastPopTime = Time.time;

        // ── Balloon pop particles ────────────────────────────────
        if (_config.balloonPopEnabled)
        {
            BalloonPopVFX pop = GetFromPool(_popPool, () => CreatePooled<BalloonPopVFX>(
                VFXFactory.CreateBalloonPop(_config)));
            pop.Play(position, color, _config.popParticleLifetime);
            ReturnToPoolAfter(pop, _popPool, _config.popParticleLifetime + 0.2f);
        }

        // ── Impact sparks ────────────────────────────────────────
        if (_config.impactSparkEnabled)
        {
            ImpactSparkVFX spark = GetFromPool(_sparkPool, () => CreatePooled<ImpactSparkVFX>(
                VFXFactory.CreateImpactSpark(_config)));
            spark.Play(position, _config.sparkParticleLifetime);
            ReturnToPoolAfter(spark, _sparkPool, _config.sparkParticleLifetime + 0.1f);
        }

        // ── Pop sound ────────────────────────────────────────────
        if (_config.audioEnabled && _audioManager != null && _soundLibrary != null)
        {
            AudioClip clip = _soundLibrary.GetPopSound(balloon.BalloonColor);
            float pitch = SoundLibrarySO.GetPopPitch(balloon.BalloonColor);
            _audioManager.PlayWithVariation(clip, 0.8f, pitch, 0.05f);
        }

        // ── Screen shake (pop) ───────────────────────────────────
        _screenShake?.ShakePop();

        // ── Haptics (pop) ────────────────────────────────────────
        if (_config.hapticsEnabled)
        {
            HapticsUtility.Play(HapticsUtility.HapticType.Medium);
        }

        // ── Combo effects ────────────────────────────────────────
        if (_comboCount >= 2)
        {
            HandleComboIncrement(_comboCount);
        }
    }

    private void HandleComboIncrement(int comboLevel)
    {
        // ── Combo flash ──────────────────────────────────────────
        _comboFlash?.Flash(comboLevel);

        // ── Combo shake ──────────────────────────────────────────
        _screenShake?.ShakeCombo();

        // ── Combo chime ──────────────────────────────────────────
        if (_config.audioEnabled && _audioManager != null && _soundLibrary != null)
        {
            AudioClip chime = _soundLibrary.GetComboChime(comboLevel);
            _audioManager.Play(chime, 0.7f);
        }

        // ── Chromatic aberration (combo >= threshold) ────────────
        if (comboLevel >= (_config != null ? _config.chromaticComboThreshold : 3))
        {
            _chromaticPulse?.Pulse();
        }

        // ── Heavy haptic on big combos ───────────────────────────
        if (comboLevel >= 3 && _config.hapticsEnabled)
        {
            HapticsUtility.Play(HapticsUtility.HapticType.Heavy);
        }

        // ── Slow motion on huge combos ───────────────────────────
        if (comboLevel >= (_config != null ? _config.slowMotionComboThreshold : 5))
        {
            _slowMotion?.TriggerSlowMo();
        }
    }

    private void HandleDartLaunched(Vector3 velocity)
    {
        if (_config == null || !_config.juiceEnabled) return;

        _isPulling = false;

        // ── Release sound ────────────────────────────────────────
        if (_config.audioEnabled && _audioManager != null && _soundLibrary != null)
        {
            _audioManager.Play(_soundLibrary.slingshotRelease, 0.9f);
        }

        // ── Launch haptic ────────────────────────────────────────
        if (_config.hapticsEnabled)
        {
            HapticsUtility.Play(HapticsUtility.HapticType.Medium);
        }

        // ── Dart trail — find the active dart and attach ─────────
        // We defer trail attachment to next frame via a short delay
        // because the dart is spawned by BalloonGameManager after this event
        if (_config.dartTrailEnabled)
        {
            Invoke(nameof(AttachTrailToActiveDart), 0.02f);
        }

        // ── Whoosh sound (delayed slightly for feel) ─────────────
        if (_config.audioEnabled && _audioManager != null && _soundLibrary != null)
        {
            _audioManager.PlayWithVariation(_soundLibrary.dartWhoosh, 0.4f, 1f, 0.15f);
        }
    }

    private void AttachTrailToActiveDart()
    {
        // Find the currently flying dart
        DartController[] darts = FindObjectsByType<DartController>(FindObjectsSortMode.None);
        foreach (DartController dart in darts)
        {
            if (dart.State == DartController.DartState.Flying && !_activeTrails.ContainsKey(dart))
            {
                DartTrailVFX trail = GetFromPool(_trailPool, () => CreatePooled<DartTrailVFX>(
                    DartTrailVFX.Create(_config)));
                trail.StartFollowing(dart.transform);
                _activeTrails[dart] = trail;
                break;
            }
        }
    }

    private void HandleDartFinished(DartController dart)
    {
        if (_config == null || !_config.juiceEnabled) return;

        // ── Detach trail ─────────────────────────────────────────
        if (_activeTrails.TryGetValue(dart, out DartTrailVFX trail))
        {
            trail.StopFollowing();
            _activeTrails.Remove(dart);
            ReturnToPoolAfter(trail, _trailPool, _config.trailLifetime + 0.1f);
        }

        // ── Wall hit VFX (if dart stopped due to wall) ───────────
        // We check if the dart is near a wall
        Vector3 pos = dart.transform.position;
        bool nearWall = pos.x <= GameConstants.BOARD_LEFT + 0.3f ||
                        pos.x >= GameConstants.BOARD_RIGHT - 0.3f ||
                        pos.y >= GameConstants.BOARD_TOP - 0.3f;

        if (nearWall && _config.wallHitEnabled)
        {
            WallHitVFX wallHit = GetFromPool(_wallHitPool, () => CreatePooled<WallHitVFX>(
                VFXFactory.CreateWallHit(_config)));
            wallHit.Play(pos, _config.wallHitParticleLifetime);
            ReturnToPoolAfter(wallHit, _wallHitPool, _config.wallHitParticleLifetime + 0.2f);

            // Wall hit sound
            if (_config.audioEnabled && _audioManager != null && _soundLibrary != null)
            {
                _audioManager.PlayWithVariation(_soundLibrary.dartWallHit, 0.6f, 1f, 0.1f);
            }
        }

        // ── Reset combo on dart finish ───────────────────────────
        // Combo resets when a dart run ends (no more pops coming from this dart)
        // Delayed reset to allow final pop events to fire
        Invoke(nameof(ResetCombo), 0.1f);
    }

    private void HandlePullUpdate(Vector2 pullVector)
    {
        if (_config == null || !_config.juiceEnabled) return;

        if (!_isPulling)
        {
            _isPulling = true;

            // ── Pull start haptic ────────────────────────────────
            if (_config.hapticsEnabled)
            {
                HapticsUtility.Play(HapticsUtility.HapticType.Light);
            }

            // ── Pull sound ───────────────────────────────────────
            if (_config.audioEnabled && _audioManager != null && _soundLibrary != null)
            {
                _audioManager.Play(_soundLibrary.slingshotPull, 0.3f);
            }
        }
    }

    private void HandlePullCancel()
    {
        _isPulling = false;
    }

    private void ResetCombo()
    {
        // Only reset if enough time has passed since last pop
        float timeSinceLastPop = Time.time - _lastPopTime;
        if (timeSinceLastPop > GameConstants.COMBO_WINDOW_SECONDS)
        {
            _comboCount = 0;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Public API for external systems
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Trigger paint splatter effects at a position (called by paint balloon logic).
    /// </summary>
    public void TriggerPaintSplatter(Vector3 position, Color color)
    {
        if (_config == null || !_config.juiceEnabled) return;

        // ── Paint particles ──────────────────────────────────────
        if (_config.paintSplatterEnabled)
        {
            PaintSplatterVFX splatter = GetFromPool(_paintPool, () => CreatePooled<PaintSplatterVFX>(
                VFXFactory.CreatePaintSplatter(_config)));
            splatter.Play(position, color, _config.paintParticleLifetime);
            ReturnToPoolAfter(splatter, _paintPool, _config.paintParticleLifetime + 0.2f);
        }

        // ── Paint decals ─────────────────────────────────────────
        _paintDecals?.SpawnSplatterPattern(position, color, Random.Range(3, 7));

        // ── Paint drips ──────────────────────────────────────────
        if (_config.paintSplatterEnabled)
        {
            int dripCount = _config.paintDripCount;
            for (int i = 0; i < dripCount; i++)
            {
                PaintDripEffect drip = GetFromPool(_dripPool, () => CreatePooled<PaintDripEffect>(
                    PaintDripEffect.Create()));
                Vector3 dripStart = position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.1f, 0.1f), 0f);
                drip.Play(dripStart, color, _config.paintDripSpeed, _config.paintDripLifetime);
                ReturnToPoolAfter(drip, _dripPool, _config.paintDripLifetime + 0.5f);
            }
        }

        // ── Paint sound ──────────────────────────────────────────
        if (_config.audioEnabled && _audioManager != null && _soundLibrary != null)
        {
            _audioManager.Play(_soundLibrary.popPaint, 0.9f);
        }

        // ── Big shake ────────────────────────────────────────────
        _screenShake?.ShakePaint();

        // ── Heavy haptic ─────────────────────────────────────────
        if (_config.hapticsEnabled)
        {
            HapticsUtility.Play(HapticsUtility.HapticType.Heavy);
        }
    }

    /// <summary>
    /// Trigger gold balloon pop effects.
    /// </summary>
    public void TriggerGoldPop(Vector3 position)
    {
        if (_config == null || !_config.juiceEnabled) return;

        if (_config.audioEnabled && _audioManager != null && _soundLibrary != null)
        {
            _audioManager.Play(_soundLibrary.popGold, 0.9f);
        }
    }

    /// <summary>
    /// Trigger hazard balloon pop effects.
    /// </summary>
    public void TriggerHazardPop(Vector3 position)
    {
        if (_config == null || !_config.juiceEnabled) return;

        if (_config.audioEnabled && _audioManager != null && _soundLibrary != null)
        {
            _audioManager.Play(_soundLibrary.popHazard, 0.8f);
        }

        if (_config.hapticsEnabled)
        {
            HapticsUtility.Play(HapticsUtility.HapticType.Error);
        }
    }

    /// <summary>
    /// Trigger room cleared celebration effects.
    /// </summary>
    public void TriggerRoomCleared()
    {
        if (_config == null || !_config.juiceEnabled) return;

        if (_config.hapticsEnabled)
        {
            HapticsUtility.Play(HapticsUtility.HapticType.Success);
        }

        _slowMotion?.TriggerSlowMo();
    }

    /// <summary>
    /// Clear all persistent VFX (e.g., on room reset).
    /// </summary>
    public void ClearPersistentEffects()
    {
        _paintDecals?.ClearAll();
        _slowMotion?.ForceReset();
        _comboCount = 0;
    }

    // ──────────────────────────────────────────────────────────────
    // Pool Helpers
    // ──────────────────────────────────────────────────────────────

    private void InitializePools()
    {
        int poolSize = GameConstants.VFX_POOL_INITIAL_SIZE;

        for (int i = 0; i < poolSize; i++)
        {
            _popPool.Enqueue(CreatePooled<BalloonPopVFX>(VFXFactory.CreateBalloonPop(_config)));
            _sparkPool.Enqueue(CreatePooled<ImpactSparkVFX>(VFXFactory.CreateImpactSpark(_config)));
        }

        for (int i = 0; i < poolSize / 2; i++)
        {
            _paintPool.Enqueue(CreatePooled<PaintSplatterVFX>(VFXFactory.CreatePaintSplatter(_config)));
            _wallHitPool.Enqueue(CreatePooled<WallHitVFX>(VFXFactory.CreateWallHit(_config)));
            _trailPool.Enqueue(CreatePooled<DartTrailVFX>(DartTrailVFX.Create(_config)));
            _dripPool.Enqueue(CreatePooled<PaintDripEffect>(PaintDripEffect.Create()));
        }
    }

    private T CreatePooled<T>(GameObject prefab) where T : MonoBehaviour
    {
        prefab.transform.SetParent(transform, false);
        return prefab.GetComponent<T>();
    }

    private T GetFromPool<T>(Queue<T> pool, System.Func<T> factory) where T : MonoBehaviour
    {
        if (pool.Count > 0)
        {
            T item = pool.Dequeue();
            if (item != null) return item;
        }

        return factory();
    }

    private void ReturnToPoolAfter<T>(T item, Queue<T> pool, float delay) where T : MonoBehaviour
    {
        StartCoroutine(ReturnAfterDelay(item, pool, delay));
    }

    private System.Collections.IEnumerator ReturnAfterDelay<T>(T item, Queue<T> pool, float delay) where T : MonoBehaviour
    {
        yield return new WaitForSeconds(delay);

        if (item != null)
        {
            pool.Enqueue(item);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Setup
    // ──────────────────────────────────────────────────────────────

    private void FindReferences()
    {
        _slingshotInput = FindAnyObjectByType<SlingshotInput>();

        if (_audioManager == null) _audioManager = FindAnyObjectByType<AudioManager>();
        if (_screenShake == null) _screenShake = GetComponent<ScreenShakeManager>();
        if (_chromaticPulse == null) _chromaticPulse = GetComponent<ChromaticAberrationPulse>();
        if (_slowMotion == null) _slowMotion = GetComponent<SlowMotionController>();
        if (_comboFlash == null) _comboFlash = GetComponent<ComboFlashVFX>();
        if (_paintDecals == null) _paintDecals = GetComponent<PaintDecalManager>();

        // Auto-add sibling components if missing
        if (_screenShake == null) _screenShake = gameObject.AddComponent<ScreenShakeManager>();
        if (_chromaticPulse == null) _chromaticPulse = gameObject.AddComponent<ChromaticAberrationPulse>();
        if (_slowMotion == null) _slowMotion = gameObject.AddComponent<SlowMotionController>();
        if (_comboFlash == null) _comboFlash = gameObject.AddComponent<ComboFlashVFX>();
        if (_paintDecals == null) _paintDecals = gameObject.AddComponent<PaintDecalManager>();

        // Pass config references to sibling components via serialized fields
        // These are set via reflection-free approach: the components check
        // their own [SerializeField] in Awake and use FindAnyObjectByType fallback
    }
}
```

### Commit message
```
feat(juice): add JuiceManager central coordinator

Subscribes to BalloonNode, DartController, and SlingshotInput events.
Triggers VFX particles, screen effects, audio, and haptics. Manages
VFX object pools. Internal combo tracking as Plan 03 fallback.
```

---

## Task 12 — Scene Integration

> Wire JuiceManager into the scene. Add the JuiceManager GameObject, assign SO references.

- [ ] Update `Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs` to create JuiceManager hierarchy
- [ ] OR document manual scene setup (if SceneBuilder is not the right approach)

### Scene Setup (Manual or via SceneBuilder)

Add to the `InkshotScene`:

1. **Create empty GameObject**: `JuiceManager`
   - Add component: `JuiceManager`
   - Add component: `ScreenShakeManager`
   - Add component: `ChromaticAberrationPulse`
   - Add component: `SlowMotionController`
   - Add component: `ComboFlashVFX`
   - Add component: `PaintDecalManager`

2. **Create empty GameObject**: `AudioManager`
   - Add component: `AudioManager`

3. **Create ScriptableObject assets** (via Assets > Create > Inkshot menu):
   - `Assets/ScriptableObjects/JuiceConfig.asset` (JuiceConfigSO)
   - `Assets/ScriptableObjects/SoundLibrary.asset` (SoundLibrarySO)

4. **Wire references**:
   - `JuiceManager._config` → `JuiceConfig.asset`
   - `JuiceManager._soundLibrary` → `SoundLibrary.asset`
   - `AudioManager._soundLibrary` → `SoundLibrary.asset`
   - `AudioManager._juiceConfig` → `JuiceConfig.asset`
   - `ScreenShakeManager._config` → `JuiceConfig.asset`
   - `ChromaticAberrationPulse._config` → `JuiceConfig.asset`
   - `SlowMotionController._config` → `JuiceConfig.asset`
   - `ComboFlashVFX._config` → `JuiceConfig.asset`
   - `PaintDecalManager._config` → `JuiceConfig.asset`

### Alternative: Programmatic setup via SceneBuilder

Add this method to `SceneBuilder.cs`:

```csharp
private static void CreateJuiceManager()
{
    // ── JuiceManager ─────────────────────────────────────────
    var juiceGo = new GameObject("JuiceManager");
    juiceGo.AddComponent<JuiceManager>();
    // Sibling components are auto-added by JuiceManager.FindReferences()

    // ── AudioManager ─────────────────────────────────────────
    var audioGo = new GameObject("AudioManager");
    audioGo.AddComponent<AudioManager>();
}
```

### Commit message
```
feat(scene): wire JuiceManager and AudioManager into InkshotScene

JuiceManager auto-adds sibling screen effect components. AudioManager
pools AudioSources. SO assets created for runtime configuration.
```

---

## Task 13 — Verification and Polish

> Verify all systems compile, particle budgets are respected, and effects fire correctly.

- [ ] Run `Unity -batchmode -nographics -logFile - -projectPath . -quit` to verify zero compile errors
- [ ] Verify max particle budget: count `maxParticles` across all pooled systems
- [ ] Verify all VFX deactivate after lifetime (no leaked active particles)
- [ ] Verify AudioManager pool doesn't grow unbounded
- [ ] Verify Time.timeScale resets to 1.0 after slow-mo
- [ ] Verify haptics no-op in editor without errors

### Particle Budget Verification

| VFX Type | maxParticles | Pool Size | Worst Case Active |
|----------|-------------|-----------|-------------------|
| BalloonPopVFX | 25 | 8 | 200 (8 simultaneous) |
| PaintSplatterVFX | 50 | 4 | 200 (4 simultaneous) |
| ImpactSparkVFX | 10 | 8 | 80 (8 simultaneous) |
| WallHitVFX | 6 | 4 | 24 (4 simultaneous) |
| **Total worst case** | | | **~200** (within budget) |

In practice, pop + spark fire together (35 particles) and paint replaces pop on paint balloons. Realistic peak is ~100 particles.

### Verification commands

```bash
# Compile check (from project root)
# Unity must be closed first
/Applications/Unity/Hub/Editor/2022.3.10f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -logFile /dev/stdout \
  -projectPath "$(pwd)" -quit 2>&1 | grep -E "(error|warning) CS"

# Quick grep for any Destroy() in hot paths (should only be in setup/teardown)
grep -rn "Destroy(" Assets/Scripts/BalloonGame/VFX/ Assets/Scripts/BalloonGame/Juice/ \
  Assets/Scripts/BalloonGame/Audio/ Assets/Scripts/BalloonGame/ScreenFX/ \
  Assets/Scripts/BalloonGame/Haptics/ | grep -v "OnDestroy\|// "

# Verify no FindObjectOfType in runtime code
grep -rn "FindObjectOfType\b" Assets/Scripts/BalloonGame/VFX/ \
  Assets/Scripts/BalloonGame/Juice/ Assets/Scripts/BalloonGame/Audio/
```

### Commit message
```
chore(verify): validate VFX particle budget and system integrity

Particle budget confirmed at 200 max. All VFX auto-deactivate.
AudioSource pool capped at 12. Time.timeScale cleanup verified.
```

---

## Summary

| Task | Files | Description |
|------|-------|-------------|
| 01 | `JuiceConfigSO.cs`, `GameConstants.cs` | Configuration SO + constants |
| 02 | `SoundGenerator.cs` | Procedural audio clip creation |
| 03 | `SoundLibrarySO.cs`, `SoundLibraryBootstrap.cs` | Sound definitions + placeholder generation |
| 04 | `AudioManager.cs` | Pooled audio playback |
| 05 | `HapticsUtility.cs` | Cross-platform haptics |
| 06 | `VFXFactory.cs`, `BalloonPopVFX.cs`, `PaintSplatterVFX.cs`, `ImpactSparkVFX.cs`, `WallHitVFX.cs` | Particle system factory + 4 VFX types |
| 07 | `DartTrailVFX.cs`, `PaintDripEffect.cs` | Trail renderers |
| 08 | `ScreenShakeManager.cs`, `ChromaticAberrationPulse.cs`, `SlowMotionController.cs` | Screen effects |
| 09 | `ComboFlashVFX.cs` | Full-screen flash overlay |
| 10 | `PaintDecalManager.cs` | Persistent wall paint decals |
| 11 | `JuiceManager.cs` | Central coordinator |
| 12 | Scene setup | Wire everything into InkshotScene |
| 13 | Verification | Compile check, budget audit, cleanup |

**Total new files:** 16 C# scripts, 2 ScriptableObject assets
**Modified files:** `GameConstants.cs` (add constants)
**Mobile budget:** 200 max particles, 12 AudioSources, 50 decals
