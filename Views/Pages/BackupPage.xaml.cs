using System.Windows.Controls;
using SickReg.Desktop.ViewModels;

namespace SickReg.Desktop.Views.Pages;

public partial class BackupPage : Page
{
    private readonly BackupViewModel _viewModel;

    public BackupPage(BackupViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        Loaded += (_, _) => _viewModel.RefreshBackups();
    }
}
