#!/usr/bin/env bash
# Runs the plugin sandbox spike tests on Linux in the dotnet SDK container with bubblewrap installed (REQ-008).
# The repo is mounted read-only and a copy of src (without bin, obj and .git) is built inside the container.
# seccomp is relaxed because bubblewrap needs to create user namespaces, which the default Docker profile blocks.
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
filter="${1:-FullyQualifiedName~LinuxEscapeTests}"
MSYS_NO_PATHCONV=1 docker run --rm \
  --security-opt seccomp=unconfined --security-opt apparmor=unconfined \
  -v "$root:/repo:ro" \
  mcr.microsoft.com/dotnet/sdk:10.0 bash -c "
    set -e
    apt-get update -qq && apt-get install -y -qq bubblewrap >/dev/null
    mkdir /work && cd /repo && tar --exclude=bin --exclude=obj --exclude=.git --exclude=.vs --exclude=TestResults -cf - src | tar -xf - -C /work
    cp -r /repo/.editorconfig /work/ 2>/dev/null || true
    cd /work/src/Spikes
    dotnet test OoBDev.Spike.PluginSandbox.Tests -p:DisableGitVersionTask=true --filter '$filter' 2>&1 | tail -40
  "
