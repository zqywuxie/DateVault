using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Specialized;
using DateVault.Application.UseCases;
using DateVault.App.Services;
using DateVault.Domain.Abstractions;
using DateVault.Domain.Models;
using DateVault.Domain.Services;

namespace DateVault.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly IConfigRepository _configRepository;
    private readonly IFileSystemGateway _fileSystemGateway;
    private readonly IShellGateway _shellGateway;
    private readonly IClipboardGateway _clipboardGateway;
    private readonly DatePathService _datePathService;
    private readonly ArchiveFilesUseCase _archiveFilesUseCase;
    private readonly CreateFolderUseCase _createFolderUseCase;
    private readonly LoadTreeChildrenUseCase _loadTreeChildrenUseCase;
    private readonly FileCategoryService _fileCategoryService;

    private AppConfig _config = new();
    private string _rootPath = string.Empty;
    private string _todayRelativePath = string.Empty;
    private string _archiveTargetPath = "未设置目标";
    private string _archiveOrganizationSummary = "直接归档到目标目录";
    private string _selectedName = "未选择";
    private string _selectedPath = string.Empty;
    private string _statusText = "就绪";
    private string _treeEmptyMessage = "请选择根目录后开始使用。";
    private FileTreeNodeViewModel? _selectedNode;

    public MainWindowViewModel(
        IConfigRepository configRepository,
        IFileSystemGateway fileSystemGateway,
        IShellGateway shellGateway,
        IClipboardGateway clipboardGateway,
        DatePathService datePathService,
        ArchiveFilesUseCase archiveFilesUseCase,
        CreateFolderUseCase createFolderUseCase,
        LoadTreeChildrenUseCase loadTreeChildrenUseCase,
        FileCategoryService fileCategoryService)
    {
        _configRepository = configRepository;
        _fileSystemGateway = fileSystemGateway;
        _shellGateway = shellGateway;
        _clipboardGateway = clipboardGateway;
        _datePathService = datePathService;
        _archiveFilesUseCase = archiveFilesUseCase;
        _createFolderUseCase = createFolderUseCase;
        _loadTreeChildrenUseCase = loadTreeChildrenUseCase;
        _fileCategoryService = fileCategoryService;

        TreeNodes.CollectionChanged += OnTreeNodesChanged;
    }

    public ObservableCollection<FileTreeNodeViewModel> TreeNodes { get; } = new();

    public ObservableCollection<LogItemViewModel> LogLines { get; } = new();

    public string RootPath
    {
        get => _rootPath;
        private set
        {
            if (SetProperty(ref _rootPath, value))
            {
                RaisePropertyChanged(nameof(HasRootPath));
            }
        }
    }

    public string TodayRelativePath
    {
        get => _todayRelativePath;
        private set => SetProperty(ref _todayRelativePath, value);
    }

    public string SelectedName
    {
        get => _selectedName;
        private set => SetProperty(ref _selectedName, value);
    }

    public string SelectedPath
    {
        get => _selectedPath;
        private set => SetProperty(ref _selectedPath, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string ArchiveTargetPath
    {
        get => _archiveTargetPath;
        private set => SetProperty(ref _archiveTargetPath, value);
    }

    public string TreeEmptyMessage
    {
        get => _treeEmptyMessage;
        private set => SetProperty(ref _treeEmptyMessage, value);
    }

    public string ArchiveOrganizationSummary
    {
        get => _archiveOrganizationSummary;
        private set => SetProperty(ref _archiveOrganizationSummary, value);
    }

    public int ArchiveCountToday { get; private set; }

    public string AppVersion => AppIdentity.Version;

    public bool HasRootPath => !string.IsNullOrWhiteSpace(RootPath);

    public bool HasTreeItems => TreeNodes.Count > 0;

    public void Initialize()
    {
        _config = _configRepository.Load();
        TodayRelativePath = _datePathService.GetRelativePath(DateTime.Today);
        RootPath = _config.RootPath;

        if (!string.IsNullOrWhiteSpace(RootPath))
        {
            RefreshTree();
            AddLog(LogLevel.Info, $"已加载根目录: {RootPath}", RootPath);
        }
        else
        {
            AddLog(LogLevel.Info, "请选择根目录后开始归档。");
        }
    }

    public void SetRootPath(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            return;
        }

        _fileSystemGateway.EnsureDirectory(rootPath);
        RootPath = rootPath;
        _config.RootPath = rootPath;
        _configRepository.Save(_config);

        RefreshTree();
        RecalculateArchiveTarget();
        AddLog(LogLevel.Info, $"根目录已更新: {rootPath}", rootPath);
    }

    public void RefreshTree(string? focusPath = null)
    {
        TreeNodes.Clear();
        SelectedNodeChanged(null);
        TodayRelativePath = _datePathService.GetRelativePath(DateTime.Today);

        if (string.IsNullOrWhiteSpace(RootPath) || !_fileSystemGateway.DirectoryExists(RootPath))
        {
            StatusText = "请选择有效根目录";
            ArchiveTargetPath = "未设置目标";
            TreeEmptyMessage = "请选择有效根目录后开始使用。";
            return;
        }

        foreach (var child in _loadTreeChildrenUseCase.Execute(RootPath))
        {
            TreeNodes.Add(FileTreeNodeViewModel.FromTreeNode(child));
        }

        if (!string.IsNullOrWhiteSpace(focusPath))
        {
            FocusPath(focusPath);
        }

        TreeEmptyMessage = TreeNodes.Count == 0
            ? "当前根目录下还没有内容，可以拖入文件或新建文件夹。"
            : string.Empty;
        StatusText = "目录已刷新";
        RecalculateArchiveTarget();
    }

    public void EnsureNodeLoaded(FileTreeNodeViewModel? node)
    {
        if (node is null || !node.IsDirectory || node.IsLoaded)
        {
            return;
        }

        try
        {
            var children = _loadTreeChildrenUseCase
                .Execute(node.FullPath)
                .Select(child => FileTreeNodeViewModel.FromTreeNode(child, node))
                .ToList();

            node.ReplaceChildren(children);
        }
        catch (Exception exception)
        {
            AddLog(LogLevel.Error, $"加载目录失败: {exception.Message}", node.FullPath);
        }
    }

    public void SelectedNodeChanged(FileTreeNodeViewModel? node)
    {
        _selectedNode = node;
        SelectedName = node?.Name ?? "未选择";
        SelectedPath = node?.FullPath ?? string.Empty;
        StatusText = node is null ? "就绪" : $"已选择: {node.Name}";
        RecalculateArchiveTarget();
    }

    public void CreateFolder(string folderName)
    {
        EnsureRootPath();

        var parentDirectory = ResolveCreateFolderTargetDirectory();
        var createdPath = _createFolderUseCase.Execute(parentDirectory, folderName);

        RefreshTree(createdPath);
        AddLog(LogLevel.Info, $"已创建文件夹: {createdPath}", createdPath);
    }

    public void ArchiveFiles(IEnumerable<string> paths)
    {
        EnsureRootPath();

        var sourcePaths = paths.Where(static path => !string.IsNullOrWhiteSpace(path)).Distinct().ToList();
        if (sourcePaths.Count == 0)
        {
            return;
        }

        var targetDirectory = ResolveArchiveTargetDirectory();
        var results = _archiveFilesUseCase.ExecuteToDirectory(
            sourcePaths,
            targetDirectory,
            _config.ArchiveOrganizationMode,
            _config.CustomCategoryRulesText);

        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                ArchiveCountToday++;
                AddLog(LogLevel.Info, $"{result.Message}: {Path.GetFileName(result.SourcePath)}", result.TargetPath);
            }
            else
            {
                AddLog(LogLevel.Error, $"归档失败: {Path.GetFileName(result.SourcePath)} - {result.Message}", result.SourcePath);
            }
        }

        RaisePropertyChanged(nameof(ArchiveCountToday));
        RefreshTree(targetDirectory);
    }

    public void OpenSelected()
    {
        if (_selectedNode is null)
        {
            return;
        }

        if (_selectedNode.IsDirectory)
        {
            _shellGateway.OpenDirectory(_selectedNode.FullPath);
        }
        else
        {
            _shellGateway.OpenWithShell(_selectedNode.FullPath);
        }
    }

    public void RevealSelected()
    {
        if (_selectedNode is null)
        {
            return;
        }

        if (_selectedNode.IsDirectory)
        {
            _shellGateway.OpenDirectory(_selectedNode.FullPath);
        }
        else
        {
            _shellGateway.RevealInExplorer(_selectedNode.FullPath);
        }
    }

    public void CopySelectedPath()
    {
        if (_selectedNode is null)
        {
            return;
        }

        _clipboardGateway.SetText(_selectedNode.FullPath);
        AddLog(LogLevel.Info, $"已复制路径: {_selectedNode.FullPath}", _selectedNode.FullPath);
    }

    public void DeleteSelected()
    {
        if (_selectedNode is null)
        {
            return;
        }

        var targetNode = _selectedNode;
        var targetPath = targetNode.FullPath;
        var targetName = targetNode.Name;
        var parentPath = _fileSystemGateway.GetParentDirectory(targetPath);

        _fileSystemGateway.DeleteEntry(targetPath);
        RefreshTree(parentPath);
        AddLog(LogLevel.Warning, $"已移到回收站: {targetName}", parentPath ?? RootPath);
    }

    public void OpenTodayDirectory()
    {
        EnsureRootPath();

        var todayPath = _datePathService.GetTodayPath(RootPath);
        _fileSystemGateway.EnsureDirectory(todayPath);
        _shellGateway.OpenDirectory(todayPath);
    }

    public AppConfig GetEditableSettings()
    {
        return new AppConfig
        {
            RootPath = RootPath,
            ConflictPolicy = _config.ConflictPolicy,
            DefaultTargetMode = _config.DefaultTargetMode,
            ArchiveOrganizationMode = _config.ArchiveOrganizationMode,
            CustomCategoryRulesText = _config.CustomCategoryRulesText,
            WindowLeft = _config.WindowLeft,
            WindowTop = _config.WindowTop,
            WindowWidth = _config.WindowWidth,
            WindowHeight = _config.WindowHeight,
            IsMaximized = _config.IsMaximized
        };
    }

    public void ApplySettings(AppConfig config)
    {
        _config = new AppConfig
        {
            RootPath = config.RootPath?.Trim() ?? string.Empty,
            ConflictPolicy = config.ConflictPolicy,
            DefaultTargetMode = config.DefaultTargetMode,
            ArchiveOrganizationMode = config.ArchiveOrganizationMode,
            CustomCategoryRulesText = config.CustomCategoryRulesText ?? string.Empty,
            WindowLeft = _config.WindowLeft,
            WindowTop = _config.WindowTop,
            WindowWidth = _config.WindowWidth,
            WindowHeight = _config.WindowHeight,
            IsMaximized = _config.IsMaximized
        };

        if (!string.IsNullOrWhiteSpace(_config.RootPath))
        {
            _fileSystemGateway.EnsureDirectory(_config.RootPath);
        }

        RootPath = _config.RootPath;
        _configRepository.Save(_config);
        RefreshTree();
        RecalculateArchiveTarget();
        AddLog(
            LogLevel.Info,
            $"设置已更新，目标: {GetTargetModeDisplayName(_config.DefaultTargetMode)}，整理: {GetArchiveOrganizationDisplayName(_config.ArchiveOrganizationMode)}",
            RootPath);
    }

    public (
        double? Left,
        double? Top,
        double? Width,
        double? Height,
        bool IsMaximized) GetSavedWindowState()
    {
        return (
            _config.WindowLeft,
            _config.WindowTop,
            _config.WindowWidth,
            _config.WindowHeight,
            _config.IsMaximized);
    }

    public void SaveWindowState(double left, double top, double width, double height, bool isMaximized)
    {
        _config.WindowLeft = left;
        _config.WindowTop = top;
        _config.WindowWidth = width;
        _config.WindowHeight = height;
        _config.IsMaximized = isMaximized;
        _configRepository.Save(_config);
    }

    public void RevealLogItem(LogItemViewModel? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.ActionPath))
        {
            return;
        }

        if (_fileSystemGateway.DirectoryExists(item.ActionPath))
        {
            _shellGateway.OpenDirectory(item.ActionPath);
            return;
        }

        if (_fileSystemGateway.FileExists(item.ActionPath))
        {
            _shellGateway.RevealInExplorer(item.ActionPath);
        }
    }

    private string ResolveCreateFolderTargetDirectory()
    {
        if (_selectedNode is null)
        {
            var todayDirectory = _datePathService.GetTodayPath(RootPath);
            _fileSystemGateway.EnsureDirectory(todayDirectory);
            return todayDirectory;
        }

        if (_selectedNode.IsDirectory)
        {
            return _selectedNode.FullPath;
        }

        return _fileSystemGateway.GetParentDirectory(_selectedNode.FullPath)
            ?? _datePathService.GetTodayPath(RootPath);
    }

    private string ResolveArchiveTargetDirectory()
    {
        if (_config.DefaultTargetMode == DefaultTargetMode.SelectedDirectory && _selectedNode is not null)
        {
            if (_selectedNode.IsDirectory)
            {
                _fileSystemGateway.EnsureDirectory(_selectedNode.FullPath);
                return _selectedNode.FullPath;
            }

            var parentDirectory = _fileSystemGateway.GetParentDirectory(_selectedNode.FullPath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
            {
                _fileSystemGateway.EnsureDirectory(parentDirectory);
                return parentDirectory;
            }
        }

        var todayDirectory = _datePathService.GetTodayPath(RootPath);
        _fileSystemGateway.EnsureDirectory(todayDirectory);
        return todayDirectory;
    }

    private void RecalculateArchiveTarget()
    {
        if (string.IsNullOrWhiteSpace(RootPath))
        {
            ArchiveTargetPath = "未设置目标";
            ArchiveOrganizationSummary = "请先设置根目录";
            return;
        }

        var targetDirectory = ResolveArchiveTargetDirectory();
        var customRuleCount = _fileCategoryService.CountCustomRuleEntries(_config.CustomCategoryRulesText);
        ArchiveTargetPath = _config.ArchiveOrganizationMode == ArchiveOrganizationMode.ByDataType
            ? $"{targetDirectory} / 按类型子文件夹"
            : targetDirectory;
        ArchiveOrganizationSummary = _config.ArchiveOrganizationMode == ArchiveOrganizationMode.ByDataType
            ? customRuleCount > 0
                ? $"会自动创建类型子文件夹，且有 {customRuleCount} 条自定义扩展名规则生效"
                : "会自动创建 图片 / 文档 / 视频 / 压缩包 等子文件夹"
            : "直接归档到当前目标目录，不再细分类型";
    }

    private void FocusPath(string path)
    {
        var normalizedPath = NormalizePath(path);
        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            return;
        }

        ClearSelection(TreeNodes);

        foreach (var rootNode in TreeNodes)
        {
            if (TryFocusNode(rootNode, normalizedPath))
            {
                break;
            }
        }
    }

    private bool TryFocusNode(FileTreeNodeViewModel node, string targetPath)
    {
        var nodePath = NormalizePath(node.FullPath);
        if (string.IsNullOrWhiteSpace(nodePath))
        {
            return false;
        }

        if (PathsEqual(nodePath, targetPath))
        {
            SelectNode(node);
            return true;
        }

        if (!node.IsDirectory || !PathContains(nodePath, targetPath))
        {
            return false;
        }

        node.IsExpanded = true;
        EnsureNodeLoaded(node);

        foreach (var child in node.Children.Where(static child => !child.IsPlaceholder))
        {
            if (TryFocusNode(child, targetPath))
            {
                node.IsExpanded = true;
                return true;
            }
        }

        SelectNode(node);
        return true;
    }

    private void SelectNode(FileTreeNodeViewModel node)
    {
        node.IsExpanded = node.IsDirectory && node.HasChildren;
        node.IsSelected = true;
        SelectedNodeChanged(node);
    }

    private static void ClearSelection(IEnumerable<FileTreeNodeViewModel> nodes)
    {
        foreach (var node in nodes)
        {
            node.IsSelected = false;
            if (node.Children.Count > 0)
            {
                ClearSelection(node.Children);
            }
        }
    }

    private static bool PathsEqual(string left, string right)
    {
        return string.Equals(
            NormalizePath(left),
            NormalizePath(right),
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool PathContains(string parentPath, string childPath)
    {
        var normalizedParent = NormalizePath(parentPath);
        var normalizedChild = NormalizePath(childPath);

        if (string.IsNullOrWhiteSpace(normalizedParent) || string.IsNullOrWhiteSpace(normalizedChild))
        {
            return false;
        }

        return normalizedChild.StartsWith(
            normalizedParent.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private void EnsureRootPath()
    {
        if (string.IsNullOrWhiteSpace(RootPath))
        {
            throw new InvalidOperationException("请先选择根目录。");
        }
    }

    private static string GetTargetModeDisplayName(DefaultTargetMode mode)
    {
        return mode == DefaultTargetMode.SelectedDirectory ? "优先当前选中目录" : "今日目录";
    }

    private static string GetArchiveOrganizationDisplayName(ArchiveOrganizationMode mode)
    {
        return mode == ArchiveOrganizationMode.ByDataType ? "按数据类型归档" : "直接归档";
    }

    private void AddLog(LogLevel level, string message, string? actionPath = null)
    {
        var prefix = level switch
        {
            LogLevel.Warning => "警告",
            LogLevel.Error => "错误",
            _ => "信息"
        };

        LogLines.Insert(0, new LogItemViewModel
        {
            DisplayText = $"{DateTime.Now:HH:mm:ss} [{prefix}] {message}",
            ActionPath = actionPath
        });

        while (LogLines.Count > 100)
        {
            LogLines.RemoveAt(LogLines.Count - 1);
        }
    }

    private void OnTreeNodesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(HasTreeItems));
    }
}
