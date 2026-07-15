import { apiClient } from './api';

// A user-defined AI text-generation tool: any name, pointing at any host that speaks one
// of the supported wire formats. Nothing here is a fixed vendor list — the user creates as
// many of these as they want, and generation walks them in Priority order, automatically
// falling through to the next enabled tool if one fails (rate limit, bad key, outage).
export type AiApiFormat = 'anthropic-messages' | 'openai-chat' | 'gemini';

export interface AiTool {
  id: string;
  name: string;
  apiFormat: AiApiFormat;
  baseUrl: string;
  model: string;
  priority: number;
  isEnabled: boolean;
  lastUsedAt?: string | null;
}

export interface CreateAiToolDto {
  name: string;
  apiFormat: AiApiFormat;
  baseUrl: string;
  model: string;
  apiKey: string;
  priority: number;
}

export interface UpdateAiToolDto {
  name: string;
  baseUrl: string;
  model: string;
  apiKey?: string;
  priority: number;
  isEnabled: boolean;
}

class AiToolService {
  async getAll(): Promise<AiTool[]> {
    const response = await apiClient.get<AiTool[]>('/ai-tools');
    return response.data;
  }

  async getFormats(): Promise<AiApiFormat[]> {
    const response = await apiClient.get<AiApiFormat[]>('/ai-tools/formats');
    return response.data;
  }

  async create(data: CreateAiToolDto): Promise<AiTool> {
    const response = await apiClient.post<AiTool>('/ai-tools', data);
    return response.data;
  }

  async update(id: string, data: UpdateAiToolDto): Promise<AiTool> {
    const response = await apiClient.put<AiTool>(`/ai-tools/${id}`, data);
    return response.data;
  }

  async remove(id: string): Promise<void> {
    await apiClient.delete(`/ai-tools/${id}`);
  }
}

export const aiToolService = new AiToolService();
