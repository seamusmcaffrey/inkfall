using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Creates a runtime global Volume with Bloom and Vignette for the noir carnival aesthetic.
/// </summary>
[DisallowMultipleComponent]
public class PostProcessingSetup : MonoBehaviour
{
    private const float BloomThreshold = 0.4f;
    private const float BloomIntensity = 5.0f;
    private const float BloomScatter = 0.82f;
    private const float VignetteIntensity = 0.62f;
    private const float VignetteSmoothness = 0.50f;
    private const float ChromaticIntensity = 0.14f;

    private Volume _volume;

    private void Awake()
    {
        // Post-processing disabled for clean even lighting.
        // Call EnsurePostProcessing() to re-enable.
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
}
