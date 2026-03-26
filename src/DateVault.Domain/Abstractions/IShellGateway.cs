namespace DateVault.Domain.Abstractions;

public interface IShellGateway
{
    void OpenWithShell(string path);

    void OpenDirectory(string path);

    void RevealInExplorer(string path);
}
