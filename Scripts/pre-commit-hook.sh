#!/bin/bash
# ClawRPG pre-commit hook
# Checks for common C# syntax errors before commit

echo "Running pre-commit checks..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

ERRORS=0

# Get list of staged .cs files
CS_FILES=$(git diff --cached --name-only --diff-filter=ACM | grep '\.cs$' | head -20)

if [ -z "$CS_FILES" ]; then
    echo "No C# files to check"
    exit 0
fi

echo "Checking $CS_FILES"

# Primary: dotnet build check
echo "Running dotnet build..."
export DOTNET_ROOT=/root/.dotnet
export PATH="$DOTNET_ROOT:$PATH"

# Copy files to temp directory for build check
TEMP_DIR=$(mktemp -d)
cp $CS_FILES "$TEMP_DIR/" 2>/dev/null
cd "$TEMP_DIR"

if dotnet build 2>&1 | grep -E "CS[0-9]{4}|error"; then
    echo -e "${RED}ERROR: Syntax errors found:${NC}"
    dotnet build 2>&1 | grep -E "CS[0-9]{4}|error" | head -10
    ERRORS=$((ERRORS + 1))
    cd /project/ClawRPG
    rm -rf "$TEMP_DIR"
    echo -e "${RED}Pre-commit check failed${NC}"
    exit 1
fi

cd /project/ClawRPG
rm -rf "$TEMP_DIR"

# Check for TODO without assignee (warning only)
echo "Checking for unassigned TODOs..."
for file in $CS_FILES; do
    UNASSIGNED_TODO=$(grep -n "TODO" "$file" | grep -v "TODO(" | head -5)
    if [ -n "$UNASSIGNED_TODO" ]; then
        echo -e "${YELLOW}WARNING: Unassigned TODO in $file:${NC}"
        echo "$UNASSIGNED_TODO"
    fi
done

if [ $ERRORS -gt 0 ]; then
    echo -e "${RED}Pre-commit check failed with $ERRORS error(s)${NC}"
    exit 1
fi

echo -e "${GREEN}Pre-commit checks passed!${NC}"
exit 0
