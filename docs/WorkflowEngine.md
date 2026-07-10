Workflow Engine — TubePilotAI

Overview

The Workflow Engine orchestrates multi-step content generation pipelines such as Movie Filmography, Ranking Workflows, and Timeline Workflows. Workflows are defined using templates that enumerate ordered steps. Each step is a single responsibility task (research, validate, generate, render, publish).

Core Concepts

- Workflow Template: JSON/YAML definition that lists ordered steps, input schema and retry policies.
- Step: An atomic operation with inputs and outputs. Steps are idempotent and can be retried.
- Execution: A runtime instance of a workflow template with state, logs and results.
- Executor: Component that runs steps, records state, and handles retries/backoff.
- Workers: Scalable background processors that execute steps (can be Kubernetes Jobs, Azure Functions, or dedicated worker VMs).

Example: Movie Filmography Workflow

1. Research
   - Call research prompts and connectors to gather movie metadata, credits, images
2. Validation
   - Run validation prompts to check for missing or inconsistent data
3. Movie Data
   - Normalize and enrich movie metadata (dates, genres, cast mapping)
4. Voice Over
   - Generate voice-over via TTS provider with SSML templates
5. Images
   - Generate or fetch images for movie scenes and thumbnails
6. Thumbnail
   - Compose thumbnail assets and select best thumbnail
7. SEO
   - Run SEO prompts to generate titles, descriptions and tags
8. Social Media
   - Create short social clips and captions
9. Rendering
   - Assemble scenes, captions and audio into final video
10. Export
   - Package final assets and metadata for publishing

Other example workflows

- Ranking Workflow: Data ingestion → normalize → rank calculation → generate visuals → render video
- Statistics Workflow: Aggregate stats → chart generation → voice summary → render
- Timeline Workflow: Collect events → sort and filter → generate narration → render

Failure Handling

- Each step defines retry policies and failure modes. Critical steps may require manual approval if retries fail.
- The executor records failure reasons and supports replay from a failed step.

Versioning & Templates

- Workflows are versioned. Template changes produce new template versions; existing executions reference the version used.

Observability

- The engine emits events and metrics for each step start/finish/failure.
- Execution logs are stored and associated with runs for debugging and audit.

Extensibility

- Steps are implemented via pluggable handlers registered in DI. New step types can be added without changing the engine core.

End of WorkflowEngine.md