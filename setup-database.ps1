#!/usr/bin/env pwsh

<#
.SYNOPSIS
Sets up SQL Server Docker container and applies EF Core migrations for TubePilotAI.

.DESCRIPTION
This script:
1. Starts a SQL Server container using docker-compose
2. Waits for SQL Server to be ready
3. Applies EF Core migrations to create the database schema

.EXAMPLE
.\setup-database.ps1
#>

Write-Host "TubePilotAI Database Setup Script" -ForegroundColor Cyan
Write-Host "==================================`n" -ForegroundColor Cyan

# Check if Docker is installed
Write-Host "Checking Docker installation..." -ForegroundColor Yellow
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
	Write-Host "❌ Docker is not installed or not in PATH." -ForegroundColor Red
	Write-Host "Please install Docker Desktop from: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
	exit 1
}
Write-Host "✅ Docker is installed`n" -ForegroundColor Green

# Check if docker-compose is available
Write-Host "Checking docker-compose..." -ForegroundColor Yellow
if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
	Write-Host "ℹ️  Using 'docker compose' (new syntax)`n" -ForegroundColor Cyan
	$composeCmd = "docker compose"
} else {
	Write-Host "✅ docker-compose is available`n" -ForegroundColor Green
	$composeCmd = "docker-compose"
}

# Navigate to infra/docker directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$dockerDir = Join-Path $scriptDir "infra" "docker"

if (-not (Test-Path $dockerDir)) {
	Write-Host "❌ Docker compose directory not found at: $dockerDir" -ForegroundColor Red
	exit 1
}

Write-Host "Starting SQL Server container..." -ForegroundColor Yellow
Write-Host "Command: $composeCmd -f $dockerDir/docker-compose.yml up -d`n" -ForegroundColor Gray

Push-Location $dockerDir
& $composeCmd up -d
Pop-Location

if ($LASTEXITCODE -ne 0) {
	Write-Host "❌ Failed to start SQL Server container" -ForegroundColor Red
	exit 1
}

Write-Host "✅ SQL Server container started`n" -ForegroundColor Green

# Wait for SQL Server to be ready
Write-Host "Waiting for SQL Server to be ready..." -ForegroundColor Yellow
$maxRetries = 30
$retryCount = 0
$isReady = $false

while ($retryCount -lt $maxRetries) {
	try {
		# Try to connect to SQL Server
		$connStr = "Server=localhost,1433;User Id=sa;Password=P@ssw0rd1234!;Encrypt=false;"
		$connection = New-Object System.Data.SqlClient.SqlConnection($connStr)
		$connection.Open()
		$connection.Close()
		$isReady = $true
		break
	}
	catch {
		$retryCount++
		if ($retryCount % 5 -eq 0) {
			Write-Host "  Attempt $retryCount/$maxRetries..." -ForegroundColor Gray
		}
		Start-Sleep -Seconds 1
	}
}

if (-not $isReady) {
	Write-Host "❌ SQL Server did not become ready in time" -ForegroundColor Red
	exit 1
}

Write-Host "✅ SQL Server is ready`n" -ForegroundColor Green

# Apply EF Core migrations
Write-Host "Applying EF Core migrations..." -ForegroundColor Yellow
$serverDir = Join-Path $scriptDir "TubePilotAIReact.Server"

Push-Location $serverDir
Write-Host "Command: dotnet ef database update`n" -ForegroundColor Gray

dotnet ef database update --verbose

if ($LASTEXITCODE -ne 0) {
	Write-Host "`n❌ Failed to apply migrations" -ForegroundColor Red
	Pop-Location
	exit 1
}

Pop-Location

Write-Host "`n✅ Database setup complete!`n" -ForegroundColor Green
Write-Host "Database Details:" -ForegroundColor Cyan
Write-Host "  Server: localhost,1433" -ForegroundColor White
Write-Host "  Username: sa" -ForegroundColor White
Write-Host "  Password: P@ssw0rd1234!" -ForegroundColor White
Write-Host "  Database: TubePilotAI" -ForegroundColor White
Write-Host "`n📝 To view logs: $composeCmd -f $dockerDir/docker-compose.yml logs -f mssql" -ForegroundColor Gray
Write-Host "📝 To stop: $composeCmd -f $dockerDir/docker-compose.yml down`n" -ForegroundColor Gray
