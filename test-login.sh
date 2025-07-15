#!/bin/bash

# Test script for login flow
echo "Testing login flow..."

# Test 1: Check if login page is accessible
echo "1. Testing login page..."
curl -s -I http://localhost:3001/login | head -1

# Test 2: Try to get token with scoopadmin user
echo "2. Testing Keycloak authentication with scoopadmin user..."
TOKEN_RESPONSE=$(curl -s -X POST http://localhost:8080/realms/student-registrar/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=student-registrar" \
  -d "client_secret=hpWpXCmMHAzDy0FrwUwrBONtTdoeXBNx" \
  -d "username=scoopadmin" \
  -d "password=K\!rtfe413y")

echo "Token response: $TOKEN_RESPONSE"

# Test 3: If token is successful, test API call
if echo "$TOKEN_RESPONSE" | grep -q "access_token"; then
    echo "3. Token obtained successfully, testing API call..."
    USER_TOKEN=$(echo "$TOKEN_RESPONSE" | python3 -c "import json,sys; print(json.load(sys.stdin)['access_token'])")
    
    echo "Testing /api/users/me endpoint..."
    curl -s -H "Authorization: Bearer $USER_TOKEN" http://localhost:5000/api/users/me
else
    echo "3. Token request failed"
fi

echo -e "\nDone!"
