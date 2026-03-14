using UnityEngine;

/// <summary>
/// Burst particles for a standard balloon pop.
/// </summary>
[DisallowMultipleComponent]
public class BalloonPopVFX : MonoBehaviour
{
    private ObjectPool _owningPool;
    private ParticleSystem _system;
    private Coroutine _returnRoutine;

    public void SetOwningPool(ObjectPool owningPool)
    {
        _owningPool = owningPool;
    }

    public void Play(Color color, JuiceConfigSO config)
    {
        _system = VFXFactory.EnsureBalloonPopSystem(transform, _system, config, color);
        _system.transform.localPosition = Vector3.zero;
        _system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _system.Play(true);
        BeginReturn(config.popParticleLifetime + 0.1f);
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
