# Archived stale EF Core migration files

These `.bak` files were recovered from two incorrectly-nested directories that
previously lived under `TubePilotAIReact.Server/`:

- `TubePilotAIReact.Server/TubePilotAIReact.Server/Data/Migrations/`
- `TubePilotAIReact.Server/TubePilotAIReact/Server/Data/Migrations/`

They were removed during a repository cleanup because:

1. Neither directory was referenced by the solution (`TubePilotAIReact.slnx`) or
   the project file (`TubePilotAIReact.Server.csproj`), so they were never part
   of the build.
2. Their namespaces were generated against the broken nested path
   (`TubePilotAIReact.Server.TubePilotAIReact.Server.Data.Migrations`) and would
   not have compiled if included.
3. The application initializes its SQLite database with
   `Database.EnsureCreated()` (see `Program.cs`) and does not use EF Core
   migrations at runtime.

They are kept here only as reference in case the project later moves to a
migration-based workflow. **Do not** drop these directly into the canonical
`TubePilotAIReact.Server/Data/Migrations/` location without first regenerating
them with `dotnet ef migrations add` from the project root — the namespaces and
timestamps below are stale.
