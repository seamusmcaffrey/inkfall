using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Global UI tuning config for HUD and menu motion.
/// </summary>
[CreateAssetMenu(fileName = "UIConfig", menuName = "INKSHOT/UI Config")]
public class UIConfigSO : ScriptableObject
{
    private const string AssetPath = "Assets/ScriptableObjects/UIConfig.asset";
    private static UIConfigSO _instance;

    [Header("Score Counter")]
    public float scoreCountSpeed = 2000f;
    public float scoreCountMinDuration = 0.15f;

    [Header("Progress Bar")]
    public float progressBarLerpSpeed = 4f;
    public float progressBarGlowThreshold = 0.75f;
    public float progressBarGlowPulseSpeed = 2f;

    [Header("Combo Display")]
    public float comboPunchDuration = 0.25f;
    public float comboPunchScale = 1.4f;
    public float comboFadeDuration = 0.5f;
    public float comboTimerWidth = 132f;
    public float comboTimerHeight = 8f;
    public float comboFeedbackOffsetY = 96f;
    public float comboCalloutWidth = 240f;
    public float comboCalloutHeight = 44f;
    public float comboCalloutDuration = 1.2f;
    public float comboCalloutFadeDuration = 0.18f;
    public float comboCalloutPunchScale = 1.08f;
    public float comboCalloutTitleSize = 8.5f;
    public float comboCalloutBodySize = 12.5f;
    public float comboCalloutRowSize = 7.5f;

    [Header("Floating Score Text")]
    public float floatingTextRisePx = 1.5f;
    public float floatingTextDuration = 0.8f;
    public float floatingTextFontSize = 8f;
    public int floatingTextPoolSize = 10;
    public float floatingTextPunchScale = 1.5f;
    public float floatingTextPunchDuration = 0.12f;
    public float floatingTextComboSizeBoost = 0.08f;
    public int floatingTextComboSizeCap = 8;

    [Header("Screen Transitions")]
    public float transitionFadeInDuration = 0.3f;
    public float transitionFadeOutDuration = 0.2f;
    public float cardSlideInDuration = 0.35f;
    public float cardSlideOvershoot = 1.15f;

    [Header("Message Display")]
    public float messageFontSize = GameConstants.MESSAGE_FONT_SIZE;
    public float messagePunchScale = 1.2f;
    public float messageAppearDuration = 0.3f;

    public static UIConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = LoadAsset() ?? CreateInstance<UIConfigSO>();
                _instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }

            return _instance;
        }
    }

    private static UIConfigSO LoadAsset()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<UIConfigSO>(AssetPath);
#else
        return null;
#endif
    }
}
