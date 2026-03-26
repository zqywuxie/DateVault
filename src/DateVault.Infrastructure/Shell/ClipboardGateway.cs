using System.Windows;
using DateVault.Domain.Abstractions;

namespace DateVault.Infrastructure.Shell;

public sealed class ClipboardGateway : IClipboardGateway
{
    public void SetText(string text)
    {
        Clipboard.SetText(text);
    }
}
