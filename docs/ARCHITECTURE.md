# TubePilotAI Architecture

## 1. Solution Structure

```text
TubePilotAIReact/
  TubePilotAIReact.slnx
  TubePilotAI.Domain/
  TubePilotAI.Application/
  TubePilotAI.Infrastructure/
  TubePilotAIReact.Server/
  tubepilotaireact.client/
  database/
  docs/
  tests/
```

The solution follows Clean Architecture. The ASP.NET Core Web API is the composition root, React is the presentation client, SQL Server is accessed only through the infrastructure layer, and domain rules remain independent from frameworks.

## 2. Projects

| Project | Technology | Responsibility |
| --- | --- | --- |
| TubePilotAI.Domain | .NET 10 class library | Entities, value objects, domain events, enums, domain rules |
| TubePilotAI.Application | .NET 10 class library | Use cases, commands, queries, interfaces, DTO contracts, validation boundaries |
| TubePilotAI.Infrastructure | .NET 10 class library | EF Core persistence, SQL Server access, external API clients, file/blob providers, email providers, AI provider adapters |
| TubePilotAIReact.Server | ASP.NET Core 10 Web API | Controllers/endpoints, authentication, authorization, API configuration, dependency injection |
| tubepilotaireact.client | React 19 + TypeScript | Browser UI, routes, components, API client, state management |
| tests | .NET and frontend test projects | Unit, integration, API, and UI tests |
| database | SQL Server assets | Database design notes, migration scripts, seed data, deployment scripts |

## 3. Folder Structure

### Backend

```text
TubePilotAI.Domain/
  Entities/
  ValueObjects/
  Enums/
  Events/
  Exceptions/
  Specifications/
  Common/

TubePilotAI.Application/
  Abstractions/
    Authentication/
    Persistence/
    ExternalServices/
  Common/
    Behaviors/
    Models/
    Results/
  Features/
    Auth/
    Channels/
    Videos/
    KeywordResearch/
    ContentPlanning/
    ScriptGeneration/
    Optimization/
    Analytics/
    Billing/
  DTOs/
  Mappings/
  Validation/

TubePilotAI.Infrastructure/
  Persistence/
    Configurations/
    Context/
    Migrations/
    Repositories/
    SeedData/
  Identity/
  ExternalServices/
    YouTube/
    OpenAI/
    Email/
    Storage/
  BackgroundJobs/
  Caching/
  Logging/

TubePilotAIReact.Server/
  Controllers/
  Endpoints/
  Middleware/
  Filters/
  Auth/
  Configuration/
  DependencyInjection/
  HealthChecks/
```

### Frontend

```text
tubepilotaireact.client/src/
  app/
  assets/
  components/
    common/
    layout/
    forms/
    charts/
  features/
    auth/
    dashboard/
    channels/
    videos/
    keywordResearch/
    contentPlanning/
    scriptGeneration/
    optimization/
    analytics/
    billing/
  hooks/
  lib/
  pages/
  routes/
  services/
    api/
  state/
  styles/
  types/
  utils/
```

### Database

```text
database/
  design/
  migrations/
  seed/
  scripts/
```

### Tests

```text
tests/
  TubePilotAI.Domain.Tests/
  TubePilotAI.Application.Tests/
  TubePilotAI.Infrastructure.Tests/
  TubePilotAI.Api.Tests/
  TubePilotAI.Client.Tests/
```

## 4. Domain Models

