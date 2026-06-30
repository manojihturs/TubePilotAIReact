export type ContentGenerationProviderKind = 'OpenAI' | 'Gemini' | 'Claude' | 'DeepSeek' | 'Ollama';

export type ContentGenerationStatus = 'Draft' | 'Running' | 'Completed' | 'Failed' | 0 | 1 | 2 | 3;

export interface ContentGenerationProviderResult {
  provider: ContentGenerationProviderKind;
  model: string;
  rawResponse: string;
}

export interface ContentResearchRow {
  keyword: string;
  searchVolume: number;
  competitionScore: number;
  opportunityScore: number;
  notes: string;
}

export interface ContentGenerationResult {
  jobId: string;
  title: string;
  promptText: string;
  providerResults: ContentGenerationProviderResult[];
  researchRows: ContentResearchRow[];
  videoTitle: string;
  description: string;
  hashtags: string;
  thumbnailText: string;
  backgroundImagePrompt: string;
  narrationScript: string;
  sceneText: string;
  voiceoverText: string;
  exportFolderPath?: string | null;
  status: ContentGenerationStatus;
  errorMessage?: string | null;
  createdAtUtc: string;
  completedAtUtc?: string | null;
}

export interface ContentGenerationJob {
  jobId: string;
  title: string;
  promptTemplateId?: string | null;
  promptText: string;
  selectedProviders: ContentGenerationProviderKind[];
  status: ContentGenerationStatus;
  exportFolderPath?: string | null;
  errorMessage?: string | null;
  createdAtUtc: string;
  completedAtUtc?: string | null;
}

export interface ContentGenerationTemplate {
  id: string;
  name: string;
  category: string;
  description?: string | null;
  templateText: string;
  systemMessage?: string | null;
  isDefault: boolean;
}

export interface GenerateContentJobPayload {
  title: string;
  promptTemplateId?: string | null;
  promptOverride?: string | null;
  selectedProviders: ContentGenerationProviderKind[];
}
