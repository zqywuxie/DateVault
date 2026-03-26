using DateVault.Domain.Abstractions;
using DateVault.Domain.Models;

namespace DateVault.Domain.Services;

public sealed class TreeService
{
    private readonly IFileSystemGateway _fileSystemGateway;

    public TreeService(IFileSystemGateway fileSystemGateway)
    {
        _fileSystemGateway = fileSystemGateway;
    }

    public IReadOnlyList<TreeNode> GetChildren(string directoryPath)
    {
        return _fileSystemGateway
            .GetDirectoryEntries(directoryPath)
            .OrderBy(static item => item.IsDirectory ? 0 : 1)
            .ThenBy(static item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Select(static item => new TreeNode
            {
                Name = item.Name,
                FullPath = item.FullPath,
                NodeType = item.IsDirectory ? NodeType.Directory : NodeType.File,
                HasChildren = item.HasChildren
            })
            .ToList();
    }
}
