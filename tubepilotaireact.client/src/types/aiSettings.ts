export type ContentGenerationProviderKind = 'OpenAI' | 'Gemini' | 'Claude' | 'DeepSeek' | 'Ollama';

export interface AIProviderSetting {
  provider: ContentGenerationProviderKind;
  isConfigured: boolean;
  maskedApiKey?: string | null;
  updatedAtUtc?: string | null;
}
