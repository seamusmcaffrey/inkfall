using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animated combo readout used by the in-game HUD.
/// </summary>
[DisallowMultipleComponent]
public class ComboDisplay : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private TextMeshProUGUI _label;
    private float _fadeVelocity;
    private float _punchTimer;

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
        if (_canvasGroup == null)
        {
            return;
        }

        if (_punchTimer > 0f)
        {
            _punchTimer -= Time.unscaledDeltaTime;
            float normalized = 1f - Mathf.Clamp01(_punchTimer / UIConfigSO.Instance.comboPunchDuration);
            float scale = Mathf.Lerp(UIConfigSO.Instance.comboPunchScale, 1f, normalized);
            _rectTransform.localScale = Vector3.one * scale;
        }

        if (_canvasGroup.alpha > 0f && _label != null && _label.text.Length > 0)
        {
            _canvasGroup.alpha = Mathf.SmoothDamp(
                _canvasGroup.alpha,
                0f,
                ref _fadeVelocity,
                UIConfigSO.Instance.comboFadeDuration);
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
            return;
        }

        _label.text = $"x{evt.ComboCount}";
        _label.color = UIColors.GetComboColor(evt.ComboCount);
        _canvasGroup.alpha = 1f;
        _fadeVelocity = 0f;
        _punchTimer = UIConfigSO.Instance.comboPunchDuration;
        _rectTransform.localScale = Vector3.one * UIConfigSO.Instance.comboPunchScale;
    }

    private void EnsureUi()
    {
        if (_rectTransform == null)
        {
            _rectTransform = ComponentUtility.EnsureComponent<RectTransform>(gameObject);
        }

        if (_canvasGroup == null)
        {
            _canvasGroup = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);
        }

        if (_label == null)
        {
            _label = ComponentUtility.EnsureComponent<TextMeshProUGUI>(gameObject);
            _label.fontSize = 56f;
            _label.alignment = TextAlignmentOptions.Center;
            _label.text = string.Empty;
            _label.raycastTarget = false;
        }
    }
}
