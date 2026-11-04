param(
	[string]$Root = (Join-Path $PSScriptRoot "..\src\app\Odyssey.Desktop\Assets"),
	[switch]$Recurse = $true
)

$ErrorActionPreference = "Stop"

$rootPath = (Resolve-Path $Root).Path
Write-Host "Scanning GIF files under: $rootPath"

function Get-WhiteBackgroundRatio {
	param([string]$Path)

	try {
		$image = [System.Drawing.Image]::FromFile($Path)
		$width = $image.Width
		$height = $image.Height
		$total = $width * $height
		$whitePixels = 0L

		for ($y = 0; $y -lt $height; $y++) {
			for ($x = 0; $x -lt $width; $x++) {
				$pixel = $image.GetPixel($x, $y)
				if ($pixel.A -ge 200 -and $pixel.R -ge 235 -and $pixel.G -ge 235 -and $pixel.B -ge 235) {
					$whitePixels++
				}
			}
		}

		$image.Dispose()
		if ($total -le 0) { return 0.0 }
		return [double]($whitePixels / $total)
	}
	catch {
		return 0.0
	}
}

function Get-AlphaPresenceRatio {
	param([string]$Path)

	try {
		$image = [System.Drawing.Image]::FromFile($Path)
		$width = $image.Width
		$height = $image.Height
		$total = $width * $height
		$alphaPixels = 0L

		for ($y = 0; $y -lt $height; $y++) {
			for ($x = 0; $x -lt $width; $x++) {
				$pixel = $image.GetPixel($x, $y)
				if ($pixel.A -lt 250) {
					$alphaPixels++
				}
			}
		}

		$image.Dispose()
		if ($total -le 0) { return 0.0 }
		return [double]($alphaPixels / $total)
	}
	catch {
		return 0.0
	}
}

function Try-MagickConversion {
	param(
		[string]$Source,
		[string]$Target
	)

	$strategies = @(
		@('magick', $Source, '-background', 'none', '-alpha', 'set', '-channel', 'RGBA', '-fuzz', '5%', '-transparent', 'white', $Target),
		@('magick', $Source, '-background', 'none', '-alpha', 'set', '-channel', 'RGBA', '-fuzz', '15%', '-transparent', 'white', $Target),
		@('magick', $Source, '-background', 'none', '-alpha', 'remove', '-alpha', 'off', $Target)
	)

	foreach ($strategy in $strategies) {
		try {
			& $strategy[0] $strategy[1..($strategy.Length - 1)] 2>$null
			if ($LASTEXITCODE -eq 0 -and (Test-Path $Target)) {
				return $true
			}
		}
		catch {
		}
	}

	return $false
}

function Convert-GifToTransparentPng {
	param(
		[string]$GifPath
	)

	$pngPath = [System.IO.Path]::ChangeExtension($GifPath, ".png")
	if (Test-Path $pngPath) {
		return [PSCustomObject]@{
			Source = $GifPath
			Target = $pngPath
			Result = "already_exists"
			WhiteRatio = 0.0
			AlphaRatio = 0.0
			Strategy = "skip"
		}
	}

	$whiteRatio = Get-WhiteBackgroundRatio -Path $GifPath
	$alphaRatio = Get-AlphaPresenceRatio -Path $GifPath
	$strategyUsed = ""

	if (Get-Command magick -ErrorAction SilentlyContinue) {
		$m = Try-MagickConversion -Source $GifPath -Target $pngPath
		if ($m) {
			$strategyUsed = "magick"
		}
	}

	if (-not $strategyUsed) {
		try {
			Add-Type -AssemblyName System.Drawing | Out-Null

			$sourceImage = [System.Drawing.Image]::FromFile($GifPath)
			$bitmap = New-Object System.Drawing.Bitmap($sourceImage.Width, $sourceImage.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
			$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
			$graphics.Clear([System.Drawing.Color]::Transparent)
			$graphics.DrawImage($sourceImage, 0, 0, $sourceImage.Width, $sourceImage.Height)

			# Make white-ish background transparent. This is useful for Magellan-style GIFs on light backgrounds.
			for ($y = 0; $y -lt $bitmap.Height; $y++) {
				for ($x = 0; $x -lt $bitmap.Width; $x++) {
					$pixel = $bitmap.GetPixel($x, $y)
					if ($pixel.A -gt 200 -and $pixel.R -ge 235 -and $pixel.G -ge 235 -and $pixel.B -ge 235) {
						$bitmap.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, $pixel.R, $pixel.G, $pixel.B))
					}
				}
			}

			$bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
			$graphics.Dispose()
			$sourceImage.Dispose()
			$bitmap.Dispose()

			$strategyUsed = "dotnet-transparent"
		}
		catch {
			$strategyUsed = "failed"
			Write-Warning "Failed to convert $GifPath using all strategies."
		}
	}

	$result = [PSCustomObject]@{
		Source = $GifPath
		Target = $pngPath
		Result = if ($strategyUsed -eq "failed") { "failed" } else { "converted" }
		WhiteRatio = [double]$whiteRatio
		AlphaRatio = [double]$alphaRatio
		Strategy = $strategyUsed
	}

	return $result
}

$gifFiles = if ($Recurse) {
	Get-ChildItem -Path $rootPath -Recurse -File -Filter *.gif | Sort-Object FullName
} else {
	Get-ChildItem -Path $rootPath -File -Filter *.gif | Sort-Object FullName
}

$results = @()
foreach ($gif in $gifFiles) {
	$results += Convert-GifToTransparentPng -GifPath $gif.FullName
}

Write-Host ""
Write-Host "Summary: $($gifFiles.Count) GIF files checked."
Write-Host "Converted: $($results | Where-Object { $_.Result -eq 'converted' }).Count"
Write-Host "Already existed: $($results | Where-Object { $_.Result -eq 'already_exists' }).Count"
Write-Host "Failed: $($results | Where-Object { $_.Result -eq 'failed' }).Count"

$results | Select-Object Source, Target, Result, WhiteRatio, AlphaRatio, Strategy | Format-Table -AutoSize
