export type PromptTemplateStatus = 'Draft' | 'Active' | 'Archived' | 0 | 1 | 2;

export interface PromptTemplate {
  id: string;
  name: string;
  category: string;
  description?: string | null;
  templateText: string;
  systemMessage?: string | null;
  variables: string[];
  status: PromptTemplateStatus;
  isDefault: boolean;
  createdBy: string;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface PromptTemplatePayload {
  name: string;
  category: string;
  description?: string | null;
  templateText: string;
  systemMessage?: string | null;
  variables: string[];
  status: PromptTemplateStatus;
  isDefault: boolean;
}

export interface CreatePromptTemplatePayload extends PromptTemplatePayload {
  createdBy: string;
}

export interface PromptTemplateFilters {
  search?: string;
  category?: string;
  status?: string;
}
