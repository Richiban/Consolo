param (
    [Parameter(Mandatory = $true)][string] $projectFileName,
    [Parameter(Mandatory = $true)][string] $previousVersion,
    [Parameter(Mandatory = $true)][string] $currentVersion
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

if (-not ($projectFileName.EndsWith('.csproj') -or $projectFileName.EndsWith('.fsproj'))) {
    $projectFileName = "$projectFileName.csproj"
}

$csproj = Get-ChildItem -Recurse $projectFileName | Select-Object -First 1

if (-not $csproj) {
    Write-Error "No .csproj file found matching '$projectFileName'."
    exit 1
}

$csprojContent = [xml](Get-Content $csproj)

function Get-FirstPropertyValue {
    param(
        [Parameter(Mandatory = $true)] $PropertyGroups,
        [Parameter(Mandatory = $true)][string] $PropertyName
    )

    foreach ($group in $PropertyGroups) {
        $value = $group.$PropertyName
        if ($null -ne $value -and -not [string]::IsNullOrWhiteSpace("$value")) {
            return "$value".Trim()
        }
    }

    return $null
}

$propertyGroups = $csprojContent.Project.PropertyGroup

$packageName = Get-FirstPropertyValue -PropertyGroups $propertyGroups -PropertyName "PackageId"

if (-not $packageName) {
    $packageName = Get-FirstPropertyValue -PropertyGroups $propertyGroups -PropertyName "AssemblyName"
}

if (-not $packageName) {
    $packageName = [System.IO.Path]::GetFileNameWithoutExtension($projectFileName)
}

$packageName = "$packageName".Trim()

$current = [System.Management.Automation.SemanticVersion]::Parse($currentVersion)

if ($current.Minor -eq 0) {
    Write-Host "Skipping baseline download because minor version is 0."
    Set-PackagePipelineVariable -BaseName 'ChangeValidationProperties' -Value '' -PackageName $packageName
    exit 0
}

Write-Host "Preparing to download baseline package for validation..."

$nugetConfigPath = Join-Path $PSScriptRoot 'nuget.config'
if (-not (Test-Path $nugetConfigPath)) {
    $nugetConfigPath = 'nuget.config'
}

if (Test-Path $nugetConfigPath) {
    [xml]$nugetConfig = Get-Content -LiteralPath $nugetConfigPath
    $devyceSource = $nugetConfig.configuration.packageSources.add | Where-Object { $_.GetAttribute('key') -eq 'devyce' }
    if (-not $devyceSource) {
        throw "Could not find a package source with key 'devyce' in nuget.config"
    }

    $feedIndexUrl = $devyceSource.GetAttribute('value')
    if (-not $feedIndexUrl) {
        throw "Package source 'devyce' does not have a 'value' attribute in nuget.config"
    }
}
else {
    Write-Host "nuget.config not found. Falling back to NuGet.org feed."
    $feedIndexUrl = "https://api.nuget.org/v3/index.json"
}

function Get-PackageBaseAddress {
    param(
        [Parameter(Mandatory = $true)][string] $IndexUrl
    )

    $index = Invoke-RestMethod -Uri $IndexUrl -Method Get
    $resource = $index.resources | Where-Object { $_.'@type' -like 'PackageBaseAddress*' } | Select-Object -First 1

    if (-not $resource.'@id') {
        throw "PackageBaseAddress not found in feed index: $IndexUrl"
    }

    $baseUrl = $resource.'@id'
    if (-not $baseUrl.EndsWith('/')) { $baseUrl += '/' }

    return $baseUrl
}

$packageBaseUrl = Get-PackageBaseAddress -IndexUrl $feedIndexUrl

$idLower = $packageName.ToLowerInvariant()
$previousVersionText = $previousVersion.ToString()
$versionLower = $previousVersionText.ToLowerInvariant()
$packageUrl = "$packageBaseUrl$idLower/$versionLower/$idLower.$versionLower.nupkg"

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

$changeValidation = ";EnablePackageValidation=True;PackageValidationBaselineVersion=$previousVersion;PackageValidationReferencePath=$destinationDir"
Set-PackagePipelineVariable -BaseName 'ChangeValidationProperties' -Value $changeValidation -PackageName $packageName
