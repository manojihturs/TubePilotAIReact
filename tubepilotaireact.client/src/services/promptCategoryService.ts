import { apiClient } from './api';

export interface PromptCategory {
  id: number;
  name: string;
  description?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePromptCategoryDto {
  name: string;
  description?: string;
}

export interface UpdatePromptCategoryDto {
  id: number;
  name: string;
  description?: string;
}

class PromptCategoryService {
  // Get all categories
  async getAll(): Promise<PromptCategory[]> {
    const response = await apiClient.get<PromptCategory[]>('/promptcategories');
    return response.data;
  }

  // Get single category
  async getById(id: number): Promise<PromptCategory> {
    const response = await apiClient.get<PromptCategory>(`/promptcategories/${id}`);
    return response.data;
  }

  // Create category
  async create(data: CreatePromptCategoryDto): Promise<PromptCategory> {
    const response = await apiClient.post<PromptCategory>('/promptcategories', data);
    return response.data;
  }

  // Update category
  async update(id: number, data: UpdatePromptCategoryDto): Promise<PromptCategory> {
    const response = await apiClient.put<PromptCategory>(`/promptcategories/${id}`, data);
    return response.data;
  }

  // Delete category
  async delete(id: number): Promise<void> {
    await apiClient.delete(`/promptcategories/${id}`);
  }
}

export const promptCategoryService = new PromptCategoryService();
