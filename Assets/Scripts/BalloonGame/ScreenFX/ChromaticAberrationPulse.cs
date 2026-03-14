using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Pulses URP chromatic aberration on high-impact moments.
/// </summary>
[DisallowMultipleComponent]
public class ChromaticAberrationPulse : MonoBehaviour
{
    private JuiceConfigSO _config;
    private ChromaticAberration _effect;

    private void Awake()
    {
        EnsureVolume();
    }

    public void SetConfig(JuiceConfigSO config)
    {
        _config = config;
    }

    public void Pulse(float intensityMultiplier = 1f)
    {
        if (_effect == null || QualityTier.IsLow)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PulseRoutine(intensityMultiplier));
    }

    private IEnumerator PulseRoutine(float intensityMultiplier)
    {
        float maxIntensity = (_config != null ? _config.chromaticMaxIntensity : 0.4f) * intensityMultiplier;
        float duration = _config != null ? _config.chromaticPulseDuration : 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _effect.intensity.value = Mathf.Lerp(maxIntensity, 0f, elapsed / duration);
            yield return null;
        }
        _effect.intensity.value = 0f;
    }

    private void EnsureVolume()
    {
        Transform existing = transform.Find("GlobalVolume");
        GameObject go = existing != null ? existing.gameObject : new GameObject("GlobalVolume");
        go.transform.SetParent(transform, false);

        Volume volume = go.GetComponent<Volume>();
        if (volume == null)
        {
            volume = go.AddComponent<Volume>();
        }

        volume.isGlobal = true;
        volume.priority = 100f;
        if (volume.profile == null)
        {
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }

        if (!volume.profile.TryGet(out _effect))
        {
            _effect = volume.profile.Add<ChromaticAberration>(true);
        }
        _effect.intensity.overrideState = true;
        _effect.intensity.value = 0f;
    }
}
