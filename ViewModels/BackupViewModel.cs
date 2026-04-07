using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui;
using Wpf.Ui.Controls;
using SickReg.Desktop.Models;
using SickReg.Desktop.Services;

namespace SickReg.Desktop.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly IRegistryBackupService _backupService;
    private readonly ISnackbarService _snackbarService;

    [ObservableProperty]
    private ObservableCollection<BackupEntry> _backups = [];

    [ObservableProperty]
    private bool _hasBackups;

    public BackupViewModel(
        IRegistryBackupService backupService,
        ISnackbarService snackbarService)
    {
        _backupService = backupService;
        _snackbarService = snackbarService;
    }

    public void RefreshBackups()
    {
        Backups = new ObservableCollection<BackupEntry>(_backupService.GetBackups());
        HasBackups = Backups.Count > 0;
    }

    [RelayCommand]
    private void RestoreBackup(BackupEntry? backup)
    {
        if (backup == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Deseja restaurar o backup '{backup.FileName}'?\nIsso ira reimportar as chaves de registro salvas.",
            "Restaurar Backup",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "regedit.exe",
                Arguments = $"/s \"{backup.FilePath}\"",
                UseShellExecute = true,
                Verb = "runas"
            });

            _snackbarService.Show("Restauracao Iniciada",
                "O backup esta sendo importado pelo regedit.",
                ControlAppearance.Info, null, TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            _snackbarService.Show("Erro", $"Falha ao restaurar: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5));
        }
    }

    [RelayCommand]
    private void DeleteBackup(BackupEntry? backup)
    {
        if (backup == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Deseja deletar permanentemente '{backup.FileName}'?",
            "Deletar Backup",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        _backupService.DeleteBackup(backup.FilePath);
        RefreshBackups();
        _snackbarService.Show("Backup Deletado", backup.FileName, ControlAppearance.Caution, null, TimeSpan.FromSeconds(3));
    }

    [RelayCommand]
    private void OpenBackupFolder()
    {
        var dir = _backupService.BackupDirectory;
        Directory.CreateDirectory(dir);
        Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
    }
}
