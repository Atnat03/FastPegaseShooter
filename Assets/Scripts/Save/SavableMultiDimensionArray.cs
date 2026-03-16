using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavableMultiDimensionArray<T>
{
    [SerializeField] public List<T> listedArray;
    [SerializeField] private Vector2Int arraySize;
    
    public SavableMultiDimensionArray(T[,] array)
    {
        FromArray(array);
    }

    public T[,] ToArray()
    {
        T[,] array = new T[arraySize.x, arraySize.y];
        for (int i = 0; i < arraySize.x; i++)
        {
            for (int j = 0; j < arraySize.y; j++)
            {
                array[i, j] = listedArray[i * arraySize.x + j];
            }
        }
        return array;
    }

    void FromArray(T[,] array)
    {
        arraySize = new Vector2Int(array.GetLength(0), array.GetLength(1));
        listedArray = new List<T>();
        
        for (int i = 0; i < arraySize.x; i++)
        {
            for (int j = 0; j < arraySize.y; j++)
            {
                listedArray.Add(array[i, j]);
            }
        }
    }
}
