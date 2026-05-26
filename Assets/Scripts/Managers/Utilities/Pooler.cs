using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;

public class Pooler<T> where T : MonoBehaviour, IPoolable
{
    private Queue<T> pool = new Queue<T>();
    private T prefab;
    private GameObject poolParent;
    
    public int Size => pool.Count;

    public Pooler(T prefab, int initialSize)
    {
        this.prefab = prefab;
        poolParent = new GameObject($"{typeof(T).Name} Pool")
        {
            transform =
            {
                position = Vector3.zero,
                rotation = Quaternion.identity,
                localScale = Vector3.one
            }
        };
        for (int i = 0; i < initialSize; i++)
        {
            /*
            T obj = GameObject.Instantiate(prefab, poolParent.transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
            */
        }
    }

    public T Spawn(Vector3 position, Quaternion rotation)
    {
        T obj = null;

        while (pool.Count > 0)
        {
            var candidate = pool.Dequeue();
            if (candidate != null && candidate.gameObject != null)
            {
                obj = candidate;
                break;
            }
        }

        if (obj == null)
        {
            obj = GameObject.Instantiate(prefab, poolParent.transform);
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
