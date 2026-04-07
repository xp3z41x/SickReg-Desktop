using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui;
using SickReg.Desktop.Models;
using SickReg.Desktop.Services;
using SickReg.Desktop.Views.Pages;

namespace SickReg.Desktop.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private readonly IRegistryScannerService _scannerService;
    private readonly INavigationService _navigationService;
    private readonly ResultsViewModel _resultsViewModel;
    private readonly DashboardViewModel _dashboardViewModel;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private string _currentCategory = "";

    [ObservableProperty]
    private string _statusMessage = "Pronto para escanear";

    [ObservableProperty]
    private int _issuesFound;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _scanCompleted;

    public ScanViewModel(
        IRegistryScannerService scannerService,
        INavigationService navigationService,
        ResultsViewModel resultsViewModel,
        DashboardViewModel dashboardViewModel)
    {
        _scannerService = scannerService;
        _navigationService = navigationService;
        _resultsViewModel = resultsViewModel;
        _dashboardViewModel = dashboardViewModel;
    }

    [RelayCommand]
    private async Task StartScan()
    {
        if (IsScanning) return;

        IsScanning = true;
        ScanCompleted = false;
        ProgressPercentage = 0;
        IssuesFound = 0;
        CurrentCategory = "";
        StatusMessage = "Iniciando scan...";

        _cts = new CancellationTokenSource();
        var progress = new Progress<ScanProgress>(p =>
        {
            ProgressPercentage = p.OverallPercentage;
            CurrentCategory = p.CurrentCategory;
            IssuesFound = p.IssuesFoundSoFar;
            StatusMessage = p.StatusMessage;
        });

        try
        {
            var result = await _scannerService.ScanAsync(progress, _cts.Token);
            _resultsViewModel.LoadResults(result);
            _dashboardViewModel.UpdateLastScan(result.Issues.Count, result.ScanDate);
            StatusMessage = $"Scan concluido em {result.Duration.TotalSeconds:F1}s - {result.Issues.Count} problemas encontrados";
            ScanCompleted = true;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Scan cancelado pelo usuario";
        }
        finally
        {
            IsScanning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private void CancelScan()
    {
        _cts?.Cancel();
    }

    [RelayCommand]
    private void ViewResults()
    {
        _navigationService.Navigate(typeof(ResultsPage));
    }
}
