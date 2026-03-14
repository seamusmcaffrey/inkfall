using System.Collections;
using UnityEngine;

/// <summary>
/// Lightweight camera shake without Cinemachine dependency.
/// </summary>
[DisallowMultipleComponent]
public class ScreenShakeManager : MonoBehaviour
{
    private JuiceConfigSO _config;
    private Transform _target;
    private Vector3 _basePosition;

    private void Awake()
    {
        _target = Camera.main != null ? Camera.main.transform : transform;
        _basePosition = _target.position;
    }

    public void SetConfig(JuiceConfigSO config)
    {
        _config = config;
    }

    public void Shake(float intensity)
    {
        if (_target == null)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(intensity));
    }

    private IEnumerator ShakeRoutine(float intensity)
    {
        float duration = _config != null ? _config.shakeDuration : 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _target.position = _basePosition + (Vector3)(Random.insideUnitCircle * intensity);
            yield return null;
        }

        _target.position = _basePosition;
    }
}
