#!/bin/bash

# Debug script for running NUnit tests with netcoredbg
# Debug Tests with Breakpoints
#
# This script launches tests with VSTEST_HOST_DEBUG=1 which pauses the test host
# and waits for you to attach a debugger.
#
# WORKFLOW:
# 1. Set breakpoints in your test files
# 2. Run this script in a terminal
# 3. When you see "Waiting for debugger to attach...", press F5 in VS Code
# 4. Select "Attach to testhost" and pick the "testhost" process
# 5. Your breakpoints will be hit!

cd "$(dirname "$0")/.."

echo "Building project..."
dotnet build UnitTests/UnitTests.csproj

echo ""
echo "Starting tests with debugger wait..."
echo "When the tests pause, press F5 and attach to the 'testhost' process"
echo ""

VSTEST_HOST_DEBUG=1 dotnet test UnitTests/UnitTests.csproj --no-build
