import type { Project } from '../../types/project';

interface Props {
  projects: Project[];
  selectedId?: string;
  onSelect: (project: Project) => void;
  onDelete: (id: string) => void;
  isLoading: boolean;
}

export function ProjectDashboard({ projects, selectedId, onSelect, onDelete, isLoading }: Props) {
  return (
    <div className="template-list" aria-label="Projects">
      {isLoading ? (
        <p className="empty-state">Loading projects...</p>
      ) : projects.length === 0 ? (
        <p className="empty-state">No projects found.</p>
      ) : (
        projects.map((project) => (
          <article className={`template-row ${project.id === selectedId ? 'selected' : ''}`} key={project.id}>
            <button type="button" onClick={() => onSelect(project)}>
              <span>
                <strong>{project.name}</strong>
                <small>{project.ownerName}</small>
              </span>
              <span className="status-stack">
                <span className="status-pill">{statusLabel(project.status)}</span>
                <span className="priority-pill">{priorityLabel(project.priority)}</span>
              </span>
            </button>
            <button className="delete-button" type="button" onClick={() => onDelete(project.id)}>
              Delete
            </button>
          </article>
        ))
      )}
    </div>
  );
}

function statusLabel(status: Project['status']) {
  if (status === 0) return 'Draft';
  if (status === 1) return 'Active';
  if (status === 2) return 'OnHold';
  if (status === 3) return 'Completed';
  if (status === 4) return 'Archived';
  return status;
}

function priorityLabel(priority: Project['priority']) {
  if (priority === 0) return 'Low';
  if (priority === 1) return 'Medium';
  if (priority === 2) return 'High';
  if (priority === 3) return 'Critical';
  return priority;
}
