// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

sealed class View
{
    [Browse(Never)]
    private readonly BaseSystem system;

    internal View(BaseSystem system) => this.system = system;

    public UpdateTick Tick => system.Tick;
    public int Id => system.Id;
    public bool Enabled => system.Enabled;
    public string Name => system.Name;
    public SystemRoot SystemRoot => system.SystemRoot;
    public SystemGroup ParentGroup => system.ParentGroup;
    public SystemPerf Perf => system.perf;

    public override string ToString() => $"Enabled: {Enabled}  Id: {Id}";
}

class SystemGroupDebugView
{
//  public View                         System          => new View(group);

    [Browse(Never)]
    private readonly SystemGroup group;

    internal SystemGroupDebugView(SystemGroup group) => this.group = group;

    public ReadOnlyList<BaseSystem> ChildSystems => group.childSystems;
}

class SystemRootDebugView
{
//  public View                         System          => new View(root);

    [Browse(Never)]
    private readonly SystemRoot root;

    internal SystemRootDebugView(SystemRoot root) => this.root = root;

    public ReadOnlyList<BaseSystem> ChildSystems => root.childSystems;
    public ReadOnlyList<EntityStore> Stores => root.stores;
}
