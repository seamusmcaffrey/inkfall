using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps stopped darts organized and caps the number left in the scene.
/// </summary>
[DisallowMultipleComponent]
public class StuckDartManager : MonoBehaviour
{
    [SerializeField] private int _maxStuckDarts = 18;
    private readonly Queue<DartController> _stuckDarts = new();

    private void OnEnable()
    {
        DartController.OnDartFinished += HandleDartFinished;
    }

    private void OnDisable()
    {
        DartController.OnDartFinished -= HandleDartFinished;
    }

    private void HandleDartFinished(DartController dart)
    {
        if (dart == null || dart.State != DartController.DartState.Stopped)
        {
            return;
        }

        dart.transform.SetParent(transform, true);
        _stuckDarts.Enqueue(dart);
        while (_stuckDarts.Count > _maxStuckDarts)
        {
            DartController oldest = _stuckDarts.Dequeue();
            if (oldest != null)
            {
                oldest.gameObject.SetActive(false);
            }
        }
    }
}
