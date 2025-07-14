#!/bin/bash

# generate-passwords.sh - Generate secure passwords for Student Registrar

set -e

echo "🔐 Student Registrar - Password Generator"
echo "========================================"
echo ""

# Function to generate a secure password
generate_password() {
    openssl rand -base64 32 | tr -d "=+/" | cut -c1-25
}

# Check if .env file exists
if [ -f ".env" ]; then
    echo "⚠️  .env file already exists. Backup will be created."
    cp .env .env.backup.$(date +%Y%m%d_%H%M%S)
fi

# Generate passwords
POSTGRES_PASSWORD=$(generate_password)
KEYCLOAK_ADMIN_PASSWORD=$(generate_password)

# Create .env file
cat > .env << EOF
# Environment variables for StudentRegistrar
# Generated on $(date)

# PostgreSQL password for Docker Compose deployment
POSTGRES_PASSWORD=${POSTGRES_PASSWORD}

# Keycloak admin password for Docker Compose deployment
KEYCLOAK_ADMIN_PASSWORD=${KEYCLOAK_ADMIN_PASSWORD}

# Other configuration
ASPNETCORE_ENVIRONMENT=Development

# NOTE: When using .NET Aspire (recommended), these passwords are auto-generated
# and do not need to be set manually. This file is only for Docker Compose deployments.
EOF

echo "✅ Generated secure passwords and saved to .env file"
echo ""
echo "📋 Generated credentials:"
echo "   PostgreSQL password: ${POSTGRES_PASSWORD}"
echo "   Keycloak admin password: ${KEYCLOAK_ADMIN_PASSWORD}"
echo ""
echo "🔒 Security notes:"
echo "   - Keep these passwords secure and private"
echo "   - Change them before deploying to production"
echo "   - Never commit the .env file to version control"
echo "   - Use a password manager for production environments"
echo ""
echo "🚀 You can now run: docker-compose up -d"
