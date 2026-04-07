using CommunityToolkit.Mvvm.ComponentModel;

namespace Core4.AvaloniaProbe.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}
