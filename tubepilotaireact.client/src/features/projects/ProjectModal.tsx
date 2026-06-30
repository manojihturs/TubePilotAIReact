import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import type { Project, ProjectPayload, ProjectPriority, ProjectStatus } from '../../types/project';
import { validateProject } from './validation';

interface Props {
  project?: Project | null;
  onSave: (payload: ProjectPayload) => Promise<void>;
  isSaving: boolean;
  onClose: () => void;
}

const emptyForm: ProjectPayload = {
  name: '',
  description: '',
  ownerName: '',
  status: 'Draft',
  priority: 'Medium',
  startDate: '',
  dueDate: '',
  budget: 0,
  tags: [],
};

export function ProjectModal({ project, onSave, isSaving, onClose }: Props) {
  const [form, setForm] = useState<ProjectPayload>(project ? toForm(project) : emptyForm);
  const [tagInput, setTagInput] = useState(project ? project.tags.join(', ') : '');
  const [errors, setErrors] = useState<string[]>([]);

  useEffect(() => {
    if (project) {
      setForm(toForm(project));
      setTagInput(project.tags.join(', '));
    }
  }, [project]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const payload = normalizePayload({
      ...form,
      tags: parseTags(tagInput),
    });
    const validationErrors = validateProject(payload);

    if (validationErrors.length > 0) {
      setErrors(validationErrors);
      return;
    }

    setErrors([]);
    await onSave(payload);
    onClose();
  }

  return (
    <form className="template-form" onSubmit={handleSubmit}>
      <div className="form-title">
        <h2>{project ? 'Edit project' : 'Create project'}</h2>
      </div>

      {errors.length > 0 && (
        <div className="notice error" role="alert">
          {errors.map((error) => (
            <p key={error}>{error}</p>
          ))}
        </div>
      )}

      <div className="form-grid">
        <label>
          Name
          <input
            required
            minLength={3}
            maxLength={160}
            value={form.name}
            onChange={(event) => setForm({ ...form, name: event.target.value })}
          />
        </label>
        <label>
          Owner
          <input
            required
            minLength={2}
            maxLength={120}
            value={form.ownerName}
            onChange={(event) => setForm({ ...form, ownerName: event.target.value })}
          />
        </label>
        <label>
          Status
          <select
            value={String(form.status)}
            onChange={(event) => setForm({ ...form, status: event.target.value as ProjectStatus })}
          >
            <option value="Draft">Draft</option>
            <option value="Active">Active</option>
            <option value="OnHold">On hold</option>
            <option value="Completed">Completed</option>
            <option value="Archived">Archived</option>
          </select>
        </label>
        <label>
          Priority
          <select
            value={String(form.priority)}
            onChange={(event) => setForm({ ...form, priority: event.target.value as ProjectPriority })}
          >
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
            <option value="Critical">Critical</option>
          </select>
        </label>
        <label>
          Start date
          <input
            type="date"
            value={form.startDate ?? ''}
            onChange={(event) => setForm({ ...form, startDate: event.target.value })}
          />
        </label>
        <label>
          Due date
          <input
            type="date"
            value={form.dueDate ?? ''}
            onChange={(event) => setForm({ ...form, dueDate: event.target.value })}
          />
        </label>
        <label>
          Budget
          <input
            min={0}
            step="0.01"
            type="number"
            value={form.budget}
            onChange={(event) => setForm({ ...form, budget: Number(event.target.value) })}
          />
        </label>
        <label>
          Tags
          <input
            value={tagInput}
            onChange={(event) => setTagInput(event.target.value)}
            placeholder="launch, scripting, analytics"
          />
        </label>
      </div>

      <label>
        Description
        <textarea
          rows={6}
          maxLength={1200}
          value={form.description ?? ''}
          onChange={(event) => setForm({ ...form, description: event.target.value })}
        />
      </label>

      <div className="form-actions">
        <button type="submit" disabled={isSaving}>
          {isSaving ? 'Saving...' : project ? 'Update Project' : 'Create Project'}
        </button>
        <button className="secondary-button" type="button" onClick={onClose}>
          Cancel
        </button>
      </div>
    </form>
  );
}

function toForm(project: Project): ProjectPayload {
  return {
    name: project.name,
    description: project.description ?? '',
    ownerName: project.ownerName,
    status: project.status,
    priority: project.priority,
    startDate: project.startDate ?? '',
    dueDate: project.dueDate ?? '',
    budget: project.budget,
    tags: project.tags,
  };
}

function normalizePayload(payload: ProjectPayload): ProjectPayload {
  return {
    ...payload,
    description: payload.description?.trim() || null,
    startDate: payload.startDate || null,
    dueDate: payload.dueDate || null,
    budget: Number.isFinite(payload.budget) ? payload.budget : 0,
  };
}

function parseTags(value: string) {
  return value
    .split(',')
    .map((tag) => tag.trim())
    .filter(Boolean);
}
