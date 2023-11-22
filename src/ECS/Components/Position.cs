﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[Component("pos")]
[StructLayout(LayoutKind.Explicit)]
public struct  Position : IComponent, IEquatable<Position>
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset(0)] public     Vector3 value;  // 12
    //
    [FieldOffset(0)] public     float   x;      // (4)
    [FieldOffset(4)] public     float   y;      // (4)
    [FieldOffset(8)] public     float   z;      // (4)
    
    [Ignore] public float ValueX { get => x; set => x = value; }
    [Ignore] public float ValueY { get => y; set => y = value; }
    [Ignore] public float ValueZ { get => z; set => z = value; }

    public readonly override string ToString() => $"{x}, {y}, {z}";

    public Position (float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public          bool    Equals      (Position other)                    => value == other.value;
    public static   bool    operator == (in Position p1, in Position p2)    => p1.value == p2.value;
    public static   bool    operator != (in Position p1, in Position p2)    => p1.value != p2.value;

    [ExcludeFromCodeCoverage] public override   int     GetHashCode()       => throw new NotImplementedException("to avoid boxing");
    [ExcludeFromCodeCoverage] public override   bool    Equals(object obj)  => throw new NotImplementedException("to avoid boxing");
}