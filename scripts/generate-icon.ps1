param(
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path (Split-Path -Parent $PSScriptRoot) "assets"
}

Add-Type -AssemblyName System.Drawing

$size = 256
$bitmap = New-Object System.Drawing.Bitmap $size, $size
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
$graphics.Clear([System.Drawing.Color]::Transparent)

$rect = New-Object System.Drawing.RectangleF 12, 12, 232, 232
$backgroundPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$radius = 56.0
$diameter = $radius * 2
$backgroundPath.AddArc($rect.X, $rect.Y, $diameter, $diameter, 180, 90)
$backgroundPath.AddArc($rect.Right - $diameter, $rect.Y, $diameter, $diameter, 270, 90)
$backgroundPath.AddArc($rect.Right - $diameter, $rect.Bottom - $diameter, $diameter, $diameter, 0, 90)
$backgroundPath.AddArc($rect.X, $rect.Bottom - $diameter, $diameter, $diameter, 90, 90)
$backgroundPath.CloseFigure()

$gradientRect = [System.Drawing.Rectangle]::Round($rect)
$gradientBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    $gradientRect,
    [System.Drawing.Color]::FromArgb(255, 248, 250, 255),
    [System.Drawing.Color]::FromArgb(255, 232, 240, 252),
    90.0
)
$graphics.FillPath($gradientBrush, $backgroundPath)

$borderPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 214, 222, 235)), 2
$graphics.DrawPath($borderPen, $backgroundPath)

$shadowBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(18, 10, 20, 40))
$graphics.FillEllipse($shadowBrush, 58, 176, 140, 20)

$calendarPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$calendarRect = New-Object System.Drawing.RectangleF 52, 46, 152, 154
$calendarRadius = 34.0
$calendarDiameter = $calendarRadius * 2
$calendarPath.AddArc($calendarRect.X, $calendarRect.Y, $calendarDiameter, $calendarDiameter, 180, 90)
$calendarPath.AddArc($calendarRect.Right - $calendarDiameter, $calendarRect.Y, $calendarDiameter, $calendarDiameter, 270, 90)
$calendarPath.AddArc($calendarRect.Right - $calendarDiameter, $calendarRect.Bottom - $calendarDiameter, $calendarDiameter, $calendarDiameter, 0, 90)
$calendarPath.AddArc($calendarRect.X, $calendarRect.Bottom - $calendarDiameter, $calendarDiameter, $calendarDiameter, 90, 90)
$calendarPath.CloseFigure()

$calendarBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 255, 255, 255))
$graphics.FillPath($calendarBrush, $calendarPath)

$calendarPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 220, 226, 236)), 2
$graphics.DrawPath($calendarPen, $calendarPath)

$headerBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 44, 120, 246))
$graphics.FillRectangle($headerBrush, 52, 46, 152, 36)

$ringBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 44, 120, 246))
$graphics.FillEllipse($ringBrush, 78, 30, 18, 34)
$graphics.FillEllipse($ringBrush, 160, 30, 18, 34)

$linePen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 222, 228, 238)), 6
$linePen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$linePen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$graphics.DrawLine($linePen, 78, 108, 178, 108)
$graphics.DrawLine($linePen, 78, 130, 162, 130)
$graphics.DrawLine($linePen, 78, 152, 144, 152)

$vaultBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 17, 17, 17))
$vaultFont = New-Object System.Drawing.Font("Segoe UI Semibold", 44, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$graphics.DrawString("D", $vaultFont, $vaultBrush, 156, 156)

$pngPath = Join-Path $OutputDirectory "datevault-icon.png"
$icoPath = Join-Path $OutputDirectory "datevault.ico"
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)

$pngBytes = [System.IO.File]::ReadAllBytes($pngPath)
$fileStream = [System.IO.File]::Create($icoPath)
$writer = New-Object System.IO.BinaryWriter($fileStream)

$writer.Write([UInt16]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]1)
$writer.Write([Byte]0)
$writer.Write([Byte]0)
$writer.Write([Byte]0)
$writer.Write([Byte]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]32)
$writer.Write([UInt32]$pngBytes.Length)
$writer.Write([UInt32]22)
$writer.Write($pngBytes)
$writer.Flush()
$writer.Dispose()
$fileStream.Dispose()

$vaultFont.Dispose()
$vaultBrush.Dispose()
$linePen.Dispose()
$ringBrush.Dispose()
$headerBrush.Dispose()
$calendarPen.Dispose()
$calendarBrush.Dispose()
$calendarPath.Dispose()
$shadowBrush.Dispose()
$borderPen.Dispose()
$gradientBrush.Dispose()
$backgroundPath.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

Write-Host "Generated:"
Write-Host $pngPath
Write-Host $icoPath
