param(
	[string]$Root = (Join-Path $PSScriptRoot "..\src\app\Odyssey.Desktop\Assets"),
	[switch]$Recurse = $true
)

$ErrorActionPreference = "Stop"

$rootPath = (Resolve-Path $Root).Path
Write-Host "Scanning PNG files under: $rootPath"

function Convert-PngToSvg {
	param(
		[string]$PngPath
	)

	$svgPath = [System.IO.Path]::ChangeExtension($PngPath, ".svg")
	if (Test-Path $svgPath) {
		return [PSCustomObject]@{
			Source = $PngPath
			Target = $svgPath
			Result = "already_exists"
			Strategy = "skip"
		}
	}

	if (Get-Command magick -ErrorAction SilentlyContinue) {
		try {
			& magick "$PngPath" -background none -alpha on -channel RGBA "$svgPath" 2>$null
			if ($LASTEXITCODE -eq 0 -and (Test-Path $svgPath)) {
				return [PSCustomObject]@{
					Source = $PngPath
					Target = $svgPath
					Result = "converted"
					Strategy = "magick"
				}
			}
		}
		catch {
		}
	}

	try {
		Add-Type -AssemblyName System.Drawing | Out-Null
		$image = [System.Drawing.Bitmap]::FromFile($PngPath)
		$width = $image.Width
		$height = $image.Height
		$base64 = [Convert]::ToBase64String([System.IO.File]::ReadAllBytes($PngPath))
		$svg = @"
<svg xmlns="http://www.w3.org/2000/svg" width="$width" height="$height" viewBox="0 0 $width $height">
  <image href="data:image/png;base64,$base64" width="$width" height="$height" preserveAspectRatio="xMidYMid meet"/>
</svg>
"@
		[System.IO.File]::WriteAllText($svgPath, $svg)
		$image.Dispose()

		return [PSCustomObject]@{
			Source = $PngPath
			Target = $svgPath
			Result = "converted"
			Strategy = "embedded-raster"
		}
	}
	catch {
		return [PSCustomObject]@{
			Source = $PngPath
			Target = $svgPath
			Result = "failed"
			Strategy = "failed"
		}
	}
}

$pngFiles = if ($Recurse) {
	Get-ChildItem -Path $rootPath -Recurse -File -Filter *.png | Sort-Object FullName
} else {
	Get-ChildItem -Path $rootPath -File -Filter *.png | Sort-Object FullName
}

$results = @()
foreach ($png in $pngFiles) {
	$results += Convert-PngToSvg -PngPath $png.FullName
}

Write-Host ""
Write-Host "Summary: $($pngFiles.Count) PNG files checked."
Write-Host "Converted: $($results | Where-Object { $_.Result -eq 'converted' }).Count"
Write-Host "Already existed: $($results | Where-Object { $_.Result -eq 'already_exists' }).Count"
Write-Host "Failed: $($results | Where-Object { $_.Result -eq 'failed' }).Count"

$results | Select-Object Source, Target, Result, Strategy | Format-Table -AutoSize
