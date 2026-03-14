using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    private const string CanvasName = "HUDCanvas";

    private Text _scoreText;
    private Text _targetText;
    private Text _dartsText;
    private Text _messageText;

    private void Awake()
    {
        CreateHud();
    }

    public void UpdateDisplay(int score, int target, int darts, BalloonGameManager.GameState state)
    {
        EnsureHud();
        _scoreText.text = score.ToString();
        _targetText.text = $"Target: {target}";
        _dartsText.text = $"Darts: {darts}";
        _scoreText.color = score >= target ? new Color(0.3f, 0.9f, 0.3f) : Color.white;

        switch (state)
        {
            case BalloonGameManager.GameState.RoomCleared:
                _messageText.gameObject.SetActive(true);
                _messageText.text = "CLEARED!\nPress R to restart";
                _messageText.color = new Color(0.3f, 0.9f, 0.3f);
                break;

            case BalloonGameManager.GameState.RoomFailed:
                _messageText.gameObject.SetActive(true);
                _messageText.text = "FAILED\nPress R to restart";
                _messageText.color = new Color(0.9f, 0.3f, 0.3f);
                break;

            default:
                _messageText.gameObject.SetActive(false);
                break;
        }
    }

    private void CreateHud()
    {
        if (_scoreText != null && _targetText != null && _dartsText != null && _messageText != null)
        {
            return;
        }

        Transform existingCanvas = transform.Find(CanvasName);
        if (existingCanvas != null)
        {
            if (Application.isPlaying)
            {
                Destroy(existingCanvas.gameObject);
            }
            else
            {
                DestroyImmediate(existingCanvas.gameObject);
            }
        }

        var canvasObject = new GameObject(CanvasName);
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);

        canvasObject.AddComponent<GraphicRaycaster>();

        _scoreText = CreateText(canvasObject.transform, "Score", TextAnchor.UpperLeft, 42, Color.white);
        RectTransform scoreRect = _scoreText.rectTransform;
        scoreRect.anchorMin = new Vector2(0f, 1f);
        scoreRect.anchorMax = new Vector2(0f, 1f);
        scoreRect.pivot = new Vector2(0f, 1f);
        scoreRect.anchoredPosition = new Vector2(30f, -30f);
        scoreRect.sizeDelta = new Vector2(400f, 60f);

        _targetText = CreateText(canvasObject.transform, "Target", TextAnchor.UpperRight, 32, new Color(0.7f, 0.7f, 0.7f));
        RectTransform targetRect = _targetText.rectTransform;
        targetRect.anchorMin = new Vector2(1f, 1f);
        targetRect.anchorMax = new Vector2(1f, 1f);
        targetRect.pivot = new Vector2(1f, 1f);
        targetRect.anchoredPosition = new Vector2(-30f, -30f);
        targetRect.sizeDelta = new Vector2(400f, 60f);

        _dartsText = CreateText(canvasObject.transform, "Darts", TextAnchor.UpperCenter, 36, new Color(0.9f, 0.7f, 0.3f));
        RectTransform dartsRect = _dartsText.rectTransform;
        dartsRect.anchorMin = new Vector2(0.5f, 1f);
        dartsRect.anchorMax = new Vector2(0.5f, 1f);
        dartsRect.pivot = new Vector2(0.5f, 1f);
        dartsRect.anchoredPosition = new Vector2(0f, -30f);
        dartsRect.sizeDelta = new Vector2(300f, 60f);

        _messageText = CreateText(canvasObject.transform, "Message", TextAnchor.MiddleCenter, 64, Color.white);
        RectTransform messageRect = _messageText.rectTransform;
        messageRect.anchorMin = new Vector2(0.5f, 0.5f);
        messageRect.anchorMax = new Vector2(0.5f, 0.5f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.anchoredPosition = Vector2.zero;
        messageRect.sizeDelta = new Vector2(800f, 200f);
        _messageText.gameObject.SetActive(false);
    }

    private Text CreateText(Transform parent, string name, TextAnchor alignment, int fontSize, Color color)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        textObject.AddComponent<RectTransform>();

        var text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        var outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        return text;
    }

    private void EnsureHud()
    {
        if (_scoreText == null || _targetText == null || _dartsText == null || _messageText == null)
        {
            CreateHud();
        }
    }
}
