using Avalonia.Controls;

namespace Core4.AvaloniaProbe.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Content = new ProbeView();
    }
}
