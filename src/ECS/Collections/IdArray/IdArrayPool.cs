// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Collections;

sealed class IdArrayPool
{
    private readonly int arraySize;
    private int freeStart;
    private StackArray<int> freeStarts;

    private int[] ids;
    private int maxStart;

    internal IdArrayPool(int poolIndex)
    {
        arraySize = 2 << poolIndex - 1;
        ids = Array.Empty<int>();
        freeStarts = new StackArray<int>(Array.Empty<int>());
    }

    public int Count { get; private set; }
    internal int FreeCount => freeStarts.Count;

    public override string ToString() => $"arraySize: {arraySize} count: {Count}";

    internal static int[] GetIds(int count, IdArrayHeap heap) => heap.pools[IdArrayHeap.PoolIndex(count)].ids;

    internal static IdArrayPool GetPool(IdArrayHeap heap, int index, out int[] ids)
    {
        var pool = heap.pools[index];
        ids = pool.ids;
        return pool;
    }

    /// <summary>
    ///     Return the start index within the returned newIds.
    /// </summary>
    internal int CreateArray(out int[] newIds)
    {
        Count++;
        if (freeStarts.TryPop(out var start))
        {
            newIds = ids;
            return start;
        }
        start = freeStart;
        freeStart = start + arraySize;
        if (start < maxStart)
        {
            newIds = ids;
            return start;
        }
        maxStart = Math.Max(4 * arraySize, 2 * maxStart);
        ArrayUtils.Resize(ref ids, maxStart);
        newIds = ids;
        return start;
    }

    /// <summary>
    ///     Delete the array with the passed start index.
    /// </summary>
    internal void DeleteArray(int start, out int[] ids)
    {
        Count--;
        ids = this.ids;
        if (Count > 0)
        {
            freeStarts.Push(start);
            return;
        }
        freeStart = 0;
        freeStarts.Clear();
    }
}
