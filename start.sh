#!/bin/bash
echo "Starting MAFFENG C# ASP.NET Core Application..."
export ASPNETCORE_ENVIRONMENT=Production
dotnet run --urls=http://0.0.0.0:5000