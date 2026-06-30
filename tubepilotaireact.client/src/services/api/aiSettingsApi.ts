import type { AIProviderSetting, ContentGenerationProviderKind } from '../../types/aiSettings';

const baseUrl = '/api/ai-provider-settings';

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
    ...init,
  });

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}.`);
  }

  return response.json() as Promise<T>;
}

export function getAIProviderSettings() {
  return request<AIProviderSetting[]>(baseUrl);
}

export function saveAIProviderSetting(provider: ContentGenerationProviderKind, apiKey: string) {
  return request<AIProviderSetting>(baseUrl, {
    method: 'POST',
    body: JSON.stringify({ provider, apiKey }),
  });
}

export function deleteAIProviderSetting(provider: ContentGenerationProviderKind) {
  return request<void>(`${baseUrl}/${provider}`, {
    method: 'DELETE',
  });
}
