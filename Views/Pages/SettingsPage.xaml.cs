using System.Windows.Controls;
using SickReg.Desktop.ViewModels;

namespace SickReg.Desktop.Views.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
