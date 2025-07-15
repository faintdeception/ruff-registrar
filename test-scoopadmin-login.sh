#!/bin/bash

echo "Testing scoopadmin login with different credentials..."

# Test with admin123 password
echo "1. Testing with admin123 password..."
RESPONSE=$(curl -s -X POST \
  http://localhost:8080/realms/student-registrar/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=student-registrar-client" \
  -d "client_secret=your-secret-key" \
  -d "username=scoopadmin" \
  -d "password=admin123" \
  -d "scope=openid profile email")

if echo "$RESPONSE" | jq -e '.access_token' > /dev/null 2>&1; then
    echo "✓ Login successful with admin123"
    echo "$RESPONSE" | jq -r '.access_token'
else
    echo "✗ Login failed with admin123"
    echo "Response: $RESPONSE"
fi

# Test with password123 password
echo -e "\n2. Testing with password123 password..."
RESPONSE=$(curl -s -X POST \
  http://localhost:8080/realms/student-registrar/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=student-registrar-client" \
  -d "client_secret=your-secret-key" \
  -d "username=scoopadmin" \
  -d "password=password123" \
  -d "scope=openid profile email")

if echo "$RESPONSE" | jq -e '.access_token' > /dev/null 2>&1; then
    echo "✓ Login successful with password123"
    echo "$RESPONSE" | jq -r '.access_token'
else
    echo "✗ Login failed with password123"
    echo "Response: $RESPONSE"
fi

# Test with Admin123! password
echo -e "\n3. Testing with Admin123! password..."
RESPONSE=$(curl -s -X POST \
  http://localhost:8080/realms/student-registrar/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=student-registrar-client" \
  -d "client_secret=your-secret-key" \
  -d "username=scoopadmin" \
  -d "password=Admin123!" \
  -d "scope=openid profile email")

if echo "$RESPONSE" | jq -e '.access_token' > /dev/null 2>&1; then
    echo "✓ Login successful with Admin123!"
    echo "$RESPONSE" | jq -r '.access_token'
else
    echo "✗ Login failed with Admin123!"
    echo "Response: $RESPONSE"
fi

echo -e "\n4. Checking if user exists in Keycloak..."
# Try to check if the user exists by testing the realm info
REALM_INFO=$(curl -s http://localhost:8080/realms/student-registrar/.well-known/openid_configuration)
if echo "$REALM_INFO" | jq -e '.issuer' > /dev/null 2>&1; then
    echo "✓ Keycloak realm is accessible"
    echo "Realm issuer: $(echo "$REALM_INFO" | jq -r '.issuer')"
else
    echo "✗ Keycloak realm is not accessible"
fi
