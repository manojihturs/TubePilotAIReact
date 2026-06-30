export type ProjectStatus = 'Draft' | 'Active' | 'OnHold' | 'Completed' | 'Archived' | 0 | 1 | 2 | 3 | 4;

export type ProjectPriority = 'Low' | 'Medium' | 'High' | 'Critical' | 0 | 1 | 2 | 3;

export interface Project {
  id: string;
  name: string;
  description?: string | null;
  ownerName: string;
  status: ProjectStatus;
  priority: ProjectPriority;
  startDate?: string | null;
  dueDate?: string | null;
  budget: number;
  tags: string[];
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface ProjectPayload {
  name: string;
  description?: string | null;
  ownerName: string;
  status: ProjectStatus;
  priority: ProjectPriority;
  startDate?: string | null;
  dueDate?: string | null;
  budget: number;
  tags: string[];
}

export interface ProjectFilters {
  search?: string;
  status?: string;
  priority?: string;
}

export interface ProjectQuery extends ProjectFilters {}
