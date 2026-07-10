Development Rules & Local Setup

This document describes recommended local development rules for secrets and seeding used by the TubePilot project.

1) JWT Secret (JwtSettings:Secret)
  - Do NOT store secrets in source control.
  - For local development use either dotnet user-secrets or environment variables.
  - Examples (PowerShell):
	- Set environment variable for current session:
	  $env:JwtSettings__Secret = 'your-dev-secret-here'
	- Use dotnet user-secrets (in project folder):
	  dotnet user-secrets init
	  dotnet user-secrets set "JwtSettings:Secret" "your-dev-secret-here"

2) Seeded admin user (optional and opt-in)
  - Seeding is opt-in and requires the environment variable TUBEPILOT_SEED_ADMIN_PASSWORD.
  - To seed an admin user when starting the API locally, set the password and the JWT secret before running:
	$env:JwtSettings__Secret = 'your-dev-secret-here'
	$env:TUBEPILOT_SEED_ADMIN_PASSWORD = 'P@ssw0rd'
	dotnet run --project src/TubePilot.API
  - The seeded user email is: admin@tubepilot.local
  - The password stored by the seeder is hashed (PBKDF2). This seeding mechanism is for local development only.

3) Secrets in CI / Production
  - Use your cloud provider's secret store (Azure Key Vault, AWS Secrets Manager) or CI-provided secret configuration.
  - Never commit secrets to the repository.

4) Recommendations
  - Rotate secrets regularly and store them in a vault.
  - Replace demo seeding with a proper migration or provisioning script for production environments.

If you need an example deployment configuration for a specific environment (Azure, Docker, etc.), ask and I will provide one.
