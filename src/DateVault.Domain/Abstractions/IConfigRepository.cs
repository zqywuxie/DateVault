using DateVault.Domain.Models;

namespace DateVault.Domain.Abstractions;

public interface IConfigRepository
{
    AppConfig Load();

    void Save(AppConfig config);
}
