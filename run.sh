#!/bin/bash

# Set PATH to include .NET
export PATH="$HOME/.dotnet:$PATH"

# Restore packages and run the application
dotnet restore
dotnet run