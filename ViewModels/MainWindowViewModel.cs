using CommunityToolkit.Mvvm.ComponentModel;

namespace SickReg.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "SickReg";
}
