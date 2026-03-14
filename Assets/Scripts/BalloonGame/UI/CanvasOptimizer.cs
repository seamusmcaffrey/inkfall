#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small editor audit tool for UI raycast and canvas usage.
/// </summary>
public static class CanvasOptimizer
{
    [MenuItem("INKSHOT/Audit Canvases")]
    public static void Audit()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            int raycastTargets = 0;
            foreach (Graphic graphic in canvas.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic.raycastTarget)
                {
                    raycastTargets++;
                }
            }

            Debug.Log($"[CanvasOptimizer] {canvas.name}: graphics={canvas.GetComponentsInChildren<Graphic>(true).Length}, raycastTargets={raycastTargets}");
        }
    }
}
#endif
