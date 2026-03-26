using DateVault.App.Services;
using System.Windows;

namespace DateVault.App.Views;

public partial class InputDialog : Window
{
    public InputDialog()
    {
        InitializeComponent();
        DialogMotion.Attach(this);
        Loaded += (_, _) => FolderNameTextBox.Focus();
    }

    public string InputText => FolderNameTextBox.Text.Trim();

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
