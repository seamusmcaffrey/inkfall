using UnityEngine;

/// <summary>
/// Spark effect at balloon impact.
/// </summary>
[DisallowMultipleComponent]
public class ImpactSparkVFX : MonoBehaviour
{
    private ObjectPool _owningPool;
    private ParticleSystem _system;
    private Coroutine _returnRoutine;

    public void SetOwningPool(ObjectPool owningPool)
    {
        _owningPool = owningPool;
    }

    public void Play(JuiceConfigSO config)
    {
        _system = VFXFactory.EnsureImpactSparkSystem(transform, _system, config);
        _system.transform.localPosition = Vector3.zero;
        _system.Play(true);
        BeginReturn(config.sparkParticleLifetime + 0.1f);
    }

    private void BeginReturn(float delay)
    {
        if (_returnRoutine != null)
        {
            StopCoroutine(_returnRoutine);
        }

        _returnRoutine = StartCoroutine(ReturnAfterDelay(delay));
    }

    private System.Collections.IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        _system?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (_owningPool != null)
        {
            _owningPool.Return(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }

        _returnRoutine = null;
    }
}
