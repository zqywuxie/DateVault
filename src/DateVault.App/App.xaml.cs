using System.Windows;
using DateVault.Application.UseCases;
using DateVault.App.ViewModels;
using DateVault.App.Views;
using DateVault.Domain.Services;
using DateVault.Infrastructure.Config;
using DateVault.Infrastructure.FileSystem;
using DateVault.Infrastructure.Shell;

namespace DateVault.App;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var fileSystemGateway = new FileSystemGateway();
        var configRepository = new JsonConfigRepository();
        var shellGateway = new ShellGateway();
        var clipboardGateway = new ClipboardGateway();

        var datePathService = new DatePathService();
        var conflictResolver = new ConflictResolver();
        var fileCategoryService = new FileCategoryService();
        var archiveService = new ArchiveService(fileSystemGateway, datePathService, conflictResolver, fileCategoryService);
        var folderService = new FolderService(fileSystemGateway, conflictResolver);
        var treeService = new TreeService(fileSystemGateway);

        var mainWindowViewModel = new MainWindowViewModel(
            configRepository,
            fileSystemGateway,
            shellGateway,
            clipboardGateway,
            datePathService,
            new ArchiveFilesUseCase(archiveService),
            new CreateFolderUseCase(folderService),
            new LoadTreeChildrenUseCase(treeService),
            fileCategoryService);

        var mainWindow = new MainWindow(mainWindowViewModel);
        mainWindow.Show();
    }
}
