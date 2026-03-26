using DateVault.App.Services;
using DateVault.Domain.Models;
using DateVault.Domain.Services;
using System.IO;
using System.Text.Json;
using Forms = System.Windows.Forms;
using Wpf = System.Windows;
using WpfControls = System.Windows.Controls;
using WpfMedia = System.Windows.Media;

namespace DateVault.App.Views;

public partial class SettingsDialog : Wpf.Window
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly FileCategoryService _fileCategoryService = new();
    private readonly Dictionary<string, string> _ruleTemplates = new(StringComparer.Ordinal)
    {
        ["DesignAssets"] = """
.psd=设计源文件
.ai,.eps=矢量源文件
.fig,.sketch,.xd=设计工程
.ttf,.otf=字体
.jpg,.jpeg,.png,.webp=设计预览
""",
        ["Development"] = """
.sln,.csproj,.props,.targets=项目配置
.cs,.xaml,.json,.xml,.yml,.yaml=源码
.sql,.db,.sqlite=数据脚本
.md,.txt=说明文档
.zip,.7z,.rar=交付压缩包
""",
        ["MediaLibrary"] = """
.jpg,.jpeg,.png,.gif,.webp=图片收藏
.mp4,.mkv,.mov,.avi=视频收藏
.mp3,.flac,.wav,.m4a=音乐收藏
.srt,.ass=字幕
.zip,.rar,.7z=资源包
"""
    };

    public SettingsDialog(AppConfig config)
    {
        InitializeComponent();
        DialogMotion.Attach(this);
        ApplyConfigToInputs(config);
        UpdateValidationState();

        Loaded += (_, _) =>
        {
            RootPathTextBox.Focus();
            RootPathTextBox.SelectAll();
        };
    }

    public AppConfig ResultConfig => new()
    {
        RootPath = RootPathTextBox.Text.Trim(),
        ConflictPolicy = ConflictPolicy.AutoRename,
        DefaultTargetMode = GetSelectedTargetMode(),
        ArchiveOrganizationMode = GetSelectedArchiveOrganizationMode(),
        CustomCategoryRulesText = CustomRulesTextBox.Text.Trim()
    };

    private void BrowseButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "选择 DateVault 归档根目录",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            InitialDirectory = RootPathTextBox.Text
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            RootPathTextBox.Text = dialog.SelectedPath;
        }
    }

    private void RootPathTextBox_TextChanged(object sender, WpfControls.TextChangedEventArgs e)
    {
        UpdateValidationState();
    }

    private void TargetModeRadioButton_Checked(object sender, Wpf.RoutedEventArgs e)
    {
        UpdateValidationState();
    }

    private void CustomRulesTextBox_TextChanged(object sender, WpfControls.TextChangedEventArgs e)
    {
        UpdateValidationState();
    }

    private void ArchiveOrganizationModeRadioButton_Checked(object sender, Wpf.RoutedEventArgs e)
    {
        UpdateValidationState();
    }

    private void ApplyTemplateButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        if (!TryGetSelectedTemplate(out var templateText))
        {
            return;
        }

        CustomRulesTextBox.Text = templateText;
    }

    private void AppendTemplateButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        if (!TryGetSelectedTemplate(out var templateText))
        {
            return;
        }

        var currentText = CustomRulesTextBox.Text.Trim();
        CustomRulesTextBox.Text = string.IsNullOrWhiteSpace(currentText)
            ? templateText
            : $"{currentText}{Environment.NewLine}{Environment.NewLine}{templateText}";
    }

    private void ImportConfigButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        using var dialog = new Forms.OpenFileDialog
        {
            Title = "导入 DateVault 配置",
            Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(dialog.FileName);
            var importedConfig = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
            if (importedConfig is null)
            {
                throw new InvalidOperationException("配置内容为空或格式无效。");
            }

            ApplyConfigToInputs(importedConfig);
            RuleTemplateComboBox.SelectedIndex = 0;
            UpdateValidationState();
        }
        catch (Exception exception)
        {
            var messageDialog = new MessageDialog(
                "导入失败",
                $"导入失败：{exception.Message}",
                MessageDialogKind.Warning)
            {
                Owner = this
            };

            messageDialog.ShowDialog();
        }
    }

    private void ExportConfigButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        if (!CanSave())
        {
            UpdateValidationState();
            var messageDialog = new MessageDialog(
                "无法导出",
                "当前设置中仍有未修正的问题，无法导出。",
                MessageDialogKind.Warning)
            {
                Owner = this
            };

            messageDialog.ShowDialog();
            return;
        }

        using var dialog = new Forms.SaveFileDialog
        {
            Title = "导出 DateVault 配置",
            Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            DefaultExt = "json",
            AddExtension = true,
            FileName = $"datevault-config-{DateTime.Now:yyyyMMdd-HHmmss}.json"
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(BuildConfigFromInputs(), JsonOptions);
            File.WriteAllText(dialog.FileName, json);
        }
        catch (Exception exception)
        {
            var messageDialog = new MessageDialog(
                "导出失败",
                $"导出失败：{exception.Message}",
                MessageDialogKind.Warning)
            {
                Owner = this
            };

            messageDialog.ShowDialog();
        }
    }

    private void SaveButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        if (!CanSave())
        {
            UpdateValidationState();
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, Wpf.RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SetTargetMode(DefaultTargetMode mode)
    {
        TodayDirectoryRadioButton.IsChecked = mode != DefaultTargetMode.SelectedDirectory;
        SelectedDirectoryRadioButton.IsChecked = mode == DefaultTargetMode.SelectedDirectory;
    }

    private DefaultTargetMode GetSelectedTargetMode()
    {
        if (SelectedDirectoryRadioButton.IsChecked == true)
        {
            return DefaultTargetMode.SelectedDirectory;
        }

        return DefaultTargetMode.TodayDirectory;
    }

    private void SetArchiveOrganizationMode(ArchiveOrganizationMode mode)
    {
        DirectArchiveRadioButton.IsChecked = mode != ArchiveOrganizationMode.ByDataType;
        ByDataTypeArchiveRadioButton.IsChecked = mode == ArchiveOrganizationMode.ByDataType;
    }

    private ArchiveOrganizationMode GetSelectedArchiveOrganizationMode()
    {
        return ByDataTypeArchiveRadioButton.IsChecked == true
            ? ArchiveOrganizationMode.ByDataType
            : ArchiveOrganizationMode.Direct;
    }

    private void ApplyConfigToInputs(AppConfig config)
    {
        RootPathTextBox.Text = config.RootPath;
        CustomRulesTextBox.Text = config.CustomCategoryRulesText;
        SetTargetMode(config.DefaultTargetMode);
        SetArchiveOrganizationMode(config.ArchiveOrganizationMode);
    }

    private AppConfig BuildConfigFromInputs()
    {
        return new AppConfig
        {
            RootPath = RootPathTextBox.Text.Trim(),
            ConflictPolicy = ConflictPolicy.AutoRename,
            DefaultTargetMode = GetSelectedTargetMode(),
            ArchiveOrganizationMode = GetSelectedArchiveOrganizationMode(),
            CustomCategoryRulesText = CustomRulesTextBox.Text.Trim()
        };
    }

    private bool TryGetSelectedTemplate(out string templateText)
    {
        templateText = string.Empty;

        if (RuleTemplateComboBox.SelectedItem is not WpfControls.ComboBoxItem selectedItem)
        {
            return false;
        }

        var templateKey = selectedItem.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(templateKey) || !_ruleTemplates.TryGetValue(templateKey, out var resolvedTemplateText))
        {
            return false;
        }

        templateText = resolvedTemplateText;
        return true;
    }

    private void UpdateValidationState()
    {
        var hasRootPath = !string.IsNullOrWhiteSpace(RootPathTextBox.Text);
        if (hasRootPath)
        {
            RootPathHintTextBlock.Foreground = (WpfMedia.Brush)FindResource("SecondaryTextBrush");
            RootPathHintTextBlock.Text = "当前目录会作为默认归档入口。";
        }
        else
        {
            RootPathHintTextBlock.Foreground = (WpfMedia.Brush)FindResource("AccentBrush");
            RootPathHintTextBlock.Text = "请先选择一个归档根目录，保存后才会生效。";
        }

        var validationErrors = _fileCategoryService.ValidateCustomRules(CustomRulesTextBox.Text);
        var hasRuleErrors = validationErrors.Count > 0;
        var customRuleCount = _fileCategoryService.CountCustomRuleEntries(CustomRulesTextBox.Text);
        var organizationMode = GetSelectedArchiveOrganizationMode();
        var rulesEnabled = organizationMode == ArchiveOrganizationMode.ByDataType;

        CustomRulesTextBox.BorderBrush = hasRuleErrors
            ? (WpfMedia.Brush)FindResource("ValidationErrorBrush")
            : (WpfMedia.Brush)FindResource("HairlineBrush");

        if (hasRuleErrors)
        {
            CustomRulesErrorBorder.Visibility = Wpf.Visibility.Visible;
            CustomRulesErrorTextBlock.Text = string.Join("；", validationErrors);
        }
        else
        {
            CustomRulesErrorBorder.Visibility = Wpf.Visibility.Collapsed;
            CustomRulesErrorTextBlock.Text = string.Empty;
        }

        CustomRulesStatusTextBlock.Foreground = (WpfMedia.Brush)FindResource(
            hasRuleErrors ? "ValidationErrorBrush" : "SecondaryTextBrush");

        if (hasRuleErrors)
        {
            CustomRulesStatusTextBlock.Text = "规则格式有误，修正后才能保存。";
        }
        else if (!rulesEnabled)
        {
            CustomRulesStatusTextBlock.Text = customRuleCount > 0
                ? $"已配置 {customRuleCount} 条自定义规则；切换到“按数据类型自动归档”后生效。"
                : "当前处于直接归档模式，自定义规则暂不生效。";
        }
        else if (customRuleCount > 0)
        {
            CustomRulesStatusTextBlock.Text = $"已识别 {customRuleCount} 条自定义扩展名规则。";
        }
        else
        {
            CustomRulesStatusTextBlock.Text = "未填写自定义规则时，将使用系统内置分类。";
        }

        SettingsSummaryTextBlock.Text = BuildSettingsSummary(customRuleCount);
        FooterHintTextBlock.Text = BuildFooterHint(hasRuleErrors, customRuleCount);
        SaveButton.IsEnabled = CanSave();
    }

    private bool CanSave()
    {
        if (string.IsNullOrWhiteSpace(RootPathTextBox.Text))
        {
            return false;
        }

        return _fileCategoryService.ValidateCustomRules(CustomRulesTextBox.Text).Count == 0;
    }

    private string BuildSettingsSummary(int customRuleCount)
    {
        var targetModeText = GetSelectedTargetMode() == DefaultTargetMode.SelectedDirectory
            ? "目标跟随当前选中目录"
            : "默认归档到今天目录";
        var organizationModeText = GetSelectedArchiveOrganizationMode() == ArchiveOrganizationMode.ByDataType
            ? "按数据类型自动整理"
            : "直接归档到目标目录";

        return customRuleCount > 0
            ? $"{targetModeText} · {organizationModeText} · 已配置 {customRuleCount} 条规则"
            : $"{targetModeText} · {organizationModeText}";
    }

    private string BuildFooterHint(bool hasRuleErrors, int customRuleCount)
    {
        if (hasRuleErrors)
        {
            return "请先修正规则格式，再保存设置。";
        }

        if (GetSelectedArchiveOrganizationMode() != ArchiveOrganizationMode.ByDataType)
        {
            return customRuleCount > 0
                ? "保存后会继续使用当前目录策略，自定义规则会保留但暂不生效。"
                : "保存后会继续使用当前目录策略，不会对已有文件做额外调整。";
        }

        return customRuleCount > 0
            ? $"保存后会按类型自动整理，并优先应用这 {customRuleCount} 条自定义规则。"
            : "保存后会按系统内置分类自动整理后续归档内容。";
    }
}
