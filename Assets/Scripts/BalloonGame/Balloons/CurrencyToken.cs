using UnityEngine;

/// <summary>
/// Simple collectible token spawned by gold balloons.
/// </summary>
[DisallowMultipleComponent]
public class CurrencyToken : MonoBehaviour
{
    [SerializeField] private int _value = 1;
    private float _lifetimeRemaining = 1.4f;
    private ObjectPool _owningPool;

    /// <summary>
    /// Assigns the pool that should reclaim this token after use.
    /// </summary>
    public void SetOwningPool(ObjectPool owningPool)
    {
        _owningPool = owningPool;
    }

    public void Initialize(int value)
    {
        _value = value;
        _lifetimeRemaining = 1.4f;
    }

    private void Update()
    {
        transform.position += Vector3.up * Time.deltaTime;
        _lifetimeRemaining -= Time.deltaTime;
        if (_lifetimeRemaining <= 0f)
        {
            Collect();
        }
    }

    public void Collect()
    {
        SaveManager.Instance.AddInk(_value);
        if (_owningPool != null)
        {
            _owningPool.Return(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
