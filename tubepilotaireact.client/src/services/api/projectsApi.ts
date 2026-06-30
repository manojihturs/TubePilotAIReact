import type { Project, ProjectFilters, ProjectPayload } from '../../types/project';

const baseUrl = '/api/projects';

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
  const contentType = response.headers.get('content-type') ?? '';

  if (contentType.includes('text/html')) {
    const body = await response.text();
    return `${fallback} Received HTML instead of JSON. ${body.slice(0, 120)}`;
  }

  try {
    const body = await response.json();
    return body.detail ?? body.title ?? body.message ?? fallback;
  } catch {
    return fallback;
  }
}

export async function getProjects(filters: ProjectFilters = {}) {
  const params = new URLSearchParams();

  if (filters.search) params.set('search', filters.search);
  if (filters.status) params.set('status', filters.status);
  if (filters.priority) params.set('priority', filters.priority);

  const query = params.toString();
  return request<Project[]>(query ? `${baseUrl}?${query}` : baseUrl);
}

export async function getProject(id: string) {
  return request<Project>(`${baseUrl}/${id}`);
}

export async function createProject(payload: ProjectPayload) {
  return request<Project>(baseUrl, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export async function updateProject(id: string, payload: ProjectPayload) {
  return request<Project>(`${baseUrl}/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  });
}

export async function deleteProject(id: string) {
  return request<void>(`${baseUrl}/${id}`, {
    method: 'DELETE',
  });
}
