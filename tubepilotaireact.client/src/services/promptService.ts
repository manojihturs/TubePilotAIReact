import { apiClient } from './api';
import type { PromptCategory } from './promptCategoryService';

export interface Prompt {
  id: string;
  name: string;
  promptText: string;
  outputSpecJson?: string | null;
  categoryId: string;
  category?: PromptCategory;
  createdDate?: string;
  updatedDate?: string;
}

export interface CreatePromptDto {
  name: string;
  promptText: string;
  categoryId: string;
}

export interface UpdatePromptDto {
  id: string;
  name: string;
  promptText: string;
  categoryId: string;
}

class PromptService {
  // Get all prompts
  async getAll(): Promise<Prompt[]> {
    const response = await apiClient.get<Prompt[]>('/prompts');
    return response.data;
  }

  // Get prompts by category
  async getByCategory(categoryId: string): Promise<Prompt[]> {
    const response = await apiClient.get<Prompt[]>('/prompts', {
      params: { categoryId },
    });
    return response.data;
  }

  // Get single prompt
  async getById(id: string): Promise<Prompt> {
    const response = await apiClient.get<Prompt>(`/prompts/${id}`);
    return response.data;
  }

  // Create prompt
  async create(data: CreatePromptDto): Promise<Prompt> {
    const response = await apiClient.post<Prompt>('/prompts', data);
    return response.data;
  }

  // Update prompt
  async update(id: string, data: UpdatePromptDto): Promise<Prompt> {
    const response = await apiClient.put<Prompt>(`/prompts/${id}`, data);
    return response.data;
  }

  // Delete prompt
  async delete(id: string): Promise<void> {
    await apiClient.delete(`/prompts/${id}`);
  }

  // Duplicate prompt
  async duplicate(id: string): Promise<Prompt> {
    const response = await apiClient.post<Prompt>(`/prompts/${id}/duplicate`);
    return response.data;
  }
}

export const promptService = new PromptService();
