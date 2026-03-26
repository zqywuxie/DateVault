using DateVault.App.Services;
using Wpf = System.Windows;

namespace DateVault.App.Views;

public partial class AboutDialog : Wpf.Window
{
    public AboutDialog()
    {
        InitializeComponent();
        DialogMotion.Attach(this);
        DataContext = new
        {
            ProductName = AppIdentity.ProductName,
            Description = AppIdentity.Description,
            Version = AppIdentity.Version,
            Company = AppIdentity.Company
        };
    }

    private void CheckUpdateButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        var dialog = new MessageDialog(
            "检查更新",
            $"当前版本：{AppIdentity.Version}{Environment.NewLine}{Environment.NewLine}尚未接入在线更新服务，请使用新的安装包覆盖安装。",
            MessageDialogKind.Information)
        {
            Owner = this
        };

        dialog.ShowDialog();
    }

    private void CloseButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        Close();
    }
}
