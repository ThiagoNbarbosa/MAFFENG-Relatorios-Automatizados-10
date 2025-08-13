#!/bin/bash
# Clean up old processes
pkill -f "dotnet\|gunicorn\|flask" 2>/dev/null || true
sleep 1

# Start C# ASP.NET Core application
dotnet run --urls=http://0.0.0.0:5000 --no-build