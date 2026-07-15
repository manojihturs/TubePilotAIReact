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

// Real stock-footage video search — free API key (no cost), used for real motion video
// scenes instead of AI-animated photos. Uses the simple UserApiKeys save/list/delete
// endpoints (one key per named provider) — AI text-generation tools use the richer,
// fully user-defined AiTool endpoints instead (see aiToolService.ts).
export const FOOTAGE_PROVIDERS = ['Pexels', 'Pixabay'] as const;

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
}

export const aiProviderService = new AiProviderService();
