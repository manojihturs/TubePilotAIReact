import type {
  CreatePromptTemplatePayload,
  PromptTemplate,
  PromptTemplateFilters,
  PromptTemplatePayload,
} from '../../types/promptTemplate';

const baseUrl = '/api/prompt-templates';

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
    ...init,
  });

  if (!response.ok) {
    const error = await readError(response);
    throw new Error(error);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

async function readError(response: Response): Promise<string> {
  const fallback = `Request failed with status ${response.status}.`;

  try {
    const body = await response.json();
    return body.detail ?? body.title ?? body.message ?? fallback;
  } catch {
    return fallback;
  }
}

export async function getPromptTemplates(filters: PromptTemplateFilters = {}) {
  const params = new URLSearchParams();

  if (filters.search) params.set('search', filters.search);
  if (filters.category) params.set('category', filters.category);
  if (filters.status) params.set('status', filters.status);

  const query = params.toString();
  return request<PromptTemplate[]>(query ? `${baseUrl}?${query}` : baseUrl);
}

export async function createPromptTemplate(payload: CreatePromptTemplatePayload) {
  return request<PromptTemplate>(baseUrl, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export async function updatePromptTemplate(id: string, payload: PromptTemplatePayload) {
  return request<PromptTemplate>(`${baseUrl}/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  });
}

export async function deletePromptTemplate(id: string) {
  return request<void>(`${baseUrl}/${id}`, {
    method: 'DELETE',
  });
}

export async function setDefaultPromptTemplate(id: string) {
  return request<PromptTemplate>(`${baseUrl}/${id}/default`, {
    method: 'POST',
  });
}
