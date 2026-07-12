import { apiClient } from './api';

export interface UserApiKeyStatus {
  providerName: string;
  saved: boolean;
  lastUsedAt?: string | null;
}

export interface SaveApiKeyDto {
  providerName: string;
  apiKey: string;
}

export interface GenerateRequestDto {
  title?: string;
  prompt: string;
  providerName: string;
  model?: string;
}

export interface GenerateResultDto {
  content: string;
  inputTokens: number;
  outputTokens: number;
  providerName: string;
  model: string;
}

export const AI_PROVIDERS = ['Claude', 'Groq', 'Gemini'] as const;

class AiProviderService {
  async getKeys(): Promise<UserApiKeyStatus[]> {
    const response = await apiClient.get<UserApiKeyStatus[]>('/user/api-keys');
    return response.data;
  }

  async saveKey(data: SaveApiKeyDto): Promise<UserApiKeyStatus> {
    const response = await apiClient.post<UserApiKeyStatus>('/user/api-keys', data);
    return response.data;
  }

  async deleteKey(providerName: string): Promise<void> {
    await apiClient.delete(`/user/api-keys/${providerName}`);
  }

  async generate(data: GenerateRequestDto): Promise<GenerateResultDto> {
    const response = await apiClient.post<GenerateResultDto>('/ai/test-generate', data);
    return response.data;
  }
}

export const aiProviderService = new AiProviderService();
