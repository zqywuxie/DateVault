namespace DateVault.Domain.Models;

public sealed class AppConfig
{
    public string RootPath { get; set; } = string.Empty;

    public ConflictPolicy ConflictPolicy { get; set; } = ConflictPolicy.AutoRename;

    public DefaultTargetMode DefaultTargetMode { get; set; } = DefaultTargetMode.TodayDirectory;

    public ArchiveOrganizationMode ArchiveOrganizationMode { get; set; } = ArchiveOrganizationMode.Direct;

    public string CustomCategoryRulesText { get; set; } = string.Empty;

    public double? WindowLeft { get; set; }

    public double? WindowTop { get; set; }

    public double? WindowWidth { get; set; }

    public double? WindowHeight { get; set; }

    public bool IsMaximized { get; set; }
}