| Model | Purpose | Key Relationships |
| --- | --- | --- |
| User | Application account owner | Owns workspaces, subscriptions, channels |
| Workspace | Logical tenant for teams/projects | Has users, channels, content plans |
| WorkspaceMember | User membership and role in a workspace | Links users to workspaces |
| SubscriptionPlan | Product plan definition | Referenced by subscriptions |
| Subscription | Workspace billing state | Belongs to workspace and plan |
| YouTubeChannel | Connected YouTube channel | Belongs to workspace and user |
| ChannelCredential | OAuth tokens and provider metadata | Belongs to YouTube channel |
| Video | Imported or planned YouTube video | Belongs to channel and workspace |
| VideoMetadata | Title, description, tags, thumbnail, language | Belongs to video |
| Keyword | Search phrase or topic term | Used by research and optimization |
| KeywordResearchRun | AI/YouTube keyword research session | Has keyword suggestions and metrics |
| KeywordSuggestion | Candidate keyword with score and metrics | Belongs to research run |
| ContentPlan | Planning board or campaign | Has content ideas and scheduled videos |
| ContentIdea | Topic idea generated or curated by user | Can become a script or video |
| Script | AI-generated or manually edited script | Belongs to content idea/video |
| ScriptSection | Hook, intro, body, CTA, outro sections | Belongs to script |
| OptimizationAudit | SEO/content audit for a video or idea | Has recommendations |
| OptimizationRecommendation | Specific improvement suggestion | Belongs to audit |
| AnalyticsSnapshot | Periodic channel/video metrics | Belongs to channel or video |
| AiGenerationRequest | Tracks prompt, model, cost, status | Linked to scripts, ideas, audits, research |
| AiGenerationResult | Stores AI output metadata and final content | Belongs to generation request |
| Notification | User-facing event or alert | Belongs to user/workspace |
| AuditLog | Security and business audit trail | Linked to user/workspace |

## 5. Database Design

### Core Tables

| Table | Primary Key | Important Columns |
| --- | --- | --- |
| Users | UserId | Email, DisplayName, PasswordHash, AuthProvider, Status, CreatedAtUtc |
| Workspaces | WorkspaceId | Name, OwnerUserId, Slug, CreatedAtUtc |
| WorkspaceMembers | WorkspaceMemberId | WorkspaceId, UserId, Role, JoinedAtUtc |
| SubscriptionPlans | SubscriptionPlanId | Name, Price, BillingCycle, LimitsJson, IsActive |
| Subscriptions | SubscriptionId | WorkspaceId, SubscriptionPlanId, Status, CurrentPeriodStartUtc, CurrentPeriodEndUtc |

### YouTube Tables

| Table | Primary Key | Important Columns |
| --- | --- | --- |
| YouTubeChannels | YouTubeChannelId | WorkspaceId, ProviderChannelId, Title, Handle, ThumbnailUrl, ConnectedAtUtc |
| ChannelCredentials | ChannelCredentialId | YouTubeChannelId, Provider, AccessTokenEncrypted, RefreshTokenEncrypted, ExpiresAtUtc |
| Videos | VideoId | WorkspaceId, YouTubeChannelId, ProviderVideoId, Status, PublishedAtUtc |
| VideoMetadata | VideoMetadataId | VideoId, Title, Description, TagsJson, ThumbnailUrl, LanguageCode |
| AnalyticsSnapshots | AnalyticsSnapshotId | WorkspaceId, YouTubeChannelId, VideoId, Views, Likes, Comments, WatchTimeMinutes, CapturedAtUtc |

### Planning And AI Tables

| Table | Primary Key | Important Columns |
| --- | --- | --- |
| Keywords | KeywordId | WorkspaceId, Phrase, LanguageCode, RegionCode |
| KeywordResearchRuns | KeywordResearchRunId | WorkspaceId, SeedKeyword, Status, StartedAtUtc, CompletedAtUtc |
| KeywordSuggestions | KeywordSuggestionId | KeywordResearchRunId, KeywordId, SearchVolume, CompetitionScore, OpportunityScore |
| ContentPlans | ContentPlanId | WorkspaceId, Name, Goal, StartDate, EndDate |
| ContentIdeas | ContentIdeaId | ContentPlanId, Title, Angle, TargetKeywordId, Status, ScheduledForUtc |
| Scripts | ScriptId | WorkspaceId, ContentIdeaId, VideoId, Title, Status, Version |
| ScriptSections | ScriptSectionId | ScriptId, SectionType, SortOrder, Content |
| OptimizationAudits | OptimizationAuditId | WorkspaceId, VideoId, ContentIdeaId, Score, CreatedAtUtc |
| OptimizationRecommendations | OptimizationRecommendationId | OptimizationAuditId, Category, Priority, RecommendationText, Status |
| AiGenerationRequests | AiGenerationRequestId | WorkspaceId, RequestType, Provider, Model, PromptHash, Status, CostUnits, CreatedAtUtc |
| AiGenerationResults | AiGenerationResultId | AiGenerationRequestId, OutputType, ContentRef, TokenUsageJson, CreatedAtUtc |

