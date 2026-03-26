using DateVault.Domain.Services;

namespace DateVault.Application.UseCases;

public sealed class CreateFolderUseCase
{
    private readonly FolderService _folderService;

    public CreateFolderUseCase(FolderService folderService)
    {
        _folderService = folderService;
    }

    public string Execute(string parentDirectory, string folderName)
    {
        return _folderService.CreateFolder(parentDirectory, folderName);
    }
}
