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

# Check for missing parentheses after 'new Type'
echo "Checking for missing parentheses..."
for file in $CS_FILES; do
    # Check for "new [A-Z][a-zA-Z]*;" pattern (likely missing parens)
    MISSING_PARENS=$(grep -n "new [A-Z][a-zA-Z]*;" "$file" | grep -v "//" | grep -v "=>" | head -5)
    if [ -n "$MISSING_PARENS" ]; then
        echo -e "${RED}ERROR: Possible missing parentheses in $file:${NC}"
        echo "$MISSING_PARENS"
        ERRORS=$((ERRORS + 1))
    fi
    
    # Check for double semicolons
    DOUBLE_SEMI=$(grep -n ";;\|,\s*;" "$file" | head -5)
    if [ -n "$DOUBLE_SEMI" ]; then
        echo -e "${RED}ERROR: Double semicolon in $file:${NC}"
        echo "$DOUBLE_SEMI"
        ERRORS=$((ERRORS + 1))
    fi
done

# Check for TODO without assignee
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
