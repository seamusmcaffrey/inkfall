using UnityEngine;

/// <summary>
/// Central event-to-feedback coordinator.
/// </summary>
[DisallowMultipleComponent]
public class JuiceManager : MonoBehaviour
{
    [SerializeField] private JuiceConfigSO _config;
    [SerializeField] private ScreenShakeManager _screenShake;
    [SerializeField] private ChromaticAberrationPulse _chromaticPulse;
    [SerializeField] private SlowMotionController _slowMotion;
    [SerializeField] private PaintDecalManager _decalManager;
    [SerializeField] private ComboFlashVFX _comboFlash;
    [SerializeField] private ObjectPool _balloonPopPool;
    [SerializeField] private ObjectPool _impactSparkPool;
    [SerializeField] private ObjectPool _paintSplatterPool;
    [SerializeField] private ObjectPool _wallHitPool;

    private void Awake()
    {
        if (_config == null)
        {
            _config = JuiceConfigSO.Instance;
        }

        _screenShake = ComponentUtility.EnsureComponent<ScreenShakeManager>(gameObject);
        _chromaticPulse = ComponentUtility.EnsureComponent<ChromaticAberrationPulse>(gameObject);
        _slowMotion = ComponentUtility.EnsureComponent<SlowMotionController>(gameObject);
        _decalManager = ComponentUtility.EnsureComponent<PaintDecalManager>(gameObject);

        if (_comboFlash == null)
        {
            GameObject flash = new("ComboFlashVFX");
            flash.transform.SetParent(transform, false);
            _comboFlash = flash.AddComponent<ComboFlashVFX>();
        }

        _screenShake.SetConfig(_config);
        _chromaticPulse.SetConfig(_config);
        _slowMotion.SetConfig(_config);
        EnsureVfxPools();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BalloonPoppedEvent>(HandleBalloonPopped);
        EventBus.Subscribe<PaintExplosionEvent>(HandlePaintExplosion);
        EventBus.Subscribe<WallBounceEvent>(HandleWallBounce);
        EventBus.Subscribe<ComboChangedEvent>(HandleComboChanged);
        EventBus.Subscribe<DartLaunchedEvent>(HandleDartLaunched);
        EventBus.Subscribe<RoomClearedEvent>(HandleRoomCleared);
        EventBus.Subscribe<RoomFailedEvent>(HandleRoomFailed);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BalloonPoppedEvent>(HandleBalloonPopped);
        EventBus.Unsubscribe<PaintExplosionEvent>(HandlePaintExplosion);
        EventBus.Unsubscribe<WallBounceEvent>(HandleWallBounce);
        EventBus.Unsubscribe<ComboChangedEvent>(HandleComboChanged);
        EventBus.Unsubscribe<DartLaunchedEvent>(HandleDartLaunched);
        EventBus.Unsubscribe<RoomClearedEvent>(HandleRoomCleared);
        EventBus.Unsubscribe<RoomFailedEvent>(HandleRoomFailed);
    }

    private void HandleBalloonPopped(BalloonPoppedEvent evt)
    {
        if (!_config.juiceEnabled || !_config.balloonPopEnabled)
        {
            return;
        }

        BalloonPopVFX popVfx = _balloonPopPool != null ? _balloonPopPool.Get<BalloonPopVFX>() : null;
        if (popVfx != null)
        {
            popVfx.transform.position = evt.WorldPosition;
            popVfx.SetOwningPool(_balloonPopPool);
            popVfx.Play(UIColors.GetBalloonTextColor(evt.BalloonColor), _config);
        }

        ImpactSparkVFX sparkVfx = _impactSparkPool != null ? _impactSparkPool.Get<ImpactSparkVFX>() : null;
        if (sparkVfx != null)
        {
            sparkVfx.transform.position = evt.WorldPosition;
            sparkVfx.SetOwningPool(_impactSparkPool);
            sparkVfx.Play(_config);
        }

        _screenShake.Shake(_config.shakeIntensityPop * _config.globalIntensity);
        AudioManager.Instance.PlaySfx(SoundLibrarySO.Instance.balloonPop, _config.sfxVolume);
        HapticsUtility.Light();
    }

