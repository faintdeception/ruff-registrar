#!/bin/bash

# Simple script to run E2E tests (assumes application is already running)
# Usage: 
#   ./test-e2e-only.sh          # Run with browser visible (default)
#   ./test-e2e-only.sh headless # Run in headless mode

HEADLESS_MODE=${1:-""}

echo "Running E2E tests..."
echo "Make sure the Student Registrar application is running on http://localhost:3001"

if [ "$HEADLESS_MODE" = "headless" ]; then
    echo "Running in HEADLESS mode (no browser window)"
    echo ""
    # Run in headless mode
    SeleniumSettings__Headless=true dotnet test tests/StudentRegistrar.E2E.Tests/ \
      --logger "console;verbosity=normal" \
      --collect:"XPlat Code Coverage"
else
    echo "Running with BROWSER VISIBLE (you can watch the tests)"
    echo ""
    # Run with browser visible (uses appsettings.json setting)
    dotnet test tests/StudentRegistrar.E2E.Tests/ \
      --logger "console;verbosity=normal" \
      --collect:"XPlat Code Coverage"
fi

echo "E2E test run completed."
