using DateVault.Domain.Models;

namespace DateVault.Domain.Abstractions;

public interface IFileSystemGateway
{
    bool PathExists(string path);

    bool FileExists(string path);

    bool DirectoryExists(string path);

    void EnsureDirectory(string path);

    IReadOnlyList<FileSystemEntryInfo> GetDirectoryEntries(string path);

    string? GetParentDirectory(string path);

    void MoveEntry(string sourcePath, string targetPath);
}
