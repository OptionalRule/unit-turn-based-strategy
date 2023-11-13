using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    private int defaultPoolSize = 10;
    
    public static ObjectPoolManager Instance { get; private set; }

    private Dictionary<GameObject, ObjectPool> pools = new Dictionary<GameObject, ObjectPool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetObject(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            CreateNewPool(prefab, defaultPoolSize);
        }

        return pools[prefab].GetObject();
    }

    public void ReturnObject(GameObject prefab, GameObject obj)
    {
        if (pools.ContainsKey(prefab))
        {
            pools[prefab].ReturnObject(obj);
        }
    }

    public void CreatePool(GameObject prefab, int initialCount)
    {
        if (!pools.ContainsKey(prefab))
        {
            CreateNewPool(prefab, initialCount);
        }

        pools[prefab].Initialize(prefab, initialCount);
    }

    private void CreateNewPool(GameObject prefab, int poolSize)
    {
        var newPoolObj = new GameObject(prefab.name + " Pool");
        var newPool = newPoolObj.AddComponent<ObjectPool>();
        newPool.Initialize(prefab, poolSize); 
        pools.Add(prefab, newPool);
    }
}
