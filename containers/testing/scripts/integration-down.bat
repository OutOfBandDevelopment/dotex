@echo off
REM OoBDev Integration Test Stack - Shutdown Script (Windows)
REM Stops and optionally removes all Docker services and volumes
REM
REM Usage:
REM   scripts\integration-down.bat           # Stop containers, keep volumes
REM   scripts\integration-down.bat --clean   # Stop containers and remove volumes
REM   scripts\integration-down.bat --purge   # Stop, remove volumes, and remove network
REM
REM Note: Using --clean removes all test data (recommended for CI/CD)

setlocal

REM Get script directory and change to testing directory
cd /d "%~dp0\.."

echo ======================================================================
echo OoBDev Integration Test Stack - Stopping Services
echo ======================================================================
echo.

REM Determine cleanup mode
set CLEAN_MODE=false
set PURGE_MODE=false

if "%1"=="--clean" set CLEAN_MODE=true
if "%1"=="--purge" (
    set CLEAN_MODE=true
    set PURGE_MODE=true
)

if "%CLEAN_MODE%"=="true" (
    echo Mode: CLEAN (will remove volumes and test data)
) else (
    echo Mode: STOP (will keep volumes and test data)
)

if "%PURGE_MODE%"=="true" (
    echo Mode: PURGE (will also remove orphaned containers)
)

echo.
echo Stopping Docker containers...
echo.

REM Stop services
if "%CLEAN_MODE%"=="true" (
    if "%PURGE_MODE%"=="true" (
        docker compose -f docker-compose.integration-tests.yml down -v --remove-orphans
    ) else (
        docker compose -f docker-compose.integration-tests.yml down -v
    )
) else (
    docker compose -f docker-compose.integration-tests.yml down
)

if %ERRORLEVEL% neq 0 (
    echo.
    echo ❌ Failed to stop Docker services
    echo.
    exit /b 1
)

echo.

if "%CLEAN_MODE%"=="true" (
    echo ✅ Docker services stopped and volumes removed
    echo.
    echo All test data has been cleaned up:
    echo   - MongoDB databases deleted
    echo   - SQL Server databases deleted
    echo   - RabbitMQ queues deleted
    echo   - OpenSearch indices deleted
    echo   - Qdrant collections deleted
    echo   - Azurite blobs/queues/tables deleted
    echo   - Keycloak data deleted
    echo.
) else (
    echo ✅ Docker services stopped (volumes preserved)
    echo.
    echo Test data has been preserved in Docker volumes.
    echo.
    echo To remove volumes and clean up all test data:
    echo   %~dp0integration-down.bat --clean
    echo.
    echo To manually remove volumes:
    echo   docker volume rm oobd-test-mongodb-data
    echo   docker volume rm oobd-test-sqlserver-data
    echo   docker volume rm oobd-test-rabbitmq-data
    echo   docker volume rm oobd-test-opensearch-data
    echo   docker volume rm oobd-test-qdrant-storage
    echo   docker volume rm oobd-test-qdrant-snapshots
    echo   docker volume rm oobd-test-azurite-data
    echo   docker volume rm oobd-test-keycloak-data
    echo.
)

echo To start services again:
echo   %~dp0integration-up.bat
echo.

endlocal
