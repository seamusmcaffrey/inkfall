using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry for object pools.
/// </summary>
public static class PoolManager
{
    private static readonly HashSet<ObjectPool> Pools = new();

    public static void Register(ObjectPool pool)
    {
        if (pool != null)
        {
            Pools.Add(pool);
        }
    }

    public static void Unregister(ObjectPool pool)
    {
        if (pool != null)
        {
            Pools.Remove(pool);
        }
    }

    public static void WarmupAll()
    {
        foreach (ObjectPool pool in Pools)
        {
            pool?.WarmupAll();
        }
    }
}
