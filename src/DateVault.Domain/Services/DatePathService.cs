namespace DateVault.Domain.Services;

public sealed class DatePathService
{
    public string GetTodayPath(string rootPath)
    {
        return GetPath(rootPath, DateTime.Today);
    }

    public string GetPath(string rootPath, DateTime date)
    {
        return Path.Combine(rootPath, date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
    }

    public string GetRelativePath(DateTime date)
    {
        return Path.Combine(date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
    }
}
