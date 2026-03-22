using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Creates a runtime global Volume with Bloom and Vignette for the noir carnival aesthetic.
/// </summary>
[DisallowMultipleComponent]
public class PostProcessingSetup : MonoBehaviour
{
    private const float BloomThreshold = 0.20f;
    private const float BloomIntensity = 10.0f;
    private const float BloomScatter = 0.92f;
    private const float VignetteIntensity = 0.50f;
    private const float VignetteSmoothness = 0.45f;
    private const float ChromaticIntensity = 0.10f;
    private const float FilmGrainIntensity = 0.15f;

    private Volume _volume;

    private void Awake()
    {
        EnsurePostProcessing();
    }

    [ContextMenu("Setup Post Processing")]
    public void EnsurePostProcessing()
    {
        if (_volume != null) return;

        _volume = gameObject.GetComponent<Volume>();
        if (_volume == null) _volume = gameObject.AddComponent<Volume>();
        _volume.isGlobal = true;
        _volume.priority = 1f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        _volume.profile = profile;

        AddBloom(profile);
        AddVignette(profile);
        AddChromaticAberration(profile);
        AddFilmGrain(profile);
        AddColorGrading(profile);
    }

    private static void AddBloom(VolumeProfile profile)
    {
        var bloom = profile.Add<Bloom>();
        bloom.threshold.overrideState = true;
        bloom.threshold.value = BloomThreshold;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = BloomIntensity;
        bloom.scatter.overrideState = true;
        bloom.scatter.value = BloomScatter;
        bloom.tint.overrideState = true;
        bloom.tint.value = new Color(1f, 0.95f, 0.9f);
    }

    private static void AddVignette(VolumeProfile profile)
    {
        var vignette = profile.Add<Vignette>();
        vignette.intensity.overrideState = true;
        vignette.intensity.value = VignetteIntensity;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = VignetteSmoothness;
        vignette.color.overrideState = true;
        vignette.color.value = new Color(0.02f, 0.01f, 0.03f);
    }

    private static void AddChromaticAberration(VolumeProfile profile)
    {
        var ca = profile.Add<ChromaticAberration>();
        ca.intensity.overrideState = true;
        ca.intensity.value = ChromaticIntensity;
    }

    private static void AddFilmGrain(VolumeProfile profile)
    {
        var grain = profile.Add<FilmGrain>();
        grain.type.overrideState = true;
        grain.type.value = FilmGrainLookup.Medium1;
        grain.intensity.overrideState = true;
        grain.intensity.value = FilmGrainIntensity;
    }

    private static void AddColorGrading(VolumeProfile profile)
    {
        var grading = profile.Add<ColorAdjustments>();
        grading.postExposure.overrideState = true;
        grading.postExposure.value = 0.15f;
        grading.contrast.overrideState = true;
        grading.contrast.value = 12f;
        grading.saturation.overrideState = true;
        grading.saturation.value = 10f;
    }
}
