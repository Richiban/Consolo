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

$versionPrefix = Get-FirstPropertyValue -PropertyGroups $propertyGroups -PropertyName "VersionPrefix"

$packageName = Get-FirstPropertyValue -PropertyGroups $propertyGroups -PropertyName "PackageId"

if (-not $packageName) {
    Write-Host "No PackageId found in csproj. Falling back to Assembly Name."
    $packageName = Get-FirstPropertyValue -PropertyGroups $propertyGroups -PropertyName "AssemblyName"
}

if (-not $packageName) {
    Write-Host "No PackageId set. Defaulting to project file name without extension."
    $packageName = [System.IO.Path]::GetFileNameWithoutExtension($projectFileName)
}

$packageName = "$packageName".Trim()
$versionPrefix = if ($versionPrefix) { "$versionPrefix".Trim() } else { $null }

Write-Host "Package name: $packageName"

if ($versionPrefix) {
    Write-Host "Version prefix found in csproj: $versionPrefix"
}

# Write-Host "Listing tags:"
# git tag

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
| ForEach-Object { [System.Management.Automation.SemanticVersion]::Parse(($_ -replace "^$packageName/", "")) } `
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

if ($null -ne $versionPrefix) {
    $currentMajorVersion = [int]($versionPrefix)
}
else {
    Write-Warning "No version prefix set. Defaulting to last used"
    $currentMajorVersion = $prevVersion.Major
}

if (($branchName -eq 'master') -or ($branchName -eq 'main')) {
    $prereleaseTag = $null
}
else {
    if (-not $buildNumber) {
        Write-Error "No build number provided."
        exit 1
    }

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
Set-PackagePipelineVariable -BaseName 'PreviousVersion' -Value $prevVersion -PackageName $packageName

exit 0