// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Friflo.Editor.Utils;

namespace Friflo.Editor.UI.Panels;

public class PanelControl : UserControl
{
    private AppEvents appEvents;

    protected PanelControl() => Focusable = true;

    internal PanelHeader Header { get; private set; }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        appEvents?.SetActivePanel(this); // appEvents is null in designer mode
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        appEvents.SetActivePanel(this);
        // Focus(); - calling Focus() explicit corrupt navigation with Key.Tab
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        appEvents = this.GetEditor();
        Header = EditorUtils.FindControl<PanelHeader>(this);
    }

    public virtual bool OnExecuteCommand(EditorCommand command) => false;
}
