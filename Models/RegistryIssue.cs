using CommunityToolkit.Mvvm.ComponentModel;

namespace SickReg.Desktop.Models;

public partial class RegistryIssue : ObservableObject
{
    public required string FullKeyPath { get; init; }
    public string? ValueName { get; init; }
    public string? ValueData { get; init; }
    public required string Description { get; init; }
    public required RegistryCategory Category { get; init; }
    public DateTime DetectedAt { get; init; } = DateTime.Now;

    [ObservableProperty]
    private bool _isSelected = true;
}
