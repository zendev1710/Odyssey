param(
  [Parameter(Mandatory = $true)]
  [ValidatePattern('^\d+\.\d+\.\d+$')]
  [string]$BaseVersion,

  [Parameter(Mandatory = $true)]
  [string[]]$CommitMessages
)

$base = [version]::Parse($BaseVersion)
$bump = 0

foreach ($message in $CommitMessages) {
  $parts = $message -split '\r?\n', 2
  $subject = $parts[0].Trim()
  $body = if ($parts.Count -gt 1) { $parts[1] } else { '' }

  $isBreaking = ($subject -match '^[a-z0-9-]+(?:\([^)]+\))?!:') -or ($body -match '(?m)^BREAKING[- ]CHANGE:')
  if ($isBreaking) {
	$bump = 3
  }
  elseif ($subject -match '^feat(?:\([^)]+\))?:') {
	$bump = [Math]::Max($bump, 2)
  }
  elseif ($subject -match '^(?:fix|perf|revert)(?:\([^)]+\))?:') {
	$bump = [Math]::Max($bump, 1)
  }
}

$version = $null
if ($bump -gt 0) {
  $major = $base.Major
  $minor = $base.Minor
  $patch = $base.Build

  if ($bump -eq 3) {
	$major++
	$minor = 0
	$patch = 0
  }
  elseif ($bump -eq 2) {
	$minor++
	$patch = 0
  }
  else {
	$patch++
  }

  $version = "$major.$minor.$patch"
}

[pscustomobject]@{
  ShouldRelease = $bump -gt 0
  Bump = switch ($bump) {
	1 { 'patch' }
	2 { 'minor' }
	3 { 'major' }
	default { 'none' }
  }
  Version = $version
} | ConvertTo-Json -Compress
