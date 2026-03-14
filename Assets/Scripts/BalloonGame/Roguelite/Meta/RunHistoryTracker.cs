using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Read-only adapter for recent run history.
/// </summary>
[DisallowMultipleComponent]
public class RunHistoryTracker : MonoBehaviour
{
    public IReadOnlyList<RunHistoryRecord> GetHistory()
    {
        return SaveManager.Instance.Data.runHistory;
    }
}
