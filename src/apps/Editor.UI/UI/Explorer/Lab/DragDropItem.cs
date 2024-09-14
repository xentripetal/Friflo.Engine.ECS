// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;

namespace Friflo.Editor.UI.Explorer.Lab;

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/Models/DragDropItem.cs
public class DragDropItem
{
    public static readonly ObservableList<DragDropItem> Root = CreateRandomItems();
    /// <summary>
    ///     use either:
    ///     <see cref="ObservableList{T}" />
    ///     <see cref="ObservableCollection{T}" />
    ///     <see cref="MyObservableCollection{T}" />
    /// </summary>
    public readonly ObservableList<DragDropItem> children; // Must always be present. Drop on an item with children == null result in COMException.
    public bool flag;

    public DragDropItem(string name)
    {
        Name = name;
        children = CreateObservable();
        children.CollectionChanged += (_, args) => {
            Console.WriteLine($"--- {args.Action}");
        };
    }

    public string Name { get; }

    private static ObservableList<DragDropItem> CreateObservable() => new ();

    // ReSharper disable once UnusedMember.Local
    private static MyObservableCollection<DragDropItem> CreateObservable_XXX()
    {
        var myCollection = new MyObservableCollection<DragDropItem>();
        myCollection.AddPropertyChangedHandler();
        return myCollection;
    }

    private static ObservableList<DragDropItem> CreateRandomItems()
    {
        var root = new DragDropItem("root");
        root.children.Add(new DragDropItem("child 1"));
        root.children.Add(new DragDropItem("child 2"));
        root.children.Add(new DragDropItem("child 3"));
        root.children.Add(new DragDropItem("child 4"));
        var result = CreateObservable();
        result.Add(root);
        result.CollectionChanged += (_, args) => {
            Console.WriteLine($"--- {args.Action}");
        };
        return result;
    }
}
