using System.Windows.Controls;
using SickReg.Desktop.ViewModels;

namespace SickReg.Desktop.Views.Pages;

public partial class ResultsPage : Page
{
    public ResultsPage(ResultsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
