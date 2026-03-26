# 贡献指南

感谢你关注 DateVault。

## 开发原则

- 保持 Windows 桌面优先，不为了跨平台牺牲轻量体验
- 新功能尽量围绕“归档效率”和“整理成本”展开
- UI 保持克制、清晰、低干扰
- 尽量不要引入沉重依赖

## 提交前建议

1. 先确认改动是否符合 `design.md` 和 `windows-desktop-design.md`
2. 本地执行构建
3. 确认没有把 `artifacts/`、`bin/`、`obj/` 等产物提交进来
4. 提交信息尽量直接说明改了什么

## 本地命令

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
powershell -ExecutionPolicy Bypass -File .\run.ps1
powershell -ExecutionPolicy Bypass -File .\publish.ps1
```

## Issue 建议

- Bug：请尽量带上复现步骤、系统环境、预期结果、实际结果
- 功能建议：请说明使用场景，不只描述想法
- UI 调整：请明确是视觉问题、交互问题，还是信息层级问题

## Pull Request 建议

- 一个 PR 尽量只解决一类问题
- UI 改动请附截图
- 交互改动请写清楚前后行为差异
- 涉及归档规则时，请说明对已有配置是否兼容
