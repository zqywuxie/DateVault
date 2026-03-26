namespace DateVault.Domain.Models;

public sealed class ArchiveItemResult
{
    public string SourcePath { get; init; } = string.Empty;

    public string TargetPath { get; init; } = string.Empty;

    public bool IsSuccess { get; init; }

    public string Message { get; init; } = string.Empty;
}
