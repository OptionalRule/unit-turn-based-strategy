using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public GameObject objectPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();

    public void Initialize(GameObject prefab, int initialCount)
    {
        objectPrefab = prefab;
        for (int i = 0; i < initialCount; i++)
        {
            AddObjectToPool();
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count == 0)
        {
            AddObjectToPool();
        }

        var obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    private void AddObjectToPool()
    {
        var newObject = Instantiate(objectPrefab);
        newObject.SetActive(false);
        pool.Enqueue(newObject);
    }
}
