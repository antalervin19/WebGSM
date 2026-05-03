#!/bin/bash
set -e

echo "== Updating system =="
sudo apt update

echo "== Checking for .NET Runtime 10 =="

if command -v dotnet &> /dev/null; then
    if dotnet --list-runtimes | grep -q "Microsoft.NETCore.App 10"; then
        echo ".NET 10 runtime is installed. Skipping."
    else
        echo "Installing .NET Runtime 10..."
        sudo apt install -y dotnet-runtime-10.0
    fi
else
    echo "dotnet not found. Installing .NET Runtime 10..."
    sudo apt install -y dotnet-runtime-10.0
fi

echo "== Checking for LXD =="

if command -v lxc &> /dev/null; then
    echo "LXD is installed."
else
    echo "Installing LXD..."
    sudo apt install -y lxd
fi

echo "== Checking LXD initialization =="

if [ ! -d /var/snap/lxd/common/lxd ]; then
    echo "Initializing LXD..."
    sudo lxd init --auto
else
    echo "LXD is initialized. Skipping."
fi

echo "== Done =="