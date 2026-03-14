using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton audio manager with pooled one-shot sources.
/// </summary>
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    private readonly Queue<AudioSource> _sfxPool = new();

    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _uiSource;
    [SerializeField] private int _poolSize = GameConstants.AUDIO_SOURCE_POOL_SIZE;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[AudioManager]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<AudioManager>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureChannels();
    }

    private void Start()
    {
        ApplySavedVolumes();
    }

    public void ApplySavedVolumes()
    {
        SaveData data = SaveManager.Instance.Data;
        AudioListener.volume = data.masterVolume;
        _musicSource.volume = data.musicVolume;
        _uiSource.volume = data.uiVolume;
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource source = GetSource();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume * SaveManager.Instance.Data.sfxVolume);
        source.loop = false;
        source.Play();
        StartCoroutine(ReturnWhenFinished(source));
    }

    public void PlayUi(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            return;
        }

        _uiSource.Stop();
        _uiSource.clip = clip;
        _uiSource.volume = Mathf.Clamp01(volume * SaveManager.Instance.Data.uiVolume);
        _uiSource.Play();
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null)
        {
            return;
        }

        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    private System.Collections.IEnumerator ReturnWhenFinished(AudioSource source)
    {
        yield return new WaitWhile(() => source != null && source.isPlaying);
        if (source != null)
        {
            source.clip = null;
            _sfxPool.Enqueue(source);
        }
    }

    private AudioSource GetSource()
    {
        EnsureChannels();
        return _sfxPool.Count > 0 ? _sfxPool.Dequeue() : CreatePooledSource();
    }

    private void EnsureChannels()
    {
        if (_musicSource == null)
        {
            _musicSource = CreateChannel("Music");
            _musicSource.loop = true;
        }

        if (_uiSource == null)
        {
            _uiSource = CreateChannel("UI");
        }

        while (_sfxPool.Count < _poolSize)
        {
            _sfxPool.Enqueue(CreatePooledSource());
        }
    }

    private AudioSource CreateChannel(string channelName)
    {
        var channel = new GameObject(channelName);
        channel.transform.SetParent(transform, false);
        return channel.AddComponent<AudioSource>();
    }

    private AudioSource CreatePooledSource()
    {
        var channel = CreateChannel("SFX");
        channel.playOnAwake = false;
        return channel;
    }
}
