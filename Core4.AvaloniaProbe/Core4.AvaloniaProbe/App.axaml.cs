using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using Core4.AvaloniaProbe.Views;

namespace Core4.AvaloniaProbe;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                desktop.MainWindow = new MainWindow();
            }
            catch (Exception error)
            {
                desktop.MainWindow = new MainWindow
                {
                    Content = CreateErrorView(error)
                };
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            try
            {
                singleViewPlatform.MainView = new ProbeView();
            }
            catch (Exception error)
            {
                singleViewPlatform.MainView = CreateErrorView(error);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static Control CreateErrorView(Exception error) => new Border
    {
        Background = new SolidColorBrush(Color.Parse("#fff7f4")),
        BorderBrush = new SolidColorBrush(Color.Parse("#d66d5c")),
        BorderThickness = new Thickness(2),
        Padding = new Thickness(18),
        Margin = new Thickness(18),
        Child = new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Core4 Avalonia Probe failed during startup.",
                        FontSize = 22,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = new SolidColorBrush(Color.Parse("#6a2116"))
                    },
                    new SelectableTextBlock
                    {
                        Text = error.ToString(),
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = FontFamily.Default,
                        FontSize = 14,
                        Foreground = new SolidColorBrush(Color.Parse("#3c2a25"))
                    }
                }
            }
        }
    };
}
