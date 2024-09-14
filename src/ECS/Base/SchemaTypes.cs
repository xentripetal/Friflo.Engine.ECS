// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Engine.ECS.Index;
using Friflo.Engine.ECS.Relations;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable RedundantJumpStatement
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

readonly struct AssemblyType
{
    internal readonly Type type;
    internal readonly int assemblyIndex;
    internal readonly SchemaTypeKind kind;

    internal AssemblyType(Type type, SchemaTypeKind kind, int assemblyIndex)
    {
        this.type = type;
        this.kind = kind;
        this.assemblyIndex = assemblyIndex;
    }
}

sealed class SchemaTypes
{
    private const BindingFlags Flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

    internal readonly List<ComponentType> components = new ();
    private readonly List<AssemblyType> componentTypes = new ();
    internal readonly List<ScriptType> scripts = new ();
    private readonly List<AssemblyType> scriptTypes = new ();
    internal readonly List<TagType> tags = new ();
    private readonly List<AssemblyType> tagTypes = new ();
    internal int indexCount;

    internal void AddSchemaType(AssemblyType type)
    {
        switch (type.kind)
        {
            case SchemaTypeKind.Component:
                componentTypes.Add(type);
                break;

            case SchemaTypeKind.Tag:
                tagTypes.Add(type);
                break;

            case SchemaTypeKind.Script:
                scriptTypes.Add(type);
                break;
        }
    }

    internal EngineDependant[] CreateSchemaTypes(TypeStore typeStore, IList<Assembly> assemblies)
    {
        var assemblyCount = assemblies.Count;
        var engineTypes = new List<SchemaType>[assemblyCount];
        for (var n = 0; n < assemblyCount; n++)
        {
            engineTypes[n] = new List<SchemaType>();
        }
        OrderComponentTypes();
        foreach (var type in tagTypes)
        {
            var schemaType = CreateTagType(type.type);
            engineTypes[type.assemblyIndex].Add(schemaType);
        }
        foreach (var type in componentTypes)
        {
            var schemaType = CreateComponentType(type.type, typeStore);
            engineTypes[type.assemblyIndex].Add(schemaType);
        }
        foreach (var type in scriptTypes)
        {
            var schemaType = CreateScriptType(type.type, typeStore);
            engineTypes[type.assemblyIndex].Add(schemaType);
        }
        var dependants = new EngineDependant[assemblyCount];
        for (var n = 0; n < assemblyCount; n++)
        {
            dependants[n] = new EngineDependant(assemblies[n], engineTypes[n]);
        }
        return dependants;
    }

    private void OrderComponentTypes()
    {
        var count = componentTypes.Count;
        var isIndexType = new bool[count];
        var buffer = new AssemblyType[count];
        indexCount = 0;
        for (var n = 0; n < count; n++)
        {
            var type = componentTypes[n];
            buffer[n] = type;
            var isIndex = ComponentIndexUtils.GetIndexType(type.type, out _) != null ||
                          RelationComponentUtils.GetEntityRelationsType(type.type, out _) != null;
            isIndexType[n] = isIndex;
            if (isIndex) indexCount++;
        }
        componentTypes.Clear();
        for (var n = 0; n < count; n++)
        {
            if (isIndexType[n])
            {
                componentTypes.Add(buffer[n]);
            }
        }
        for (var n = 0; n < count; n++)
        {
            if (!isIndexType[n])
            {
                componentTypes.Add(buffer[n]);
            }
        }
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    private SchemaType CreateTagType(Type type)
    {
        var tagIndex = tags.Count + 1;
        var createParams = new object[]
        {
            tagIndex
        };
        var method = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateTagType), Flags);
        var genericMethod = method!.MakeGenericMethod(type);
        var tagType = (TagType)genericMethod.Invoke(null, createParams);
        tags.Add(tagType);
        return tagType;
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    private SchemaType CreateComponentType(Type type, TypeStore typeStore)
    {
        var structIndex = components.Count + 1;
        var indexType = ComponentIndexUtils.GetIndexType(type, out var indexValueType);
        var relationType = RelationComponentUtils.GetEntityRelationsType(type, out var keyType);
        var createParams = new object[]
        {
            typeStore, structIndex, indexType, indexValueType, relationType, keyType
        };
        var method = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateComponentType), Flags);
        var genericMethod = method!.MakeGenericMethod(type);
        var componentType = (ComponentType)genericMethod.Invoke(null, createParams);
        components.Add(componentType);
        return componentType;
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    private SchemaType CreateScriptType(Type type, TypeStore typeStore)
    {
        var scriptIndex = scripts.Count + 1;
        var createParams = new object[]
        {
            typeStore, scriptIndex
        };
        var method = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateScriptType), Flags);
        var genericMethod = method!.MakeGenericMethod(type);
        var scriptType = (ScriptType)genericMethod.Invoke(null, createParams);
        scripts.Add(scriptType);
        return scriptType;
    }
}
