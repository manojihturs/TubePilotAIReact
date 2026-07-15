import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ChevronLeft, KeyRound, Plus, Trash2, CheckCircle2, Sparkles, Pencil, Power } from 'lucide-react';
import { Link } from 'react-router-dom';
import { aiProviderService, FOOTAGE_PROVIDERS, aiToolService, type AiTool, type AiApiFormat } from '../services';
import { LoadingSkeleton } from '../components/LoadingCard';
import { ErrorState } from '../components/ErrorState';
import { useToast } from '../contexts/ToastContext';
import { logActivity } from '../services/logger';

const FORMAT_LABELS: Record<AiApiFormat, string> = {
  'anthropic-messages': 'Anthropic Messages (Claude)',
  'openai-chat': 'OpenAI-compatible Chat (OpenAI, Groq, NVIDIA NIM, DeepSeek, OpenRouter, Ollama, ...)',
  'gemini': 'Google Gemini',
};

const FORMAT_DEFAULT_BASE_URL: Record<AiApiFormat, string> = {
  'anthropic-messages': 'https://api.anthropic.com',
  'openai-chat': 'https://api.groq.com/openai/v1',
  'gemini': 'https://generativelanguage.googleapis.com',
};

const FORMAT_DEFAULT_MODEL: Record<AiApiFormat, string> = {
  'anthropic-messages': 'claude-sonnet-5',
  'openai-chat': 'llama-3.3-70b-versatile',
  'gemini': 'gemini-2.0-flash',
};

const emptyToolForm = {
  name: '',
  apiFormat: 'openai-chat' as AiApiFormat,
  baseUrl: FORMAT_DEFAULT_BASE_URL['openai-chat'],
  model: FORMAT_DEFAULT_MODEL['openai-chat'],
  apiKey: '',
  priority: 0,
};

