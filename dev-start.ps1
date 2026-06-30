# TubePilotAIReact Development Server Startup Script
# This script starts both the ASP.NET backend and Vite frontend dev servers

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "TubePilotAIReact Development Setup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Get the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = $scriptDir

Write-Host "Starting development servers..." -ForegroundColor Green
Write-Host ""

# Start the backend server in a new window
Write-Host "1. Starting ASP.NET Backend Server (http://localhost:5000)..." -ForegroundColor Yellow
$backendPath = Join-Path $rootDir "TubePilotAIReact.Server"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$host.UI.RawUI.WindowTitle='TubePilotAIReact Backend'; cd '$backendPath'; dotnet run --launch-profile http"

# Wait a few seconds for the backend to start
Start-Sleep -Seconds 3

# Start the frontend dev server in a new window
Write-Host "2. Starting Vite Frontend Dev Server (https://localhost:49153)..." -ForegroundColor Yellow
$clientPath = Join-Path $rootDir "tubepilotaireact.client"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$host.UI.RawUI.WindowTitle='TubePilotAIReact Frontend'; cd '$clientPath'; npm run dev"

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Servers Started!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Backend API:  http://localhost:5000" -ForegroundColor Cyan
Write-Host "Frontend App: https://localhost:49153" -ForegroundColor Cyan
Write-Host "OpenAPI Docs: http://localhost:5000/openapi/v1.json" -ForegroundColor Cyan
Write-Host ""
Write-Host "Two new PowerShell windows have been opened." -ForegroundColor Yellow
Write-Host "Press Ctrl+C in each window to stop the servers." -ForegroundColor Yellow
Write-Host ""
