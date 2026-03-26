namespace DateVault.Domain.Models;

public sealed class FileSystemEntryInfo
{
    public string Name { get; init; } = string.Empty;

    public string FullPath { get; init; } = string.Empty;

    public bool IsDirectory { get; init; }

    public bool HasChildren { get; init; }
}
