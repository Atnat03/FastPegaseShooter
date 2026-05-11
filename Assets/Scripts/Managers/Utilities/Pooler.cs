using UnityEngine;
using System.Collections.Generic;

using UnityEngine;
using System.Collections.Generic;

public class Pooler<T> where T : MonoBehaviour, IPoolable
{
    private Queue<T> pool = new Queue<T>();
    private T prefab;

    public Pooler(T prefab, int initialSize)
    {
        this.prefab = prefab;

        for (int i = 0; i < initialSize; i++)
        {
            T obj = GameObject.Instantiate(prefab);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T Spawn(Vector3 position, Quaternion rotation)
    {
        T obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = GameObject.Instantiate(prefab);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;

        obj.gameObject.SetActive(true);
        obj.Spawn();

        return obj;
    }

    public void ReturnToPool(T obj)
    {
        obj.ReturnToPool();
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}

public interface IPoolable
{
    void Spawn();
    void ReturnToPool();
}
