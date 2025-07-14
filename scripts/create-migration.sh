#!/bin/bash

# Script to create and apply initial database migration

set -e

echo "Creating initial database migration..."

# Create the initial migration
dotnet ef migrations add InitialCreate --project src/StudentRegistrar.Data --startup-project src/StudentRegistrar.Api

echo "Migration created successfully!"
echo "To apply the migration, run:"
echo "dotnet ef database update --project src/StudentRegistrar.Data --startup-project src/StudentRegistrar.Api"
