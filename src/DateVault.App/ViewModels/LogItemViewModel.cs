namespace DateVault.App.ViewModels;

public sealed class LogItemViewModel
{
    public string DisplayText { get; init; } = string.Empty;

    public string? ActionPath { get; init; }

    public bool CanReveal => !string.IsNullOrWhiteSpace(ActionPath);
}
