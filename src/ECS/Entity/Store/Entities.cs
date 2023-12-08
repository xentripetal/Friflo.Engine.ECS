﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Friflo.Fliox.Engine.ECS.Serialize;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using static Friflo.Fliox.Engine.ECS.NodeFlags;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// This file contains implementation specific for storing Entity's.
// The reason to separate handling of Entity's is to enable 'entity / component support' without Entity's.
// EntityStore remarks.
public partial class EntityStore
{
    public static     EntitySchema         GetEntitySchema()=> Static.EntitySchema;
    
    /// <returns>an <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public Entity CreateEntity()
    {
        var id      = sequenceId++;
        EnsureNodesLength(id + 1);
        var pid = GeneratePid(id);
        return CreateEntityNode(id, pid);
    }
    
    /// <returns>an <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public Entity CreateEntity(int id)
    {
        if (id < Static.MinNodeId) {
            throw InvalidEntityIdException(id, nameof(id));
        }
        if (id < nodes.Length && nodes[id].Is(Created)) {
            throw IdAlreadyInUseException(id, nameof(id));
        }
        EnsureNodesLength(id + 1);
        var pid = GeneratePid(id);
        return CreateEntityNode(id, pid);
    }
    
    public Entity CloneEntity(Entity original)
    {
        var entity          = CreateEntity();
        var archetype       = original.archetype;
        if (archetype != defaultArchetype) {
            entity.refCompIndex    = archetype.AddEntity(entity.id);
            entity.refArchetype    = archetype;
        }
        bool isBlittable = true;
        foreach (var componentType in archetype.componentTypes) {
            if (!componentType.blittable) {
                isBlittable = false;
                break;
            }
        }
        var scripts = original.Scripts;
        var scriptTypeByType = Static.EntitySchema.ScriptTypeByType;
        if (isBlittable) {
            foreach (var script in scripts)
            {
                var scriptType = scriptTypeByType[script.GetType()];
                if (!scriptType.blittable) {
                    isBlittable = false;
                    break;
                }    
            }
        }
        // todo optimize - serialize / deserialize only non blittable components and scripts
        if (isBlittable) {
            // CopyComponents() can be used only in case all component types are blittable
            archetype.CopyComponents(original.compIndex, entity.compIndex);
            // --- clone scripts
            foreach (var script in scripts) {
                var scriptType  = scriptTypeByType[script.GetType()];
                var clone       = scriptType.CloneScript(script);
                clone.entity    = entity;                                   // todo add test assertion
                entity.archetype.entityStore.AddScript(entity, clone, scriptType);
            }
        } else {
            // --- serialize entity
            var converter       = EntityConverter.Default;
            converter.EntityToDataEntity(original, dataBuffer);
            
            // --- deserialize DataEntity
            dataBuffer.pid      = IdToPid(entity.id);
            // convert will use entity created above
            converter.DataEntityToEntity(dataBuffer, this, out _);
        }
        return entity;
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private void AssertIdInNodes(int id) {
        if (id < nodes.Length) {
            return;
        }
        throw new InvalidOperationException("expect id < nodes.length");
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private static void AssertPid(long pid, long expected) {
        if (expected == pid) {
            return;
        }
        throw new InvalidOperationException($"invalid pid. expected: {expected}, was: {pid}");
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private static void AssertPid0(long pid, long expected) {
        if (pid == 0 || pid == expected) {
            return;
        }
        throw new InvalidOperationException($"invalid pid. expected: 0 or {expected}, was: {pid}");
    }

    /// <summary>expect <see cref="EntityStore.nodes"/> Length > id</summary> 
    private Entity CreateEntityNode(int id, long pid)
    {
        AssertIdInNodes(id);
        ref var node = ref nodes[id];
        if (node.Is(Created)) {
            AssertPid(node.pid, pid);
            return new Entity(id, this);
        }
        nodesCount++;
        if (nodesMaxId < id) {
            nodesMaxId = id;
        }
        AssertPid0(node.pid, pid);
        var entity          = new Entity(id, this);
        node.pid            = pid;
        node.archetype      = defaultArchetype;
        node.scriptIndex    = EntityUtils.NoScripts;
        // node.parentId    = Static.NoParentId;     // Is not set. A previous parent node has .parentId already set.
        node.childIds       = Static.EmptyChildNodes;
        node.flags          = Created;
        // node.entity      = entity;
        return entity;
    }
    
    public void SetStoreRoot(Entity entity) {
        if (entity.IsNull) {
            throw new ArgumentNullException(nameof(entity));
        }
        if (this != entity.archetype.store) {
            throw InvalidStoreException(nameof(entity));
        }
        SetStoreRootEntity(entity);
    }
    
    /// <summary>
    /// Creates a new entity with the components and tags of the given <paramref name="archetype"/>
    /// </summary>
    public Entity CreateEntity(Archetype archetype)
    {
        if (this != archetype.store) {
            throw InvalidStoreException(nameof(archetype));
        }
        var entity          = CreateEntity();
        entity.refArchetype = archetype;
        entity.refCompIndex = archetype.AddEntity(entity.id);
        return entity;
    }
}
