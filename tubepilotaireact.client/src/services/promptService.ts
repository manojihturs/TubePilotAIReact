import { apiClient } from './api';
import type { PromptCategory } from './promptCategoryService';

export interface Prompt {
  id: number;
  title: string;
  content: string;
  categoryId: number;
  category?: PromptCategory;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePromptDto {
  title: string;
  content: string;
  categoryId: number;
}

export interface UpdatePromptDto {
  id: number;
  title: string;
  content: string;
  categoryId: number;
}

class PromptService {
  // Get all prompts
  async getAll(): Promise<Prompt[]> {
    const response = await apiClient.get<Prompt[]>('/prompts');
    return response.data;
  }

  // Get prompts by category
  async getByCategory(categoryId: number): Promise<Prompt[]> {
    const response = await apiClient.get<Prompt[]>('/prompts', {
      params: { categoryId },
    });
    return response.data;
  }

  // Get single prompt
  async getById(id: number): Promise<Prompt> {
    const response = await apiClient.get<Prompt>(`/prompts/${id}`);
    return response.data;
  }

  // Create prompt
  async create(data: CreatePromptDto): Promise<Prompt> {
    const response = await apiClient.post<Prompt>('/prompts', data);
    return response.data;
  }

  // Update prompt
  async update(id: number, data: UpdatePromptDto): Promise<Prompt> {
    const response = await apiClient.put<Prompt>(`/prompts/${id}`, data);
    return response.data;
  }

  // Delete prompt
  async delete(id: number): Promise<void> {
    await apiClient.delete(`/prompts/${id}`);
  }

  // Duplicate prompt
  async duplicate(id: number): Promise<Prompt> {
    const response = await apiClient.post<Prompt>(`/prompts/${id}/duplicate`);
    return response.data;
  }
}

export const promptService = new PromptService();
