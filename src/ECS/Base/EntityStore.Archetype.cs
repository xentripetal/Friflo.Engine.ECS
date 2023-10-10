﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// Hard rule: this file/section MUST NOT use GameEntity instances

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed partial class EntityStore
{
    private Archetype GetArchetypeWith<T>(Archetype current)
        where T : struct, IStructComponent
    {
        var hash = GetHashWith(typeof(T), current);
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var config      = GetArchetypeConfig();
        var schema      = Static.ComponentSchema;
        var heaps       = current.Heaps;
        var currentLen  = heaps.Length;
        var types       = new List<ComponentType>(currentLen + 1);
        for (int n = 0; n < currentLen; n++) {
            var heap = heaps[n];
            types.Add(schema.GetStructType(heap.structIndex, heap.type));
        }
        types.Add(schema.GetStructType(StructHeap<T>.StructIndex, typeof(T)));
        archetype = Archetype.CreateWithStructTypes(config, types, current.tags);
        AddArchetype(archetype);
        return archetype;
    }
    
    private Archetype GetArchetypeWithout(Archetype archetype, Type removeType)
    {
        var hash = GetHashWithout(removeType, archetype);
        if (TryGetArchetype(hash, out var result)) {
            return result;
        }
        var heaps           = archetype.Heaps;
        var componentCount  = heaps.Length - 1;
        if (componentCount == 0) {
            return null;
        }
        var types       = new List<ComponentType>(componentCount);
        var config      = GetArchetypeConfig();
        var schema      = Static.ComponentSchema;
        foreach (var heap in heaps) {
            if (heap.type == removeType)
                continue;
            types.Add(schema.GetStructType(heap.structIndex, heap.type));
        }
        result      = Archetype.CreateWithStructTypes(config, types, archetype.tags);
        AddArchetype(result);
        return result;
    }
    
    internal bool TryGetArchetype (long hash, out Archetype result) {
        foreach (var arch in archetypeInfos) {
            if (arch.hash != hash) {
                continue;
            }
            result = arch.type;
            return true;
        }
        result = null;
        return false;
    }
    
    internal void AddArchetype (Archetype archetype)
    {
        if (archetypesCount == archetypes.Length) {
            var newLen = 2 * archetypes.Length;
            Utils.Resize(ref archetypes,     newLen);
            Utils.Resize(ref archetypeInfos, newLen);
        }
        if (archetype.archIndex != archetypesCount) {
            throw new InvalidOperationException("invalid archIndex");
        }
        archetypes    [archetypesCount] = archetype;
        archetypeInfos[archetypesCount] = new ArchetypeInfo(archetype.typeHash, archetype);
        archetypesCount++;
    }
    
    // ------------------------------------ hash utils ------------------------------------
    private static long GetHashWith(Type newType, Archetype archetype) {
        return newType.Handle() ^ archetype.typeHash;
    }
    
    private static long GetHashWithout(Type removeType, Archetype archetype)
    {
        return removeType.Handle() ^ archetype.typeHash;
    }
    
    internal static long GetHash(StructHeap[] heaps) {
        long hash = default;
        foreach (var heap in heaps) {
            hash ^= heap.hash;
        }
        return hash;
    }
    
    // ------------------------------------ add / remove struct component ------------------------------------
    internal bool AddComponent<T>(
            int                 id,
        ref Archetype           archetype,  // possible mutation is not null
        ref int                 compIndex,
        in  T                   component)
        where T : struct, IStructComponent
    {
        var arch = archetype;
        if (arch != defaultArchetype) {
            var structHeap = arch.heapMap[StructHeap<T>.StructIndex];
            if (structHeap != null) {
                // --- change component value 
                var heap = (StructHeap<T>)structHeap;
                heap.chunks[compIndex / ChunkSize].components[compIndex % ChunkSize] = component;
                return false;
            }
            // --- change entity archetype
            var newArchetype    = GetArchetypeWith<T>(arch);
            compIndex           = arch.MoveEntityTo(id, compIndex, newArchetype);
            archetype           = arch = newArchetype;
        } else {
            // --- add entity to archetype
            arch                = GetArchetype<T>(arch.tags);
            compIndex           = arch.AddEntity(id);
            archetype           = arch;
        }
        // --- set component value 
        var heap2 = (StructHeap<T>)arch.heapMap[StructHeap<T>.StructIndex];
        heap2.chunks[compIndex / ChunkSize].components[compIndex % ChunkSize] = component;
        return true;
    }
    
    internal bool RemoveComponent<T>(
            int                 id,
        ref Archetype           archetype,    // possible mutation is not null
        ref int                 compIndex)
        where T : struct, IStructComponent
    {
        var arch = archetype;
        var heap = arch.heapMap[StructHeap<T>.StructIndex];
        if (heap == null) {
            return false;
        }
        var newArchetype = GetArchetypeWithout(arch, typeof(T));
        if (newArchetype == defaultArchetype) {
            int removePos = compIndex; 
            // --- update entity
            compIndex   = 0;
            archetype   = defaultArchetype;
            arch.MoveLastComponentsTo(removePos);
            return true;
        }
        // --- change entity archetype
        compIndex   = arch.MoveEntityTo(id, compIndex, newArchetype);
        archetype   = newArchetype;
        return true;
    }
    
    // ------------------------------------ add / remove entity Tag ------------------------------------
    internal bool AddTags(
        in Tags             tags,
        int                 id,
        ref Archetype       archetype,      // possible mutation is not null
        ref int             compIndex)
    {
        return true;
    }
    
    internal bool RemoveTags(
        in Tags             tags,
        int                 id,
        ref Archetype       archetype,      // possible mutation is not null
        ref int             compIndex)
    {
        return true;
    }
}
