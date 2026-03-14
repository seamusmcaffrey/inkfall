using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Overlay settings panel for audio and haptics.
/// </summary>
[DisallowMultipleComponent]
public partial class SettingsPanel : MonoBehaviour
{
    private const float SliderWidth = 300f;
    private CanvasGroup _group;
    private Slider _masterSlider;
    private Slider _sfxSlider;
    private Slider _musicSlider;
    private Toggle _hapticsToggle;

    private void Awake()
    {
        BuildUi();
        Hide();
    }

    public void Show()
    {
        LoadFromSave();
        gameObject.SetActive(true);
        _group.alpha = 1f;
        _group.blocksRaycasts = true;
    }

    public void Hide()
    {
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void LoadFromSave()
    {
        SaveData data = SaveManager.Instance.Data;
        _masterSlider.value = data.masterVolume;
        _sfxSlider.value = data.sfxVolume;
        _musicSlider.value = data.musicVolume;
        _hapticsToggle.isOn = data.hapticsEnabled;
    }

    private void Save()
    {
        SaveManager.Instance.SetVolumes(_masterSlider.value, _sfxSlider.value, _musicSlider.value, SaveManager.Instance.Data.uiVolume);
        SaveManager.Instance.SetHapticsEnabled(_hapticsToggle.isOn);
        AudioManager.Instance.ApplySavedVolumes();
    }
}
