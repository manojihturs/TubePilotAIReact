import { apiClient } from './api';

export interface PromptVariable {
  id: string;
  name: string;
  promptId: string;
  placeholder: string;
  dataType?: string;
  description?: string;
  defaultValue?: string;
  isRequired?: boolean;
  createdDate?: string;
  updatedDate?: string;
}

export interface CreatePromptVariableDto {
  name: string;
  promptId: string;
  placeholder: string;
  dataType?: string;
  defaultValue?: string;
  isRequired?: boolean;
}

export interface UpdatePromptVariableDto {
  id: string;
  name: string;
  promptId: string;
  placeholder: string;
  dataType?: string;
  defaultValue?: string;
  isRequired?: boolean;
}

class PromptVariableService {
  // Get all variables
  async getAll(): Promise<PromptVariable[]> {
    const response = await apiClient.get<PromptVariable[]>('/promptvariables');
    return response.data;
  }

  // Get variables by prompt
  async getByPrompt(promptId: string): Promise<PromptVariable[]> {
    const response = await apiClient.get<PromptVariable[]>('/promptvariables', {
      params: { promptId },
    });
    return response.data;
  }

  // Get single variable
  async getById(id: string): Promise<PromptVariable> {
    const response = await apiClient.get<PromptVariable>(`/promptvariables/${id}`);
    return response.data;
  }

  // Create variable
  async create(data: CreatePromptVariableDto): Promise<PromptVariable> {
    const response = await apiClient.post<PromptVariable>('/promptvariables', data);
    return response.data;
  }

  // Update variable
  async update(id: string, data: UpdatePromptVariableDto): Promise<PromptVariable> {
    const response = await apiClient.put<PromptVariable>(`/promptvariables/${id}`, data);
    return response.data;
  }

  // Delete variable
  async delete(id: string): Promise<void> {
    await apiClient.delete(`/promptvariables/${id}`);
  }
}

export const promptVariableService = new PromptVariableService();
