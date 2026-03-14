using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic GameObject pool for spawned runtime content.
/// </summary>
[DisallowMultipleComponent]
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _initialSize = GameConstants.DEFAULT_POOL_SIZE;
    [SerializeField] private bool _expandIfNeeded = true;

    private readonly Queue<GameObject> _available = new();
    private readonly HashSet<GameObject> _leased = new();
    private Transform _container;
    private bool _isWarm;

    public int CreatedCount { get; private set; }
    public int PeakActiveCount { get; private set; }
    public int GrowthCount { get; private set; }
    public int ActiveCount => _leased.Count;

    private void OnEnable()
    {
        PoolManager.Register(this);
    }

    private void OnDisable()
    {
        PoolManager.Unregister(this);
    }

    public void Configure(GameObject prefab, int initialSize, bool expandIfNeeded = true)
    {
        _prefab = prefab;
        _initialSize = Mathf.Max(1, initialSize);
        _expandIfNeeded = expandIfNeeded;
        _isWarm = false;
        WarmupAll();
    }

    public void WarmupAll()
    {
        if (_prefab == null)
        {
            return;
        }

        EnsureContainer();
        while (_available.Count + _leased.Count < _initialSize)
        {
            Return(CreateInstance());
        }

        _isWarm = true;
    }

    public GameObject Get()
    {
        if (_prefab == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"[{nameof(ObjectPool)}] Pool on {name} has no prefab configured.");
#endif
            return null;
        }

        if (!_isWarm)
        {
            WarmupAll();
        }

        GameObject instance;
        if (_available.Count > 0)
        {
            instance = _available.Dequeue();
        }
        else if (_expandIfNeeded)
        {
            GrowthCount++;
            instance = CreateInstance();
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[{nameof(ObjectPool)}] Pool '{name}' exhausted.");
#endif
            return null;
        }

        _leased.Add(instance);
        PeakActiveCount = Mathf.Max(PeakActiveCount, _leased.Count);
        instance.SetActive(true);
        return instance;
    }

    public T Get<T>() where T : Component
    {
        GameObject instance = Get();
        return instance != null ? instance.GetComponent<T>() : null;
    }

    public void Return(Component instance)
    {
        if (instance != null)
        {
            Return(instance.gameObject);
        }
    }

    public void Return(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        EnsureContainer();
        _leased.Remove(instance);
        instance.transform.SetParent(_container, false);
        instance.SetActive(false);
        if (!_available.Contains(instance))
        {
            _available.Enqueue(instance);
        }
    }

    private GameObject CreateInstance()
    {
        EnsureContainer();
        GameObject instance = Instantiate(_prefab, _container);
        instance.name = _prefab.name;
        CreatedCount++;
        return instance;
    }

    private void EnsureContainer()
    {
        if (_container != null)
        {
            return;
        }

        var go = new GameObject($"{name}_Pool");
        go.transform.SetParent(transform, false);
        _container = go.transform;
    }
}
