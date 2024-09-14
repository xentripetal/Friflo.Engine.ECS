// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace Friflo.Engine.ECS.Collections;

sealed class ExplorerItemEnumerator : IEnumerator<ExplorerItem>
{
    private readonly ExplorerItemTree tree;
    private ChildEnumerator entityEnumerator;

    internal ExplorerItemEnumerator(ExplorerItem item)
    {
        entityEnumerator = item.entity.ChildEntities.GetEnumerator();
        tree = item.tree;
    }

    public ExplorerItem Current => tree.GetItemById(entityEnumerator.Current.Id);
    object IEnumerator.Current => Current;

    // --- IEnumerator
    public bool MoveNext() => entityEnumerator.MoveNext();

    public void Reset() => entityEnumerator.Reset();

    public void Dispose() { }
}
