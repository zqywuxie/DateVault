# DateVault

DateVault is a lightweight Windows desktop file archiver built around date-based folders.

Current desktop package version: `1.0.0`

## Requirements

- Windows
- .NET 8 SDK
- Visual Studio 2022 or a compatible MSBuild toolchain

## Solution Layout

```text
DateVault.sln
src/
  DateVault.App/
  DateVault.Application/
  DateVault.Domain/
  DateVault.Infrastructure/
```

## Current MVP Skeleton

- WPF desktop shell
- Root folder selection
- Date-based archive target
- Lazy-loaded file tree
- New folder flow
- Drag and drop archive flow
- Open, reveal, and copy-path actions
- JSON config persistence

## Open And Run

1. Install the .NET 8 SDK.
2. Open `DateVault.sln` in Visual Studio.
3. Set `DateVault.App` as the startup project.
4. Build and run.

## Local Build Scripts

- `.\setup-dotnet.ps1`
- `.\build.ps1`
- `.\run.ps1`
- `.\publish.ps1`
- `.\package-portable.ps1`
- `.\package-installer.ps1`
- `.\sign-package-template.ps1`

These scripts use the project-local SDK in `.dotnet` and do not depend on a machine-wide SDK installation.

## Packaging

- `.\publish.ps1`
  Produces a self-contained Windows executable in `artifacts\publish\win-x64`
- `.\package-portable.ps1`
  Produces a zip package in `artifacts\portable`
- `.\package-installer.ps1`
  Produces a lightweight per-user installer bundle and zip in `artifacts\installer`
- `.\sign-package-template.ps1`
  Signing template for exe or zip artifacts when `signtool.exe` and a code-signing certificate are available

## Branding

- App icon source script: `scripts\generate-icon.ps1`
- Generated icon: `assets\datevault.ico`

## Installer Mode

- Installer type: per-user, no admin required
- Install path: `%LOCALAPPDATA%\Programs\DateVault`
- Creates desktop and Start Menu shortcuts
- Registers uninstall information in the current user's Windows Apps list

## About And Versioning

- The app now exposes version information in the main window footer and About dialog
- About dialog includes a local update guidance placeholder for packaged builds

## Notes

- The project includes a local SDK bootstrap flow through `setup-dotnet.ps1`.
- The code is structured to match the design documents in `design.md` and `windows-desktop-design.md`.
