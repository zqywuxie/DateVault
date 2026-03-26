namespace DateVault.App.ViewModels;

public sealed class LogItemViewModel
{
    public string TimestampText { get; init; } = string.Empty;

    public string MessageText { get; init; } = string.Empty;

    public string DisplayText => $"{TimestampText} {MessageText}";

    public string LevelText { get; init; } = "信息";

    public string? ActionPath { get; init; }

    public bool CanReveal => !string.IsNullOrWhiteSpace(ActionPath);

    public bool IsError => string.Equals(LevelText, "错误", StringComparison.Ordinal);

    public bool IsWarning => string.Equals(LevelText, "警告", StringComparison.Ordinal);

    public string BadgeText => IsError ? "错" : IsWarning ? "警" : "信";

    public string ActionHintText => CanReveal ? "双击定位" : string.Empty;
}
