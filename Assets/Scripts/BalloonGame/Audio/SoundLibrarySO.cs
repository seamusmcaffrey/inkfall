using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Holds procedural game audio clips.
/// </summary>
[CreateAssetMenu(fileName = "SoundLibrary", menuName = "INKSHOT/Sound Library")]
public class SoundLibrarySO : ScriptableObject
{
    private const string AssetPath = "Assets/ScriptableObjects/SoundLibrary.asset";
    private static SoundLibrarySO _instance;

    public AudioClip balloonPop;
    public AudioClip paintBurst;
    public AudioClip wallHit;
    public AudioClip comboRise;
    public AudioClip launch;
    public AudioClip roomCleared;
    public AudioClip roomFailed;

    public static SoundLibrarySO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = LoadAsset() ?? CreateInstance<SoundLibrarySO>();
                SoundLibraryBootstrap.EnsurePopulated(_instance);
            }

            return _instance;
        }
    }

    private static SoundLibrarySO LoadAsset()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<SoundLibrarySO>(AssetPath);
#else
        return null;
#endif
    }
}
