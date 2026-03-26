using DateVault.Domain.Models;
using DateVault.Domain.Services;

namespace DateVault.Application.UseCases;

public sealed class LoadTreeChildrenUseCase
{
    private readonly TreeService _treeService;

    public LoadTreeChildrenUseCase(TreeService treeService)
    {
        _treeService = treeService;
    }

    public IReadOnlyList<TreeNode> Execute(string directoryPath)
    {
        return _treeService.GetChildren(directoryPath);
    }
}
