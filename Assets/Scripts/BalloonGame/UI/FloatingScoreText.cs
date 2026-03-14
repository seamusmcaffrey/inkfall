using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pooled floating score popups that rise from popped balloon positions.
/// </summary>
[DisallowMultipleComponent]
public class FloatingScoreText : MonoBehaviour
{
    private sealed class ActiveText
    {
        public RectTransform Rect;
        public CanvasGroup Group;
        public TextMeshProUGUI Label;
        public Vector3 WorldPosition;
        public float Elapsed;
    }

    private readonly Queue<ActiveText> _pool = new();
    private readonly List<ActiveText> _active = new();
    private Canvas _canvas;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _canvas = GetComponentInParent<Canvas>();
        WarmPool();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BalloonScoredEvent>(HandleBalloonScored);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BalloonScoredEvent>(HandleBalloonScored);
    }

    private void Update()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        for (int index = _active.Count - 1; index >= 0; index--)
        {
            ActiveText item = _active[index];
            item.Elapsed += Time.unscaledDeltaTime;
            float duration = UIConfigSO.Instance.floatingTextDuration;
            float normalized = Mathf.Clamp01(item.Elapsed / duration);
            Vector3 screenPosition = _camera != null
                ? _camera.WorldToScreenPoint(item.WorldPosition + Vector3.up * UIConfigSO.Instance.floatingTextRisePx * normalized)
                : Vector3.zero;

            item.Rect.position = screenPosition;
            item.Group.alpha = 1f - normalized;
            item.Rect.localScale = Vector3.one * Mathf.Lerp(1f, 1.15f, normalized * 0.3f);

            if (normalized >= 1f)
            {
                item.Group.alpha = 0f;
                item.Label.text = string.Empty;
                _active.RemoveAt(index);
                _pool.Enqueue(item);
            }
        }
    }

    public void AttachTo(Transform parent)
    {
        transform.SetParent(parent, false);
        _canvas = GetComponentInParent<Canvas>();
        WarmPool();
    }

    private void HandleBalloonScored(BalloonScoredEvent evt)
    {
        if (_pool.Count == 0)
        {
            WarmPool();
        }

        if (_pool.Count == 0)
        {
            return;
        }

        ActiveText item = _pool.Dequeue();
        item.WorldPosition = evt.WorldPosition + Vector3.forward * GameConstants.FLOATING_SCORE_Z_OFFSET;
        item.Elapsed = 0f;
        item.Group.alpha = 1f;
        item.Label.text = evt.FinalPoints >= 0 ? $"+{evt.FinalPoints}" : evt.FinalPoints.ToString();
        item.Label.color = UIColors.GetBalloonTextColor(evt.BalloonColor);
        _active.Add(item);
    }

    private void WarmPool()
    {
        int desired = UIConfigSO.Instance.floatingTextPoolSize;
        while (_pool.Count + _active.Count < desired)
        {
            GameObject go = new("FloatingScore");
            go.transform.SetParent(transform, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            CanvasGroup group = go.AddComponent<CanvasGroup>();
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.fontSize = UIConfigSO.Instance.floatingTextFontSize * 10f;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            group.alpha = 0f;
            _pool.Enqueue(new ActiveText { Rect = rect, Group = group, Label = label });
        }
    }
}
