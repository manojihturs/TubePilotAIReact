import type {
  ContentGenerationJob,
  ContentGenerationResult,
  ContentGenerationTemplate,
  GenerateContentJobPayload,
} from '../../types/contentGeneration';

const baseUrl = '/api/content-generation';

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

  return response.json() as Promise<T>;
}

async function readError(response: Response): Promise<string> {
  const contentType = response.headers.get('content-type') ?? '';
  const fallback = `Request failed with status ${response.status}.`;

  if (contentType.includes('text/html')) {
    const text = await response.text();
    return `${fallback} Received HTML instead of JSON. ${text.slice(0, 120)}`;
  }

  try {
    const body = await response.json();
    return body.message ?? body.detail ?? body.title ?? fallback;
  } catch {
    return fallback;
  }
}

export async function getContentJobs() {
  return request<ContentGenerationJob[]>(`${baseUrl}/jobs`);
}

export async function getContentJob(id: string) {
  return request<ContentGenerationResult>(`${baseUrl}/jobs/${id}`);
}

export async function getContentTemplates() {
  return request<ContentGenerationTemplate[]>(`${baseUrl}/prompt-templates`);
}

export async function generateContent(payload: GenerateContentJobPayload) {
  return request<ContentGenerationResult>(`${baseUrl}/generate`, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export async function exportContentJob(id: string) {
  return request<{ path: string }>(`${baseUrl}/jobs/${id}/export`, {
    method: 'POST',
  });
}
