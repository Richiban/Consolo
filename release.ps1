$ErrorActionPreference = "Stop"

dotnet build

dotnet test

$projectXml = [xml](Get-Content .\Consolo\Consolo.csproj)

$version = $projectXml.Project.PropertyGroup[1].version

if ($version -eq $null) {
    Write-Host "Version not found in project file"
    exit 1
}

Write-Host "Version: $version"

$tagName = "release/$version"

write-host "Debug: git tags"

git tag -l

$versionAlreadyTagged = git tag -l $tagName

if ($versionAlreadyTagged) {
    Write-Host "Version $version already tagged"
    exit 0
}

dotnet pack Consolo -o .

$nupkg = get-item Consolo.$version.nupkg

if (-not $nupkg) {
    Write-Host "NuGet package not found"
    exit 1
}

write-host $nupkg

dotnet nuget push `
    $nupkg `
    --api-key $env:NUGET_API_KEY_CONSOLO_CI `
    --source https://api.nuget.org/v3/index.json

Write-Host "Creating tag $tagName"

git tag $tagName

git push origin tag $tagName