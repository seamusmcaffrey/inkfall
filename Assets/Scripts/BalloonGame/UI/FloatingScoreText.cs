using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pooled floating score popups that rise from popped balloon positions.
/// Features punch-scale on spawn, point-value-based color and sizing,
/// and combo-aware scaling for dramatic high-value hits.
/// </summary>
[DisallowMultipleComponent]
public class FloatingScoreText : MonoBehaviour
{
    private const float PointScaleBase = 1f;
    private const float PointScalePerHundred = 0.08f;
    private const float PointScaleMax = 2.2f;
    private const int HighValueThreshold = 300;

    private sealed class ActiveText
    {
        public RectTransform Rect;
        public CanvasGroup Group;
        public TextMeshProUGUI Label;
        public Vector3 WorldPosition;
        public float Elapsed;
        public float BaseScale;
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
            _camera = Camera.main;

        UIConfigSO ui = UIConfigSO.Instance;
        float duration = ui.floatingTextDuration;
        float punchDuration = ui.floatingTextPunchDuration;

        for (int index = _active.Count - 1; index >= 0; index--)
        {
            ActiveText item = _active[index];
            item.Elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(item.Elapsed / duration);

            // Ease-out rise: fast at start, decelerates
            float riseCurve = 1f - (1f - normalized) * (1f - normalized);
            Vector3 screenPosition = _camera != null
                ? _camera.WorldToScreenPoint(item.WorldPosition + Vector3.up * ui.floatingTextRisePx * riseCurve)
                : Vector3.zero;

            item.Rect.position = screenPosition;

            // Alpha: hold full for first 40%, then fade out with ease
            float alphaStart = 0.4f;
            float alpha = normalized < alphaStart
                ? 1f
                : 1f - Mathf.Clamp01((normalized - alphaStart) / (1f - alphaStart));
            item.Group.alpha = alpha;

            // Scale: dramatic punch on spawn, settle to base
            float scale;
            if (item.Elapsed < punchDuration)
            {
                float punchT = item.Elapsed / punchDuration;
                float punchEase = Mathf.Sin(punchT * Mathf.PI);
                scale = Mathf.Lerp(item.BaseScale, item.BaseScale * ui.floatingTextPunchScale, punchEase);
            }
            else
            {
                // Subtle grow over lifetime for drama
                float lifeT = (item.Elapsed - punchDuration) / Mathf.Max(duration - punchDuration, 0.01f);
                scale = item.BaseScale * (1f + lifeT * 0.05f);
            }

            item.Rect.localScale = Vector3.one * scale;

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
            WarmPool();

        if (_pool.Count == 0)
            return;

        UIConfigSO ui = UIConfigSO.Instance;
        int comboClamped = Mathf.Min(evt.ComboCount, ui.floatingTextComboSizeCap);
        float comboBoost = comboClamped * ui.floatingTextComboSizeBoost;

        // Point-value-based scaling: bigger numbers = bigger text
        int absPoints = Mathf.Abs(evt.FinalPoints);
        float pointScale = Mathf.Min(
            PointScaleBase + (absPoints / 100f) * PointScalePerHundred,
            PointScaleMax);

        ActiveText item = _pool.Dequeue();
        item.WorldPosition = evt.WorldPosition + Vector3.forward * GameConstants.FLOATING_SCORE_Z_OFFSET;
        item.Elapsed = 0f;
        item.BaseScale = pointScale + comboBoost;
        item.Group.alpha = 1f;
        item.Label.text = evt.FinalPoints >= 0 ? $"+{evt.FinalPoints}" : evt.FinalPoints.ToString();

        // High-value pops get gold/premium color; standard pops use balloon color
        item.Label.color = absPoints >= HighValueThreshold
            ? UIColors.GetFloatingScoreColor(absPoints)
            : UIColors.GetBalloonTextColor(evt.BalloonColor);

        // Bold for high-value hits
        item.Label.fontStyle = absPoints >= HighValueThreshold ? FontStyles.Bold : FontStyles.Normal;

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
            _pool.Enqueue(new ActiveText { Rect = rect, Group = group, Label = label, BaseScale = 1f });
        }
    }
}