    private void HandlePaintExplosion(PaintExplosionEvent evt)
    {
        if (!_config.juiceEnabled || !_config.paintSplatterEnabled)
        {
            return;
        }

        Color color = UIColors.GetBalloonTextColor(evt.SourceColor);
        PaintSplatterVFX paintVfx = _paintSplatterPool != null ? _paintSplatterPool.Get<PaintSplatterVFX>() : null;
        if (paintVfx != null)
        {
            paintVfx.transform.position = evt.WorldPosition;
            paintVfx.SetOwningPool(_paintSplatterPool);
            paintVfx.Play(color, _config);
        }

        _decalManager.SpawnDecal(evt.WorldPosition, color, _config);
        _screenShake.Shake(_config.shakeIntensityPaint * _config.globalIntensity);
        AudioManager.Instance.PlaySfx(SoundLibrarySO.Instance.paintBurst, _config.sfxVolume);
        HapticsUtility.Medium();
    }

    private void HandleWallBounce(WallBounceEvent evt)
    {
        if (!_config.wallHitEnabled)
        {
            return;
        }

        WallHitVFX wallHitVfx = _wallHitPool != null ? _wallHitPool.Get<WallHitVFX>() : null;
        if (wallHitVfx != null)
        {
            wallHitVfx.transform.position = evt.Position;
            wallHitVfx.SetOwningPool(_wallHitPool);
            wallHitVfx.Play(_config);
        }

        AudioManager.Instance.PlaySfx(SoundLibrarySO.Instance.wallHit, 0.45f);
    }

    private void HandleComboChanged(ComboChangedEvent evt)
    {
        if (evt.WasReset || evt.ComboCount < 2)
        {
            return;
        }

        AudioManager.Instance.PlaySfx(SoundLibrarySO.Instance.comboRise, 0.4f);
        _screenShake.Shake(_config.shakeIntensityCombo * _config.globalIntensity);
        if (_config.comboFlashEnabled)
        {
            _comboFlash.Flash(UIColors.GetComboColor(evt.ComboCount), _config);
        }

        if (_config.chromaticPulseEnabled && evt.ComboCount >= _config.chromaticComboThreshold)
        {
            _chromaticPulse.Pulse(1f);
        }

        if (_config.slowMotionEnabled && evt.ComboCount >= _config.slowMotionComboThreshold)
        {
            _slowMotion.Trigger();
        }
    }

    private void HandleDartLaunched(DartLaunchedEvent evt)
    {
        AudioManager.Instance.PlaySfx(SoundLibrarySO.Instance.launch, 0.35f);
    }

    private void HandleRoomCleared(RoomClearedEvent evt)
    {
        AudioManager.Instance.PlayUi(SoundLibrarySO.Instance.roomCleared, 0.6f);
        HapticsUtility.Success();
    }

    private void HandleRoomFailed(RoomFailedEvent evt)
    {
        AudioManager.Instance.PlayUi(SoundLibrarySO.Instance.roomFailed, 0.6f);
        HapticsUtility.Heavy();
    }

    private void EnsureVfxPools()
    {
        _balloonPopPool ??= CreateVfxPool<BalloonPopVFX>("BalloonPopPool");
        _impactSparkPool ??= CreateVfxPool<ImpactSparkVFX>("ImpactSparkPool");
        _paintSplatterPool ??= CreateVfxPool<PaintSplatterVFX>("PaintSplatterPool");
        _wallHitPool ??= CreateVfxPool<WallHitVFX>("WallHitPool");
    }

    private ObjectPool CreateVfxPool<T>(string name) where T : Component
    {
        GameObject host = new(name);
        host.transform.SetParent(transform, false);
        ObjectPool pool = host.AddComponent<ObjectPool>();

        GameObject prefab = new($"{typeof(T).Name}Prefab");
        prefab.SetActive(false);
        prefab.AddComponent<T>();

        pool.Configure(prefab, GameConstants.VFX_POOL_INITIAL_SIZE);
        return pool;
    }
}
