using DateVault.Domain.Abstractions;

namespace DateVault.Domain.Services;

public sealed class FolderService
{
    private readonly IFileSystemGateway _fileSystemGateway;
    private readonly ConflictResolver _conflictResolver;

    public FolderService(IFileSystemGateway fileSystemGateway, ConflictResolver conflictResolver)
    {
        _fileSystemGateway = fileSystemGateway;
        _conflictResolver = conflictResolver;
    }

    public string CreateFolder(string parentDirectory, string folderName)
    {
        ValidateFolderName(folderName);

        var desiredPath = Path.Combine(parentDirectory, folderName);
        var targetPath = _conflictResolver.ResolvePath(desiredPath, _fileSystemGateway.PathExists);
        _fileSystemGateway.EnsureDirectory(targetPath);
        return targetPath;
    }

    private static void ValidateFolderName(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
        {
            throw new InvalidOperationException("文件夹名称不能为空。");
        }

        if (folderName.Length > 128)
        {
            throw new InvalidOperationException("文件夹名称不能超过 128 个字符。");
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        if (folderName.IndexOfAny(invalidChars) >= 0)
        {
            throw new InvalidOperationException("文件夹名称包含非法字符。");
        }
    }
}