function AiToolsSection() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState(emptyToolForm);

  const { data: tools = [], isLoading, isError, refetch } = useQuery({
    queryKey: ['ai-tools'],
    queryFn: () => aiToolService.getAll(),
  });

  const resetForm = () => {
    setForm(emptyToolForm);
    setEditingId(null);
    setIsFormOpen(false);
  };

  const createMutation = useMutation({
    mutationFn: () => aiToolService.create(form),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ai-tools'] });
      showToast(`AI tool "${form.name}" added`, 'success');
      logActivity({ action: 'create', entity: 'AiTool', label: `Added AI tool "${form.name}"`, status: 'success' });
      resetForm();
    },
    onError: (err: unknown) => {
      showToast(`Failed to add AI tool "${form.name}"`, 'error');
      logActivity({ action: 'create', entity: 'AiTool', label: `Add AI tool "${form.name}"`, status: 'error', detail: String(err) });
    },
  });

  const updateMutation = useMutation({
    mutationFn: () => aiToolService.update(editingId as string, form),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ai-tools'] });
      showToast(`AI tool "${form.name}" updated`, 'success');
      logActivity({ action: 'update', entity: 'AiTool', label: `Updated AI tool "${form.name}"`, status: 'success' });
      resetForm();
    },
    onError: (err: unknown) => {
      showToast(`Failed to update AI tool "${form.name}"`, 'error');
      logActivity({ action: 'update', entity: 'AiTool', label: `Update AI tool "${form.name}"`, status: 'error', detail: String(err) });
    },
  });

  const toggleMutation = useMutation({
    mutationFn: (tool: AiTool) =>
      aiToolService.update(tool.id, {
        name: tool.name,
        baseUrl: tool.baseUrl,
        model: tool.model,
        priority: tool.priority,
        isEnabled: !tool.isEnabled,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['ai-tools'] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => aiToolService.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ai-tools'] });
      showToast('AI tool removed', 'success');
      logActivity({ action: 'delete', entity: 'AiTool', label: 'Removed AI tool', status: 'success' });
    },
    onError: (err: unknown) => {
      showToast('Failed to remove AI tool', 'error');
      logActivity({ action: 'delete', entity: 'AiTool', label: 'Remove AI tool', status: 'error', detail: String(err) });
    },
  });

  const startEdit = (tool: AiTool) => {
    setEditingId(tool.id);
    setForm({
      name: tool.name,
      apiFormat: tool.apiFormat,
      baseUrl: tool.baseUrl,
      model: tool.model,
      apiKey: '',
      priority: tool.priority,
    });
    setIsFormOpen(true);
  };

  const handleFormatChange = (apiFormat: AiApiFormat) => {
    setForm((f) => ({
      ...f,
      apiFormat,
      // Only swap in defaults if the user hasn't customized them yet, to avoid clobbering
      // a base URL/model they already typed while browsing formats.
      baseUrl: f.baseUrl === FORMAT_DEFAULT_BASE_URL[f.apiFormat] ? FORMAT_DEFAULT_BASE_URL[apiFormat] : f.baseUrl,
      model: f.model === FORMAT_DEFAULT_MODEL[f.apiFormat] ? FORMAT_DEFAULT_MODEL[apiFormat] : f.model,
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim() || !form.baseUrl.trim() || !form.model.trim()) return;
    if (!editingId && !form.apiKey.trim()) return;
    if (editingId) {
      updateMutation.mutate();
    } else {
      createMutation.mutate();
    }
  };

  if (isLoading) return <LoadingSkeleton />;

  const sorted = [...tools].sort((a, b) => a.priority - b.priority);

  return (
    <div className="mb-10">
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
            <Sparkles size={18} className="text-indigo-500" /> AI Tools
          </h2>
          <p className="text-sm text-slate-600 dark:text-slate-400">
            Add any number of AI tools — any name, any host, your own key. Generation tries them in priority order
            (lowest first) and automatically moves to the next enabled tool if one fails or hits a rate limit.
          </p>
        </div>
        <button
          onClick={() => {
            resetForm();
            setIsFormOpen(true);
          }}
          className="flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors whitespace-nowrap flex-shrink-0"
        >
          <Plus size={20} />
          <span className="hidden sm:inline">Add AI Tool</span>
          <span className="sm:hidden">Add</span>
        </button>
      </div>

      {isFormOpen && (
        <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6 mb-6">
          <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-4">
            {editingId ? 'Edit AI Tool' : 'Add AI Tool'}
          </h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Name *</label>
                <input
                  type="text"
                  required
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  placeholder="e.g., My Groq Key, Work NVIDIA NIM"
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Priority</label>
                <input
                  type="number"
                  value={form.priority}
                  onChange={(e) => setForm({ ...form, priority: Number(e.target.value) })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
                <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">Lower runs first.</p>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">API Format *</label>
              <select
                required
                disabled={!!editingId}
                value={form.apiFormat}
                onChange={(e) => handleFormatChange(e.target.value as AiApiFormat)}
                className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-60"
              >
                {(Object.keys(FORMAT_LABELS) as AiApiFormat[]).map((format) => (
                  <option key={format} value={format}>
                    {FORMAT_LABELS[format]}
                  </option>
                ))}
              </select>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Base URL *</label>
                <input
                  type="text"
                  required
                  value={form.baseUrl}
                  onChange={(e) => setForm({ ...form, baseUrl: e.target.value })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Model *</label>
                <input
                  type="text"
                  required
                  value={form.model}
                  onChange={(e) => setForm({ ...form, model: e.target.value })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono text-sm"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                API Key {editingId ? '' : '*'}
              </label>
              <input
                type="password"
                required={!editingId}
                value={form.apiKey}
                onChange={(e) => setForm({ ...form, apiKey: e.target.value })}
                className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono text-sm"
                placeholder={editingId ? 'Leave blank to keep the existing key' : 'sk-...'}
                autoComplete="off"
              />
              <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                Your key is encrypted at rest and never displayed again after saving.
              </p>
            </div>

            <div className="flex gap-2">
              <button
                type="submit"
                disabled={createMutation.isPending || updateMutation.isPending}
                className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors"
              >
                {createMutation.isPending || updateMutation.isPending ? 'Saving...' : editingId ? 'Save Changes' : 'Add Tool'}
              </button>
              <button
                type="button"
                onClick={resetForm}
                className="px-4 py-2 border border-slate-300 dark:border-slate-600 text-slate-900 dark:text-white rounded-lg hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {isError && <ErrorState message="Failed to load AI tools" onRetry={() => refetch()} />}

      {sorted.length === 0 ? (
        <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-12 text-center">
          <Sparkles className="mx-auto text-slate-400 mb-3" size={32} />
          <p className="text-slate-600 dark:text-slate-400 mb-4">No AI tools added yet</p>
          <button
            onClick={() => setIsFormOpen(true)}
            className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            <Plus size={20} />
            <span>Add AI Tool</span>
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {sorted.map((tool) => (
            <div
              key={tool.id}
              className={`bg-white dark:bg-slate-900 rounded-lg border p-5 ${tool.isEnabled ? 'border-slate-200 dark:border-slate-800' : 'border-slate-200 dark:border-slate-800 opacity-60'}`}
            >
              <div className="flex items-start justify-between mb-2">
                <div className="flex items-center gap-2 min-w-0">
                  <div className="w-9 h-9 rounded-lg bg-indigo-50 dark:bg-indigo-500/10 flex items-center justify-center flex-shrink-0">
                    <Sparkles size={18} className="text-indigo-600 dark:text-indigo-400" />
                  </div>
                  <div className="min-w-0">
                    <h3 className="font-bold text-slate-900 dark:text-white truncate">{tool.name}</h3>
                    <p className="text-xs text-slate-500 dark:text-slate-500">Priority {tool.priority}</p>
                  </div>
                </div>
                <div className="flex items-center gap-1 flex-shrink-0">
                  <button
                    onClick={() => toggleMutation.mutate(tool)}
                    disabled={toggleMutation.isPending}
                    className={`p-1.5 rounded transition-colors disabled:opacity-50 ${tool.isEnabled ? 'text-green-600 hover:bg-green-50 dark:hover:bg-green-900/20' : 'text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800'}`}
                    title={tool.isEnabled ? 'Enabled — click to disable' : 'Disabled — click to enable'}
                  >
                    <Power size={16} />
                  </button>
                  <button
                    onClick={() => startEdit(tool)}
                    className="p-1.5 text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-800 rounded transition-colors"
                    title="Edit"
                  >
                    <Pencil size={16} />
                  </button>
                  <button
                    onClick={() => {
                      if (confirm(`Remove the AI tool "${tool.name}"?`)) {
                        deleteMutation.mutate(tool.id);
                      }
                    }}
                    disabled={deleteMutation.isPending}
                    className="p-1.5 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded transition-colors disabled:opacity-50"
                    title="Remove"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              </div>
              <p className="text-xs text-slate-500 dark:text-slate-500 truncate" title={tool.baseUrl}>{tool.baseUrl}</p>
              <p className="text-xs text-slate-500 dark:text-slate-500 truncate" title={tool.model}>{tool.model}</p>
              <div className="flex items-center gap-1.5 text-sm text-green-600 dark:text-green-400 mt-2">
                <CheckCircle2 size={14} />
                <span>{tool.isEnabled ? 'Enabled' : 'Disabled'}</span>
              </div>
              {tool.lastUsedAt && (
                <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                  Last used {new Date(tool.lastUsedAt).toLocaleString()}
                </p>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function FootageKeysSection() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [providerName, setProviderName] = useState<string>(FOOTAGE_PROVIDERS[0]);
  const [apiKey, setApiKey] = useState('');

  const { data: keys = [], isLoading, isError, refetch } = useQuery({
    queryKey: ['api-keys'],
    queryFn: () => aiProviderService.getKeys(),
  });

  const saveMutation = useMutation({
    mutationFn: () => aiProviderService.saveKey({ providerName, apiKey }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['api-keys'] });
      showToast(`${providerName} API key saved`, 'success');
      logActivity({ action: 'create', entity: 'ApiKey', label: `Saved ${providerName} API key`, status: 'success' });
      setApiKey('');
      setIsFormOpen(false);
    },
    onError: (err: unknown) => {
      showToast(`Failed to save ${providerName} API key`, 'error');
      logActivity({ action: 'create', entity: 'ApiKey', label: `Save ${providerName} API key`, status: 'error', detail: String(err) });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (provider: string) => aiProviderService.deleteKey(provider),
    onSuccess: (_data, provider) => {
      queryClient.invalidateQueries({ queryKey: ['api-keys'] });
      showToast(`${provider} API key removed`, 'success');
      logActivity({ action: 'delete', entity: 'ApiKey', label: `Removed ${provider} API key`, status: 'success' });
    },
    onError: (err: unknown, provider) => {
      showToast(`Failed to remove ${provider} API key`, 'error');
      logActivity({ action: 'delete', entity: 'ApiKey', label: `Remove ${provider} API key`, status: 'error', detail: String(err) });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!apiKey.trim()) return;
    saveMutation.mutate();
  };

  if (isLoading) return <LoadingSkeleton />;

  // Legacy Claude/Groq/Gemini/NVidia rows may still exist in the DB from before AI tools
  // became fully user-defined — this section is scoped to footage providers only now.
  const footageKeys = keys.filter((k) => (FOOTAGE_PROVIDERS as readonly string[]).includes(k.providerName));
  const savedProviders = new Set(footageKeys.map((k) => k.providerName));

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
            <KeyRound size={18} className="text-indigo-500" /> Real Stock Footage Keys
          </h2>
          <p className="text-sm text-slate-600 dark:text-slate-400">
            Free Pexels/Pixabay keys used for real motion video clips instead of animated photos.
          </p>
        </div>
        <button
          onClick={() => setIsFormOpen(true)}
          className="flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors whitespace-nowrap flex-shrink-0"
        >
          <Plus size={20} />
          <span className="hidden sm:inline">Add Key</span>
          <span className="sm:hidden">Add</span>
        </button>
      </div>

      {isFormOpen && (
        <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6 mb-6">
          <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-4">Add Footage Key</h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Provider *</label>
              <select
                required
                value={providerName}
                onChange={(e) => setProviderName(e.target.value)}
                className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
              >
                {FOOTAGE_PROVIDERS.map((provider) => (
                  <option key={provider} value={provider}>
                    {provider}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">API Key *</label>
              <input
                type="password"
                required
                value={apiKey}
                onChange={(e) => setApiKey(e.target.value)}
                className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono text-sm"
                placeholder="Your API key"
                autoComplete="off"
              />
              <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                Your key is encrypted at rest and never displayed again after saving.
              </p>
            </div>
            <div className="flex gap-2">
              <button
                type="submit"
                disabled={saveMutation.isPending}
                className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors"
              >
                {saveMutation.isPending ? 'Saving...' : 'Save Key'}
              </button>
              <button
                type="button"
                onClick={() => {
                  setApiKey('');
                  setIsFormOpen(false);
                }}
                className="px-4 py-2 border border-slate-300 dark:border-slate-600 text-slate-900 dark:text-white rounded-lg hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {isError && <ErrorState message="Failed to load API keys" onRetry={() => refetch()} />}

      {footageKeys.length === 0 ? (
        <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-12 text-center">
          <KeyRound className="mx-auto text-slate-400 mb-3" size={32} />
          <p className="text-slate-600 dark:text-slate-400 mb-4">No footage keys connected yet</p>
          <button
            onClick={() => setIsFormOpen(true)}
            className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            <Plus size={20} />
            <span>Add Key</span>
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {footageKeys.map((key) => (
            <div
              key={key.providerName}
              className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-5"
            >
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-2">
                  <div className="w-9 h-9 rounded-lg bg-indigo-50 dark:bg-indigo-500/10 flex items-center justify-center">
                    <KeyRound size={18} className="text-indigo-600 dark:text-indigo-400" />
                  </div>
                  <h3 className="font-bold text-slate-900 dark:text-white">{key.providerName}</h3>
                </div>
                <button
                  onClick={() => {
                    if (confirm(`Remove the ${key.providerName} API key?`)) {
                      deleteMutation.mutate(key.providerName);
                    }
                  }}
                  disabled={deleteMutation.isPending}
                  className="p-1.5 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded transition-colors disabled:opacity-50"
                  title="Remove"
                >
                  <Trash2 size={16} />
                </button>
              </div>
              <div className="flex items-center gap-1.5 text-sm text-green-600 dark:text-green-400">
                <CheckCircle2 size={14} />
                <span>Connected</span>
              </div>
              {key.lastUsedAt && (
                <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                  Last used {new Date(key.lastUsedAt).toLocaleString()}
                </p>
              )}
            </div>
          ))}
          {FOOTAGE_PROVIDERS.filter((p) => !savedProviders.has(p)).length > 0 && (
            <button
              onClick={() => setIsFormOpen(true)}
              className="flex flex-col items-center justify-center gap-2 rounded-lg border-2 border-dashed border-slate-300 dark:border-slate-700 p-5 text-slate-500 dark:text-slate-400 hover:border-indigo-400 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors"
            >
              <Plus size={20} />
              <span className="text-sm font-medium">Add another provider</span>
            </button>
          )}
        </div>
      )}
    </div>
  );
}

export function ApiKeysPage() {
  return (
    <div className="w-full h-full flex flex-col bg-slate-50 dark:bg-slate-950 overflow-hidden">
      <main className="flex-1 overflow-auto">
        <div className="p-4 sm:p-6 lg:p-8 max-w-7xl mx-auto">
          <div className="flex items-center gap-4 mb-8">
            <Link to="/" className="p-2 hover:bg-slate-200 dark:hover:bg-slate-800 rounded-lg">
              <ChevronLeft size={24} className="text-slate-600 dark:text-slate-400" />
            </Link>
            <div className="min-w-0">
              <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 dark:text-white truncate">
                API Keys
              </h1>
              <p className="text-sm sm:text-base text-slate-600 dark:text-slate-400 mt-1">
                Connect your own AI tools and footage-search keys to generate content
              </p>
            </div>
          </div>

          <AiToolsSection />
          <FootageKeysSection />
        </div>
      </main>
    </div>
  );
}
