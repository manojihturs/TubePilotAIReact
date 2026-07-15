import { apiClient } from './api';

export interface Project {
  id: string;
  title: string;
  folderPath: string;
  thumbnailPath?: string | null;
  createdAt: string;
}

export interface CreateProjectDto {
  title: string;
  promptId?: string;
}

export interface GeneratedOutput {
  outputItemName: string;
  type: string;
  folderName: string;
  filePath: string;
  rowCount: number;
}

export interface GenerateResult {
  projectId: string;
  outputs: GeneratedOutput[];
  warnings: string[];
}

export interface DataRow {
  id: string;
  projectOutputId: string;
  outputItemName: string;
  rowIndex: number;
  data: Record<string, string>;
  imageStatus: 'NotRequired' | 'Pending' | 'CandidatesFetched' | 'Confirmed';
  confirmedImagePath?: string | null;
  isVideoClip: boolean;
}

export interface ImageCandidate {
  thumbnailUrl: string;
  fullSizeUrl: string;
  sourceUrl: string;
  license: string;
}

export interface ImageCandidatesResult {
  rowId: string;
  query: string;
  candidates: ImageCandidate[];
}

export interface VideoClipCandidate {
  previewImageUrl: string;
  downloadUrl: string;
  sourceUrl: string;
  license: string;
  durationSeconds: number;
}

export interface VideoClipCandidatesResult {
  rowId: string;
  query: string;
  candidates: VideoClipCandidate[];
}

export interface TextOutput {
  id: string;
  outputItemName: string;
  content: string;
}

export interface RenderJob {
  id: string;
  projectId: string;
  format: 'Desktop' | 'Shorts';
  durationSeconds: number;
  language: string;
  status: 'Queued' | 'Rendering' | 'Complete' | 'Failed';
  outputPath?: string | null;
  errorMessage?: string | null;
  createdAt: string;
  completedAt?: string | null;
}

class ProjectService {
  async getAll(): Promise<Project[]> {
    const response = await apiClient.get<Project[]>('/projects');
    return response.data;
  }

  async getById(id: string): Promise<Project> {
    const response = await apiClient.get<Project>(`/projects/${id}`);
    return response.data;
  }

  async create(data: CreateProjectDto): Promise<Project> {
    const response = await apiClient.post<Project>('/projects', data);
    return response.data;
  }

  async generateThumbnail(id: string, prompt?: string): Promise<Project> {
    const response = await apiClient.post<Project>(`/projects/${id}/thumbnail`, { prompt });
    return response.data;
  }

  async generate(projectId: string, preferredAiToolId?: string): Promise<GenerateResult> {
    const response = await apiClient.post<GenerateResult>('/generate', { projectId, preferredAiToolId });
    return response.data;
  }

  async getRows(projectId: string): Promise<DataRow[]> {
    const response = await apiClient.get<DataRow[]>(`/projects/${projectId}/rows`);
    return response.data;
  }

  async getTextOutputs(projectId: string): Promise<TextOutput[]> {
    const response = await apiClient.get<TextOutput[]>(`/projects/${projectId}/text-outputs`);
    return response.data;
  }

  async isReadyForVideo(projectId: string): Promise<boolean> {
    const response = await apiClient.get<boolean>(`/projects/${projectId}/ready-for-video`);
    return response.data;
  }

  async getImageCandidates(projectId: string, rowId: string, query?: string): Promise<ImageCandidatesResult> {
    const response = await apiClient.post<ImageCandidatesResult>(
      `/projects/${projectId}/rows/${rowId}/image-candidates`,
      { query }
    );
    return response.data;
  }

  async selectImage(projectId: string, rowId: string, fullSizeUrl: string): Promise<DataRow> {
    const response = await apiClient.post<DataRow>(
      `/projects/${projectId}/rows/${rowId}/image-select`,
      { fullSizeUrl }
    );
    return response.data;
  }

  async generateRowImage(projectId: string, rowId: string, prompt?: string): Promise<DataRow> {
    const response = await apiClient.post<DataRow>(
      `/projects/${projectId}/rows/${rowId}/image-generate`,
      { prompt }
    );
    return response.data;
  }

  // Searches a free source (Wikipedia lead image → Wikimedia Commons) and auto-confirms the
  // top real photo. Returns the row unchanged (still not Confirmed) when nothing is found.
  async autoFetchRowImage(projectId: string, rowId: string): Promise<DataRow> {
    const response = await apiClient.post<DataRow>(
      `/projects/${projectId}/rows/${rowId}/image-auto`,
      {}
    );
    return response.data;
  }

  async getClipCandidates(projectId: string, rowId: string, query?: string): Promise<VideoClipCandidatesResult> {
    const response = await apiClient.post<VideoClipCandidatesResult>(
      `/projects/${projectId}/rows/${rowId}/clip-candidates`,
      { query }
    );
    return response.data;
  }

  async selectClip(projectId: string, rowId: string, downloadUrl: string): Promise<DataRow> {
    const response = await apiClient.post<DataRow>(
      `/projects/${projectId}/rows/${rowId}/clip-select`,
      { downloadUrl }
    );
    return response.data;
  }

  // Searches every connected footage provider (Pexels, Pixabay) and auto-confirms the top
  // real motion-video clip. Returns the row unchanged (still not Confirmed) when nothing is
  // found or no footage provider key is connected.
  async autoFetchRowClip(projectId: string, rowId: string): Promise<DataRow> {
    const response = await apiClient.post<DataRow>(
      `/projects/${projectId}/rows/${rowId}/clip-auto`,
      {}
    );
    return response.data;
  }

  // Local project files (thumbnail, confirmed row images) require the auth header, so a
  // plain <img src> won't work — fetch as a blob and hand back an object URL instead.
  async getFileObjectUrl(projectId: string, relativePath: string): Promise<string> {
    const encodedPath = relativePath.split(/[\\/]/).map(encodeURIComponent).join('/');
    const response = await apiClient.get(`/projects/${projectId}/files/${encodedPath}`, {
      responseType: 'blob',
    });
    return URL.createObjectURL(response.data as Blob);
  }

  async checkRenderAvailability(projectId: string): Promise<boolean> {
    const response = await apiClient.get<boolean>(`/projects/${projectId}/render/availability`);
    return response.data;
  }

  async startRender(projectId: string, format: 'Desktop' | 'Shorts', durationSeconds: number, language: string): Promise<RenderJob> {
    const response = await apiClient.post<RenderJob>(`/projects/${projectId}/render`, { format, durationSeconds, language });
    return response.data;
  }

  async getRenderJobs(projectId: string): Promise<RenderJob[]> {
    const response = await apiClient.get<RenderJob[]>(`/projects/${projectId}/render`);
    return response.data;
  }
}

export const projectService = new ProjectService();

// Backend stores absolute filesystem paths (folderPath, thumbnailPath, confirmedImagePath).
// The files endpoint takes a path relative to the project's own folder — strip the prefix.
export function toRelativePath(fullPath: string, projectFolderPath: string): string {
  const normalizedFull = fullPath.replace(/\\/g, '/');
  const normalizedRoot = projectFolderPath.replace(/\\/g, '/');
  return normalizedFull.startsWith(normalizedRoot)
    ? normalizedFull.slice(normalizedRoot.length).replace(/^\//, '')
    : normalizedFull;
}
