using Wpf.Ui;
using Wpf.Ui.Controls;
using SickReg.Desktop.ViewModels;

namespace SickReg.Desktop.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow(
        MainWindowViewModel viewModel,
        INavigationService navigationService,
        ISnackbarService snackbarService)
    {
        DataContext = viewModel;
        InitializeComponent();

        navigationService.SetNavigationControl(RootNavigation);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);

        Loaded += (_, _) => RootNavigation.Navigate(typeof(Views.Pages.DashboardPage));
    }
}
