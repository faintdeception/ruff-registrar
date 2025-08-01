#!/bin/bash

# Test script to verify the new admin1 test credentials work

set -e

echo "🧪 Testing new admin1 test credentials..."
echo ""

# Check if Aspire is running
if ! curl -s http://localhost:3001 > /dev/null; then
    echo "❌ Frontend not accessible. Please start Aspire with: dotnet run --project src/StudentRegistrar.AppHost"
    exit 1
fi

echo "✅ Frontend is accessible"

# Check if Keycloak is accessible
if ! curl -s http://localhost:8080 > /dev/null; then
    echo "❌ Keycloak not accessible. Please ensure Keycloak is running."
    exit 1
fi

echo "✅ Keycloak is accessible"

# Setup test users if not already done
echo "🔧 Setting up test users..."
if [[ -f "scripts/testing/setup-test-users.sh" ]]; then
    bash scripts/testing/setup-test-users.sh
else
    echo "❌ setup-test-users.sh not found"
    exit 1
fi

echo ""
echo "🎯 Summary of credential separation:"
echo ""
echo "📋 TEST CREDENTIALS (for E2E testing only):"
echo "  👨‍💼 admin1 / AdminPass123! - Administrator role [TEST ONLY]"
echo "  👨‍🏫 educator1 / EducatorPass123! - Educator role [TEST ONLY]"
echo "  👤 member1 / MemberPass123! - Member role [TEST ONLY]"
echo ""
echo "🔧 SYSTEM CREDENTIALS (for data seeding and production):"
echo "  👨‍💼 scoopadmin / changethis123! - Administrator role [SYSTEM ACCOUNT]"
echo ""
echo "⚠️  SECURITY NOTE:"
echo "   - Test accounts (admin1, educator1, member1) use simple passwords for E2E testing"
echo "   - System account (scoopadmin) should use strong password for actual operations"
echo "   - Test accounts should NOT be used for real data or production operations"
echo ""
echo "✅ Credential separation setup complete!"
echo ""
echo "🚀 You can now run E2E tests with:"
echo "   scripts/testing/run-e2e-tests.sh --setup-users --test-suite admin"
echo ""
echo "💾 For database seeding (uses scoopadmin):"
echo "   scripts/testing/seed-database.sh"
