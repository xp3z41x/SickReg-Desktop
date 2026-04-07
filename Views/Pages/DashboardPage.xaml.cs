using System.Windows.Controls;
using SickReg.Desktop.ViewModels;

namespace SickReg.Desktop.Views.Pages;

public partial class DashboardPage : Page
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        Loaded += (_, _) => _viewModel.RefreshData();
    }
}
