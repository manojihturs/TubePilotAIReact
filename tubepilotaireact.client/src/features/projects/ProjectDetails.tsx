import type { Project } from '../../types/project';

interface Props {
  project: Project;
}

export function ProjectDetails({ project }: Props) {
  async function handleExport() {
    await fetch(`/api/projects/${project.id}/export`, { method: 'POST' });
    alert('Project exported successfully.');
  }

  return (
    <section className="project-details" aria-label="Project details">
      <div>
        <h2>{project.name}</h2>
        <button onClick={handleExport}>Export Project Files</button>
        <p>{project.description || 'No description provided.'}</p>
      </div>
      <dl>
        <div>
          <dt>Owner</dt>
          <dd>{project.ownerName}</dd>
        </div>
        <div>
          <dt>Timeline</dt>
          <dd>{formatDateRange(project.startDate, project.dueDate)}</dd>
        </div>
        <div>
          <dt>Budget</dt>
          <dd>{formatCurrency(project.budget)}</dd>
        </div>
        <div>
          <dt>Tags</dt>
          <dd>{project.tags.length > 0 ? project.tags.join(', ') : 'None'}</dd>
        </div>
      </dl>
    </section>
  );
}

function formatDateRange(startDate?: string | null, dueDate?: string | null) {
  if (!startDate && !dueDate) {
    return 'No dates set';
  }

  return `${formatDate(startDate)} to ${formatDate(dueDate)}`;
}

function formatDate(value?: string | null) {
  if (!value) {
    return 'Open';
  }

  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(`${value}T00:00:00`));
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat(undefined, { currency: 'USD', style: 'currency' }).format(value);
}
