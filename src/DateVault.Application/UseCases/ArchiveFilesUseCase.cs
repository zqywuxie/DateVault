using DateVault.Domain.Models;
using DateVault.Domain.Services;

namespace DateVault.Application.UseCases;

public sealed class ArchiveFilesUseCase
{
    private readonly ArchiveService _archiveService;

    public ArchiveFilesUseCase(ArchiveService archiveService)
    {
        _archiveService = archiveService;
    }

    public IReadOnlyList<ArchiveItemResult> ExecuteToDate(IEnumerable<string> sourcePaths, string rootPath, DateTime date)
    {
        return _archiveService.ArchiveToDateFolder(sourcePaths, rootPath, date);
    }

    public IReadOnlyList<ArchiveItemResult> ExecuteToDate(
        IEnumerable<string> sourcePaths,
        string rootPath,
        DateTime date,
        ArchiveOrganizationMode organizationMode,
        string? customCategoryRulesText)
    {
        return _archiveService.ArchiveToDateFolder(sourcePaths, rootPath, date, organizationMode, customCategoryRulesText);
    }

    public IReadOnlyList<ArchiveItemResult> ExecuteToDirectory(IEnumerable<string> sourcePaths, string targetDirectory)
    {
        return _archiveService.ArchiveToDirectory(sourcePaths, targetDirectory);
    }

    public IReadOnlyList<ArchiveItemResult> ExecuteToDirectory(
        IEnumerable<string> sourcePaths,
        string targetDirectory,
        ArchiveOrganizationMode organizationMode,
        string? customCategoryRulesText)
    {
        return _archiveService.ArchiveToDirectory(sourcePaths, targetDirectory, organizationMode, customCategoryRulesText);
    }
}
