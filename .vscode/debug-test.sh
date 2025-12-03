#!/bin/bash

# Debug script for running NUnit tests with netcoredbg
export DOTNET_ROOT=/opt/homebrew
NETCOREDBG="${PWD}/.vscode/netcoredbg/netcoredbg/netcoredbg"
DOTNET="/opt/homebrew/bin/dotnet"

# Parse arguments
TEST_FILTER=""
if [ "$1" == "--filter" ]; then
    TEST_FILTER="--filter FullyQualifiedName~$2"
fi

# Run netcoredbg with dotnet test
exec "${NETCOREDBG}" --interpreter=vscode -- "${DOTNET}" test "${PWD}/UnitTests/UnitTests.csproj" --no-build ${TEST_FILTER} --logger:console;verbosity=detailed
