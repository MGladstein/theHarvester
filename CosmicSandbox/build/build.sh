#!/bin/bash
set -e

# Cosmic Sandbox Build Script (Linux/macOS)
# Note: This is primarily for CI/CD pipelines
# The application targets Windows, but can be built on other platforms

CONFIGURATION="${1:-Release}"
SOLUTION_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOLUTION_FILE="$SOLUTION_DIR/CosmicSandbox.sln"

echo "========================================"
echo "Cosmic Sandbox Build Script"
echo "========================================"
echo ""
echo "Solution: $SOLUTION_FILE"
echo "Configuration: $CONFIGURATION"
echo "Date: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""

# Verify solution exists
if [ ! -f "$SOLUTION_FILE" ]; then
    echo "Error: Solution file not found: $SOLUTION_FILE"
    exit 1
fi

# Restore packages
echo "========================================"
echo "Restoring NuGet Packages"
echo "========================================"
dotnet restore "$SOLUTION_FILE"

# Build
echo ""
echo "========================================"
echo "Building Solution"
echo "========================================"
dotnet build "$SOLUTION_FILE" \
    --configuration "$CONFIGURATION" \
    --no-restore

# Test
echo ""
echo "========================================"
echo "Running Tests"
echo "========================================"
dotnet test "$SOLUTION_FILE" \
    --configuration "$CONFIGURATION" \
    --no-build \
    --verbosity normal

echo ""
echo "========================================"
echo "Build Complete"
echo "========================================"
echo "Configuration: $CONFIGURATION"
echo "Timestamp: $(date '+%Y-%m-%d %H:%M:%S')"
