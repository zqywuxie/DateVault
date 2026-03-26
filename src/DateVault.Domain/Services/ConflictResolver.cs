namespace DateVault.Domain.Services;

public sealed class ConflictResolver
{
    public string ResolvePath(string desiredPath, Func<string, bool> exists)
    {
        if (!exists(desiredPath))
        {
            return desiredPath;
        }

        var directory = Path.GetDirectoryName(desiredPath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(desiredPath);
        var extension = Path.GetExtension(desiredPath);
        var index = 1;

        while (true)
        {
            var candidate = Path.Combine(directory, $"{fileName}({index}){extension}");
            if (!exists(candidate))
            {
                return candidate;
            }

            index++;
        }
    }
}
