Prompt Library — Overview

This document describes the prompt categories and high-level organization for the prompt library. No actual prompt content is included in this document — it is a catalog and specification.

Prompt Categories

- Research: Prompts used for data gathering, summarization and source extraction.
- Movie: Prompts for filmography, movie summaries, cast lists and timelines.
- Ranking: Prompts that generate ranked lists, leaderboards and comparative content.
- Timeline: Prompts to generate chronological narratives and event summaries.
- SEO: Prompts for title, description, keywords and meta content generation.
- Thumbnail: Prompts that define thumbnail composition and alternative text.
- Image: Prompts for image generation (scenes, backgrounds, assets).
- Voice: Prompts and SSML templates for voice-over generation.
- Translation: Prompts for translation and localization.
- Validation: Prompts to validate or fact-check generated content.
- Rendering: Prompts that guide scene composition and rendering parameters.
- Export: Prompts that define packaging, metadata and export formats.

Prompt Metadata

Each prompt entry will include metadata (when created):
- Id
- CategoryId
- Name
- Description
- Variables (placeholders and types)
- DefaultValues
- Model/Provider recommendations
- Version and change history
- Tags
- Access/visibility (public, private, system)

Guidelines

- Keep prompts modular and reusable. Prefer small composable prompts over monolithic templates.
- Use variables for user-specific content (e.g., {{MOVIE_TITLE}}, {{YEAR}}).
- Provide examples and expected outputs for complex prompts.

No actual prompt text is included in this document.

End of PromptLibrary.md