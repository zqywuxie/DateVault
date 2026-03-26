using Wpf = System.Windows;
using WpfMedia = System.Windows.Media;

namespace DateVault.App.Views;

public partial class MessageDialog : Wpf.Window
{
    public MessageDialog(string titleText, string messageText, MessageDialogKind kind)
    {
        InitializeComponent();
        DataContext = new
        {
            TitleText = titleText,
            MessageText = messageText,
            BadgeText = GetBadgeText(kind),
            BadgeBackground = GetBadgeBackground(kind),
            BadgeForeground = GetBadgeForeground(kind)
        };
    }

    private void ConfirmButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private WpfMedia.Brush GetBadgeBackground(MessageDialogKind kind)
    {
        return (WpfMedia.Brush)FindResource(kind switch
        {
            MessageDialogKind.Error => "DangerSoftBrush",
            MessageDialogKind.Warning => "WarningSoftBrush",
            _ => "AccentSoftBrush"
        });
    }

    private WpfMedia.Brush GetBadgeForeground(MessageDialogKind kind)
    {
        return (WpfMedia.Brush)FindResource(kind switch
        {
            MessageDialogKind.Error => "DangerBrush",
            MessageDialogKind.Warning => "WarningBrush",
            _ => "AccentBrush"
        });
    }

    private static string GetBadgeText(MessageDialogKind kind)
    {
        return kind switch
        {
            MessageDialogKind.Error => "!",
            MessageDialogKind.Warning => "!",
            _ => "i"
        };
    }
}
