#!/bin/bash

# Enable error handling
set -e

# Check if SolutionDir is set
if [ -z "$SolutionDir" ]; then
    SolutionDir="$(dirname "$0")/"
fi

# Check if PublishPath is set
if [ -z "$PublishPath" ]; then
    PublishPath="${SolutionDir}publish/libs/"
fi

echo "Build Web Project"

# Remove the publish path if it exists
rm -rf "$PublishPath"
# Create the publish directory
mkdir -p "$PublishPath"

# Build the project
dotnet build --configuration Release --output "$PublishPath" /bl:logfile=./docs/build/solution.binlog

# Capture the exit status
TEST_ERR=$?

# Check if the build failed
if [ "$TEST_ERR" -ne 0 ]; then
    echo "Build Failed! $TEST_ERR"
    exit $TEST_ERR
fi

exit 0
