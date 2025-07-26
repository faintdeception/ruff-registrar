#!/bin/bash

echo "Testing scoopadmin login with the correct credentials..."

# Use the correct port and configuration from setup-keycloak.sh output
KEYCLOAK_URL="http://localhost:8080"
REALM_NAME="student-registrar"
CLIENT_ID="student-registrar"
CLIENT_SECRET="hpWpXCmMHAzDy0FrwUwrBONtTdoeXBNx"

# Test with K!rtfe413y password (from setup script)
echo "1. Testing with K!rtfe413y password..."
RESPONSE=$(curl -s -X POST \
  "${KEYCLOAK_URL}/realms/${REALM_NAME}/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=${CLIENT_ID}" \
  -d "client_secret=${CLIENT_SECRET}" \
  -d "username=scoopadmin" \
  -d "password=K!rtfe413y")

if echo "$RESPONSE" | jq -e '.access_token' > /dev/null 2>&1; then
    echo "✓ Login successful with K!rtfe413y"
    echo "Access token: $(echo "$RESPONSE" | jq -r '.access_token')"
else
    echo "✗ Login failed with K!rtfe413y"
    echo "Response: $RESPONSE"
fi

# Test with admin123 password (fallback test)
echo -e "\n2. Testing with admin123 password..."
RESPONSE=$(curl -s -X POST \
  "${KEYCLOAK_URL}/realms/${REALM_NAME}/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=${CLIENT_ID}" \
  -d "client_secret=${CLIENT_SECRET}" \
  -d "username=scoopadmin" \
  -d "password=admin123")

if echo "$RESPONSE" | jq -e '.access_token' > /dev/null 2>&1; then
    echo "✓ Login successful with admin123"
    echo "Access token: $(echo "$RESPONSE" | jq -r '.access_token')"
else
    echo "✗ Login failed with admin123"
    echo "Response: $RESPONSE"
fi

echo -e "\n3. Checking if user exists in Keycloak..."
# Try to check if the user exists by testing the realm info
REALM_INFO=$(curl -s "${KEYCLOAK_URL}/realms/${REALM_NAME}/.well-known/openid_configuration")
if echo "$REALM_INFO" | jq -e '.issuer' > /dev/null 2>&1; then
    echo "✓ Keycloak realm is accessible"
    echo "Realm issuer: $(echo "$REALM_INFO" | jq -r '.issuer')"
else
    echo "✗ Keycloak realm is not accessible"
fi
