# TubePilotAIReact Development Server Startup Script (Single Window)
# This script starts both servers in background jobs in a single PowerShell window

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "TubePilotAIReact Development Setup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Get the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = $scriptDir

Write-Host "Starting development servers..." -ForegroundColor Green
Write-Host ""

# Start the backend server in background
Write-Host "1. Starting ASP.NET Backend Server..." -ForegroundColor Yellow
$backendPath = Join-Path $rootDir "TubePilotAIReact.Server"
$backendJob = Start-Job -Name "Backend" -WorkingDirectory $backendPath -ScriptBlock {
	& dotnet run --launch-profile http
}

# Wait for backend to initialize
Start-Sleep -Seconds 4

# Start the frontend dev server in background
Write-Host "2. Starting Vite Frontend Dev Server..." -ForegroundColor Yellow
$clientPath = Join-Path $rootDir "tubepilotaireact.client"
$frontendJob = Start-Job -Name "Frontend" -WorkingDirectory $clientPath -ScriptBlock {
	& npm run dev
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Servers Started!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Backend API:  http://localhost:5000" -ForegroundColor Cyan
Write-Host "Frontend App: https://localhost:49153" -ForegroundColor Cyan
Write-Host "OpenAPI Docs: http://localhost:5000/openapi/v1.json" -ForegroundColor Cyan
Write-Host ""
Write-Host "Job Status:" -ForegroundColor Yellow
Get-Job | Format-Table Name, State, @{Name="Id"; Expression={$_.Id}}
Write-Host ""
Write-Host "Commands:" -ForegroundColor Yellow
Write-Host "  Get-Job                  # Check job status" -ForegroundColor Gray
Write-Host "  Receive-Job -Name Backend # View backend output" -ForegroundColor Gray
Write-Host "  Receive-Job -Name Frontend # View frontend output" -ForegroundColor Gray
Write-Host "  Stop-Job -Name Backend    # Stop backend server" -ForegroundColor Gray
Write-Host "  Stop-Job -Name Frontend   # Stop frontend server" -ForegroundColor Gray
Write-Host "  Remove-Job *              # Clean up jobs" -ForegroundColor Gray
Write-Host ""
