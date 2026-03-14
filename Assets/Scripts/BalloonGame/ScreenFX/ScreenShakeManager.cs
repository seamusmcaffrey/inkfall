using UnityEngine;

/// <summary>
/// Perlin-noise-based camera shake with exponential decay.
/// Supports single-pop shake and escalating combo shake.
/// Driven by JuiceConfigSO — no magic numbers.
/// </summary>
[DisallowMultipleComponent]
public class ScreenShakeManager : MonoBehaviour
{
    private JuiceConfigSO _config;
    private Transform _target;
    private Vector3 _basePosition;

    private float _currentIntensity;
    private float _maxIntensity;
    private float _elapsed;
    private float _duration;
    private float _perlinSeedX;
    private float _perlinSeedY;
    private bool _isShaking;

    private void Awake()
    {
        _target = Camera.main != null ? Camera.main.transform : transform;
        _basePosition = _target.position;
        RandomizePerlinSeeds();
    }

    public void SetConfig(JuiceConfigSO config)
    {
        _config = config;
    }

    /// <summary>
    /// Triggers a single shake at the given intensity (e.g. pop or paint).
    /// If already shaking, the stronger intensity wins and duration resets.
    /// </summary>
    public void Shake(float intensity)
    {
        if (_target == null || _config == null || !_config.screenShakeEnabled)
        {
            return;
        }

        ApplyShake(intensity, _config.shakeDuration);
    }

    /// <summary>
    /// Triggers a combo-escalating shake that grows with combo count.
    /// </summary>
    public void ShakeForCombo(float baseIntensity, int comboCount)
    {
        if (_target == null || _config == null || !_config.screenShakeEnabled)
        {
            return;
        }

        int clampedCombo = Mathf.Min(comboCount, _config.shakeComboEscalationCap);
        float escalation = 1f + (clampedCombo - 1) * _config.shakeComboEscalationPerHit;
        float intensity = baseIntensity * escalation;
        float duration = _config.shakeDuration * Mathf.Lerp(1f, 1.5f, (float)clampedCombo / _config.shakeComboEscalationCap);

        ApplyShake(intensity, duration);
    }

    private void ApplyShake(float intensity, float duration)
    {
        if (intensity > _currentIntensity || !_isShaking)
        {
            _maxIntensity = intensity;
            _currentIntensity = intensity;
            _duration = duration;
            _elapsed = 0f;
            _isShaking = true;
            RandomizePerlinSeeds();
        }
    }

    private void LateUpdate()
    {
        if (!_isShaking || _target == null)
        {
            return;
        }

        _elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        // Exponential decay for smooth falloff
        float decayExponent = _config != null ? _config.shakeDecayExponent : GameConstants.SHAKE_DECAY_EXPONENT;
        _currentIntensity = _maxIntensity * (1f - Mathf.Pow(t, decayExponent));

        if (_currentIntensity <= 0.001f)
        {
            FinishShake();
            return;
        }

        float perlinSpeed = _config != null ? _config.shakePerlinSpeed : GameConstants.SHAKE_PERLIN_SPEED;
        float time = _elapsed * perlinSpeed;

        // Perlin noise gives smooth, organic displacement instead of random jitter
        float offsetX = (Mathf.PerlinNoise(_perlinSeedX + time, 0f) - 0.5f) * 2f * _currentIntensity;
        float offsetY = (Mathf.PerlinNoise(0f, _perlinSeedY + time) - 0.5f) * 2f * _currentIntensity;

        _target.position = _basePosition + new Vector3(offsetX, offsetY, 0f);
    }

    private void FinishShake()
    {
        _isShaking = false;
        _currentIntensity = 0f;
        _target.position = _basePosition;
    }

    private void RandomizePerlinSeeds()
    {
        _perlinSeedX = Random.Range(0f, 100f);
        _perlinSeedY = Random.Range(0f, 100f);
    }

    /// <summary>
    /// Updates the base position if the camera moves for other reasons.
    /// </summary>
    public void SetBasePosition(Vector3 position)
    {
        _basePosition = position;
        if (!_isShaking && _target != null)
        {
            _target.position = _basePosition;
        }
    }

    private void OnDisable()
    {
        if (_isShaking && _target != null)
        {
            FinishShake();
        }
    }
}
