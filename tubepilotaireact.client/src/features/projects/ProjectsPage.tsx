import { useEffect, useMemo, useState } from 'react';
import { useProjects } from '../../hooks/useProjects';
import type { Project, ProjectPayload } from '../../types/project';
import { ProjectDashboard } from './ProjectDashboard';
import { ProjectDetails } from './ProjectDetails';
import { ProjectModal } from './ProjectModal';

export function ProjectsPage() {
  const { loading, error, fetchProjects, fetchProject, saveProject, removeProject } = useProjects();
  const [projects, setProjects] = useState<Project[]>([]);
  const [selectedProject, setSelectedProject] = useState<Project | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [message, setMessage] = useState('');
  const [search, setSearch] = useState('');

  const projectTotals = useMemo(() => {
    const active = projects.filter((project) => statusLabel(project.status) === 'Active').length;
    const completed = projects.filter((project) => statusLabel(project.status) === 'Completed').length;
    return { active, completed, total: projects.length };
  }, [projects]);

  useEffect(() => {
    loadProjects();
  }, []);

  async function loadProjects() {
    const data = await fetchProjects({
      search: search.trim() || undefined,
    });
    setProjects(data);
  }

  async function handleSelect(project: Project) {
    const details = await fetchProject(project.id);
    if (details) {
      setSelectedProject(details);
    }
  }

  async function handleSave(payload: ProjectPayload) {
    const savedProject = await saveProject(payload, selectedProject?.id);
    if (savedProject) {
      setMessage(selectedProject ? 'Project updated.' : 'Project created.');
      setSelectedProject(savedProject);
      await loadProjects();
    }
  }

  async function handleDelete(id: string) {
    if (!window.confirm('Delete this project?')) return;
    const success = await removeProject(id);
    if (success) {
      setMessage('Project deleted.');
      if (selectedProject?.id === id) {
        setSelectedProject(null);
      }
      await loadProjects();
    }
  }

  return (
    <main className="prompt-page">
      <header className="prompt-header">
        <div>
          <p className="eyebrow">TubePilotAI</p>
          <h1>Projects</h1>
        </div>
        <button className="secondary-button" type="button" onClick={() => { setSelectedProject(null); setIsModalOpen(true); }}>
          New Project
        </button>
      </header>

      <section className="project-summary" aria-label="Project summary">
        <div>
          <strong>{projectTotals.total}</strong>
          <span>Total</span>
        </div>
        <div>
          <strong>{projectTotals.active}</strong>
          <span>Active</span>
        </div>
        <div>
          <strong>{projectTotals.completed}</strong>
          <span>Completed</span>
        </div>
      </section>

      <section className="toolbar" aria-label="Project filters">
        <label>
          Search
          <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Name, owner, or description" />
        </label>
        <button type="button" onClick={loadProjects}>Apply</button>
      </section>

      {error && <div className="notice error">{error}</div>}
      {message && <div className="notice success">{message}</div>}

      <section className="prompt-layout project-layout">
        <ProjectDashboard
          projects={projects}
          selectedId={selectedProject?.id}
          onSelect={handleSelect}
          onDelete={handleDelete}
          isLoading={loading}
        />

        <div className="project-workspace">
          {selectedProject ? (
            <>
              <ProjectDetails project={selectedProject} />
              <button className="secondary-button" onClick={() => setIsModalOpen(true)}>Edit Project</button>
            </>
          ) : (
            <p>Select a project to view details</p>
          )}
        </div>
      </section>

      {isModalOpen && (
        <div className="modal-overlay">
          <ProjectModal
            project={selectedProject}
            onSave={handleSave}
            isSaving={loading}
            onClose={() => setIsModalOpen(false)}
          />
        </div>
      )}
    </main>
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
