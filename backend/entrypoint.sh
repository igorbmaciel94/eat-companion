#!/bin/bash
set -e

echo "Applying database migrations..."
./efbundle --connection "$ConnectionStrings__DefaultConnection" || true

echo "Starting Eat Companion API..."
exec dotnet EatCompanion.Api.dll
