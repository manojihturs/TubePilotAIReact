API — TubePilotAI

This document describes current and planned REST APIs, authentication/authorization, response formats, and common patterns.

Authentication & Authorization

- Authentication: JWT bearer tokens using asymmetric or symmetric signing keys. Tokens include user id, role and tenant (future multi-tenant support).
- Authorization: Role-based access control. Use [Authorize(Roles="Admin,Editor")] attributes on controllers/actions where needed.

Common Response Format

- All endpoints return ApiResponse<T> with the following structure:
  - success: bool
  - message: string (nullable)
  - data: T (payload)
  - meta: object (optional; pagination info)

Pagination

- Query parameters: page (default 1), pageSize (default 20), sortBy (field), sortDir (asc|desc)
- Meta object includes: page, pageSize, totalItems, totalPages

Filtering, Searching & Sorting

- Filtering: use query parameters for common filters (e.g., isActive=true)
- Searching: free-text search query parameter 'search' against name/description fields
- Sorting: sortBy and sortDir parameters. Use explicit white-listing of sortable fields.

Error Handling

- Errors return 4xx/5xx with ApiResponse containing success=false and a helpful message.
- Validation errors return 400 with a details field listing field-specific messages.

Current APIs (implemented)

- /api/auth/login [POST]
  - Request: { email, password }
  - Response: ApiResponse<{ token, expiresAt, user }> 

- /api/users [GET|POST|PUT|DELETE]
  - Standard CRUD for admin users (role-based access)

- /api/promptcategories [GET]
  - Query params: page, pageSize, search, sortBy, sortDir
  - Response: ApiResponse<PagedResult<PromptCategoryDto>>

- /api/promptcategories/{id} [GET|PUT|DELETE|POST]
  - Standard CRUD

- /api/prompts [GET]
  - Query params: page,pageSize,search,categoryId,sortBy,sortDir,tags
  - Response: ApiResponse<PagedResult<PromptDto>>

- /api/prompts/{id} [GET|PUT|DELETE|POST]
  - Standard CRUD

- /api/promptvariables [GET|POST|PUT|DELETE]
  - Standard CRUD; associated with prompts

Future APIs (planned)

- /api/providers - Manage AI providers
- /api/workflows - Workflow template CRUD
- /api/executions - Trigger prompt/workflow executions and get status
- /api/media - Media assets listing and download
- /api/render - Submit render jobs and get results
- /api/seo - SEO suggestions and reports
- /api/publish - Publishing connectors and history

Best Practices

- Use DTOs for all public API models; avoid exposing domain entities.
- Keep controllers thin; delegate to Application services.
- Validate inputs and return structured validation errors.
- Use appropriate HTTP status codes (201 for created, 204 for no-content deletes when successful).

Rate Limiting & Throttling

- Implement rate limits at gateway level for external providers and per-user throttling for intensive operations.

Versioning

- Start with v1 via URL segment (/api/v1/...), plan breaking changes using new version segments.

End of API.md