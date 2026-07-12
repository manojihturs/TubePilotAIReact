import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ChevronLeft, KeyRound, Plus, Trash2, CheckCircle2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { aiProviderService, AI_PROVIDERS } from '../services';
import { LoadingSkeleton } from '../components/LoadingCard';
import { ErrorState } from '../components/ErrorState';
import { useToast } from '../contexts/ToastContext';
import { logActivity } from '../services/logger';

export function ApiKeysPage() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [providerName, setProviderName] = useState<string>(AI_PROVIDERS[0]);
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

  const savedProviders = new Set(keys.map((k) => k.providerName));

  return (
    <div className="w-full h-full flex flex-col bg-slate-50 dark:bg-slate-950 overflow-hidden">
      <main className="flex-1 overflow-auto">
        <div className="p-4 sm:p-6 lg:p-8 max-w-7xl mx-auto">
          {/* Header */}
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
            <div className="flex items-center gap-4">
              <Link to="/" className="p-2 hover:bg-slate-200 dark:hover:bg-slate-800 rounded-lg">
                <ChevronLeft size={24} className="text-slate-600 dark:text-slate-400" />
              </Link>
              <div className="min-w-0">
                <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 dark:text-white truncate">
                  AI Provider API Keys
                </h1>
                <p className="text-sm sm:text-base text-slate-600 dark:text-slate-400 mt-1">
                  Connect your own AI provider keys to generate content
                </p>
              </div>
            </div>
            <button
              onClick={() => setIsFormOpen(true)}
              className="flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors whitespace-nowrap flex-shrink-0"
            >
              <Plus size={20} />
              <span className="hidden sm:inline">Add API Key</span>
              <span className="sm:hidden">Add</span>
            </button>
          </div>

          {/* Form */}
          {isFormOpen && (
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6 mb-8">
              <h2 className="text-xl font-bold text-slate-900 dark:text-white mb-4">Add API Key</h2>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                    Provider *
                  </label>
                  <select
                    required
                    value={providerName}
                    onChange={(e) => setProviderName(e.target.value)}
                    className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  >
                    {AI_PROVIDERS.map((provider) => (
                      <option key={provider} value={provider}>
                        {provider}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                    API Key *
                  </label>
                  <input
                    type="password"
                    required
                    value={apiKey}
                    onChange={(e) => setApiKey(e.target.value)}
                    className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono text-sm"
                    placeholder="sk-ant-..."
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

          {/* Error State */}
          {isError && <ErrorState message="Failed to load API keys" onRetry={() => refetch()} />}

          {/* Keys List */}
          {keys.length === 0 ? (
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-12 text-center">
              <KeyRound className="mx-auto text-slate-400 mb-3" size={32} />
              <p className="text-slate-600 dark:text-slate-400 mb-4">No API keys connected yet</p>
              <button
                onClick={() => setIsFormOpen(true)}
                className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
              >
                <Plus size={20} />
                <span>Add API Key</span>
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {keys.map((key) => (
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
              {AI_PROVIDERS.filter((p) => !savedProviders.has(p)).length > 0 && (
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
      </main>
    </div>
  );
}
