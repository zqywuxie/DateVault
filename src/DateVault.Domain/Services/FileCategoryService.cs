namespace DateVault.Domain.Services;

public sealed class FileCategoryService
{
    private static readonly char[] InvalidFolderNameChars = Path.GetInvalidFileNameChars();

    private static readonly Dictionary<string, string> ExtensionToCategory = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "图片",
        [".jpeg"] = "图片",
        [".png"] = "图片",
        [".gif"] = "图片",
        [".webp"] = "图片",
        [".bmp"] = "图片",
        [".tif"] = "图片",
        [".tiff"] = "图片",
        [".svg"] = "图片",
        [".ico"] = "图片",
        [".heic"] = "图片",
        [".raw"] = "图片",

        [".mp4"] = "视频",
        [".mov"] = "视频",
        [".avi"] = "视频",
        [".mkv"] = "视频",
        [".wmv"] = "视频",
        [".flv"] = "视频",
        [".webm"] = "视频",
        [".m4v"] = "视频",
        [".mts"] = "视频",

        [".mp3"] = "音频",
        [".wav"] = "音频",
        [".flac"] = "音频",
        [".aac"] = "音频",
        [".m4a"] = "音频",
        [".ogg"] = "音频",
        [".wma"] = "音频",
        [".ape"] = "音频",

        [".doc"] = "文档",
        [".docx"] = "文档",
        [".txt"] = "文档",
        [".md"] = "文档",
        [".rtf"] = "文档",
        [".odt"] = "文档",
        [".pages"] = "文档",

        [".pdf"] = "PDF",

        [".xls"] = "表格",
        [".xlsx"] = "表格",
        [".csv"] = "表格",
        [".ods"] = "表格",
        [".numbers"] = "表格",

        [".ppt"] = "演示",
        [".pptx"] = "演示",
        [".odp"] = "演示",
        [".key"] = "演示",

        [".zip"] = "压缩包",
        [".rar"] = "压缩包",
        [".7z"] = "压缩包",
        [".tar"] = "压缩包",
        [".gz"] = "压缩包",
        [".bz2"] = "压缩包",
        [".xz"] = "压缩包",
        [".iso"] = "压缩包",
        [".cab"] = "压缩包",

        [".cs"] = "代码",
        [".js"] = "代码",
        [".jsx"] = "代码",
        [".ts"] = "代码",
        [".tsx"] = "代码",
        [".json"] = "代码",
        [".xml"] = "代码",
        [".yml"] = "代码",
        [".yaml"] = "代码",
        [".toml"] = "代码",
        [".ini"] = "代码",
        [".py"] = "代码",
        [".java"] = "代码",
        [".cpp"] = "代码",
        [".c"] = "代码",
        [".h"] = "代码",
        [".hpp"] = "代码",
        [".go"] = "代码",
        [".rs"] = "代码",
        [".sql"] = "代码",
        [".sh"] = "代码",
        [".ps1"] = "代码",
        [".bat"] = "代码",
        [".cmd"] = "代码",

        [".exe"] = "程序",
        [".msi"] = "程序",
        [".msix"] = "程序",
        [".appx"] = "程序",
        [".apk"] = "程序"
    };

    public IReadOnlyDictionary<string, string> ParseCustomRules(string? rulesText)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(rulesText))
        {
            return result;
        }

        foreach (var (_, line) in EnumerateRuleLines(rulesText))
        {
            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0 || separatorIndex >= line.Length - 1)
            {
                continue;
            }

            var extensionPart = line[..separatorIndex].Trim();
            var categoryPart = line[(separatorIndex + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(extensionPart) || string.IsNullOrWhiteSpace(categoryPart))
            {
                continue;
            }

            var extensions = extensionPart
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeExtension)
                .Where(static extension => !string.IsNullOrWhiteSpace(extension));

            foreach (var extension in extensions)
            {
                result[extension] = categoryPart;
            }
        }

        return result;
    }

    public IReadOnlyList<string> ValidateCustomRules(string? rulesText)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(rulesText))
        {
            return errors;
        }

        foreach (var (lineNumber, line) in EnumerateRuleLines(rulesText))
        {
            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0 || separatorIndex >= line.Length - 1)
            {
                errors.Add($"第 {lineNumber} 行格式无效，应为 .扩展名=文件夹名");
                continue;
            }

            var extensionPart = line[..separatorIndex].Trim();
            var categoryPart = line[(separatorIndex + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(extensionPart) || string.IsNullOrWhiteSpace(categoryPart))
            {
                errors.Add($"第 {lineNumber} 行缺少扩展名或文件夹名");
                continue;
            }

            if (categoryPart.IndexOfAny(InvalidFolderNameChars) >= 0)
            {
                errors.Add($"第 {lineNumber} 行的文件夹名包含非法字符");
            }

            var extensions = extensionPart
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (extensions.Length == 0)
            {
                errors.Add($"第 {lineNumber} 行缺少扩展名");
                continue;
            }

            foreach (var rawExtension in extensions)
            {
                var normalizedExtension = NormalizeExtension(rawExtension);
                if (string.IsNullOrWhiteSpace(normalizedExtension) || normalizedExtension == ".")
                {
                    errors.Add($"第 {lineNumber} 行存在空扩展名");
                    continue;
                }

                if (!normalizedExtension.StartsWith('.') ||
                    normalizedExtension.Length < 2 ||
                    normalizedExtension.Contains(' ') ||
                    normalizedExtension[1..].Any(static ch => !char.IsLetterOrDigit(ch)))
                {
                    errors.Add($"第 {lineNumber} 行扩展名 {normalizedExtension} 无效");
                }
            }
        }

        return errors.Distinct(StringComparer.Ordinal).ToList();
    }

    public int CountCustomRuleEntries(string? rulesText)
    {
        return ParseCustomRules(rulesText).Count;
    }

    public string GetCategoryFolderName(
        string sourcePath,
        bool isDirectory,
        IReadOnlyDictionary<string, string>? customRules = null)
    {
        if (isDirectory)
        {
            return "文件夹";
        }

        var extension = NormalizeExtension(Path.GetExtension(sourcePath));
        if (!string.IsNullOrWhiteSpace(extension) &&
            customRules is not null &&
            customRules.TryGetValue(extension, out var customCategory))
        {
            return customCategory;
        }

        if (!string.IsNullOrWhiteSpace(extension) &&
            ExtensionToCategory.TryGetValue(extension, out var category))
        {
            return category;
        }

        return "其他";
    }

    private static string NormalizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        return extension.StartsWith('.') ? extension.Trim() : $".{extension.Trim()}";
    }

    private static IEnumerable<(int LineNumber, string Line)> EnumerateRuleLines(string rulesText)
    {
        var lines = rulesText.Split(["\r\n", "\n"], StringSplitOptions.None);

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            yield return (index + 1, line);
        }
    }
}
