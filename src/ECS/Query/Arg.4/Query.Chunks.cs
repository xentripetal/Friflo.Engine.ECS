﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
///     Contains the components returned by a component query.
///     See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#enumerate-query-chunks">Example.</a>
/// </summary>
public readonly struct Chunks<T1, T2, T3, T4>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
{
    public int Length => Chunk1.Length;
    public readonly Chunk<T1> Chunk1; //  16
    public readonly Chunk<T2> Chunk2; //  16
    public readonly Chunk<T3> Chunk3; //  16
    public readonly Chunk<T4> Chunk4; //  16
    public readonly ChunkEntities Entities; //  32

    public override string ToString() => Entities.GetChunksString();

    internal Chunks(Chunk<T1> chunk1, Chunk<T2> chunk2, Chunk<T3> chunk3, Chunk<T4> chunk4, in ChunkEntities entities)
    {
        Chunk1 = chunk1;
        Chunk2 = chunk2;
        Chunk3 = chunk3;
        Chunk4 = chunk4;
        Entities = entities;
    }

    internal Chunks(in Chunks<T1, T2, T3, T4> chunks, int start, int length, int taskIndex)
    {
        Chunk1 = new Chunk<T1>(chunks.Chunk1, start, length);
        Chunk2 = new Chunk<T2>(chunks.Chunk2, start, length);
        Chunk3 = new Chunk<T3>(chunks.Chunk3, start, length);
        Chunk4 = new Chunk<T4>(chunks.Chunk4, start, length);
        Entities = new ChunkEntities(chunks.Entities, start, length, taskIndex);
    }

    internal Chunks(in ChunkEntities entities, int taskIndex) => Entities = new ChunkEntities(entities, taskIndex);

    public void Deconstruct(out Chunk<T1> chunk1, out Chunk<T2> chunk2, out Chunk<T3> chunk3, out Chunk<T4> chunk4, out ChunkEntities entities)
    {
        chunk1 = Chunk1;
        chunk2 = Chunk2;
        chunk3 = Chunk3;
        chunk4 = Chunk4;
        entities = Entities;
    }
}

/// <summary>
///     Contains the component chunks returned by a component query.
///     See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#enumerate-query-chunks">Example.</a>
/// </summary>
public readonly struct QueryChunks<T1, T2, T3, T4> : IEnumerable<Chunks<T1, T2, T3, T4>>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
{
    private readonly ArchetypeQuery<T1, T2, T3, T4> query;

    public int Count => query.Count;

    /// <summary> Obsolete. Renamed to <see cref="Count" />. </summary>
    [Obsolete($"Renamed to {nameof(Count)}")]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int EntityCount => query.Count;

    public override string ToString() => query.GetQueryChunksString();

    internal QueryChunks(ArchetypeQuery<T1, T2, T3, T4> query) => this.query = query;

    // --- IEnumerable<>
    [ExcludeFromCodeCoverage]
    IEnumerator<Chunks<T1, T2, T3, T4>>
        IEnumerable<Chunks<T1, T2, T3, T4>>.GetEnumerator() => new ChunkEnumerator<T1, T2, T3, T4>(query);

    // --- IEnumerable
    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => new ChunkEnumerator<T1, T2, T3, T4>(query);

    // --- IEnumerable
    public ChunkEnumerator<T1, T2, T3, T4> GetEnumerator() => new (query);
}

public struct ChunkEnumerator<T1, T2, T3, T4> : IEnumerator<Chunks<T1, T2, T3, T4>>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
{
    private readonly int structIndex1; //  4
    private readonly int structIndex2; //  4
    private readonly int structIndex3; //  4
    private readonly int structIndex4; //  4
    //
    private readonly Archetypes archetypes; // 16
    //
    private int archetypePos; //  4


    internal ChunkEnumerator(ArchetypeQuery<T1, T2, T3, T4> query)
    {
        structIndex1 = query.signatureIndexes.T1;
        structIndex2 = query.signatureIndexes.T2;
        structIndex3 = query.signatureIndexes.T3;
        structIndex4 = query.signatureIndexes.T4;
        archetypes = query.GetArchetypes();
        archetypePos = -1;
    }

    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public Chunks<T1, T2, T3, T4> Current { get; private set; }

    // --- IEnumerator
    [ExcludeFromCodeCoverage]
    public void Reset()
    {
        archetypePos = -1;
        Current = default;
    }

    [ExcludeFromCodeCoverage]
    object IEnumerator.Current => Current;

    // --- IEnumerator
    public bool MoveNext()
    {
        Archetype archetype;
        int count;
        var start = 0;
        var types = archetypes;
        var pos = archetypePos;
        if (types.chunkPositions != null)
        {
            goto SingleEntity;
        }
        do
        {
            if (pos >= types.last)
            { // last = length - 1
                archetypePos = pos;
                return false;
            }
            archetype = types.array[++pos];
            count = archetype.entityCount;
        } while (count == 0); // skip archetypes without entities
        SetChunks:
        archetypePos = pos;
        var heapMap = archetype.heapMap;
        var chunks1 = (StructHeap<T1>)heapMap[structIndex1];
        var chunks2 = (StructHeap<T2>)heapMap[structIndex2];
        var chunks3 = (StructHeap<T3>)heapMap[structIndex3];
        var chunks4 = (StructHeap<T4>)heapMap[structIndex4];

        var chunk1 = new Chunk<T1>(chunks1.components, count, start);
        var chunk2 = new Chunk<T2>(chunks2.components, count, start);
        var chunk3 = new Chunk<T3>(chunks3.components, count, start);
        var chunk4 = new Chunk<T4>(chunks4.components, count, start);
        var entities = new ChunkEntities(archetype, count, start);
        Current = new Chunks<T1, T2, T3, T4>(chunk1, chunk2, chunk3, chunk4, entities);
        return true;
        SingleEntity:
        if (pos >= types.last)
        {
            return false;
        }
        pos++;
        start = types.chunkPositions[pos];
        archetype = types.array[pos];
        count = 1;
        goto SetChunks;
    }

    // --- IDisposable
    public void Dispose() { }
}
