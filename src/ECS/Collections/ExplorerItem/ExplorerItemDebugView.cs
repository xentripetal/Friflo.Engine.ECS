// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Friflo.Engine.ECS.Collections;

sealed class ExplorerItemDebugView
{
    [Browse(Never)]
    private readonly ExplorerItem explorerItem;

    internal ExplorerItemDebugView(ExplorerItem item) => explorerItem = item;

    [Browse(RootHidden)]
    public ExplorerItem[] Items => GetItems();

    private ExplorerItem[] GetItems()
    {
        var items = new ExplorerItem[explorerItem.entity.ChildCount];
        var n = 0;
        foreach (var item in explorerItem)
        {
            items[n++] = item;
        }
        return items;
    }
}
