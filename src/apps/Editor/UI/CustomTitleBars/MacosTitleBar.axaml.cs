using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CustomTitleBarTemplate.Views.CustomTitleBars;

/// <summary>template project (MIT): https://github.com/FrankenApps/Avalonia-CustomTitleBarTemplate</summary>
public partial class MacosTitleBar : UserControl
{
    private static readonly bool CustomSystemButtons = false;

    public static readonly StyledProperty<bool> IsSeamlessProperty =
        AvaloniaProperty.Register<MacosTitleBar, bool>(nameof(IsSeamless));
    private readonly Button closeButton;
    private readonly Button minimizeButton;
    private readonly StackPanel titleAndWindowIconWrapper;

    private readonly DockPanel titleBarBackground;
    private readonly Button zoomButton;

    public MacosTitleBar()
    {
        InitializeComponent();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) == false)
        {
            IsVisible = false;
        }
        else
        {
            if (CustomSystemButtons)
            {
                minimizeButton = this.FindControl<Button>("MinimizeButton");
                zoomButton = this.FindControl<Button>("ZoomButton");
                closeButton = this.FindControl<Button>("CloseButton");

                minimizeButton.Click += MinimizeWindow;
                zoomButton.Click += MaximizeWindow;
                closeButton.Click += CloseWindow;
            }

            titleBarBackground = this.FindControl<DockPanel>("TitleBarBackground");
            titleAndWindowIconWrapper = this.FindControl<StackPanel>("TitleAndWindowIconWrapper");

            SubscribeToWindowState();
        }
    }

    public bool IsSeamless
    {
        get => GetValue(IsSeamlessProperty);
        set
        {
            SetValue(IsSeamlessProperty, value);
            if (titleBarBackground != null && titleAndWindowIconWrapper != null)
            {
                titleBarBackground.IsVisible = IsSeamless ? false : true;
                titleAndWindowIconWrapper.IsVisible = IsSeamless ? false : true;
            }
        }
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        var hostWindow = (Window)VisualRoot;
        hostWindow.Close();
    }

    private void MaximizeWindow(object sender, RoutedEventArgs e)
    {
        var hostWindow = (Window)VisualRoot;

        if (hostWindow.WindowState == WindowState.Normal)
        {
            hostWindow.WindowState = WindowState.Maximized;
        }
        else
        {
            hostWindow.WindowState = WindowState.Normal;
        }
    }

    private void MinimizeWindow(object sender, RoutedEventArgs e)
    {
        var hostWindow = (Window)VisualRoot;
        hostWindow.WindowState = WindowState.Minimized;
    }

    async private void SubscribeToWindowState()
    {
        var hostWindow = (Window)VisualRoot;

        while (hostWindow == null)
        {
            hostWindow = (Window)VisualRoot;
            await Task.Delay(50);
        }

        hostWindow.ExtendClientAreaTitleBarHeightHint = 44;
        hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s => {
            if (s != WindowState.Maximized)
            {
                hostWindow.Padding = new Thickness(0, 0, 0, 0);
            }
            if (s == WindowState.Maximized)
            {
                hostWindow.Padding = new Thickness(7, 7, 7, 7);

                // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                /*hostWindow.Padding = new Thickness(
                        hostWindow.OffScreenMargin.Left,
                        hostWindow.OffScreenMargin.Top,
                        hostWindow.OffScreenMargin.Right,
                        hostWindow.OffScreenMargin.Bottom);*/
            }
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
