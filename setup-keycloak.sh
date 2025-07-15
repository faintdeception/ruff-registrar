#!/bin/bash

# setup-keycloak.sh - Set up Keycloak realm and roles for Student Registrar

set -e

KEYCLOAK_URL="http://localhost:55928"
ADMIN_USER="zach-admin"
REALM_NAME="student-registrar"
CLIENT_ID="student-registrar"

echo "🔐 Setting up Keycloak for Student Registrar"
echo "============================================="
echo ""

# Function to get admin access token
get_admin_token() {
    local admin_password=$1
    curl -s -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "username=${ADMIN_USER}" \
        -d "password=${admin_password}" \
        -d "grant_type=password" \
        -d "client_id=admin-cli" | jq -r '.access_token'
}

# Prompt for admin password
echo "📋 First, get your Keycloak admin password:"
echo "   1. Start your application: dotnet run --project src/StudentRegistrar.AppHost"
echo "   2. Open Aspire Dashboard: http://localhost:15888"
echo "   3. Go to Resources tab and find the Keycloak admin password"
echo ""
read -s -p "Enter Keycloak admin password: " ADMIN_PASSWORD
echo ""

# Get admin token
echo "🔑 Getting admin access token..."
TOKEN=$(get_admin_token "$ADMIN_PASSWORD")

if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
    echo "❌ Failed to get admin token. Please check your password and try again."
    exit 1
fi

echo "✅ Admin token obtained successfully"

# Create realm
echo "🏗️  Creating realm: $REALM_NAME"
curl -s -X POST "${KEYCLOAK_URL}/admin/realms" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
        \"realm\": \"$REALM_NAME\",
        \"enabled\": true,
        \"displayName\": \"Student Registrar\",
        \"loginWithEmailAllowed\": true,
        \"registrationAllowed\": false,
        \"rememberMe\": true,
        \"verifyEmail\": false,
        \"resetPasswordAllowed\": true
    }" || echo "Realm may already exist"

# Create roles
echo "👥 Creating user roles..."
ROLES=("Administrator" "Educator" "Parent" "Student")

for role in "${ROLES[@]}"; do
    echo "   Creating role: $role"
    curl -s -X POST "${KEYCLOAK_URL}/admin/realms/$REALM_NAME/roles" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d "{
            \"name\": \"$role\",
            \"description\": \"$role role for Student Registrar\"
        }" || echo "   Role $role may already exist"
done

# Create client
echo "🔗 Creating client: $CLIENT_ID"
curl -s -X POST "${KEYCLOAK_URL}/admin/realms/$REALM_NAME/clients" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
        \"clientId\": \"$CLIENT_ID\",
        \"enabled\": true,
        \"publicClient\": false,
        \"bearerOnly\": false,
        \"standardFlowEnabled\": true,
        \"directAccessGrantsEnabled\": true,
        \"serviceAccountsEnabled\": true,
        \"redirectUris\": [\"http://localhost:3000/*\", \"http://localhost:3001/*\"],
        \"webOrigins\": [\"http://localhost:3000\", \"http://localhost:3001\"],
        \"attributes\": {
            \"saml.assertion.signature\": \"false\",
            \"saml.force.post.binding\": \"false\",
            \"saml.multivalued.roles\": \"false\",
            \"saml.encrypt\": \"false\",
            \"saml.server.signature\": \"false\",
            \"saml.server.signature.keyinfo.ext\": \"false\",
            \"exclude.session.state.from.auth.response\": \"false\",
            \"saml_force_name_id_format\": \"false\",
            \"saml.client.signature\": \"false\",
            \"tls.client.certificate.bound.access.tokens\": \"false\",
            \"saml.authnstatement\": \"false\",
            \"display.on.consent.screen\": \"false\",
            \"saml.onetimeuse.condition\": \"false\"
        }
    }" || echo "Client may already exist"

