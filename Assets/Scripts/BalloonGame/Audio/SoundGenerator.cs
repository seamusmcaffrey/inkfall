using UnityEngine;

/// <summary>
/// Procedurally generates simple one-shot audio clips.
/// </summary>
public static class SoundGenerator
{
    private static int SampleRate => GameConstants.AUDIO_SAMPLE_RATE;

    public static AudioClip Sine(float frequency, float duration, float volume = 1f, string name = "Sine")
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * Envelope(t, duration);
        }

        return CreateClip(name, samples);
    }

    public static AudioClip Noise(float duration, float volume = 1f, string name = "Noise")
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            samples[i] = Random.Range(-1f, 1f) * volume * Envelope(t, duration);
        }

        return CreateClip(name, samples);
    }

    public static AudioClip Chirp(float startFrequency, float endFrequency, float duration, float volume = 1f, string name = "Chirp")
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, t / duration);
            phase += 2f * Mathf.PI * frequency / SampleRate;
            samples[i] = Mathf.Sin(phase) * volume * Envelope(t, duration);
        }

        return CreateClip(name, samples);
    }

    public static AudioClip Mix(string name, params AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        int length = 0;
        foreach (AudioClip clip in clips)
        {
            length = Mathf.Max(length, clip.samples);
        }

        float[] mix = new float[length];
        foreach (AudioClip clip in clips)
        {
            if (clip == null)
            {
                continue;
            }

            float[] buffer = new float[clip.samples];
            clip.GetData(buffer, 0);
            for (int index = 0; index < buffer.Length; index++)
            {
                mix[index] += buffer[index];
            }
        }

        for (int index = 0; index < mix.Length; index++)
        {
            mix[index] = Mathf.Clamp(mix[index], -1f, 1f);
        }

        return CreateClip(name, mix);
    }

    private static float Envelope(float t, float duration)
    {
        float attack = Mathf.Clamp(duration * 0.08f, 0.005f, 0.03f);
        float release = Mathf.Clamp(duration * 0.35f, 0.03f, duration);

        if (t < attack)
        {
            return t / attack;
        }

        float releaseStart = duration - release;
        if (t > releaseStart)
        {
            return Mathf.Clamp01((duration - t) / release);
        }

        return 1f;
    }

    private static AudioClip CreateClip(string name, float[] samples)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
