using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui;
using Wpf.Ui.Controls;
using SickReg.Desktop.Models;
using SickReg.Desktop.Services;

namespace SickReg.Desktop.ViewModels;

public partial class ResultsGroupItem : ObservableObject
{
    public RegistryCategory Category { get; init; }
    public string DisplayName { get; init; } = "";
    public ObservableCollection<RegistryIssue> Issues { get; init; } = [];

    [ObservableProperty]
    private bool _isExpanded;

    public int Count => Issues.Count;
    public int SelectedCount => Issues.Count(i => i.IsSelected);
}

public partial class ResultsViewModel : ObservableObject
{
    private readonly IRegistryBackupService _backupService;
    private readonly IRegistryCleanerService _cleanerService;
    private readonly ISnackbarService _snackbarService;

    [ObservableProperty]
    private ObservableCollection<ResultsGroupItem> _groups = [];

    [ObservableProperty]
    private int _totalIssues;

    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private bool _hasResults;

    [ObservableProperty]
    private bool _isCleaning;

    public ResultsViewModel(
        IRegistryBackupService backupService,
        IRegistryCleanerService cleanerService,
        ISnackbarService snackbarService)
    {
        _backupService = backupService;
        _cleanerService = cleanerService;
        _snackbarService = snackbarService;
    }

    public void LoadResults(ScanResult result)
    {
        Groups.Clear();

        foreach (var group in result.Issues.GroupBy(i => i.Category))
        {
            var groupItem = new ResultsGroupItem
            {
                Category = group.Key,
                DisplayName = group.Key.GetDisplayName(),
                Issues = new ObservableCollection<RegistryIssue>(group)
            };

            foreach (var issue in groupItem.Issues)
            {
                issue.PropertyChanged += (_, e) =>
                {
                    if (!_suppressSelectionUpdate && e.PropertyName == nameof(RegistryIssue.IsSelected))
                        UpdateSelectedCount();
                };
            }

            Groups.Add(groupItem);
        }

        TotalIssues = result.Issues.Count;
        HasResults = result.Issues.Count > 0;
        UpdateSelectedCount();
    }

    private void UpdateSelectedCount()
    {
        SelectedCount = Groups.SelectMany(g => g.Issues).Count(i => i.IsSelected);
    }

    [RelayCommand]
    private void SelectAll()
    {
        SetAllSelection(true);
    }

    [RelayCommand]
    private void DeselectAll()
    {
        SetAllSelection(false);
    }

    private void SetAllSelection(bool selected)
    {
        _suppressSelectionUpdate = true;
        foreach (var issue in Groups.SelectMany(g => g.Issues))
            issue.IsSelected = selected;
        _suppressSelectionUpdate = false;
        UpdateSelectedCount();
    }

    private bool _suppressSelectionUpdate;

    [RelayCommand]
    private async Task CleanSelected()
    {
        if (IsCleaning) return;

        var selected = Groups.SelectMany(g => g.Issues).Where(i => i.IsSelected).ToList();
        if (selected.Count == 0) return;

        var msgResult = System.Windows.MessageBox.Show(
            $"{selected.Count} itens serao removidos do registro.\nUm backup sera criado automaticamente antes da limpeza.\n\nDeseja continuar?",
            "Confirmar Limpeza",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (msgResult != System.Windows.MessageBoxResult.Yes) return;

        IsCleaning = true;

        try
        {
            // Backup first
            var backupPath = await _backupService.BackupAsync(selected);
            _snackbarService.Show("Backup Criado", $"Salvo em: {Path.GetFileName(backupPath)}", ControlAppearance.Success, null, TimeSpan.FromSeconds(3));

            // Clean
            var (succeeded, failed) = await _cleanerService.CleanAsync(selected);

            // Remove cleaned items from UI
            foreach (var group in Groups.ToList())
            {
                foreach (var issue in selected)
                    group.Issues.Remove(issue);

                if (group.Issues.Count == 0)
                    Groups.Remove(group);
            }

            TotalIssues = Groups.SelectMany(g => g.Issues).Count();
            HasResults = TotalIssues > 0;
            UpdateSelectedCount();

            _snackbarService.Show("Limpeza Concluida",
                $"{succeeded} itens removidos com sucesso. {failed} falharam.",
                failed > 0 ? ControlAppearance.Caution : ControlAppearance.Success,
                null, TimeSpan.FromSeconds(4));
        }
        catch (Exception ex)
        {
            _snackbarService.Show("Erro", $"Falha na limpeza: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5));
        }
        finally
        {
            IsCleaning = false;
        }
    }
}
