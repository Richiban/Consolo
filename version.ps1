param (
    [Parameter(Mandatory = $true)][string] $projectFileName,
    [string] $buildNumber = $env:GITHUB_RUN_NUMBER,
    [string] $branchName = $(if ($env:GITHUB_REF_NAME) { $env:GITHUB_REF_NAME } elseif ($env:GITHUB_REF) { $env:GITHUB_REF } else { $null })
)

$ErrorActionPreference = 'Stop'

function Set-PipelineVariable {
    param(
        [Parameter(Mandatory = $true)][string] $Name,
        [AllowNull()][string] $Value
    )

    if ($null -eq $Value) {
        $Value = ''
    }

    Write-Host "Setting pipeline variable $Name=$Value"

    if ($env:GITHUB_OUTPUT) {
        "$Name=$Value" | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding utf8 -Append
    }

    if ($env:GITHUB_ENV) {
        "$Name=$Value" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
    }
}

function Set-PackagePipelineVariable {
    param(
        [Parameter(Mandatory = $true)][string] $BaseName,
        [AllowNull()][string] $Value,
        [Parameter(Mandatory = $true)][string] $PackageName
    )

    $packageVariablePrefix = ($packageName -replace '[^0-9A-Za-z]', '_')

    if ([string]::IsNullOrWhiteSpace($packageVariablePrefix)) {
        Set-PipelineVariable -Name $BaseName -Value $Value
    }
    else {
        Set-PipelineVariable -Name ("{0}_{1}" -f $packageVariablePrefix, $BaseName) -Value $Value
    }
}

$branchName = "$branchName".Split('/')[-1]

Write-Host "Branch name: $branchName"

if (-not ($projectFileName.EndsWith('.csproj') -or $projectFileName.EndsWith('.fsproj'))) {
    $projectFileName = "$projectFileName.csproj"
}

Write-Host "Project file name: $projectFileName"

$csproj = Get-ChildItem -Recurse $projectFileName | Select-Object -First 1

if (-not $csproj) {
    ls -Recurse -Filter $projectFileName

    tree

    Write-Error "No .csproj file found matching '$projectFileName'."

    exit 1
}

Write-Host "Found project file at '$csproj'"

$csprojContent = [xml](Get-Content $csproj)
$versionPrefix = $csprojContent.Project.PropertyGroup.VersionPrefix

$packageName = $csprojContent.Project.PropertyGroup.PackageId

if (-not $packageName) {
    Write-Host "No PackageId found in csproj. Falling back to Assembly Name."
    $packageName = $csprojContent.Project.PropertyGroup.AssemblyName
}

if (-not $packageName) {
    Write-Host "No PackageId set. Defaulting to project file name without extension."
    $packageName = [System.IO.Path]::GetFileNameWithoutExtension($projectFileName)
}

Write-Host "Package name: $packageName"

Write-Host "Listing tags:"
git tag

$lastTaggedCommit = git describe --tags --abbrev=0 --match "$packageName/*" 2>$null

Write-Host "Last tagged commit for pattern $packageName/*: $lastTaggedCommit"

$lastTags = git tag --merged HEAD --list "$packageName/*" --points-at $lastTaggedCommit

Write-Host "Last tags found:"
Write-Host $lastTags

if ($LASTEXITCODE -ne 0) {
    $lastTag = ''
    Write-Warning "No previous tag found for package pattern $packageName/*"
}

