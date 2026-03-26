using DateVault.App.Services;
using DateVault.App.ViewModels;
using Forms = System.Windows.Forms;
using Wpf = System.Windows;
using WpfAnimation = System.Windows.Media.Animation;
using WpfControls = System.Windows.Controls;
using WpfInput = System.Windows.Input;
using WpfMedia = System.Windows.Media;
using WpfThreading = System.Windows.Threading;

namespace DateVault.App.Views;

public partial class MainWindow : Wpf.Window
{
    private readonly WpfMedia.SolidColorBrush _dropBrush = new((WpfMedia.Color)WpfMedia.ColorConverter.ConvertFromString("#EAF3FF"));
    private readonly WpfMedia.Color _dropBaseColor = (WpfMedia.Color)WpfMedia.ColorConverter.ConvertFromString("#EAF3FF");
    private readonly WpfMedia.Color _dropHighlightColor = (WpfMedia.Color)WpfMedia.ColorConverter.ConvertFromString("#D8E9FF");
    private bool _windowStateApplied;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        DropAreaBorder.Background = _dropBrush;
        Title = $"{AppIdentity.ProductName} {AppIdentity.Version}";
    }

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        ApplyDefaultWindowSize();
    }

    private void Window_Loaded(object sender, Wpf.RoutedEventArgs e)
    {
        try
        {
            ViewModel.Initialize();
            var fadeIn = new WpfAnimation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new WpfAnimation.CubicEase
                {
                    EasingMode = WpfAnimation.EasingMode.EaseOut
                }
            };

            BeginAnimation(OpacityProperty, fadeIn);
        }
        catch (Exception exception)
        {
            var messageDialog = new MessageDialog(
                "启动失败",
                exception.Message,
                MessageDialogKind.Error)
            {
                Owner = this
            };

            messageDialog.ShowDialog();
        }
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            var bounds = WindowState == Wpf.WindowState.Normal ? new Wpf.Rect(Left, Top, Width, Height) : RestoreBounds;
            ViewModel.SaveWindowState(
                bounds.Left,
                bounds.Top,
                bounds.Width,
                bounds.Height,
                WindowState == Wpf.WindowState.Maximized);
        }
        catch
        {
            // Ignore persistence errors during shutdown.
        }
    }

    private void WindowCloseButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        Close();
    }

    private void SelectRootButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "选择 DateVault 归档根目录",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            ViewModel.SetRootPath(dialog.SelectedPath);
        }
    }

    private void OpenTodayButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        ExecuteSafely(ViewModel.OpenTodayDirectory);
    }

    private void CreateFolderButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        var dialog = new InputDialog
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true)
        {
            ExecuteSafely(() => ViewModel.CreateFolder(dialog.InputText));
        }
    }

    private void RefreshButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        ExecuteSafely(() => ViewModel.RefreshTree());
    }

    private void SettingsButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        var dialog = new SettingsDialog(ViewModel.GetEditableSettings())
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true)
        {
            ExecuteSafely(() => ViewModel.ApplySettings(dialog.ResultConfig));
        }
    }

    private void AboutButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        var dialog = new AboutDialog
        {
            Owner = this
        };

        dialog.ShowDialog();
    }

    private void ArchiveTreeView_SelectedItemChanged(object sender, Wpf.RoutedPropertyChangedEventArgs<object> e)
    {
        ViewModel.SelectedNodeChanged(e.NewValue as FileTreeNodeViewModel);
        Dispatcher.BeginInvoke(new Action(BringSelectedTreeItemIntoView), WpfThreading.DispatcherPriority.Background);
    }

    private void TreeViewItem_Expanded(object sender, Wpf.RoutedEventArgs e)
    {
        if (e.OriginalSource is WpfControls.TreeViewItem treeViewItem && treeViewItem.DataContext is FileTreeNodeViewModel node)
        {
            ViewModel.EnsureNodeLoaded(node);
        }
    }

    private void TreeViewItem_PreviewMouseRightButtonDown(object sender, WpfInput.MouseButtonEventArgs e)
    {
        if (sender is WpfControls.TreeViewItem treeViewItem)
        {
            treeViewItem.Focus();
            treeViewItem.IsSelected = true;
            e.Handled = false;
        }
    }

    private void TreeViewItem_MouseDoubleClick(object sender, WpfInput.MouseButtonEventArgs e)
    {
        if (sender is WpfControls.TreeViewItem treeViewItem && treeViewItem.DataContext is FileTreeNodeViewModel)
        {
            ExecuteSafely(ViewModel.OpenSelected);
            e.Handled = true;
        }
    }

    private void ArchiveTreeView_KeyDown(object sender, WpfInput.KeyEventArgs e)
    {
        if (e.Key == WpfInput.Key.Enter)
        {
            ExecuteSafely(ViewModel.OpenSelected);
            e.Handled = true;
            return;
        }

        if (e.Key == WpfInput.Key.Delete)
        {
            DeleteSelectedWithConfirmation();
            e.Handled = true;
        }
    }

    private void LogListBox_MouseDoubleClick(object sender, WpfInput.MouseButtonEventArgs e)
    {
        if (sender is WpfControls.ListBox listBox && listBox.SelectedItem is LogItemViewModel item)
        {
            ExecuteSafely(() => ViewModel.RevealLogItem(item));
            e.Handled = true;
        }
    }

    private void DropAreaBorder_DragEnter(object sender, Wpf.DragEventArgs e)
    {
        UpdateDropState(e);
    }

    private void DropAreaBorder_DragOver(object sender, Wpf.DragEventArgs e)
    {
        UpdateDropState(e);
    }

    private void DropAreaBorder_DragLeave(object sender, Wpf.DragEventArgs e)
    {
        AnimateDropBackground(_dropBaseColor);
    }

    private void DropAreaBorder_Drop(object sender, Wpf.DragEventArgs e)
    {
        AnimateDropBackground(_dropBaseColor);

        if (!e.Data.GetDataPresent(Wpf.DataFormats.FileDrop))
        {
            return;
        }

        var paths = e.Data.GetData(Wpf.DataFormats.FileDrop) as string[];
        if (paths is null)
        {
            return;
        }

        ExecuteSafely(() => ViewModel.ArchiveFiles(paths));
    }

    private void OpenMenuItem_Click(object sender, Wpf.RoutedEventArgs e)
    {
        ExecuteSafely(ViewModel.OpenSelected);
    }

    private void RevealMenuItem_Click(object sender, Wpf.RoutedEventArgs e)
    {
        ExecuteSafely(ViewModel.RevealSelected);
    }

    private void CopyPathMenuItem_Click(object sender, Wpf.RoutedEventArgs e)
    {
        ExecuteSafely(ViewModel.CopySelectedPath);
    }

    private void DeleteMenuItem_Click(object sender, Wpf.RoutedEventArgs e)
    {
        DeleteSelectedWithConfirmation();
    }

    private void UpdateDropState(Wpf.DragEventArgs e)
    {
        var hasFiles = e.Data.GetDataPresent(Wpf.DataFormats.FileDrop);
        e.Effects = hasFiles ? Wpf.DragDropEffects.Move : Wpf.DragDropEffects.None;
        AnimateDropBackground(hasFiles ? _dropHighlightColor : _dropBaseColor);
        e.Handled = true;
    }

    private void AnimateDropBackground(WpfMedia.Color targetColor)
    {
        var animation = new WpfAnimation.ColorAnimation
        {
            To = targetColor,
            Duration = TimeSpan.FromMilliseconds(140),
            EasingFunction = new WpfAnimation.CubicEase
            {
                EasingMode = WpfAnimation.EasingMode.EaseOut
            }
        };

        _dropBrush.BeginAnimation(WpfMedia.SolidColorBrush.ColorProperty, animation);
    }

    private void ApplyDefaultWindowSize()
    {
        if (_windowStateApplied)
        {
            return;
        }

        var workArea = Wpf.SystemParameters.WorkArea;
        var savedState = ViewModel.GetSavedWindowState();
        var hasSavedBounds =
            savedState.Left.HasValue &&
            savedState.Top.HasValue &&
            savedState.Width.HasValue &&
            savedState.Height.HasValue;

        if (hasSavedBounds &&
            IsReasonableWindowBounds(
                workArea,
                savedState.Left!.Value,
                savedState.Top!.Value,
                savedState.Width!.Value,
                savedState.Height!.Value))
        {
            Left = savedState.Left.Value;
            Top = savedState.Top.Value;
            Width = Math.Max(MinWidth, savedState.Width.Value);
            Height = Math.Max(MinHeight, savedState.Height.Value);
            WindowState = savedState.IsMaximized ? Wpf.WindowState.Maximized : Wpf.WindowState.Normal;
            _windowStateApplied = true;
            return;
        }

        var targetWidth = Math.Min(Math.Max(MinWidth, Math.Round(workArea.Width * 0.38)), 820);
        var targetHeight = Math.Min(Math.Max(MinHeight, Math.Round(workArea.Height * 0.48)), 620);

        Width = targetWidth;
        Height = targetHeight;
        Left = workArea.Left + ((workArea.Width - Width) / 2);
        Top = workArea.Top + ((workArea.Height - Height) / 2);
        WindowState = Wpf.WindowState.Normal;
        _windowStateApplied = true;
    }

    private static bool IsReasonableWindowBounds(Wpf.Rect workArea, double left, double top, double width, double height)
    {
        if (width < 680 || height < 500)
        {
            return false;
        }

        if (width > workArea.Width * 1.2 || height > workArea.Height * 1.2)
        {
            return false;
        }

        var rect = new Wpf.Rect(left, top, width, height);
        var visibleRect = Wpf.Rect.Intersect(workArea, rect);
        return visibleRect.Width >= 240 && visibleRect.Height >= 180;
    }

    private void BringSelectedTreeItemIntoView()
    {
        var selectedContainer = FindSelectedContainer(ArchiveTreeView);
        selectedContainer?.BringIntoView();
    }

    private static WpfControls.TreeViewItem? FindSelectedContainer(WpfControls.ItemsControl parent)
    {
        foreach (var item in parent.Items)
        {
            if (parent.ItemContainerGenerator.ContainerFromItem(item) is WpfControls.TreeViewItem container)
            {
                if (container.IsSelected)
                {
                    return container;
                }

                var child = FindSelectedContainer(container);
                if (child is not null)
                {
                    return child;
                }
            }
        }

        return null;
    }

    private void ExecuteSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            var messageDialog = new MessageDialog(
                "操作提示",
                exception.Message,
                MessageDialogKind.Warning)
            {
                Owner = this
            };

            messageDialog.ShowDialog();
        }
    }

    private void DeleteSelectedWithConfirmation()
    {
        if (string.IsNullOrWhiteSpace(ViewModel.SelectedPath))
        {
            return;
        }

        var dialog = new ConfirmDialog(
            "移到回收站",
            $"确定要将“{ViewModel.SelectedName}”移到回收站吗？此操作不会直接永久删除。",
            "移到回收站")
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        ExecuteSafely(ViewModel.DeleteSelected);
    }
}
