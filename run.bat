@echo off
setlocal enabledelayedexpansion

for /f %%i in ('docker ps --format {{.Names}} 2^>nul ^| find "estadio_db"') do set RUNNING=1

if not defined RUNNING (
    echo Levantando Docker...
    docker-compose up -d
    timeout /t 5 /nobreak >nul
)

echo Aplicando migraciones...
dotnet ef database update --project StadiumSystem.csproj --startup-project StadiumSystem.csproj >nul 2>&1

echo Compilando...
dotnet build

echo Ejecutando...
dotnet run --no-build

set /p choice="Detener Docker (s/n): "
if /i "%choice%"=="s" docker-compose down
