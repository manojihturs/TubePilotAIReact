import { useEffect, useMemo, useState } from 'react';
import {
  exportContentJob,
  generateContent,
  getContentJob,
  getContentJobs,
  getContentTemplates,
} from '../../services/api/contentGenerationApi';
import { getAIProviderSettings } from '../../services/api/aiSettingsApi';
import type {
  ContentGenerationJob,
  ContentGenerationProviderKind,
  ContentGenerationResult,
  ContentGenerationTemplate,
  GenerateContentJobPayload,
} from '../../types/contentGeneration';
import type { AIProviderSetting } from '../../types/aiSettings';

const providerOptions: ContentGenerationProviderKind[] = ['OpenAI', 'Gemini', 'Claude', 'DeepSeek', 'Ollama'];

export function ContentGenerationPage() {
  const [jobs, setJobs] = useState<ContentGenerationJob[]>([]);
  const [templates, setTemplates] = useState<ContentGenerationTemplate[]>([]);
  const [aiSettings, setAiSettings] = useState<AIProviderSetting[]>([]);
  const [selectedJob, setSelectedJob] = useState<ContentGenerationResult | null>(null);
  const [title, setTitle] = useState('');
  const [selectedTemplateId, setSelectedTemplateId] = useState('');
  const [promptOverride, setPromptOverride] = useState('');
  const [selectedProvider, setSelectedProvider] = useState<ContentGenerationProviderKind | ''>('');
  const [isGenerating, setIsGenerating] = useState(false);
  const [isExporting, setIsExporting] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const [message, setMessage] = useState('');
  const [activeTab, setActiveTab] = useState<'research' | 'script' | 'assets' | 'history'>('research');

  const defaultTemplate = useMemo(
    () => templates.find((template) => template.isDefault) ?? templates[0] ?? null,
    [templates],
  );

  const configuredProviders = useMemo(
    () => aiSettings.filter((setting) => setting.isConfigured).map((setting) => setting.provider),
    [aiSettings],
  );

  const selectedTemplate = useMemo(
    () => templates.find((template) => template.id === selectedTemplateId) ?? defaultTemplate,
    [defaultTemplate, selectedTemplateId, templates],
  );

  useEffect(() => {
    void refresh();
  }, []);

  useEffect(() => {
    if (defaultTemplate && !selectedTemplateId) {
      setSelectedTemplateId(defaultTemplate.id);
      setPromptOverride(defaultTemplate.templateText);
    }
  }, [defaultTemplate, selectedTemplateId]);

  useEffect(() => {
    if (selectedTemplate) {
      setPromptOverride(selectedTemplate.templateText);
    }
  }, [selectedTemplate]);

  useEffect(() => {
    if (!selectedProvider || configuredProviders.includes(selectedProvider)) {
      return;
    }

    setSelectedProvider(configuredProviders[0] ?? '');
  }, [configuredProviders, selectedProvider]);

  async function refresh() {
    setErrors([]);
    try {
      const [jobList, templateList, settingsList] = await Promise.all([
        getContentJobs(),
        getContentTemplates(),
        getAIProviderSettings(),
      ]);
      setJobs(jobList);
      setTemplates(templateList);
      setAiSettings(settingsList);
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    }
  }

  async function handleGenerate() {
    if (!title.trim()) {
      setErrors(['Title is required.']);
      return;
    }

    if (!selectedProvider) {
      setErrors(['Select one configured AI provider.']);
      return;
    }

    const payload: GenerateContentJobPayload = {
      title: title.trim(),
      promptTemplateId: (selectedTemplateId || defaultTemplate?.id) ?? null,
      promptOverride: promptOverride.trim() || null,
      selectedProviders: [selectedProvider],
    };

    setIsGenerating(true);
    setErrors([]);
    setMessage('');

    try {
      const result = await generateContent(payload);
      setSelectedJob(result);
      setMessage(`Generated package for ${result.title}.`);
      await refresh();
      setActiveTab('research');
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    } finally {
      setIsGenerating(false);
    }
  }

  async function openJob(job: ContentGenerationJob) {
    try {
      const details = await getContentJob(job.jobId);
      setSelectedJob(details);
      setMessage('');
      setActiveTab('research');
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    }
  }

  async function handleExport() {
    if (!selectedJob) return;
    setIsExporting(true);
    setErrors([]);
    try {
      const result = await exportContentJob(selectedJob.jobId);
      setMessage(`Exported to ${result.path}`);
      await refresh();
      const refreshed = await getContentJob(selectedJob.jobId);
      setSelectedJob(refreshed);
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    } finally {
      setIsExporting(false);
    }
  }

  return (
    <main className="prompt-page">
      <header className="prompt-header">
        <div>
          <p className="eyebrow">TubePilotAI</p>
          <h1>Content Generation</h1>
        </div>
        <button className="secondary-button" type="button" onClick={handleGenerate} disabled={isGenerating || !selectedProvider}>
          {isGenerating ? 'Generating...' : 'Generate Package'}
        </button>
      </header>

      <section className="project-summary">
        <div><strong>{jobs.length}</strong><span>Jobs</span></div>
        <div><strong>{configuredProviders.length}</strong><span>Configured AI Tools</span></div>
        <div><strong>{templates.filter((template) => template.isDefault).length}</strong><span>Default Prompt</span></div>
      </section>

      <section className="content-layout">
        <aside className="content-panel">
          <h2>Level 0 Input</h2>
          <label>
            Title
            <input value={title} onChange={(event) => setTitle(event.target.value)} placeholder="Paste your title here" />
          </label>

          <label>
            Prompt template
            <select
              value={selectedTemplateId}
              onChange={(event) => setSelectedTemplateId(event.target.value)}
            >
              {templates.map((template) => (
                <option key={template.id} value={template.id}>
                  {template.isDefault ? `${template.name} (Default)` : template.name} - {template.category}
                </option>
              ))}
            </select>
          </label>

          <label>
            Prompt
            <textarea
              rows={8}
              value={promptOverride}
              onChange={(event) => setPromptOverride(event.target.value)}
              placeholder={selectedTemplate?.templateText ?? 'Select a prompt template to load the default prompt.'}
            />
          </label>

          <div className="content-stack">
            <h3>AI Tools</h3>
            {configuredProviders.length === 0 ? (
              <p className="empty-state">Add an API key in AI Settings to enable a tool here.</p>
            ) : (
              <div className="provider-grid">
                {providerOptions.map((provider) => {
                  const isConfigured = configuredProviders.includes(provider);
                  return (
                    <label key={provider} className={`provider-chip ${!isConfigured ? 'disabled' : ''}`}>
                      <input
                        type="radio"
                        name="ai-provider"
                        checked={selectedProvider === provider}
                        disabled={!isConfigured}
                        onChange={() => setSelectedProvider(provider)}
                      />
                      <span>{provider}</span>
                    </label>
                  );
                })}
              </div>
            )}
          </div>

          <div className="form-actions">
            <button type="button" onClick={handleGenerate} disabled={isGenerating || !selectedProvider}>
              {isGenerating ? 'Generating...' : 'Generate'}
            </button>
            <button className="secondary-button" type="button" onClick={handleExport} disabled={isExporting || !selectedJob}>
              {isExporting ? 'Exporting...' : 'Export Folder'}
            </button>
          </div>

          {errors.length > 0 && (
            <div className="notice error" role="alert">
              {errors.map((error) => <p key={error}>{error}</p>)}
            </div>
          )}
          {message && <div className="notice success">{message}</div>}
        </aside>

        <section className="content-workspace">
          <div className="tabs">
            <button className={activeTab === 'research' ? 'active' : ''} type="button" onClick={() => setActiveTab('research')}>Research</button>
            <button className={activeTab === 'script' ? 'active' : ''} type="button" onClick={() => setActiveTab('script')}>Script</button>
            <button className={activeTab === 'assets' ? 'active' : ''} type="button" onClick={() => setActiveTab('assets')}>Assets</button>
            <button className={activeTab === 'history' ? 'active' : ''} type="button" onClick={() => setActiveTab('history')}>History</button>
          </div>

          {!selectedJob ? (
            <p className="empty-state">Generate a package to see the output here.</p>
          ) : activeTab === 'research' ? (
            <div className="content-stack">
              <h3>Research CSV Preview</h3>
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Keyword</th>
                    <th>Volume</th>
                    <th>Competition</th>
                    <th>Opportunity</th>
                    <th>Notes</th>
                  </tr>
                </thead>
                <tbody>
                  {selectedJob.researchRows.map((row) => (
                    <tr key={row.keyword}>
                      <td>{row.keyword}</td>
                      <td>{row.searchVolume}</td>
                      <td>{row.competitionScore}</td>
                      <td>{row.opportunityScore}</td>
                      <td>{row.notes}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <p><strong>Video title:</strong> {selectedJob.videoTitle}</p>
              <p><strong>Description:</strong> {selectedJob.description}</p>
              <p><strong>Hashtags:</strong> {selectedJob.hashtags}</p>
            </div>
          ) : activeTab === 'script' ? (
            <div className="content-stack">
              <h3>Narration script</h3>
              <pre>{selectedJob.narrationScript}</pre>
              <h3>Scene text</h3>
              <pre>{selectedJob.sceneText}</pre>
              <h3>Voiceover text</h3>
              <pre>{selectedJob.voiceoverText}</pre>
            </div>
          ) : activeTab === 'assets' ? (
            <div className="content-stack">
              <p><strong>Export folder:</strong> {selectedJob.exportFolderPath ?? 'Not exported yet'}</p>
              <p><strong>Thumbnail text:</strong> {selectedJob.thumbnailText}</p>
              <p><strong>Background prompt:</strong> {selectedJob.backgroundImagePrompt}</p>
              <p><strong>Data files:</strong> `data/video-title.txt`, `data/description.txt`, `data/hashtags.txt`, `data/research.csv`, `data/narration-script.txt`, `data/scene-text.txt`, `data/voiceover-text.txt`</p>
              <p><strong>Asset files:</strong> `assets/thumbnail-text.txt`, `assets/background-image-prompt.txt`, `assets/thumbnail/thumbnail-hd.png`</p>
              <p><strong>Status:</strong> {selectedJob.status}</p>
            </div>
          ) : (
            <div className="content-stack">
              {jobs.length === 0 ? (
                <p className="empty-state">No generation jobs yet.</p>
              ) : (
                jobs.map((job) => (
                  <button key={job.jobId} className="history-row" type="button" onClick={() => openJob(job)}>
                    <span>
                      <strong>{job.title}</strong>
                      <small>{job.promptText.slice(0, 120)}</small>
                    </span>
                    <span>{job.status}</span>
                  </button>
                ))
              )}
            </div>
          )}
        </section>
      </section>
    </main>
  );
}

function getErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : 'Something went wrong.';
}
