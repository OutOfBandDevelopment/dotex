#!/bin/bash
# Initialize all git submodules in the repository

set -e

echo "Initializing git submodules..."
git submodule update --init --recursive

echo ""
echo "Pulling LFS files in submodules..."
git submodule foreach --recursive 'git lfs pull || true'

echo ""
echo "✓ Submodules initialized successfully"
