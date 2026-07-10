# NuGet Upgrade Plan (High-level)

This document lists packages with advisories surfaced during `dotnet restore` and proposes next steps.

Vulnerable packages observed in build logs:
- Azure.Identity 1.7.0 (moderate/high) - advisory links in build logs
- Microsoft.Data.SqlClient 5.1.1 (high)
- Microsoft.Extensions.Caching.Memory 8.0.0 (high)
- Microsoft.IdentityModel.JsonWebTokens 6.24.0 / 7.0.3 (moderate)
- System.IdentityModel.Tokens.Jwt 6.24.0 / 7.0.3 (moderate)

Recommended plan:
1. Create a branch `chore/upgrade-nuget-security` and add a PR template referencing this plan.
2. For each package, identify the latest non-vulnerable version compatible with .NET 10 and the project's package graph.
3. Update packages incrementally (one package per PR) and run full build and tests.
4. If tests or runtime behavior changes, investigate breaking changes and adapt code.
5. After all upgrades, run integration tests and perform a smoke test of the API.

Notes:
- Major upgrades may require code changes; schedule extra time for compatibility testing.
- If any package cannot be upgraded safely, evaluate mitigations (e.g., runtime configuration, limited usage) and document.