$prevVersion = $lastTags `
| foreach { [System.Management.Automation.SemanticVersion]::Parse(($_ -replace "^$packageName/", "")) } `
| Sort-Object -Descending `
| Select-Object -First 1

write-host "Previous version:"
write-host $prevVersion

if (-not $prevVersion) {
    if ($allowFirstRun) {
        $prevVersion = [System.Management.Automation.SemanticVersion]::new(0, 0, 0)
        Write-Host "First run detected. Defaulting prevVersion to $prevVersion"
    }
    else {
        Write-Error "No previous build found matching '$packageName/*'."
        exit 1
    }
}

Write-Host "Previous version:"
Write-Host $prevVersion

if ($versionPrefix -ne $null) {
    Write-Host "Version prefix found in csproj"
    $currentMajorVersion = [int]($versionPrefix)
}
else {
    Write-Warning "No version prefix found in csproj. Defaulting to last used"
    $currentMajorVersion = $prevVersion.Major
}

if (($branchName -eq 'master') -or ($branchName -eq 'main')) {
    $prereleaseTag = $null
}
else {
    $prereleaseTag = "CI-$buildNumber"
}

if ($prevVersion.Major -eq $currentMajorVersion) {
    $newVersion = [System.Management.Automation.SemanticVersion]::new($prevVersion.Major, $prevVersion.Minor + 1, 0, $prereleaseTag)
}
elseif ($prevVersion.Major -lt $currentMajorVersion) {
    $newVersion = [System.Management.Automation.SemanticVersion]::new($currentMajorVersion, 0, 0, $prereleaseTag)
}
elseif ($prevVersion.major -gt $currentMajorVersion) {
    Write-Error "Major version downgrade detected"
    exit 2
}
else {
    Write-Error "Some unknown error occurred, $($prevVersion.major) $($currentMajorVersion)"
    exit 1
}

Write-Host "This version:"
Write-Host $newVersion
Write-Host ""

$tagName = "$packageName/$newVersion"

Set-PackagePipelineVariable -BaseName 'CurrentVersion' -Value $newVersion -PackageName $packageName
Set-PackagePipelineVariable -BaseName 'tagName' -Value $tagName -PackageName $packageName
    
if ($newVersion.Minor -ne 0) {
    try {
        Write-Host "Preparing to download baseline package for validation..."

        $nugetConfigPath = Join-Path $PSScriptRoot 'nuget.config'
        if (-not (Test-Path $nugetConfigPath)) {
            $nugetConfigPath = 'nuget.config'
        }
        if (-not (Test-Path $nugetConfigPath)) {
            throw "nuget.config not found. Cannot determine feed URL to download baseline package."
        }

        [xml]$nugetConfig = Get-Content -LiteralPath $nugetConfigPath
        $devyceSource = $nugetConfig.configuration.packageSources.add | Where-Object { $_.GetAttribute('key') -eq 'devyce' }
        if (-not $devyceSource) {
            throw "Could not find a package source with key 'devyce' in nuget.config"
        }

        $feedIndexUrl = $devyceSource.GetAttribute('value')
        if (-not $feedIndexUrl) {
            throw "Package source 'devyce' does not have a 'value' attribute in nuget.config"
        }

        $baseUrl = $feedIndexUrl
        $lowerBase = $baseUrl.ToLowerInvariant()
        $suffixToTrim = '/index.json'
        if ($lowerBase.EndsWith($suffixToTrim)) {
            $baseUrl = $baseUrl.Substring(0, $baseUrl.Length - $suffixToTrim.Length)
        }
        if (-not $baseUrl.EndsWith('/')) { $baseUrl += '/' }
        $flatBase = $baseUrl + 'flat2/'

        $idLower = $packageName.ToLowerInvariant()
        $previousVersionText = $prevVersion.ToString()
        $versionLower = $previousVersionText.ToLowerInvariant()
        $packageUrl = "$flatBase$idLower/$versionLower/$idLower.$versionLower.nupkg"

        Write-Host "Baseline package URL: $packageUrl"

        $homeDir = if ($env:USERPROFILE) { $env:USERPROFILE } elseif ($env:HOME) { $env:HOME } else { '~' }
        $globalPackages = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path (Join-Path $homeDir '.nuget') 'packages' }

        $destinationDir = Join-Path (Join-Path $globalPackages $idLower) $previousVersionText
        $destinationFile = Join-Path $destinationDir ("{0}.{1}.nupkg" -f $idLower, $versionLower)

        if (-not (Test-Path $destinationDir)) { New-Item -ItemType Directory -Force -Path $destinationDir | Out-Null }

        $headers = @{}
        $token = $env:NUGET_DOWNLOAD_TOKEN
        if (-not $token) {
            $token = $env:GITHUB_TOKEN
        }
        if ($token) {
            $pair = "anything:$token"
            $bytes = [System.Text.Encoding]::ASCII.GetBytes($pair)
            $auth = [System.Convert]::ToBase64String($bytes)
            $headers['Authorization'] = "Basic $auth"
        }

        if (-not (Test-Path $destinationFile)) {
            Write-Host "Downloading baseline package to '$destinationFile'..."
            try {
                Invoke-WebRequest -Uri $packageUrl -Headers $headers -OutFile $destinationFile -MaximumRedirection 5
            }
            catch {
                throw "Failed to download baseline package from $packageUrl. $(($_.Exception.Message))"
            }
        }
        else {
            Write-Host "Baseline package already exists at '$destinationFile'"
        }

        $changeValidation = ";EnablePackageValidation=True;PackageValidationBaselineVersion=$prevVersion;PackageValidationReferencePath=$destinationDir"
        Set-PackagePipelineVariable -BaseName 'ChangeValidationProperties' -Value $changeValidation -PackageName $packageName
    }
    catch {
        Write-Error "Baseline package download failed: $(($_.Exception.Message))"
        exit 1
    }
}
else {
    Set-PackagePipelineVariable -BaseName 'ChangeValidationProperties' -Value '' -PackageName $packageName
}

exit 0