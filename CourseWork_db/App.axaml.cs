using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace CourseWork_db;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            ShowFatalError(e.ExceptionObject as Exception);

        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            ShowFatalError(e.Exception);
            e.Handled = true;
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ShowFatalError(Exception? ex)
    {
        if (ex is null) return;

        var message = ex.InnerException?.Message ?? ex.Message;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {
            _ = new Window
            {
                Title = "Помилка",
                Width = 420,
                Height = 200,
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = message,
                        Margin = new Thickness(16),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                }
            }.ShowDialog(desktop.MainWindow);
        }
    }
}