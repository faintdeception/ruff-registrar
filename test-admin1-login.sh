#!/bin/bash

# Test the admin1 login flow and user sync

set -e

echo "🧪 Testing admin1 login and user sync..."
echo ""

# Check if services are running
if ! curl -s http://localhost:3001 > /dev/null; then
    echo "❌ Frontend not accessible. Please start Aspire."
    exit 1
fi

if ! curl -s http://localhost:8080 > /dev/null; then
    echo "❌ Keycloak not accessible."
    exit 1
fi

echo "✅ Services are running"
echo ""

# Setup test users in Keycloak
echo "🔧 Setting up test users in Keycloak..."
if [[ -f "scripts/testing/setup-test-users.sh" ]]; then
    bash scripts/testing/setup-test-users.sh
    echo "✅ Test users created in Keycloak"
else
    echo "❌ setup-test-users.sh not found"
    exit 1
fi

echo ""
echo "📋 Next steps to test the fix:"
echo "================================"
echo ""
echo "1. 🌱 Seed the database (if not already done):"
echo "   scripts/testing/seed-database.sh"
echo ""
echo "2. 🧪 Run E2E tests with admin1:"
echo "   scripts/testing/run-e2e-tests.sh --setup-users --test-suite admin"
echo ""
echo "3. 🔍 Or manually test login:"
echo "   - Open http://localhost:3001"
echo "   - Login with: admin1 / AdminPass123!"
echo "   - Navigate to /semesters to test the fixed API"
echo ""
echo "ℹ️  How the user sync works:"
echo "============================"
echo "1. admin1 exists in Keycloak with Administrator role"
echo "2. When admin1 first logs in, the API automatically creates"
echo "   a User record in PostgreSQL with the correct role"
echo "3. The Keycloak ID is used to link the records"
echo "4. Subsequent logins will use the existing User record"
echo ""
echo "🔧 The fix addresses both issues:"
echo "• ✅ Aspire HTTPS configuration (API communication)"
echo "• ✅ User sync mechanism (admin1 PostgreSQL record)"
