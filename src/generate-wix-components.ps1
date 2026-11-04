# Usage: .\generate-wix-components.ps1 -PublishDir "publish\net8.0" -OutputFile "setup\ComponentsForNet80.wxs"

param(
    [string]$PublishDir = "publish\net8.0",
    [string]$OutputFile = "setup\ComponentsForNet80.wxs"
)

function New-Guid {
    [guid]::NewGuid().ToString()
}

$header = @"
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Fragment>
    <ComponentGroup Id="ComponentsForNet80" Directory="INSTALLFOLDER">
"@

$footer = @"
    </ComponentGroup>
  </Fragment>
</Wix>
"@

$components = ""

Get-ChildItem -Path $PublishDir -File | ForEach-Object {
    $fileName = $_.Name
    $filePath = $_.FullName
    $relativePath = "..\" + (Join-Path $PublishDir $fileName)
    $componentId = $fileName.Replace('.', '_')
    $guid = New-Guid
    $components += "      <Component Id=""$componentId"" Guid=""$guid"">`n        <File Source=""$relativePath"" />`n      </Component>`n"
}

$content = $header + $components + $footer
Set-Content -Path $OutputFile -Value $content -Encoding UTF8

Write-Host "Generated $OutputFile with components for all files in $PublishDir"