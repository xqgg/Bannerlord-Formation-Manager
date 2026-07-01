# scripts/publish.ps1
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot "publish"
$tempPublishDir = Join-Path $repoRoot ".tmp_publish"
$gameModulesRoot = "E:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\Modules"
$modName = "FormationManager"

# 1. Build project
Write-Host "Building project in Release configuration..." -ForegroundColor Cyan
$projectPath = Join-Path (Join-Path $repoRoot $modName) "$modName.csproj"
& dotnet build $projectPath -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed."
    exit $LASTEXITCODE
}

# 2. Package zip
Write-Host "Packaging release zip..." -ForegroundColor Cyan
if (-not (Test-Path $publishDir)) {
    New-Item -ItemType Directory -Path $publishDir | Out-Null
}

if (Test-Path $tempPublishDir) {
    Remove-Item -Recurse -Force $tempPublishDir | Out-Null
}
New-Item -ItemType Directory -Path (Join-Path $tempPublishDir $modName) | Out-Null

$sourcePath = Join-Path $gameModulesRoot $modName
if (-not (Test-Path $sourcePath)) {
    Write-Error "Could not find deployed mod files at $sourcePath"
    exit 1
}

# Copy files
Copy-Item -Path "$sourcePath\*" -Destination (Join-Path $tempPublishDir $modName) -Recurse -Force | Out-Null

# Exclude pdb
Get-ChildItem -Path (Join-Path $tempPublishDir $modName) -Filter "*.pdb" -Recurse | Remove-Item -Force | Out-Null

$zipPath = Join-Path $publishDir "$modName.zip"
if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath | Out-Null
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($tempPublishDir, $zipPath)

# Cleanup
if (Test-Path $tempPublishDir) {
    Remove-Item -Recurse -Force $tempPublishDir | Out-Null
}

Write-Host "Mod packaged successfully at $zipPath" -ForegroundColor Green
