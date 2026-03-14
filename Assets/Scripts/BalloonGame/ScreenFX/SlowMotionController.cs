using System.Collections;
using UnityEngine;

/// <summary>
/// Handles short bursts of slow motion using realtime waits.
/// </summary>
[DisallowMultipleComponent]
public class SlowMotionController : MonoBehaviour
{
    private JuiceConfigSO _config;

    public void SetConfig(JuiceConfigSO config)
    {
        _config = config;
    }

    public void Trigger(float durationMultiplier = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(SlowMotionRoutine(durationMultiplier));
    }

    private IEnumerator SlowMotionRoutine(float durationMultiplier)
    {
        if (_config == null)
        {
            _config = JuiceConfigSO.Instance;
        }

        float targetScale = _config.slowMotionTimeScale;
        float elapsed = 0f;
        while (elapsed < _config.slowMotionRampUpTime)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1f, targetScale, elapsed / _config.slowMotionRampUpTime);
            yield return null;
        }

        Time.timeScale = targetScale;
        yield return new WaitForSecondsRealtime(_config.slowMotionDuration * durationMultiplier);

        elapsed = 0f;
        while (elapsed < _config.slowMotionRampDownTime)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(targetScale, 1f, elapsed / _config.slowMotionRampDownTime);
            yield return null;
        }

        Time.timeScale = 1f;
    }
}
