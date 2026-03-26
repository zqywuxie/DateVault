using System.IO;
using DateVault.Domain.Abstractions;
using DateVault.Domain.Models;

namespace DateVault.Infrastructure.FileSystem;

public sealed class FileSystemGateway : IFileSystemGateway
{
    public bool PathExists(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public void EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public IReadOnlyList<FileSystemEntryInfo> GetDirectoryEntries(string path)
    {
        if (!Directory.Exists(path))
        {
            return Array.Empty<FileSystemEntryInfo>();
        }

        var entries = new List<FileSystemEntryInfo>();

        foreach (var directory in Directory.EnumerateDirectories(path))
        {
            entries.Add(new FileSystemEntryInfo
            {
                Name = Path.GetFileName(directory),
                FullPath = directory,
                IsDirectory = true,
                HasChildren = Directory.EnumerateFileSystemEntries(directory).Take(1).Any()
            });
        }

        foreach (var file in Directory.EnumerateFiles(path))
        {
            entries.Add(new FileSystemEntryInfo
            {
                Name = Path.GetFileName(file),
                FullPath = file,
                IsDirectory = false,
                HasChildren = false
            });
        }

        return entries;
    }

    public string? GetParentDirectory(string path)
    {
        return Directory.Exists(path)
            ? Directory.GetParent(path)?.FullName
            : Path.GetDirectoryName(path);
    }

    public void MoveEntry(string sourcePath, string targetPath)
    {
        if (File.Exists(sourcePath))
        {
            File.Move(sourcePath, targetPath);
            return;
        }

        if (Directory.Exists(sourcePath))
        {
            Directory.Move(sourcePath, targetPath);
            return;
        }

        throw new FileNotFoundException("找不到待移动的文件或目录。", sourcePath);
    }
}
