using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Transitional room intro overlay.
/// </summary>
[DisallowMultipleComponent]
public class RoomIntroScreen : MonoBehaviour
{
    private CanvasGroup _group;
    private TextMeshProUGUI _label;

    private void Awake()
    {
        EnsureUi();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
    }

    public void Show(RoomConfig config)
    {
        EnsureUi();
        _label.text = $"ROOM {config.roomNumber}\n{config.roomName.ToUpperInvariant()}\nTARGET {config.targetScore}";
        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        _group.alpha = 1f;
        yield return new WaitForSecondsRealtime(1.1f);
        float elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.4f);
            yield return null;
        }
    }

    private void EnsureUi()
    {
        if (_label == null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        Canvas canvas = ComponentUtility.EnsureComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 430;
        _group = ComponentUtility.EnsureComponent<CanvasGroup>(gameObject);
        CanvasScaler scaler = ComponentUtility.EnsureComponent<CanvasScaler>(gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameConstants.UI_REFERENCE_RESOLUTION;
        scaler.matchWidthOrHeight = 0.5f;
        ComponentUtility.EnsureComponent<GraphicRaycaster>(gameObject);

        if (_label == null)
        {
            GameObject go = new("Label");
            go.transform.SetParent(transform, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, 300f);
            _label = go.AddComponent<TextMeshProUGUI>();
            _label.alignment = TextAlignmentOptions.Center;
            _label.fontSize = 56f;
            _label.raycastTarget = false;
        }
    }
}