# Get client secret
echo "🔐 Retrieving client secret..."
CLIENT_UUID=$(curl -s -X GET "${KEYCLOAK_URL}/admin/realms/$REALM_NAME/clients?clientId=$CLIENT_ID" \
    -H "Authorization: Bearer $TOKEN" | jq -r '.[0].id')

if [ "$CLIENT_UUID" != "null" ] && [ -n "$CLIENT_UUID" ]; then
    CLIENT_SECRET=$(curl -s -X GET "${KEYCLOAK_URL}/admin/realms/$REALM_NAME/clients/$CLIENT_UUID/client-secret" \
        -H "Authorization: Bearer $TOKEN" | jq -r '.value')
    
    # Create test user
    echo "👤 Creating test user: scoopadmin"
    USER_RESPONSE=$(curl -s -X POST "${KEYCLOAK_URL}/admin/realms/$REALM_NAME/users" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d "{
            \"username\": \"scoopadmin\",
            \"enabled\": true,
            \"email\": \"scoopadmin@example.com\",
            \"firstName\": \"Scoop\",
            \"lastName\": \"Admin\",
            \"emailVerified\": true,
            \"credentials\": [{
                \"type\": \"password\",
                \"value\": \"K\\!rtfe413y\",
                \"temporary\": false
            }]
        }")
    
    # Get user ID and assign Administrator role
    echo "   Assigning Administrator role to scoopadmin..."
    USER_ID=$(curl -s -X GET "${KEYCLOAK_URL}/admin/realms/$REALM_NAME/users?username=scoopadmin" \
        -H "Authorization: Bearer $TOKEN" | jq -r '.[0].id')
    
    if [ "$USER_ID" != "null" ] && [ -n "$USER_ID" ]; then
        # Get Administrator role
        ADMIN_ROLE=$(curl -s -X GET "${KEYCLOAK_URL}/admin/realms/$REALM_NAME/roles/Administrator" \
            -H "Authorization: Bearer $TOKEN")
        
        # Assign role to user
        curl -s -X POST "${KEYCLOAK_URL}/admin/realms/$REALM_NAME/users/$USER_ID/role-mappings/realm" \
            -H "Authorization: Bearer $TOKEN" \
            -H "Content-Type: application/json" \
            -d "[$ADMIN_ROLE]"
        
        echo "✅ Test user created successfully!"
    else
        echo "⚠️  User creation may have failed, but setup continues..."
    fi
    
    echo "✅ Setup complete!"
    echo ""
    echo "📋 Configuration Summary:"
    echo "========================="
    echo "Realm: $REALM_NAME"
    echo "Client ID: $CLIENT_ID"
    echo "Client Secret: $CLIENT_SECRET"
    echo "Keycloak URL: $KEYCLOAK_URL"
    echo ""
    echo "👤 Test User Created:"
    echo "===================="
    echo "Username: scoopadmin"
    echo "Password: K!rtfe413y"
    echo "Email: scoopadmin@example.com"
    echo "Role: Administrator"
    echo ""
    echo "🔧 Add this to your API configuration:"
    echo "====================================="
    echo "{"
    echo "  \"Keycloak\": {"
    echo "    \"Realm\": \"$REALM_NAME\","
    echo "    \"ClientId\": \"$CLIENT_ID\","
    echo "    \"ClientSecret\": \"$CLIENT_SECRET\""
    echo "  }"
    echo "}"
    echo ""
    echo "💡 Or set as user secrets:"
    echo "=========================="
    echo "dotnet user-secrets set \"Keycloak:Realm\" \"$REALM_NAME\" --project src/StudentRegistrar.AppHost"
    echo "dotnet user-secrets set \"Keycloak:ClientId\" \"$CLIENT_ID\" --project src/StudentRegistrar.AppHost"
    echo "dotnet user-secrets set \"Keycloak:ClientSecret\" \"$CLIENT_SECRET\" --project src/StudentRegistrar.AppHost"
    echo ""
else
    echo "❌ Failed to retrieve client information"
fi

echo "🎉 Keycloak setup complete! Your configuration will now persist across restarts."
echo "   You can now create users via the API or Keycloak Admin Console."
