#!/bin/bash
set -e

echo "== Checking for .NET Runtime 10 =="

if command -v dotnet &> /dev/null; then
    if dotnet --list-runtimes | grep -q "Microsoft.NETCore.App 10"; then
        echo ".NET 10 runtime is already installed. Skipping step"
    else
        echo "Installing .NET Runtime 10..."
        sudo apt update
        sudo apt install -y dotnet-runtime-10.0
    fi
else
    echo "Installing .NET Runtime 10..."
    sudo apt update
    sudo apt install -y dotnet-runtime-10.0
fi

echo "== Done =="