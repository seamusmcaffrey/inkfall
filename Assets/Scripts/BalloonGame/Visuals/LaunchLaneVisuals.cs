using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Decorative lane visuals and remaining-darts pips.
/// </summary>
[DisallowMultipleComponent]
public class LaunchLaneVisuals : MonoBehaviour
{
    private readonly List<SpriteRenderer> _dartPips = new();

    private void OnEnable()
    {
        EventBus.Subscribe<DartsRemainingChangedEvent>(HandleDartsChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DartsRemainingChangedEvent>(HandleDartsChanged);
    }

    private void Awake()
    {
        for (int index = 0; index < GameConstants.STARTING_DARTS + 4; index++)
        {
            GameObject pip = new($"DartPip_{index}");
            pip.transform.SetParent(transform, false);
            pip.transform.localPosition = new Vector3(-1.2f + index * 0.35f, -1.4f, 0f);
            SpriteRenderer renderer = pip.AddComponent<SpriteRenderer>();
            renderer.color = UIColors.DartBlue;
            _dartPips.Add(renderer);
        }
    }

    private void HandleDartsChanged(DartsRemainingChangedEvent evt)
    {
        for (int index = 0; index < _dartPips.Count; index++)
        {
            _dartPips[index].color = index < evt.DartsRemaining ? UIColors.DartBlue : new Color(1f, 1f, 1f, 0.12f);
        }
    }
}
