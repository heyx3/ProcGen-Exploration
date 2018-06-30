using System;
using System.Collections.Generic;
using System.Linq;

using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// A queue that automatically sorts its elements based on a floating-point "weight".
/// </summary>
public class PriorityQueue<T>
{
    private class TComparer : IComparer<T>
    {
        public Dictionary<T, float> ItemWeights;
        public int IsAscending;

        public int Compare(T x, T y)
        {
            float xF = ItemWeights[x],
                  yF = ItemWeights[y];

            if (xF < yF)
            {
                return -IsAscending;
            }
            else if (xF > yF)
            {
                return IsAscending;
            }
            else
            {
                return 0;
            }
        }
    }


    /// <summary>
    /// "True" if the first element out of the queue is the highest-weight.
    /// "False" if the first element out of the queue is the lowest-weight.
    /// </summary>
    public bool IsAscending { get; private set; }

    public int Count { get { return sortedItems.Count; } }


    private Dictionary<T, float> itemWeights = new Dictionary<T, float>();
    private List<T> sortedItems = new List<T>();


    private TComparer myComparer = null;
    private TComparer MyComparer
    {
        get
        {
            if (myComparer == null)
            {
                myComparer = new TComparer();
                myComparer.ItemWeights = itemWeights;
                myComparer.IsAscending = (IsAscending ? 1 : -1);
            }
            return myComparer;
        }
    }

    /// <summary>
    /// Gets the index of the given item using an efficient search.
    /// </summary>
    private int IndexOf(T item)
    {
        return sortedItems.BinarySearch(item, MyComparer);
    }

    /// <summary>
    /// Pass "true" if the first element out of the queue is the lowest-weight.
    /// Pass "false" if the first element out of the queue is the highest-weight.
    /// </summary>
    public PriorityQueue(bool isAscending)
    {
        IsAscending = isAscending;
    }


    /// <summary>
    /// Adds the given item to this priority queue with the given weight.
    /// </summary>
    public void Add(T item, float weight)
    {
        Assert.IsTrue(!itemWeights.ContainsKey(item) && !sortedItems.Contains(item));

        itemWeights.Add(item, weight);

        //Insert the item based on weight.
        int i;
        for (i = 0; i < sortedItems.Count; ++i)
        {
            float tempWeight = itemWeights[sortedItems[i]];
            if ((IsAscending && tempWeight < weight) ||
                (!IsAscending && tempWeight > weight))
            {
                sortedItems.Insert(i, item);
                break;
            }
        }
        if (i == sortedItems.Count)
            sortedItems.Add(item);
    }

    public T Peek()
    {
        return sortedItems[sortedItems.Count - 1];
    }
    public T Peek(out float weight)
    {
        weight = itemWeights[sortedItems[sortedItems.Count - 1]];
        return sortedItems[sortedItems.Count - 1];
    }
    public T Pop()
    {
        T t = sortedItems[sortedItems.Count - 1];
        sortedItems.RemoveAt(sortedItems.Count - 1);
        itemWeights.Remove(t);
        return t;
    }
    public T Pop(out float weight)
    {
        weight = itemWeights[sortedItems[sortedItems.Count - 1]];
        return Pop();
    }

    public bool Contains(T t)
    {
        return sortedItems.Contains(t);
    }


    /// <summary>
    /// Returns the weight of the removed item, or NaN if it couldn't be found.
    /// </summary>
    public float Remove(T item)
    {
        if (itemWeights.ContainsKey(item))
        {
            float f = itemWeights[item];
            itemWeights.Remove(item);

            int index = IndexOf(item);
            Assert.IsTrue(index > -1);
            sortedItems.RemoveAt(index);

            return f;
        }

        return float.NaN;
    }
}