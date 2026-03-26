using System.Collections.ObjectModel;
using DateVault.Domain.Models;
using DomainTreeNode = DateVault.Domain.Models.TreeNode;

namespace DateVault.App.ViewModels;

public sealed class FileTreeNodeViewModel : ObservableObject
{
    private bool _isExpanded;
    private bool _isLoaded;
    private bool _isSelected;

    public FileTreeNodeViewModel(
        string name,
        string fullPath,
        NodeType nodeType,
        bool hasChildren,
        FileTreeNodeViewModel? parent = null,
        bool isPlaceholder = false)
    {
        Name = name;
        FullPath = fullPath;
        NodeType = nodeType;
        HasChildren = hasChildren;
        Parent = parent;
        IsPlaceholder = isPlaceholder;

        if (hasChildren && !isPlaceholder)
        {
            Children.Add(CreatePlaceholder(this));
        }
    }

    public string Name { get; }

    public string FullPath { get; }

    public NodeType NodeType { get; }

    public bool HasChildren { get; }

    public bool IsPlaceholder { get; }

    public bool IsDirectory => NodeType == NodeType.Directory;

    public FileTreeNodeViewModel? Parent { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsLoaded
    {
        get => _isLoaded;
        set => SetProperty(ref _isLoaded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string TypeLabel => IsDirectory ? "目录" : "文件";

    public ObservableCollection<FileTreeNodeViewModel> Children { get; } = new();

    public void ReplaceChildren(IEnumerable<FileTreeNodeViewModel> nodes)
    {
        Children.Clear();
        foreach (var node in nodes)
        {
            Children.Add(node);
        }

        IsLoaded = true;
    }

    public static FileTreeNodeViewModel FromTreeNode(DomainTreeNode node, FileTreeNodeViewModel? parent = null)
    {
        return new FileTreeNodeViewModel(node.Name, node.FullPath, node.NodeType, node.HasChildren, parent);
    }

    private static FileTreeNodeViewModel CreatePlaceholder(FileTreeNodeViewModel parent)
    {
        return new FileTreeNodeViewModel("加载中", string.Empty, NodeType.File, hasChildren: false, parent, isPlaceholder: true);
    }
}
