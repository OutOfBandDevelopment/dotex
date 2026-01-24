@echo off
REM OoBDev Integration Test Stack - Health Check Script (Windows)
REM Waits for all Docker services to become healthy before proceeding
REM
REM Usage:
REM   scripts\wait-for-services.bat            # Wait up to 120 seconds (default)
REM   scripts\wait-for-services.bat 180        # Wait up to 180 seconds
REM
REM Exit codes:
REM   0 - All services are healthy
REM   1 - Timeout reached, some services are not healthy

setlocal enabledelayedexpansion

echo Running: %0
set SCRIPT_PATH=%~dp0
echo SCRIPT_PATH: %SCRIPT_PATH%
set TESTING_DIR=%SCRIPT_PATH%..\
echo TESTING_DIR: %TESTING_DIR%
set STARTING_PATH=%CD%
echo STARTING_PATH: %STARTING_PATH%

REM Get script directory and change to testing directory
PUSHD "%TESTING_DIR%"

REM Configuration
set TIMEOUT=%1
if "%TIMEOUT%"=="" set TIMEOUT=120
set /a INTERVAL=5
set COMPOSE_FILE=docker-compose.integration-tests.yml

echo ======================================================================
echo Waiting for Docker services to become healthy...
echo ======================================================================
echo.
echo Timeout: %TIMEOUT% seconds
echo Check interval: %INTERVAL% seconds
echo.

REM Service list (container names from docker-compose file)
set SERVICES=oobd-test-tika oobd-test-smtp oobd-test-mongodb oobd-test-sqlserver oobd-test-rabbitmq oobd-test-redis oobd-test-opensearch oobd-test-qdrant oobd-test-azurite oobd-test-localstack oobd-test-servicebus oobd-test-keycloak oobd-test-sbert oobd-test-ollama oobd-test-azurinsight

REM Main health check loop
set /a elapsed=0
set all_healthy=0

:check_loop
if %elapsed% geq %TIMEOUT% goto timeout_reached

set all_healthy=1
set /a healthy_count=0
set /a total_count=0

REM Check each service
for %%s in (%SERVICES%) do (
    set /a total_count+=1

    REM Check if container exists and get health status
    for /f "delims=" %%h in ('docker inspect --format="{{.State.Health.Status}}" %%s 2^>nul') do set health_status=%%h

    REM If health check failed, try to get running status
    if "!health_status!"=="" (
        for /f "delims=" %%r in ('docker inspect --format="{{.State.Status}}" %%s 2^>nul') do set health_status=%%r
    )

    REM Check health status
    if "!health_status!"=="healthy" (
        set /a healthy_count+=1
    ) else if "!health_status!"=="running" (
        set /a healthy_count+=1
    ) else (
        set all_healthy=0
    )
)

REM Check if all services are healthy
if %all_healthy%==1 (
    echo.
    echo ✅ All services are healthy!
    POPD
    exit /b 0
)

REM Print progress
<nul set /p="Waiting... [%elapsed%s/%TIMEOUT%s] Healthy: %healthy_count%/%total_count%"
echo.

REM Wait before next check
timeout /t %INTERVAL% /nobreak >nul 2>&1
set /a elapsed+=%INTERVAL%

goto check_loop

:timeout_reached
echo.
echo.
echo ❌ Timeout reached (%TIMEOUT%s)
echo.
echo Services that are not healthy:
echo.

REM Show detailed status for each service
for %%s in (%SERVICES%) do (
    for /f "delims=" %%h in ('docker inspect --format="{{.State.Health.Status}}" %%s 2^>nul') do set health_status=%%h

    if "!health_status!"=="" (
        for /f "delims=" %%r in ('docker inspect --format="{{.State.Status}}" %%s 2^>nul') do set health_status=%%r
    )

    if "!health_status!"=="healthy" (
        echo   ✅ %%s
    ) else if "!health_status!"=="running" (
        echo   ✅ %%s
    ) else if "!health_status!"=="starting" (
        echo   ⏳ %%s (still starting)
    ) else if "!health_status!"=="" (
        echo   ❌ %%s (not found)
    ) else (
        echo   ❌ %%s (status: !health_status!)
    )
)

echo.
echo Troubleshooting:
echo.
echo 1. Check service status:
echo    docker compose -f %COMPOSE_FILE% ps
echo.
echo 2. View logs for failing services:
echo    docker compose -f %COMPOSE_FILE% logs [service-name]
echo.
echo 3. Restart services:
echo    %SCRIPT_PATH%integration-down.bat --clean
echo    %SCRIPT_PATH%integration-up.bat
echo.

POPD
exit /b 1
