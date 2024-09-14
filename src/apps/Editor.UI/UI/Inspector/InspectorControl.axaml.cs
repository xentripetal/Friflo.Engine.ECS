﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Friflo.Editor.Utils;

namespace Friflo.Editor.UI.Inspector;

public partial class InspectorControl : UserControl
{
    internal readonly InspectorControlModel model = new ();

    public InspectorControl()
    {
        DataContext = model;
        InitializeComponent();

        TagGroup.GroupAdd.AddSchemaTypes(this, "tags");
        ComponentGroup.GroupAdd.AddSchemaTypes(this, "components");
        ScriptGroup.GroupAdd.AddSchemaTypes(this, "scripts");
        //
        if (!EditorUtils.IsDesignMode)
        {
            Tags.Children.Clear();
            Components.Children.Clear();
            Scripts.Children.Clear();
        }
    }

    internal InspectorObserver Observer { get; private set; }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        // designer example data
        if (EditorUtils.IsDesignMode)
        {
            model.EntityId = 123456789;
            model.TagCount = 4;
            model.ComponentCount = 3;
            model.ScriptCount = 1;
        }
        var appEvents = this.GetEditor();
        Observer = new InspectorObserver(this, appEvents);
        appEvents?.AddObserver(Observer);
    }
}
