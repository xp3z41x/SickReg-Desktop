using System.Security.Principal;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui;
using SickReg.Desktop.Services;
using SickReg.Desktop.Views.Pages;

namespace SickReg.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IRegistryBackupService _backupService;

    [ObservableProperty]
    private int _lastScanIssues;

    [ObservableProperty]
    private string _lastScanDate = "Nunca";

    [ObservableProperty]
    private int _backupCount;

    [ObservableProperty]
    private bool _isAdmin;

    public DashboardViewModel(INavigationService navigationService, IRegistryBackupService backupService)
    {
        _navigationService = navigationService;
        _backupService = backupService;
        RefreshData();
    }

    public void RefreshData()
    {
        BackupCount = _backupService.GetBackups().Count();
        IsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
            .IsInRole(WindowsBuiltInRole.Administrator);
    }

    public void UpdateLastScan(int issueCount, DateTime scanDate)
    {
        LastScanIssues = issueCount;
        LastScanDate = scanDate.ToString("dd/MM/yyyy HH:mm");
    }

    [RelayCommand]
    private void StartScan()
    {
        _navigationService.Navigate(typeof(ScanPage));
    }

    [RelayCommand]
    private void GoToBackups()
    {
        _navigationService.Navigate(typeof(BackupPage));
    }
}
