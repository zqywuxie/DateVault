namespace DateVault.Domain.Models;

public sealed class TreeNode
{
    public string Name { get; init; } = string.Empty;

    public string FullPath { get; init; } = string.Empty;

    public NodeType NodeType { get; init; }

    public bool HasChildren { get; init; }
}
