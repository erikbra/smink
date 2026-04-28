#! /usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$MainBranch = "main",

    [switch]$SkipPull,
    [switch]$SkipTests,
    [switch]$Draft,
    [switch]$Prerelease,
    [switch]$NoGenerateNotes
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Description,
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host "==> $Description" -ForegroundColor Cyan
    & $Action
}

function Assert-LastExitCode {
    param([string]$ErrorMessage)
    if ($LASTEXITCODE -ne 0) {
        throw $ErrorMessage
    }
}

$tag = $Version
$title = "smink $tag"

Invoke-Step "Checking required tools" {
    git --version | Out-Null
    Assert-LastExitCode "git is required but was not found."

    dotnet --version | Out-Null
    Assert-LastExitCode "dotnet is required but was not found."

    gh --version | Out-Null
    Assert-LastExitCode "gh CLI is required but was not found."
}

Invoke-Step "Checking git repository status" {
    git rev-parse --is-inside-work-tree | Out-Null
    Assert-LastExitCode "Current directory is not a git repository."

    $status = git status --porcelain
    Assert-LastExitCode "Failed to read git status."

    if ($status) {
        throw "Working tree is not clean. Commit or stash changes before releasing."
    }
}

Invoke-Step "Switching to $MainBranch" {
    git checkout $MainBranch
    Assert-LastExitCode "Failed to checkout branch '$MainBranch'."
}

if (-not $SkipPull) {
    Invoke-Step "Pulling latest changes from origin/$MainBranch" {
        git pull origin $MainBranch
        Assert-LastExitCode "Failed to pull latest changes from origin/$MainBranch."
    }
}

if (-not $SkipTests) {
    Invoke-Step "Running unit tests" {
        dotnet test smink.UnitTests --verbosity minimal
        Assert-LastExitCode "Unit tests failed. Aborting release."
    }
}

Invoke-Step "Checking tag availability" {
    git rev-parse -q --verify "refs/tags/$tag" | Out-Null
    if ($LASTEXITCODE -eq 0) {
        throw "Tag '$tag' already exists locally."
    }

    git ls-remote --tags origin $tag | Out-Null
    if ($LASTEXITCODE -eq 0) {
        $remoteTag = git ls-remote --tags origin $tag
        if ($remoteTag) {
            throw "Tag '$tag' already exists on origin."
        }
    }
}

Invoke-Step "Creating annotated tag $tag" {
    git tag -a $tag -m "$title"
    Assert-LastExitCode "Failed to create tag '$tag'."
}

Invoke-Step "Pushing tag $tag" {
    git push origin $tag
    Assert-LastExitCode "Failed to push tag '$tag' to origin."
}

Invoke-Step "Creating GitHub release" {
    $ghArgs = @("release", "create", $tag, "--title", $title)

    if ($NoGenerateNotes) {
        $ghArgs += @("--notes", "Release $tag")
    }
    else {
        $ghArgs += "--generate-notes"
    }

    if ($Draft) {
        $ghArgs += "--draft"
    }

    if ($Prerelease) {
        $ghArgs += "--prerelease"
    }

    & gh @ghArgs
    Assert-LastExitCode "Failed to create GitHub release for '$tag'."
}

Write-Host "" 
Write-Host "Release created successfully: $tag" -ForegroundColor Green
