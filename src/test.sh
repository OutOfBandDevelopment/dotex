#!/bin/bash

# Enable error handling
set -e

TestProject="OoBDev.Libs.sln"
# Configuration="Debug" # Uncomment if needed
Configuration="Release"

LATEST_TEST_RESULTS_TRX="./TestResults/Coverage/Reports/LatestTestResults.trx"

# Handle command-line arguments
if [[ "$1" == "--no-start" ]]; then
    DO_NOT_START=1
    shift
fi

if [[ "$1" == "--no-run" ]]; then
    DO_NOT_RUN=1
    shift
fi

# Reset TestFilter if it was previously set
if [[ -n "$1" && -n "$TestFilter" ]]; then
    TestFilter=""
fi

# Process TestFilter arguments
while [[ -n "$1" ]]; do
    temp_filter="$1"
    if [[ -z "$TestFilter" ]]; then
        TestFilter="TestCategory=$temp_filter"
    else
        TestFilter="$TestFilter|TestCategory=$temp_filter"
    fi
    shift
done

# Default TestFilter if none was provided
if [[ -z "$TestFilter" ]]; then
    TestFilter="TestCategory=Unit|TestCategory=Simulate"
fi

rm -rf "./TestResults"
echo "$TestFilter"

# Install report generator tool if not already installed
dotnet tool install --global dotnet-reportgenerator-globaltool 2>/dev/null || true

# Run tests if DO_NOT_RUN is not set
if [[ -z "$DO_NOT_RUN" ]]; then
    rm -rf "./TestResults" 2>/dev/null || true
    mkdir -p "./TestResults/Coverage/Reports" 2>/dev/null || true

    echo "Run Unit Tests as $Configuration"
    dotnet test "$TestProject" \
        --configuration "$Configuration" \
        --nologo \
        --filter "$TestFilter"
fi

TEST_ERR=$?

# Echo statements related to source linking are commented out
# Uncomment if necessary
# echo "Run Fix-SourceLink"
# dotnet run \
#     --project Tools/OoBDev.FixSourceLinks.Cli \
#     --configuration Release \
#     -- \
#     --include=./TestResults/**/*coverage.*;./TestResults/**/*Cobertura.* \
#     --target=./

echo "Merge Test Results"
pushd "./TestResults" > /dev/null
dotnet dotnet-coverage merge \
    coverage.*.xml \
    --recursive \
    --output "./Cobertura.coverage" \
    --output-format cobertura
popd > /dev/null

echo "Run trx-merge"
dotnet trx-merge \
    --dir="./TestResults" \
    --output="$LATEST_TEST_RESULTS_TRX" \
    --loglevel=Verbose \
    --recursive

# Fix the XML issue with PowerShell
pwsh -Command "& {param(\$FileName) (Get-Content \$FileName).Replace('xmlns=\"\"\"\"','') | Set-Content \$FileName }" -ArgumentList ignored "$LATEST_TEST_RESULTS_TRX"

echo "Run reportgenerator"
reportgenerator "-reports:./TestResults/**/coverage.*.xml" "-targetDir:./TestResults/Coverage/Reports" -reportTypes:Html;HtmlSummary;Cobertura;MarkdownSummary "-title:$TestProject - ($Configuration)"

# Open results if DO_NOT_START is not set
if [[ -z "$DO_NOT_START" ]]; then
    # Start the summary report in the default browser
    xdg-open "./TestResults/Coverage/Reports/index.html" &
    xdg-open "./TestResults/Cobertura.coverage" &
    # Uncomment the following line if you want to open the trx file
    # xdg-open "$LATEST_TEST_RESULTS_TRX" &
fi

echo "TEST_ERR=$TEST_ERR"
if [[ "$TEST_ERR" -eq 0 ]]; then
    echo "No Errors :)"
fi

exit $TEST_ERR
