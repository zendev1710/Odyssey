$ErrorActionPreference = 'Stop'
$stableTagPattern = '^v(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)$'
$releaseVersion = $null
$tagName = $null

if ($env:GITHUB_REF -like 'refs/tags/*') {
  $tagName = $env:GITHUB_REF_NAME
  if ($tagName -notmatch $stableTagPattern) {
	throw "Release tags must use the stable vMAJOR.MINOR.PATCH format; received '$tagName'."
  }
  $releaseVersion = $tagName.Substring(1)
}
else {
  $currentTag = @(git tag --points-at $env:GITHUB_SHA | Where-Object { $_ -match $stableTagPattern } | Select-Object -First 1)
  if ($LASTEXITCODE -ne 0) {
	throw 'Unable to list tags pointing at the current commit.'
  }

  if ($currentTag.Count -gt 0) {
	$tagName = $currentTag[0]
	$releaseVersion = $tagName.Substring(1)
  }
  else {
	$latestTag = @(git tag --list 'v*' --sort=-version:refname | Where-Object { $_ -match $stableTagPattern } | Select-Object -First 1)
	if ($LASTEXITCODE -ne 0) {
	  throw 'Unable to list existing release tags.'
	}

	if ($latestTag.Count -gt 0) {
	  $baseVersion = $latestTag[0].Substring(1)
	}
	else {
	  [xml]$versionProps = Get-Content (Join-Path $PSScriptRoot '..\Directory.Build.props') -Raw
		$baseVersion = $versionProps.Project.PropertyGroup[0].OdysseyVersion.InnerText
	}

	$before = $env:PUSH_BEFORE
	if ([string]::IsNullOrWhiteSpace($before) -or $before -match '^0+$') {
	  $commitIds = @(git rev-list --no-merges $env:GITHUB_SHA)
	}
	else {
	  $commitIds = @(git rev-list --no-merges "$before..$env:GITHUB_SHA")
	}
	if ($LASTEXITCODE -ne 0) {
	  throw 'Unable to inspect commits introduced by this push.'
	}

	$commitMessages = @()
	foreach ($commitId in $commitIds) {
	  $subject = (git show -s --format=%s $commitId) -join ''
	  if ($LASTEXITCODE -ne 0) {
		throw "Unable to read commit $commitId."
	  }
	  $body = (git show -s --format=%b $commitId) -join "`n"
	  if ($LASTEXITCODE -ne 0) {
		throw "Unable to read the body of commit $commitId."
	  }
	  $commitMessages += "$subject`n$body"
	}

	if ($commitMessages.Count -eq 0) {
	  Write-Host 'No commits were introduced by this push; skipping release.'
	  Add-Content -Path $env:GITHUB_OUTPUT -Value 'should_release=false'
		return
	}

	$calculator = Join-Path $PSScriptRoot 'Calculate-ReleaseVersion.ps1'
	$result = (& $calculator -BaseVersion $baseVersion -CommitMessages $commitMessages | ConvertFrom-Json)
	if (-not $result.ShouldRelease) {
	  Write-Host 'No release-triggering Conventional Commit found; skipping release.'
	  Add-Content -Path $env:GITHUB_OUTPUT -Value 'should_release=false'
		return
	}

	$releaseVersion = $result.Version
	$tagName = "v$releaseVersion"
	$existingTag = @(git tag --list $tagName)
	if ($LASTEXITCODE -ne 0) {
	  throw 'Unable to check whether the next release tag already exists.'
	}
	if ($existingTag.Count -gt 0) {
	  $existingCommit = (git rev-list -n 1 "refs/tags/$tagName") -join ''
	  if ($LASTEXITCODE -ne 0 -or $existingCommit -ne $env:GITHUB_SHA) {
		throw "Release tag '$tagName' already exists on a different commit."
	  }
	}
	else {
	  git config user.name 'github-actions[bot]'
	  git config user.email '41898282+github-actions[bot]@users.noreply.github.com'
	  git tag -a $tagName -m "Release $tagName" $env:GITHUB_SHA
	  if ($LASTEXITCODE -ne 0) {
		throw "Unable to create release tag '$tagName'."
	  }
	  git push origin "refs/tags/$tagName"
	  if ($LASTEXITCODE -ne 0) {
		throw "Unable to push release tag '$tagName'."
	  }
	}
  }
}

Add-Content -Path $env:GITHUB_OUTPUT -Value 'should_release=true'
Add-Content -Path $env:GITHUB_OUTPUT -Value "version=$releaseVersion"
Add-Content -Path $env:GITHUB_OUTPUT -Value "tag_name=$tagName"
