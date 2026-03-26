# DateVault

DateVault 是一个面向 Windows 的轻量桌面归档工具，核心目标是把文件按日期目录整理起来，并在需要时继续按数据类型自动分流。

当前桌面版本：`1.0.0`

## 项目定位

- 专注 Windows 桌面，不追求大而全的跨平台能力
- 以轻量、直接、低学习成本为优先
- 适合个人文件归档、素材收纳、日常下载整理
- 默认围绕“今日目录”工作，也支持归档到当前选中目录

## 主要功能

- 选择归档根目录
- 自动生成按日期组织的目录结构
- 文件树懒加载浏览
- 拖拽文件快速归档
- 新建文件夹
- 打开文件、打开所在位置、复制路径
- 按数据类型自动归档
- 自定义扩展名到文件夹规则
- JSON 配置持久化
- 中文界面
- 小窗口默认启动，记忆窗口位置与大小
- 便携包发布
- 单用户安装包发布

## 当前界面与交互特性

- 主界面采用 WPF 原生桌面实现
- 整体视觉偏克制、浅色、卡片化
- 文件树支持双击打开、`Enter` 打开、右键操作
- 新建文件夹和归档完成后会自动刷新并尽量定位到目标位置
- 设置页支持规则模板、自定义规则校验、配置导入导出
- 关于窗口可查看版本信息与本地更新说明

## 技术栈

- `.NET 8`
- `WPF`
- `PowerShell`
- 分层结构：`App / Application / Domain / Infrastructure`

## 项目结构

```text
DateVault.sln
src/
  DateVault.App/             WPF 桌面应用层
  DateVault.Application/     用例编排层
  DateVault.Domain/          领域模型与领域服务
  DateVault.Infrastructure/  文件系统、配置、Shell 等基础设施实现
assets/                      图标等静态资源
scripts/                     辅助脚本
```

## 运行环境

- Windows
- `.NET 8 SDK`
- Visual Studio 2022 或可兼容的 .NET / MSBuild 工具链

## 本地开发运行

### 方式一：Visual Studio

1. 打开 `DateVault.sln`
2. 将 `DateVault.App` 设为启动项目
3. 直接生成并运行

### 方式二：项目自带脚本

可用脚本：

- `.\setup-dotnet.ps1`
- `.\build.ps1`
- `.\run.ps1`
- `.\publish.ps1`
- `.\package-portable.ps1`
- `.\package-installer.ps1`
- `.\sign-package-template.ps1`

说明：

- 项目支持本地 SDK 引导流程
- 脚本优先使用项目目录下的 `.dotnet`
- 不强依赖机器全局安装的 SDK

## 归档规则说明

DateVault 支持两种整理方式：

1. 直接归档到目标目录
2. 按数据类型自动归档

按数据类型归档时，内置支持以下分类：

- 图片
- 视频
- 音频
- 文档
- PDF
- 表格
- 演示
- 压缩包
- 代码
- 程序
- 文件夹
- 其他

同时支持自定义规则，例如：

```text
.psd=设计源文件
.ai,.sketch,.fig=设计工程
.md,.txt=笔记
```

规则格式要求：

- 每行一条
- 格式为 `.扩展名=文件夹名`
- 支持多个扩展名共用一个目标文件夹
- 空行和以 `#` 开头的行会被忽略

## 发布与分发

### 发布可执行文件

```powershell
powershell -ExecutionPolicy Bypass -File .\publish.ps1
```

发布目录：

- `artifacts\publish\win-x64`

### 生成便携包

```powershell
powershell -ExecutionPolicy Bypass -File .\package-portable.ps1
```

输出目录：

- `artifacts\portable`

### 生成安装包

```powershell
powershell -ExecutionPolicy Bypass -File .\package-installer.ps1
```

输出目录：

- `artifacts\installer`

安装包特点：

- 当前用户安装，无需管理员权限
- 默认安装到 `%LOCALAPPDATA%\Programs\DateVault`
- 可创建桌面快捷方式和开始菜单快捷方式
- 自动写入 Windows 当前用户卸载信息

## 图标与签名

图标资源：

- 源脚本：`scripts\generate-icon.ps1`
- 图标文件：`assets\datevault.ico`
- 预览图：`assets\datevault-icon.png`

签名准备：

- 脚本模板：`.\sign-package-template.ps1`
- 用于在本机具备 `signtool.exe` 和代码签名证书时，对 exe 或 zip 进行签名

## 设计文档

- `design.md`
- `windows-desktop-design.md`

这两个文档分别对应：

- 项目总体设计
- Windows 轻量桌面应用专项设计

## 当前状态

目前项目已经具备以下交付能力：

- 可正常编译运行
- 可发布为自包含 Windows 可执行文件
- 可打包为便携版 zip
- 可打包为轻量单用户安装包
- 支持卸载链路

## 后续可扩展方向

- 在线更新服务
- 更细粒度的分类模板
- 配置云同步
- 文件预览与批量操作
- 正式代码签名与安装器升级
