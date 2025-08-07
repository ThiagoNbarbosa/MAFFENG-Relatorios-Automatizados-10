#!/bin/bash
echo "Starting C# MAFFENG Application..."
cd /home/runner/workspace
export ASPNETCORE_ENVIRONMENT=Production
dotnet run --urls=http://0.0.0.0:5001