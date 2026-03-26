using System.Collections.ObjectModel;
using System.IO;
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

    public string TypeLabel => IsDirectory ? "Folder" : "File";

    public string MarkerColor => ResolveMarkerColor();

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
        return new FileTreeNodeViewModel("Loading", string.Empty, NodeType.File, hasChildren: false, parent, isPlaceholder: true);
    }

    private string ResolveMarkerColor()
    {
        if (IsPlaceholder)
        {
            return "#C5CAD3";
        }

        if (IsDirectory)
        {
            return "#8EB8FF";
        }

        var extension = Path.GetExtension(Name);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return "#C5CAD3";
        }

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg" or ".ico" or ".heic" or ".raw" or ".tif" or ".tiff" => "#59C28A",
            ".mp4" or ".mov" or ".avi" or ".mkv" or ".wmv" or ".flv" or ".webm" or ".m4v" or ".mts" => "#F08A5D",
            ".mp3" or ".wav" or ".flac" or ".aac" or ".m4a" or ".ogg" or ".wma" or ".ape" => "#C678DD",
            ".doc" or ".docx" or ".txt" or ".md" or ".rtf" or ".odt" or ".pages" => "#5AA9E6",
            ".pdf" => "#E35D6A",
            ".xls" or ".xlsx" or ".csv" or ".ods" or ".numbers" => "#4CB782",
            ".ppt" or ".pptx" or ".odp" or ".key" => "#F3A64A",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" or ".xz" or ".iso" or ".cab" => "#A67C52",
            ".cs" or ".js" or ".jsx" or ".ts" or ".tsx" or ".json" or ".xml" or ".yml" or ".yaml" or ".toml" or ".ini" or ".py" or ".java" or ".cpp" or ".c" or ".h" or ".hpp" or ".go" or ".rs" or ".sql" or ".sh" or ".ps1" or ".bat" or ".cmd" => "#7B8CFF",
            ".exe" or ".msi" or ".msix" or ".appx" or ".apk" => "#7C8594",
            _ => "#C5CAD3"
        };
    }
}
