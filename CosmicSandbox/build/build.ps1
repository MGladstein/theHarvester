#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build script for Cosmic Sandbox

.DESCRIPTION
    Builds the Cosmic Sandbox solution with all dependencies

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release

.PARAMETER Clean
    Clean before building

.PARAMETER Test
    Run tests after building

.PARAMETER Publish
    Create publishable package

.EXAMPLE
    .\build.ps1 -Configuration Release -Test
#>

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$Clean,
    [switch]$Test,
    [switch]$Publish
)

$ErrorActionPreference = 'Stop'

# Colors for output
function Write-Header {
    param([string]$Message)
    Write-Host "`n==================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "==================================`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Yellow
}

# Get solution directory
$SolutionDir = Split-Path -Parent $PSScriptRoot
$SolutionFile = Join-Path $SolutionDir "CosmicSandbox.sln"

Write-Header "Cosmic Sandbox Build Script"

# Verify solution file exists
if (-not (Test-Path $SolutionFile)) {
    Write-Error-Custom "Solution file not found: $SolutionFile"
    exit 1
}

Write-Info "Solution: $SolutionFile"
Write-Info "Configuration: $Configuration"
Write-Info "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"

# Clean
if ($Clean) {
    Write-Header "Cleaning Solution"

    dotnet clean $SolutionFile --configuration $Configuration

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Clean completed"
    } else {
        Write-Error-Custom "Clean failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

# Restore packages
Write-Header "Restoring NuGet Packages"

dotnet restore $SolutionFile

if ($LASTEXITCODE -eq 0) {
    Write-Success "Restore completed"
} else {
    Write-Error-Custom "Restore failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Build
Write-Header "Building Solution"

dotnet build $SolutionFile `
    --configuration $Configuration `
    --no-restore `
    /p:Platform="Any CPU"

if ($LASTEXITCODE -eq 0) {
    Write-Success "Build completed"
} else {
    Write-Error-Custom "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Test
if ($Test) {
    Write-Header "Running Tests"

    dotnet test $SolutionFile `
        --configuration $Configuration `
        --no-build `
        --verbosity normal `
        --logger "trx;LogFileName=test-results.trx" `
        --collect:"XPlat Code Coverage"

    if ($LASTEXITCODE -eq 0) {
        Write-Success "All tests passed"
    } else {
        Write-Error-Custom "Tests failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

# Publish
if ($Publish) {
    Write-Header "Publishing Application"

    $PublishDir = Join-Path $SolutionDir "publish"
    $AppProject = Join-Path $SolutionDir "src\CosmicSandbox.App\CosmicSandbox.App.csproj"

    # Clean publish directory
    if (Test-Path $PublishDir) {
        Remove-Item $PublishDir -Recurse -Force
    }

    # Publish for Windows x64
    dotnet publish $AppProject `
        --configuration $Configuration `
        --runtime win-x64 `
        --self-contained true `
        --output $PublishDir `
        /p:PublishSingleFile=false `
        /p:PublishTrimmed=false `
        /p:DebugType=None `
        /p:DebugSymbols=false

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Publish completed"
        Write-Info "Output: $PublishDir"

        # Copy assets
        $AssetsSource = Join-Path $SolutionDir "assets\Processed"
        $AssetsDest = Join-Path $PublishDir "Assets"

        if (Test-Path $AssetsSource) {
            Copy-Item $AssetsSource -Destination $AssetsDest -Recurse -Force
            Write-Success "Assets copied"
        }

        # Create ZIP package
        $ZipFile = Join-Path $SolutionDir "CosmicSandbox-v1.0.0-win-x64.zip"
        if (Test-Path $ZipFile) {
            Remove-Item $ZipFile -Force
        }

        Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipFile -CompressionLevel Optimal
        Write-Success "Package created: $ZipFile"

    } else {
        Write-Error-Custom "Publish failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

Write-Header "Build Complete"
Write-Success "All operations completed successfully"
Write-Info "Build configuration: $Configuration"
Write-Info "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
