#!/bin/bash

set -e

if ! docker ps --format '{{.Names}}' | grep -q '^estadio_db$'; then
    echo "Levantando Docker..."
    docker-compose up -d
    sleep 5
fi

echo "Aplicando migraciones..."
dotnet ef database update --project StadiumSystem.csproj --startup-project StadiumSystem.csproj 2>/dev/null || true

echo "Compilando..."
dotnet build

echo "Ejecutando..."
dotnet run --no-build

read -p "¿Detener Docker? (s/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Ss]$ ]]; then
    docker-compose down
fi
