using System.IO;
using System.Text.Json;
using DateVault.Domain.Abstractions;
using DateVault.Domain.Models;

namespace DateVault.Infrastructure.Config;

public sealed class JsonConfigRepository : IConfigRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _configFilePath;

    public JsonConfigRepository(string? configFilePath = null)
    {
        _configFilePath = configFilePath
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DateVault",
                "config.json");
    }

    public AppConfig Load()
    {
        if (!File.Exists(_configFilePath))
        {
            return new AppConfig();
        }

        try
        {
            var json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public void Save(AppConfig config)
    {
        var directory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(_configFilePath, json);
    }
}
