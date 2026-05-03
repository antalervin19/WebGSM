#!/bin/bash
set -e

echo "== Updating system =="
sudo apt update

# ---------------- .NET ----------------
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

# ---------------- LXD ----------------
echo "== Checking for LXD =="

if ! command -v lxc &> /dev/null; then
    echo "Installing LXD..."
    sudo apt install -y lxd
else
    echo "LXD already installed."
fi

# ---------------- INIT ----------------
echo "== Ensuring LXD is initialized =="

if ! lxc info &> /dev/null; then
    echo "Initializing LXD..."
    sudo lxd init --auto
    sleep 5
else
    echo "LXD already initialized."
fi

# ---------------- STORAGE ----------------
echo "== Ensuring storage pool =="

if ! lxc storage list | grep -q "default"; then
    echo "Creating default storage pool..."
    lxc storage create default dir
else
    echo "Storage pool already exists."
fi

# ---------------- PROFILE ROOT DISK ----------------
echo "== Ensuring default profile root disk =="

lxc profile device add default root disk path=/ pool=default 2>/dev/null || true

# ---------------- NETWORK ----------------
echo "== Ensuring LXD network =="

if ! lxc network list | grep -q lxdbr0; then
    echo "Creating default network bridge..."
    lxc network create lxdbr0 ipv4.address=auto ipv6.address=auto
else
    echo "Network bridge already exists."
fi

# ---------------- PROFILE NETWORK ----------------
echo "== Attaching network to default profile =="

lxc profile device add default eth0 nic network=lxdbr0 2>/dev/null || true

echo "== Hyper Setup complete =="