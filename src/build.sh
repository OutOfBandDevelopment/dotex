#!/bin/bash
set -e  # Exit immediately if a command exits with a non-zero status

export MSBUILDTERMINALLOGGER=off

# Set SolutionDir if not already set
if [ -z "$SolutionDir" ]; then
    SolutionDir="$(dirname "$(realpath "$0")")"
fi

# Set PublishPath if not already set
if [ -z "$PublishPath" ]; then
    PublishPath="$SolutionDir/publish/libs/"
fi

echo "Build Web Project"

# Remove and recreate the publish directory
rm -rf "$PublishPath"
mkdir -p "$PublishPath"

# Build the project
if ! dotnet build \
    --configuration Release \
    --output "$PublishPath" \
    /bl:logfile=./docs/build/solution.binlog; then
    echo "Build Failed! $?"
    exit 1
fi
