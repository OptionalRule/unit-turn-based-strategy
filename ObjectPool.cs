using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> : MonoBehaviour where T : Component
{
    public T prefab;
    private Queue<T> pool = new Queue<T>();

    public void Initialize(T prefab, int initialCount)
    {
        this.prefab = prefab;
        for (int i = 0; i < initialCount; i++)
        {
            AddObjectToPool();
        }
    }

    public T GetObject()
    {
        if (pool.Count == 0)
        {
            AddObjectToPool();
        }

        T obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnObject(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }

    private void AddObjectToPool()
    {
        T newObject = Instantiate(prefab);
        newObject.gameObject.SetActive(false);
        pool.Enqueue(newObject);
    }
}
