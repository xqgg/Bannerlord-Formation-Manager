# Scripts/tag-release.ps1
param(
    [string]$CompareTo = "HEAD~1",
    [switch]$Push
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

if ($CompareTo -eq "0000000000000000000000000000000000000000" -or [string]::IsNullOrEmpty($CompareTo)) {
    Write-Host "CompareTo ref was empty or all zeros. Defaulting to HEAD~1" -ForegroundColor Yellow
    $CompareTo = "HEAD~1"
}

Write-Host "Checking for releases by comparing HEAD against $CompareTo" -ForegroundColor Cyan

try {
    Write-Host "Fetching tags from origin..." -ForegroundColor Gray
    git fetch --tags
} catch {
    Write-Warning "Could not fetch tags from origin. Proceeding with local tags only."
}

try {
    $changedFiles = git diff --name-only "$CompareTo..HEAD"
} catch {
    Write-Error "Failed to run git diff. Make sure ref $CompareTo is available."
    exit 1
}

if ($changedFiles.Count -eq 0) {
    Write-Host "No changed files detected between $CompareTo and HEAD." -ForegroundColor Green
    exit 0
}

# Discover all mods dynamically (directories containing SubModule.xml)
$mods = Get-ChildItem -Path $repoRoot -Directory | Where-Object {
    Test-Path (Join-Path $_.FullName "SubModule.xml")
} | Select-Object -ExpandProperty Name

$newTags = @()

foreach ($mod in $mods) {
    $modChanges = $changedFiles | Where-Object { $_ -like "$mod/*" }
    if ($modChanges.Count -eq 0) {
        continue
    }

    $modDir = Join-Path $repoRoot $mod
    $xmlPath = Join-Path $modDir "SubModule.xml"

    $version = $null
    try {
        [xml]$xml = Get-Content $xmlPath
        $version = $xml.Module.Version.value
    } catch {
        Write-Warning "Could not parse SubModule.xml for ${mod}: $_"
        continue
    }
    if (-not $version) {
        Write-Warning "Could not read version value for $mod"
        continue
    }

    # Format tag name (e.g. FormationManager-v0.1.0)
    $tagName = "${mod}-${version}"

    $tagExists = git tag -l $tagName
    if (-not $tagExists) {
        Write-Host "Creating tag '$tagName'..." -ForegroundColor Green
        try {
            git tag -a $tagName -m "Release $mod version $version" HEAD
            $newTags += $tagName
        } catch {
            Write-Error "Failed to create tag $tagName"
            exit 1
        }
    } else {
        Write-Host "Tag '$tagName' already exists." -ForegroundColor Yellow
    }
}

if ($newTags.Count -gt 0) {
    Write-Host "Successfully created local tags: $($newTags -join ', ')" -ForegroundColor Green
    if ($Push) {
        Write-Host "Pushing new tags to origin..." -ForegroundColor Cyan
        try {
            foreach ($tag in $newTags) {
                git push origin $tag
            }
            Write-Host "Push completed successfully." -ForegroundColor Green
        } catch {
            Write-Error "Failed to push tags to origin."
            exit 1
        }
    } else {
        Write-Host "Skipped pushing tags. Run with -Push to push automatically." -ForegroundColor Yellow
    }
} else {
    Write-Host "No new tags needed." -ForegroundColor Green
}

exit 0
