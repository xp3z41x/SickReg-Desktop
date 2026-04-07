using System.Windows.Controls;
using SickReg.Desktop.ViewModels;

namespace SickReg.Desktop.Views.Pages;

public partial class ScanPage : Page
{
    public ScanPage(ScanViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
