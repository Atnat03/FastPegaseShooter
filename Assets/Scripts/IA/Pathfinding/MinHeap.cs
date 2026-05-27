using System;
using System.Collections.Generic;

/// <summary>
/// Min-heap optimized for A* pathfinding. No allocations after initial capacity is reached.
/// </summary>
public class MinHeap<T> where T : class
{
    private T[] _items;
    private int _count;
    private readonly Comparison<T> _comparison;

    public int Count => _count;

    public MinHeap(int initialCapacity, Comparison<T> comparison)
    {
        _items = new T[initialCapacity];
        _count = 0;
        _comparison = comparison;
    }

    public void Clear()
    {
        // Don't reallocate, just reset count
        Array.Clear(_items, 0, _count);
        _count = 0;
    }

    public void Add(T item)
    {
        if (_count == _items.Length)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }

        _items[_count] = item;
        BubbleUp(_count);
        _count++;
    }

    public T RemoveMin()
    {
        if (_count == 0) return null;

        T min = _items[0];
        _count--;
        _items[0] = _items[_count];
        _items[_count] = null;
        BubbleDown(0);
        return min;
    }

    public T PeekMin()
    {
        return _count > 0 ? _items[0] : null;
    }

    private void BubbleUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (_comparison(_items[index], _items[parent]) < 0)
            {
                Swap(index, parent);
                index = parent;
            }
            else break;
        }
    }

    private void BubbleDown(int index)
    {
        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int smallest = index;

            if (left < _count && _comparison(_items[left], _items[smallest]) < 0)
                smallest = left;
            if (right < _count && _comparison(_items[right], _items[smallest]) < 0)
                smallest = right;

            if (smallest != index)
            {
                Swap(index, smallest);
                index = smallest;
            }
            else break;
        }
    }

    private void Swap(int a, int b)
    {
        T temp = _items[a];
        _items[a] = _items[b];
        _items[b] = temp;
    }
}