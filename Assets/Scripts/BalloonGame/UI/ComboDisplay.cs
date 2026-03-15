using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animated combo readout with escalating size, color tiers, and glow effect.
/// Size grows from base 48pt (+4pt per combo, capped at 96pt).
/// Color tiers: white -> yellow -> orange -> hot pink.
/// </summary>
[DisallowMultipleComponent]
public class ComboDisplay : MonoBehaviour
{
    private const float BaseFontSize = 32f;
    private const float FontSizePerCombo = 3f;
    private const float MaxFontSize = 64f;
    private const float PunchScaleMax = 1.5f;
    private const float GlowDilateAmount = 0.3f;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private TextMeshProUGUI _label;
    private Image _glowBackground;
    private float _fadeVelocity;
    private float _punchTimer;
    private int _currentCombo;

    private void Awake()
    {
        EnsureUi();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ComboChangedEvent>(HandleComboChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ComboChangedEvent>(HandleComboChanged);
    }

    private void Update()
    {
        if (_canvasGroup == null) return;

        if (_punchTimer > 0f)
        {
            _punchTimer -= Time.unscaledDeltaTime;
            float normalized = 1f - Mathf.Clamp01(_punchTimer / UIConfigSO.Instance.comboPunchDuration);
            // Dramatic snap-back: overshoot to PunchScaleMax then ease to 1.0
            float scale = Mathf.Lerp(PunchScaleMax, 1f, EaseOutBack(normalized));
            _rectTransform.localScale = Vector3.one * scale;
        }

        if (_canvasGroup.alpha > 0f && _label != null && _label.text.Length > 0)
        {
            _canvasGroup.alpha = Mathf.SmoothDamp(
                _canvasGroup.alpha,
                0f,
                ref _fadeVelocity,
                UIConfigSO.Instance.comboFadeDuration);

            // Fade glow with text
            if (_glowBackground != null)
            {
                Color glow = _glowBackground.color;
                _glowBackground.color = new Color(glow.r, glow.g, glow.b, _canvasGroup.alpha * glow.a);
            }
        }
    }

    public void AttachTo(Transform parent)
    {
        transform.SetParent(parent, false);
        EnsureUi();
    }

    private void HandleComboChanged(ComboChangedEvent evt)
    {
        EnsureUi();
        if (evt.ComboCount <= 1 || evt.WasReset)
        {
            _label.text = string.Empty;
            if (_glowBackground != null) _glowBackground.color = Color.clear;
            return;
        }

        _currentCombo = evt.ComboCount;

        // Escalating font size: base + 4pt per combo, capped
        float fontSize = Mathf.Min(BaseFontSize + FontSizePerCombo * _currentCombo, MaxFontSize);
        _label.fontSize = fontSize;

        _label.text = $"x{_currentCombo}";
        _label.color = UIColors.GetComboColor(_currentCombo);

        // TMP outline for readability glow
        _label.outlineWidth = GlowDilateAmount;
        _label.outlineColor = UIColors.GetComboGlowColor(_currentCombo);

        // Glow background behind text
        if (_glowBackground != null)
        {
            Color glowColor = UIColors.GetComboGlowColor(_currentCombo);
            _glowBackground.color = glowColor;
        }

        _canvasGroup.alpha = 1f;
        _fadeVelocity = 0f;
        _punchTimer = UIConfigSO.Instance.comboPunchDuration;
        _rectTransform.localScale = Vector3.one * PunchScaleMax;
    }

    private void EnsureUi()
    {
        if (_rectTransform == null)
            _rectTransform = ComponentUtility.EnsureComponent<RectTransform>(gameObject);

        if (_canvasGroup == null)
            _canvasGroup = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);

        if (_glowBackground == null)
        {
            GameObject glowGo = new("ComboGlow");
            glowGo.transform.SetParent(transform, false);
            RectTransform glowRect = glowGo.AddComponent<RectTransform>();
            glowRect.anchorMin = glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.sizeDelta = new Vector2(200f, 80f);
            _glowBackground = glowGo.AddComponent<Image>();
            _glowBackground.color = Color.clear;
            _glowBackground.raycastTarget = false;
        }

        if (_label == null)
        {
            _label = ComponentUtility.EnsureComponent<TextMeshProUGUI>(gameObject);
            _label.fontSize = BaseFontSize;
            _label.fontStyle = FontStyles.Bold;
            _label.alignment = TextAlignmentOptions.Center;
            _label.text = string.Empty;
            _label.raycastTarget = false;
            _label.enableWordWrapping = false;
        }
    }

    /// <summary>
    /// Ease-out-back curve for snappy overshoot animation.
    /// </summary>
    private static float EaseOutBack(float t)
    {
        const float overshoot = 1.70158f;
        float f = t - 1f;
        return 1f + f * f * ((overshoot + 1f) * f + overshoot);
    }
}
