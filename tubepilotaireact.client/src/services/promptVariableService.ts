import { apiClient } from './api';

export interface PromptVariable {
  id: number;
  name: string;
  promptId: number;
  variableType: string;
  defaultValue?: string;
  isRequired?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePromptVariableDto {
  name: string;
  promptId: number;
  variableType: string;
  defaultValue?: string;
  isRequired?: boolean;
}

export interface UpdatePromptVariableDto {
  id: number;
  name: string;
  promptId: number;
  variableType: string;
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
  async getByPrompt(promptId: number): Promise<PromptVariable[]> {
    const response = await apiClient.get<PromptVariable[]>('/promptvariables', {
      params: { promptId },
    });
    return response.data;
  }

  // Get single variable
  async getById(id: number): Promise<PromptVariable> {
    const response = await apiClient.get<PromptVariable>(`/promptvariables/${id}`);
    return response.data;
  }

  // Create variable
  async create(data: CreatePromptVariableDto): Promise<PromptVariable> {
    const response = await apiClient.post<PromptVariable>('/promptvariables', data);
    return response.data;
  }

  // Update variable
  async update(id: number, data: UpdatePromptVariableDto): Promise<PromptVariable> {
    const response = await apiClient.put<PromptVariable>(`/promptvariables/${id}`, data);
    return response.data;
  }

  // Delete variable
  async delete(id: number): Promise<void> {
    await apiClient.delete(`/promptvariables/${id}`);
  }
}

export const promptVariableService = new PromptVariableService();
