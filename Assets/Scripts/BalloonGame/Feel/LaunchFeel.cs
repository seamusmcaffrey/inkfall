using System.Collections;
using UnityEngine;

/// <summary>
/// Adds small launch-scale pulses around a throw.
/// </summary>
[DisallowMultipleComponent]
public class LaunchFeel : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private void Awake()
    {
        ComponentUtility.EnsureReference(ref _target, transform);
    }

    private void OnEnable()
    {
        ComponentUtility.EnsureReference(ref _target, transform);
        EventBus.Subscribe<DartLaunchedEvent>(HandleLaunch);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DartLaunchedEvent>(HandleLaunch);
    }

    private void HandleLaunch(DartLaunchedEvent evt)
    {
        if (ComponentUtility.EnsureReference(ref _target, transform) == null)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        Transform target = ComponentUtility.EnsureReference(ref _target, transform);
        if (target == null)
        {
            yield break;
        }

        Vector3 startScale = target.localScale;
        Vector3 peakScale = startScale * 1.04f;
        float elapsed = 0f;
        while (elapsed < 0.08f)
        {
            elapsed += Time.unscaledDeltaTime;
            if (target == null)
            {
                yield break;
            }

            target.localScale = Vector3.Lerp(startScale, peakScale, elapsed / 0.08f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 0.16f)
        {
            elapsed += Time.unscaledDeltaTime;
            if (target == null)
            {
                yield break;
            }

            target.localScale = Vector3.Lerp(peakScale, startScale, elapsed / 0.16f);
            yield return null;
        }

        if (target != null)
        {
            target.localScale = startScale;
        }
    }
}
