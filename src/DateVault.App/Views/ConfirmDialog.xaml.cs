using Wpf = System.Windows;

namespace DateVault.App.Views;

public partial class ConfirmDialog : Wpf.Window
{
    public ConfirmDialog(string titleText, string messageText, string confirmText = "确定")
    {
        InitializeComponent();
        DataContext = new
        {
            TitleText = titleText,
            MessageText = messageText,
            ConfirmText = confirmText
        };
    }

    private void ConfirmButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
