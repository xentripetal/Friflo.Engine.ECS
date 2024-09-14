﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

static class SortUtils<TValue>
{
    private static readonly Comparer<TValue> Comparer = Comparer<TValue>.Default;

    internal static void AddValueInRangeEntities(
        TValue min,
        TValue max,
        HashSet<int> idSet,
        Dictionary<TValue, IdArray> map,
        ComponentIndex<TValue> componentIndex
    )
    {
        var count = map.Count;
        var buffer = componentIndex.sortBuffer;
        if (componentIndex.modified)
        {
            componentIndex.modified = false;

            if (buffer.Length < count)
            {
                buffer = componentIndex.sortBuffer = new TValue[count];
            }
            var n = 0;
            foreach (var pair in map)
            {
                buffer[n++] = pair.Key;
            }

            Array.Sort(buffer, 0, count);
        }
        var minIndex = LowerBound(buffer, count, min);
        var maxIndex = UpperBound(buffer, count, max);
        var idHeap = componentIndex.idHeap;

        for (var index = minIndex; index < maxIndex; index++)
        {
            var ids = map[buffer[index]];
            var idSpan = ids.GetSpan(idHeap, componentIndex.store);
            foreach (var id in idSpan)
            {
                idSet.Add(id);
            }
        }
    }

    // https://stackoverflow.com/questions/23806296/what-is-the-fastest-way-to-get-all-the-keys-between-2-keys-in-a-sortedlist
    private static int LowerBound(TValue[] list, int count, TValue value)
    {
        int lower = 0, upper = count - 1;

        while (lower <= upper)
        {
            var middle = lower + (upper - lower) / 2;
            var comparisonResult = Comparer.Compare(value, list[middle]);

            // slightly adapted here
            if (comparisonResult <= 0)
                upper = middle - 1;
            else
                lower = middle + 1;
        }
        return lower;
    }

    private static int UpperBound(TValue[] list, int count, TValue value)
    {
        int lower = 0, upper = count - 1;

        while (lower <= upper)
        {
            var middle = lower + (upper - lower) / 2;
            var comparisonResult = Comparer.Compare(value, list[middle]);

            // slightly adapted here
            if (comparisonResult < 0)
                upper = middle - 1;
            else
                lower = middle + 1;
        }
        return lower;
    }
}
