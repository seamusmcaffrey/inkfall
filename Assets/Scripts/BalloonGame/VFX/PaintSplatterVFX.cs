using UnityEngine;

/// <summary>
/// Paint burst and drip spawning for paint balloons.
/// </summary>
[DisallowMultipleComponent]
public class PaintSplatterVFX : MonoBehaviour
{
    private ObjectPool _owningPool;
    private ParticleSystem _system;
    private PaintDripEffect[] _drips;
    private Coroutine _returnRoutine;

    private void Awake()
    {
        EnsureDrips();
    }

    public void SetOwningPool(ObjectPool owningPool)
    {
        _owningPool = owningPool;
    }

    public void Play(Color color, JuiceConfigSO config)
    {
        _system = VFXFactory.EnsurePaintSplatterSystem(transform, _system, config, color);
        _system.transform.localPosition = Vector3.zero;
        _system.Play(true);

        EnsureDrips();
        int activeDrips = Mathf.Min(config.paintDripCount, _drips.Length);
        for (int index = 0; index < _drips.Length; index++)
        {
            _drips[index].gameObject.SetActive(index < activeDrips);
            if (index < activeDrips)
            {
                _drips[index].Play(color, config, Random.insideUnitSphere * 0.2f);
            }
        }

        BeginReturn(Mathf.Max(config.paintParticleLifetime, config.paintDripLifetime) + 0.2f);
    }

    private void EnsureDrips()
    {
        if (_drips != null)
        {
            return;
        }

        _drips = new PaintDripEffect[GameConstants.MAX_PAINT_DRIPS];
        for (int index = 0; index < _drips.Length; index++)
        {
            GameObject drip = new($"PaintDrip_{index}");
            drip.transform.SetParent(transform, false);
            _drips[index] = drip.AddComponent<PaintDripEffect>();
            drip.SetActive(false);
        }
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
        if (_drips != null)
        {
            foreach (PaintDripEffect drip in _drips)
            {
                if (drip != null)
                {
                    drip.gameObject.SetActive(false);
                }
            }
        }

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
