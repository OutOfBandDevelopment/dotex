@echo off
REM Setup script for Ollama integration testing
REM Pulls phi3 model (small, CPU-friendly model for CI/CD)

setlocal enabledelayedexpansion

if "%CONTAINER_NAME%"=="" set CONTAINER_NAME=oobd-test-ollama
if "%OLLAMA_MODEL%"=="" set OLLAMA_MODEL=phi3

echo =========================================
echo Ollama Setup Script
echo =========================================
echo Container: %CONTAINER_NAME%
echo Model: %OLLAMA_MODEL%
echo.

REM Check if container is running
docker ps --format "{{.Names}}" | findstr /r "^%CONTAINER_NAME%$" >nul 2>&1
if errorlevel 1 (
    echo Error: Container '%CONTAINER_NAME%' is not running
    echo Please start the integration test stack first:
    echo   cd containers\testing
    echo   .\scripts\integration-up.bat
    exit /b 1
)

REM Wait for Ollama to be healthy
echo Waiting for Ollama to be ready...
set MAX_ATTEMPTS=30
set ATTEMPT=0

:wait_loop
if !ATTEMPT! geq !MAX_ATTEMPTS! goto timeout
docker exec %CONTAINER_NAME% curl -sf http://localhost:11434/api/tags >nul 2>&1
if errorlevel 1 (
    set /a ATTEMPT+=1
    echo Attempt !ATTEMPT!/%MAX_ATTEMPTS% - waiting...
    timeout /t 2 /nobreak >nul
    goto wait_loop
)
echo Ollama is ready!
goto ready

:timeout
echo Error: Ollama did not become ready in time
exit /b 1

:ready
REM Pull the model
echo.
echo Pulling model: %OLLAMA_MODEL%
echo This may take several minutes on first run...
echo.

docker exec %CONTAINER_NAME% ollama pull %OLLAMA_MODEL%

echo.
echo =========================================
echo Ollama Setup Complete!
echo =========================================
echo Model '%OLLAMA_MODEL%' is ready for testing
echo.
echo Test with:
echo   docker exec %CONTAINER_NAME% ollama run %OLLAMA_MODEL% "Hello!"
echo.

endlocal
