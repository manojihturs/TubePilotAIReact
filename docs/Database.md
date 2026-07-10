Database — Current and Future Tables

Purpose

This document lists current database tables (as implemented) and proposed future tables for planned features. This file is documentation only — no code changes.

Current Tables (implemented)

1. Users
- Columns (sample): Id (GUID), Email, Password, Role, CreatedDate, UpdatedDate, CreatedBy, UpdatedBy, SoftDelete
- Purpose: Authentication, authorization and user identity.

2. PromptCategories
- Columns: Id, Name, Description, DisplayOrder, Icon, Color, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, SoftDelete
- Purpose: Categorize prompts for browsing and filtering.

3. Prompts
- Columns: Id, CategoryId (FK), Name, Description, PromptText, Temperature, TopP, MaxTokens, Model, Provider, Version, Status, Tags, IsSystem, IsPublic, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
- Purpose: Store prompt templates and metadata used to generate content.

4. PromptVariables
- Columns: Id, PromptId (FK), Name, Placeholder, Description, DataType, IsRequired, DefaultValue, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, SoftDelete
- Purpose: Define variables used inside prompt templates (e.g. {{TOPIC}}, {{ACTOR}}).

5. PromptVersions (if present)
- Columns: Id, PromptId (FK), VersionNumber/Label, PromptText, Metadata fields, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, SoftDelete
- Purpose: Track historical versions of prompts.

Notes on current DB design

- All tables include auditing fields: CreatedDate, UpdatedDate, CreatedBy, UpdatedBy and SoftDelete flags.
- Repositories implement soft-deletion semantics; global filters are planned for later phases.

Future Tables (designed for upcoming phases)

1. PromptExecutions
- Columns: Id, PromptId, PromptVersionId, InputVariables (JSON), ProviderResponse (JSON), Status, StartedAt, CompletedAt, DurationMs, CostEstimate, CreatedBy, CreatedDate
- Purpose: Track each execution of a prompt (for auditing, debugging and analytics).

2. WorkflowTemplates
- Columns: Id, Name, Definition (JSON/YAML), Version, OwnerId, Status, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
- Purpose: Store workflow definitions used by the Workflow Engine.

3. WorkflowSteps
- Columns: Id, WorkflowTemplateId, StepIndex, Type, Config (JSON), RetryPolicy, CreatedBy, CreatedDate
- Purpose: Define ordered steps inside a workflow template.

4. AIProviders
- Columns: Id, Name, ProviderType, Metadata (JSON), RateLimitConfig, CredentialsReference, CreatedBy, CreatedDate
- Purpose: Registry of AI providers and their configuration.

5. MediaAssets
- Columns: Id, Uri, Type (image/audio/video), MimeType, Size, Metadata (JSON), CreatedBy, CreatedDate
- Purpose: Catalog generated media assets and their metadata.

6. GeneratedImages
- Columns: Id, MediaAssetId (FK), PromptExecutionId, Width, Height, Format, CreatedDate
- Purpose: Details about generated images.

7. GeneratedVideos
- Columns: Id, MediaAssetId, RenderJobId, Duration, Width, Height, Encoding, CreatedDate
- Purpose: Details about generated videos.

8. VoiceOvers
- Columns: Id, MediaAssetId, Text, Voice, Duration, Provider, CreatedDate
- Purpose: Track generated audio assets.

9. SEOResults
- Columns: Id, PromptExecutionId, Title, Description, Keywords, Score, CreatedDate
- Purpose: Store generated SEO artifacts and evaluations.

10. SocialPosts
- Columns: Id, Content, Platform, ScheduledAt, PublishedAt, Status, CreatedBy, CreatedDate
- Purpose: Store social messages derived from templates.

11. Projects
- Columns: Id, Name, OwnerId, Settings (JSON), CreatedDate, UpdatedDate
- Purpose: Logical grouping of tasks, templates and assets per project.

12. ProjectExecutions
- Columns: Id, ProjectId, WorkflowExecutionId, Status, StartedAt, CompletedAt, CreatedBy
- Purpose: Track runs scoped to Projects.

13. Subscriptions, Billing
- Columns: SubscriptionId, PlanId, UserId/OrganizationId, BillingStatus, NextInvoiceDate, CreatedDate
- Purpose: Support subscription billing for advanced features.

14. AuditLogs
- Columns: Id, Entity, EntityId, Action, Data (JSON), PerformedBy, PerformedAt
- Purpose: Centralized audit trail for compliance.

15. Notifications
- Columns: Id, Type, Recipient, Payload, SentAt, ReadAt, Status
- Purpose: Notifications system for jobs and user alerts.

16. Settings
- Columns: Key, Value (JSON), Scope, UpdatedBy, UpdatedDate
- Purpose: Global and per-project configurable settings.

Design Notes

- Use GUIDs for global identifiers to allow distributed generation and merging across systems.
- Store variable input and provider responses as JSON to support flexible schema for different providers and outputs.
- Indexing strategy: index FK columns (PromptId, CategoryId), common filter fields (Status, IsPublic, CreatedDate) and unique constraints where required (e.g., category name uniqueness).
- Migrations will be incremental and additive; existing migrations must not be modified.

End of Database.md
