using DateVault.Domain.Abstractions;
using DateVault.Domain.Models;

namespace DateVault.Domain.Services;

public sealed class ArchiveService
{
    private readonly IFileSystemGateway _fileSystemGateway;
    private readonly DatePathService _datePathService;
    private readonly ConflictResolver _conflictResolver;
    private readonly FileCategoryService _fileCategoryService;

    public ArchiveService(
        IFileSystemGateway fileSystemGateway,
        DatePathService datePathService,
        ConflictResolver conflictResolver,
        FileCategoryService fileCategoryService)
    {
        _fileSystemGateway = fileSystemGateway;
        _datePathService = datePathService;
        _conflictResolver = conflictResolver;
        _fileCategoryService = fileCategoryService;
    }

    public IReadOnlyList<ArchiveItemResult> ArchiveToDateFolder(
        IEnumerable<string> sourcePaths,
        string rootPath,
        DateTime date,
        ArchiveOrganizationMode organizationMode = ArchiveOrganizationMode.Direct,
        string? customCategoryRulesText = null)
    {
        var targetDirectory = _datePathService.GetPath(rootPath, date);
        return ArchiveToDirectory(sourcePaths, targetDirectory, organizationMode, customCategoryRulesText);
    }

    public IReadOnlyList<ArchiveItemResult> ArchiveToDirectory(
        IEnumerable<string> sourcePaths,
        string targetDirectory,
        ArchiveOrganizationMode organizationMode = ArchiveOrganizationMode.Direct,
        string? customCategoryRulesText = null)
    {
        var results = new List<ArchiveItemResult>();
        var customRules = _fileCategoryService.ParseCustomRules(customCategoryRulesText);
        _fileSystemGateway.EnsureDirectory(targetDirectory);

        foreach (var sourcePath in sourcePaths.Where(static path => !string.IsNullOrWhiteSpace(path)))
        {
            try
            {
                var normalizedSourcePath = sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var entryName = Path.GetFileName(normalizedSourcePath);
                var targetContainer = ResolveTargetContainer(normalizedSourcePath, targetDirectory, organizationMode, customRules);
                var desiredPath = Path.Combine(targetContainer, entryName);
                var targetPath = _conflictResolver.ResolvePath(desiredPath, _fileSystemGateway.PathExists);

                _fileSystemGateway.MoveEntry(normalizedSourcePath, targetPath);

                results.Add(new ArchiveItemResult
                {
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    IsSuccess = true,
                    Message = BuildSuccessMessage(targetContainer, targetDirectory)
                });
            }
            catch (Exception exception)
            {
                results.Add(new ArchiveItemResult
                {
                    SourcePath = sourcePath,
                    TargetPath = string.Empty,
                    IsSuccess = false,
                    Message = exception.Message
                });
            }
        }

        return results;
    }

    private string ResolveTargetContainer(
        string sourcePath,
        string targetDirectory,
        ArchiveOrganizationMode organizationMode,
        IReadOnlyDictionary<string, string> customRules)
    {
        if (organizationMode != ArchiveOrganizationMode.ByDataType)
        {
            _fileSystemGateway.EnsureDirectory(targetDirectory);
            return targetDirectory;
        }

        var isDirectory = _fileSystemGateway.DirectoryExists(sourcePath);
        var categoryFolderName = _fileCategoryService.GetCategoryFolderName(sourcePath, isDirectory, customRules);
        var categoryDirectory = Path.Combine(targetDirectory, categoryFolderName);
        _fileSystemGateway.EnsureDirectory(categoryDirectory);
        return categoryDirectory;
    }

    private static string BuildSuccessMessage(string targetContainer, string targetDirectory)
    {
        if (string.Equals(
            targetContainer.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            targetDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase))
        {
            return "已归档";
        }

        return $"已归档到 {Path.GetFileName(targetContainer)}";
    }
}