### Operational Tables

| Table | Primary Key | Important Columns |
| --- | --- | --- |
| Notifications | NotificationId | UserId, WorkspaceId, Type, Title, Message, ReadAtUtc |
| AuditLogs | AuditLogId | WorkspaceId, UserId, Action, EntityName, EntityId, CreatedAtUtc |
| OutboxMessages | OutboxMessageId | Type, PayloadJson, Status, CreatedAtUtc, ProcessedAtUtc |

### Database Conventions

- Use `uniqueidentifier` primary keys for aggregate roots.
- Store all timestamps as UTC with `datetime2`.
- Use soft delete fields where user-owned data may need recovery: `IsDeleted`, `DeletedAtUtc`.
- Encrypt OAuth tokens and other provider secrets at rest.
- Use JSON columns for flexible provider metadata only when it is not frequently queried.
- Add indexes on foreign keys, provider IDs, workspace IDs, status fields, and captured/published dates.

## 6. Clean Architecture Layout

### Dependency Direction

```text
React Client
  -> ASP.NET Core Web API
      -> Application
          -> Domain
      -> Infrastructure
          -> Application
          -> Domain
```

Domain has no dependencies on other projects. Application depends only on Domain and defines abstractions for persistence and external services. Infrastructure implements those abstractions. API wires everything together through dependency injection.

### Layer Responsibilities

| Layer | Allowed | Not Allowed |
| --- | --- | --- |
| Domain | Entities, value objects, business rules, domain events | EF Core, HTTP, logging, appsettings, DTOs |
| Application | Use cases, command/query contracts, validation, interfaces | SQL Server details, controller attributes, React concerns |
| Infrastructure | EF Core, SQL Server, repositories, external APIs, background jobs | Business decisions that belong in Domain |
| API | Routing, auth, request/response handling, DI setup | Persistence logic, AI prompt orchestration logic |
| React Client | UI, client routes, API calls, browser state | Server business rules, SQL access |

### Aggregate Boundaries

| Aggregate Root | Includes |
| --- | --- |
| Workspace | Workspace members, subscription relationship |
| YouTubeChannel | Channel credentials, channel-level metadata |
| Video | Video metadata and video-level analytics references |
| KeywordResearchRun | Keyword suggestions |
| ContentPlan | Content ideas |
| Script | Script sections |
| OptimizationAudit | Optimization recommendations |
| AiGenerationRequest | AI generation result |

### Initial API Areas

| Area | Route Prefix |
| --- | --- |
| Authentication | `/api/auth` |
| Workspaces | `/api/workspaces` |
| Channels | `/api/channels` |
| Videos | `/api/videos` |
| Keyword Research | `/api/keyword-research` |
| Content Plans | `/api/content-plans` |
| Scripts | `/api/scripts` |
| Optimization | `/api/optimization` |
| Analytics | `/api/analytics` |
| Billing | `/api/billing` |

### Frontend Feature Areas

| Feature | Responsibility |
| --- | --- |
| auth | Sign in, registration, auth callback, session state |
| dashboard | Workspace overview, recent activity, high-level metrics |
| channels | Connect/manage YouTube channels |
| videos | Imported videos, metadata editor, publishing state |
| keywordResearch | Seed keyword research and opportunity scoring |
| contentPlanning | Calendar, ideas, campaign planning |
| scriptGeneration | Script generation, editing, versioning |
| optimization | SEO/content audits and recommendations |
| analytics | Channel and video performance views |
| billing | Subscription and usage views |
