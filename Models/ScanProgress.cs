namespace SickReg.Desktop.Models;

public record ScanProgress(
    string CurrentCategory,
    double OverallPercentage,
    int IssuesFoundSoFar,
    string StatusMessage);
