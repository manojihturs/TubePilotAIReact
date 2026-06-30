import { useEffect, useMemo, useState } from 'react';
import type { FormEvent } from 'react';
import {
  createPromptTemplate,
  deletePromptTemplate,
  getPromptTemplates,
  updatePromptTemplate,
} from '../../services/api/promptTemplatesApi';
import type { CreatePromptTemplatePayload, PromptTemplate, PromptTemplateStatus } from '../../types/promptTemplate';
import { validatePromptTemplate } from './validation';

const emptyForm: CreatePromptTemplatePayload = {
  name: '',
  category: '',
  description: '',
  templateText: '',
  systemMessage: '',
  variables: [],
  status: 'Draft',
  isDefault: false,
  createdBy: 'TubePilot Admin',
};

export function PromptTemplatesPage() {
  const [templates, setTemplates] = useState<PromptTemplate[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [form, setForm] = useState<CreatePromptTemplatePayload>(emptyForm);
  const [variableInput, setVariableInput] = useState('');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const [message, setMessage] = useState('');

  const categories = useMemo(
    () => Array.from(new Set(templates.map((template) => template.category))).sort(),
    [templates],
  );

  const selectedTemplate = templates.find((template) => template.id === selectedId);

  useEffect(() => {
    loadTemplates();
  }, []);

  async function loadTemplates() {
    setIsLoading(true);
    setErrors([]);

    try {
      const data = await getPromptTemplates({
        search: search.trim() || undefined,
        status: status || undefined,
      });
      setTemplates(data);
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    } finally {
      setIsLoading(false);
    }
  }

  function selectTemplate(template: PromptTemplate) {
    setSelectedId(template.id);
    setVariableInput(template.variables.join(', '));
    setForm({
      name: template.name,
      category: template.category,
      description: template.description ?? '',
      templateText: template.templateText,
      systemMessage: template.systemMessage ?? '',
      variables: template.variables,
      status: template.status,
      isDefault: template.isDefault,
      createdBy: template.createdBy,
    });
    setErrors([]);
    setMessage('');
  }

  function startNewTemplate(clearFeedback = true) {
    setSelectedId(null);
    setVariableInput('');
    setForm(emptyForm);
    if (clearFeedback) {
      setErrors([]);
      setMessage('');
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const payload = {
      ...form,
      variables: parseVariables(variableInput),
    };
    const validationErrors = validatePromptTemplate(payload);

    if (validationErrors.length > 0) {
      setErrors(validationErrors);
      setMessage('');
      return;
    }

    setIsSaving(true);
    setErrors([]);

    try {
      let savedTemplate: PromptTemplate | null = null;

      if (selectedId) {
        const updatePayload = {
          name: payload.name,
          category: payload.category,
          description: payload.description,
          templateText: payload.templateText,
          systemMessage: payload.systemMessage,
          variables: payload.variables,
          status: payload.status,
          isDefault: payload.isDefault,
        };
        savedTemplate = await updatePromptTemplate(selectedId, updatePayload);
        setMessage('Prompt template updated.');
      } else {
        savedTemplate = await createPromptTemplate(payload);
        setMessage('Prompt template created.');
      }

      await loadTemplates();
      if (savedTemplate) {
        selectTemplate(savedTemplate);
      }
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    } finally {
      setIsSaving(false);
    }
  }

  async function handleDelete(id: string) {
    const confirmed = window.confirm('Delete this prompt template?');
    if (!confirmed) {
      return;
    }

    setErrors([]);

    try {
      await deletePromptTemplate(id);
      setMessage('Prompt template deleted.');
      if (selectedId === id) {
        startNewTemplate(false);
      }
      await loadTemplates();
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    }
  }

  return (
    <main className="prompt-page">
      <header className="prompt-header">
        <div>
          <p className="eyebrow">TubePilotAI</p>
          <h1>Prompt Templates</h1>
        </div>
        <button className="secondary-button" type="button" onClick={() => startNewTemplate()}>
          New Template
        </button>
      </header>

      <section className="toolbar" aria-label="Prompt template filters">
        <label>
          Search
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Title, category, description, or prompt"
          />
        </label>
        <label>
          Status
          <select value={status} onChange={(event) => setStatus(event.target.value)}>
            <option value="">All statuses</option>
            <option value="Draft">Draft</option>
            <option value="Active">Active</option>
            <option value="Archived">Archived</option>
          </select>
        </label>
        <button type="button" onClick={loadTemplates}>
          Apply
        </button>
      </section>

      {errors.length > 0 && (
        <div className="notice error" role="alert">
          {errors.map((error) => (
            <p key={error}>{error}</p>
          ))}
        </div>
      )}

      {message && <div className="notice success">{message}</div>}

      <section className="prompt-layout">
        <div className="template-list" aria-label="Prompt templates">
          {isLoading ? (
            <p className="empty-state">Loading templates...</p>
          ) : templates.length === 0 ? (
            <p className="empty-state">No prompt templates found.</p>
          ) : (
            templates.map((template) => (
              <article
                className={`template-row ${template.id === selectedId ? 'selected' : ''}`}
                key={template.id}
              >
                <button type="button" onClick={() => selectTemplate(template)}>
                  <span>
                    <strong>{template.name}</strong>
                    <small>{template.category}</small>
                  </span>
                  <span className="status-stack">
                    <span className="status-pill">{statusLabel(template.status)}</span>
                    {template.isDefault && <span className="priority-pill">Default</span>}
                  </span>
                </button>
                <button className="delete-button" type="button" onClick={() => handleDelete(template.id)}>
                  Delete
                </button>
              </article>
            ))
          )}
        </div>

        <form className="template-form" onSubmit={handleSubmit}>
          <div className="form-title">
            <h2>{selectedTemplate ? 'Edit template' : 'Create template'}</h2>
            {categories.length > 0 && <span>{categories.length} categories</span>}
          </div>

          <div className="form-grid">
            <label>
              Name
              <input
                required
                minLength={3}
                maxLength={120}
                value={form.name}
                onChange={(event) => setForm({ ...form, name: event.target.value })}
              />
            </label>
            <label>
              Category
              <input
                required
                minLength={2}
                maxLength={80}
                list="prompt-template-categories"
                value={form.category}
                onChange={(event) => setForm({ ...form, category: event.target.value })}
              />
              <datalist id="prompt-template-categories">
                {categories.map((category) => (
                  <option key={category} value={category} />
                ))}
              </datalist>
            </label>
            <label>
              Status
              <select
                value={String(form.status)}
                onChange={(event) => setForm({ ...form, status: event.target.value as PromptTemplateStatus })}
              >
                <option value="Draft">Draft</option>
                <option value="Active">Active</option>
                <option value="Archived">Archived</option>
              </select>
            </label>
            <label className="checkbox-field">
              <input
                type="checkbox"
                checked={form.isDefault}
                onChange={(event) => {
                  const isDefault = event.target.checked;
                  setForm((current) => ({ ...current, isDefault }));
                }}
              />
              <span>Default template</span>
            </label>
            <label>
              Created by
              <input
                disabled={Boolean(selectedId)}
                required
                minLength={2}
                maxLength={120}
                value={form.createdBy}
                onChange={(event) => setForm({ ...form, createdBy: event.target.value })}
              />
            </label>
          </div>

          <label>
            Description
            <input
              maxLength={500}
              value={form.description ?? ''}
              onChange={(event) => setForm({ ...form, description: event.target.value })}
            />
          </label>

          <label>
            Variables
            <input
              value={variableInput}
              onChange={(event) => setVariableInput(event.target.value)}
              placeholder="topic, audience, tone"
            />
          </label>

          <label>
            System message
            <textarea
              rows={4}
              maxLength={2000}
              value={form.systemMessage ?? ''}
              onChange={(event) => setForm({ ...form, systemMessage: event.target.value })}
            />
          </label>

          <label>
            Prompt template
            <textarea
              required
              rows={10}
              minLength={10}
              maxLength={8000}
              value={form.templateText}
              onChange={(event) => setForm({ ...form, templateText: event.target.value })}
              placeholder="Write a YouTube script outline for {{topic}} targeting {{audience}}..."
            />
          </label>

          <div className="form-actions">
            <button type="submit" disabled={isSaving}>
              {isSaving ? 'Saving...' : selectedId ? 'Update Template' : 'Create Template'}
            </button>
            <button className="secondary-button" type="button" onClick={() => startNewTemplate()}>
              Clear
            </button>
          </div>
        </form>
      </section>
    </main>
  );
}

function parseVariables(value: string) {
  return value
    .split(',')
    .map((variable) => variable.trim())
    .filter(Boolean);
}

function statusLabel(status: PromptTemplate['status']) {
  if (status === 0) return 'Draft';
  if (status === 1) return 'Active';
  if (status === 2) return 'Archived';
  return status;
}

function getErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : 'Something went wrong.';
}
