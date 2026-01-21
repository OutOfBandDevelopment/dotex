@echo off
REM Initialize all git submodules in the repository

echo Initializing git submodules...
git submodule update --init --recursive

echo.
echo Pulling LFS files in submodules...
git submodule foreach --recursive "git lfs pull || exit 0"

echo.
echo Submodules initialized successfully
