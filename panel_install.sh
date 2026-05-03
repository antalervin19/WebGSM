#!/bin/bash

set -e

echo ":: Updating System ::"
sudo apt update && sudo apt upgrade -y

echo ":: Installing Node.js @ NPM ::"
sudo apt install -y nodejs npm

echo

