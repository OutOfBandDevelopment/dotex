@echo off
REM OoBDev Integration Test Stack - Startup Script (Windows)
REM Starts all Docker services needed for integration testing
REM
REM Usage:
REM   scripts\integration-up.bat                # Start in detached mode
REM   scripts\integration-up.bat --wait         # Start and wait for health checks
REM   scripts\integration-up.bat --build        # Rebuild images before starting
REM   scripts\integration-up.bat --build --wait # Rebuild and wait for health checks
REM
REM Requirements:
REM   - Docker Desktop for Windows installed
REM   - Ports available: 25, 1433, 4566, 5000, 5080, 5672-5673, 6333-6334, 6379, 7777, 8081, 9200, 9998, 10000-10002, 11434, 15672, 27017

setlocal

echo Running: %0
set SCRIPT_PATH=%~dp0
echo SCRIPT_PATH: %SCRIPT_PATH%
set TESTING_DIR=%SCRIPT_PATH%..\
echo TESTING_DIR: %TESTING_DIR%
set STARTING_PATH=%CD%
echo STARTING_PATH: %STARTING_PATH%

REM Parse command line arguments
set BUILD_FLAG=
set WAIT_FLAG=

:parse_args
if "%1"=="" goto :end_parse
if /i "%1"=="--build" set BUILD_FLAG=--build
if /i "%1"=="--wait" set WAIT_FLAG=true
shift
goto :parse_args
:end_parse

REM Get script directory and change to testing directory
PUSHD "%TESTING_DIR%"

echo ======================================================================
echo OoBDev Integration Test Stack - Starting Services
echo ======================================================================
echo.
echo Working directory: %CD%
echo Compose file: docker-compose.integration-tests.yml
echo Environment: .env.integration
echo.

REM Load default environment variables
if exist ".env.integration" (
    echo Loading environment from .env.integration
    for /f "delims=" %%i in (.env.integration) do (
        set %%i
    )
) else (
    echo Warning: .env.integration not found, using defaults
)

echo.
if defined BUILD_FLAG (
    echo Rebuilding Docker images and starting containers...
) else (
    echo Starting Docker containers...
)
echo.

REM Start services in detached mode
docker compose -f docker-compose.integration-tests.yml up -d %BUILD_FLAG%

if %ERRORLEVEL% neq 0 (
    echo.
    echo ❌ Failed to start Docker services
    echo.
    echo Check Docker Desktop is running and try again.
    exit /b 1
)

echo.
echo ✅ Docker services started successfully
echo.
echo Services running:
echo   - Apache Tika:        http://localhost:9998
echo   - SMTP4Dev:           http://localhost:7777
echo   - MongoDB:            mongodb://localhost:27017
echo   - SQL Server:         localhost,1433 (sa/IntegrationTest123!)
echo   - RabbitMQ:           amqp://localhost:5673, http://localhost:15672
echo   - Redis:              localhost:6379
echo   - OpenSearch:         https://localhost:9200 (admin/IntegrationTest123!)
echo   - Qdrant:             http://localhost:6333
echo   - Azurite:            http://localhost:10000 (Blob), 10001 (Queue), 10002 (Table)
echo   - LocalStack:         http://localhost:4566
echo   - Service Bus:        localhost:5672
echo   - Keycloak:           http://localhost:8081 (admin/admin)
echo   - SBert:              http://localhost:5080
echo   - Ollama:             http://localhost:11434
echo   - Azurinsight:        http://localhost:5000
echo.

REM Check if --wait flag is provided
if "%WAIT_FLAG%"=="true" (
    echo Waiting for services to become healthy...
    echo.
    call "%SCRIPT_PATH%wait-for-services.bat"

    if %ERRORLEVEL% equ 0 (
        echo.
        echo ======================================================================
        echo ✅ All services are healthy!
        echo ======================================================================
        echo.

        REM Setup Ollama model if container is running
        echo Setting up Ollama model...
        call "%SCRIPT_PATH%setup-ollama.bat"
        if %ERRORLEVEL% equ 0 (
            echo ✅ Ollama model ready
        ) else (
            echo ⚠️  Ollama model setup failed (may already be installed)
        )

        echo.
        echo ======================================================================
        echo ✅ Stack is ready for testing!
        echo ======================================================================
        echo.
        echo You can now run integration tests:
        echo   cd ..\..\src
        echo   dotnet test --filter TestCategory=Integration
        echo.
    ) else (
        echo.
        echo ======================================================================
        echo ❌ Some services failed to become healthy
        echo ======================================================================
        echo.
        echo Check service health:
        echo   docker compose -f docker-compose.integration-tests.yml ps
        echo.
        echo View logs:
        echo   docker compose -f docker-compose.integration-tests.yml logs [service-name]
        echo.
        exit /b 1
    )
) else (
    echo Services are starting in the background...
    echo.
    echo To wait for health checks:
    echo   %SCRIPT_PATH%wait-for-services.bat
    echo.
    echo To check status:
    echo   docker compose -f docker-compose.integration-tests.yml ps
    echo.
    echo To view logs:
    echo   docker compose -f docker-compose.integration-tests.yml logs -f [service-name]
    echo.
    echo To stop services:
    echo   %SCRIPT_PATH%integration-down.bat
    echo.
)

POPD
endlocal