Coding Standards — TubePilotAI

This document defines naming, organization and testing conventions for the codebase.

Folder Naming

- Use PascalCase for project folders and root folders under src/ (e.g., TubePilot.API, TubePilot.Application).
- Use kebab-case for static web assets or scripts if needed.

Class Naming

- Use PascalCase for classes and interfaces. Prefix interfaces with 'I' (e.g., IUserRepository).
- Keep class names concise and descriptive.

Entity Naming

- Use singular PascalCase for entity classes (e.g., PromptCategory, PromptVariable).
- Table names in DB should be pluralized (Prompts, PromptCategories).

Repository Naming

- Repository interfaces: I{Entity}Repository (e.g., IPromptRepository).
- Implementations: {Entity}Repository in Infrastructure.

Controller Naming

- Controllers should be named {Entity}Controller and live in the API project under Controllers/.
- Use route attribute [Route("api/[controller]")].

API Naming

- Use RESTful routes. Use HTTP verbs consistently (GET for reads, POST for create, PUT for update, DELETE for soft-delete).
- Use snake-case or kebab-case for query parameters (consistent across APIs).

DTO Naming

- DTOs: {Entity}Dto for output models, Create{Entity}Request and Update{Entity}Request for input models.

Migration Naming

- Use clear migration names prefixed with the date/time or feature name (e.g., AddPromptCategory, AddPromptVariable).

Testing Standards

- Use xUnit for unit tests. Name tests as ClassName_MethodUnderTest_Expectation (e.g., PromptRepository_GetById_ReturnsPrompt).
- Use InMemory provider for repository tests when possible and separate integration tests that run against real DB.

Logging Standards

- Use structured logging with semantic properties (e.g., LogInformation("User created", new { userId = user.Id })).
- Include correlation ids in HTTP request logs.

Formatting & Style

- Use the project's .editorconfig. Prefer 4-space indentation and UTF-8 without BOM.
- Keep line length reasonable (~120 chars) and prefer expressive variable names.

Code Reviews

- Require one approving review for non-trivial changes and two for major architectural changes.

End of CodingStandards.md