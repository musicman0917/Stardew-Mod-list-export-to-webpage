<#
.SYNOPSIS
  Builds a distributable zip of the mod (so end users never need to compile it
  themselves) and, optionally, publishes it as a GitHub Release.

.EXAMPLE
  .\scripts\Release.ps1
  .\scripts\Release.ps1 -PublishToGitHub
#>

param(
    [string]$ProjectDir = "$PSScriptRoot\..\StardewModListExporter",
    [string]$Configuration = "Release",
    [switch]$PublishToGitHub
)

$manifestPath = Join-Path $ProjectDir "manifest.json"
if (-not (Test-Path $manifestPath)) {
    throw "Couldn't find manifest.json at $manifestPath"
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$version = $manifest.Version
if (-not $version) {
    throw "manifest.json has no Version field"
}

Write-Host "Building $Configuration for version $version..."
dotnet build $ProjectDir --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed (exit code $LASTEXITCODE)"
}

# Find the target framework's output folder (e.g. bin\Release\net6.0) that the mod
# build package already zipped as part of the build.
$targetFrameworkDir = Get-ChildItem (Join-Path $ProjectDir "bin\$Configuration") -Directory |
    Select-Object -First 1
if (-not $targetFrameworkDir) {
    throw "Couldn't find a build output folder under bin\$Configuration - did the build actually run?"
}

$builtZip = Get-ChildItem $targetFrameworkDir.FullName -Filter "*.zip" | Select-Object -First 1
if (-not $builtZip) {
    throw "No zip found in $($targetFrameworkDir.FullName) - is EnableModZip set in the csproj?"
}

$distDir = Join-Path "$PSScriptRoot\.." "dist"
New-Item -ItemType Directory -Force -Path $distDir | Out-Null

$releaseZipName = "ModListExporter-v$version.zip"
$releaseZipPath = Join-Path $distDir $releaseZipName
Copy-Item $builtZip.FullName $releaseZipPath -Force

Write-Host "`nRelease zip ready: $releaseZipPath"
Write-Host "This is the file to attach to a GitHub Release, Nexus Mods upload, etc."
Write-Host "Give end users INSTALL.md alongside it - they extract this zip's contents"
Write-Host "into their Mods folder as described there."

if ($PublishToGitHub) {
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        throw "GitHub CLI ('gh') not found. Install it from https://cli.github.com/ and run 'gh auth login' first, or omit -PublishToGitHub and upload the zip manually."
    }

    $tag = "v$version"
    Write-Host "`nCreating GitHub release $tag..."
    gh release create $tag $releaseZipPath --title $tag --notes "See INSTALL.md for setup instructions."
    if ($LASTEXITCODE -ne 0) {
        throw "gh release create failed (exit code $LASTEXITCODE) - does tag $tag already exist?"
    }
}
