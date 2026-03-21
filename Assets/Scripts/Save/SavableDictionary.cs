using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SavableDictionary <TKey,TValue>
{
    public List<TKey>  keys = new List<TKey>();
    public List<TValue> values = new List<TValue>();
    
    public SavableDictionary(Dictionary<TKey, TValue> dictionary)
    {
        FromDictionary(dictionary);
    }

    public void Add(TKey key, TValue value)
    {
        keys.Add(key);
        values.Add(value);
    }

    public void Remove(TKey key)
    {
        int index = keys.IndexOf(key);
        keys.RemoveAt(index);
        values.RemoveAt(index);
    }
    public void Remove(TValue value)
    {
        int index = values.IndexOf(value);
        keys.RemoveAt(index);
        values.RemoveAt(index);
    }

    public void SortBy<TKeySort>(Func<TKey, TValue, TKeySort> selector)
    {
        var combined = keys.Zip(values, (k,v) => new { k, v })
            .OrderBy(x => selector(x.k, x.v))
            .ToList();
        
        keys = combined.Select(x => x.k).ToList();
        values = combined.Select(x => x.v).ToList();
    }

    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < keys.Count; i++)
        {
            dict.Add(keys[i], values[i]);
        }
        return dict;
    }

    void FromDictionary(Dictionary<TKey, TValue> dict)
    {
        foreach (KeyValuePair<TKey, TValue> pair in dict)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
}
