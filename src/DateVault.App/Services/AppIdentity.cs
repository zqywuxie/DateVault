using System.Reflection;

namespace DateVault.App.Services;

public static class AppIdentity
{
    private static Assembly EntryAssembly => Assembly.GetEntryAssembly() ?? typeof(AppIdentity).Assembly;

    public static string ProductName =>
        EntryAssembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
        ?? "DateVault";

    public static string Description =>
        EntryAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description
        ?? "Lightweight Windows desktop file archiver.";

    public static string Version =>
        EntryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? EntryAssembly.GetName().Version?.ToString()
        ?? "1.0.0";

    public static string Company =>
        EntryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company
        ?? "DateVault";
}
