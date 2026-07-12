import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Edit2, Trash2, ChevronLeft } from 'lucide-react';
import { Link } from 'react-router-dom';
import { promptVariableService, promptService, type CreatePromptVariableDto, type PromptVariable } from '../services';
import { LoadingSkeleton } from '../components/LoadingCard';
import { ErrorState } from '../components/ErrorState';
import { useToast } from '../contexts/ToastContext';
import { logActivity } from '../services/logger';

export function PromptVariablesPage() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState<CreatePromptVariableDto>({
    name: '',
    promptId: '',
    placeholder: '',
    dataType: 'text',
    defaultValue: '',
    isRequired: true,
  });

  const { data: variables = [], isLoading: variablesLoading, isError: variablesError, refetch: refetchVariables } = useQuery({
    queryKey: ['prompt-variables'],
    queryFn: () => promptVariableService.getAll(),
  });

  const { data: prompts = [], isLoading: promptsLoading } = useQuery({
    queryKey: ['prompts'],
    queryFn: () => promptService.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreatePromptVariableDto) => promptVariableService.create(data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['prompt-variables'] });
      showToast(`Variable "${variables.name}" created`, 'success');
      logActivity({ action: 'create', entity: 'PromptVariable', label: `Created variable "${variables.name}"`, status: 'success' });
      resetForm();
    },
    onError: (err: unknown, variables) => {
      showToast(`Failed to create variable "${variables.name}"`, 'error');
      logActivity({ action: 'create', entity: 'PromptVariable', label: `Create variable "${variables.name}"`, status: 'error', detail: String(err) });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreatePromptVariableDto }) =>
      promptVariableService.update(id, { id, ...data }),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['prompt-variables'] });
      showToast(`Variable "${variables.data.name}" updated`, 'success');
      logActivity({ action: 'update', entity: 'PromptVariable', label: `Updated variable "${variables.data.name}"`, status: 'success' });
      resetForm();
    },
    onError: (err: unknown, variables) => {
      showToast(`Failed to update variable "${variables.data.name}"`, 'error');
      logActivity({ action: 'update', entity: 'PromptVariable', label: `Update variable "${variables.data.name}"`, status: 'error', detail: String(err) });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => promptVariableService.delete(id),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['prompt-variables'] });
      const name = variables.find((v) => v.id === id)?.name ?? id;
      showToast(`Variable "${name}" deleted`, 'success');
      logActivity({ action: 'delete', entity: 'PromptVariable', label: `Deleted variable "${name}"`, status: 'success' });
    },
    onError: (err: unknown, id) => {
      const name = variables.find((v) => v.id === id)?.name ?? id;
      showToast(`Failed to delete variable "${name}"`, 'error');
      logActivity({ action: 'delete', entity: 'PromptVariable', label: `Delete variable "${name}"`, status: 'error', detail: String(err) });
    },
  });

  const resetForm = () => {
    setFormData({
      name: '',
      promptId: '',
      placeholder: '',
      dataType: 'text',
      defaultValue: '',
      isRequired: true,
    });
    setEditingId(null);
    setIsFormOpen(false);
  };

  const handleEdit = (variable: PromptVariable) => {
    setFormData({
      name: variable.name,
      promptId: variable.promptId,
      placeholder: variable.placeholder,
      dataType: variable.dataType || 'text',
      defaultValue: variable.defaultValue || '',
      isRequired: variable.isRequired !== false,
    });
    setEditingId(variable.id);
    setIsFormOpen(true);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name.trim() || !formData.promptId || !formData.placeholder.trim()) return;

    if (editingId) {
      updateMutation.mutate({ id: editingId, data: formData });
    } else {
      createMutation.mutate(formData);
    }
  };

  const isLoading = variablesLoading || promptsLoading;

  if (isLoading) return <LoadingSkeleton />;

  const getPromptTitle = (promptId: string) => {
    return prompts.find(p => p.id === promptId)?.name || 'Unknown';
  };

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
                Prompt Variables
              </h1>
              <p className="text-sm sm:text-base text-slate-600 dark:text-slate-400 mt-1">
                Manage dynamic variables for your prompts
              </p>
            </div>
          </div>
          <button
            onClick={() => {
              resetForm();
              setIsFormOpen(true);
            }}
            className="flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors whitespace-nowrap flex-shrink-0"
          >
            <Plus size={20} />
            <span className="hidden sm:inline">New Variable</span>
            <span className="sm:hidden">New</span>
          </button>
        </div>

        {/* Form */}
        {isFormOpen && (
          <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6 mb-8">
            <h2 className="text-xl font-bold text-slate-900 dark:text-white mb-4">
              {editingId ? 'Edit Variable' : 'Create New Variable'}
            </h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                    Associated Prompt *
                  </label>
                  <select
                    required
                    value={formData.promptId}
                    onChange={(e) => setFormData({ ...formData, promptId: e.target.value })}
                    className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  >
                    <option value="">Select a prompt</option>
                    {prompts.map((p) => (
                      <option key={p.id} value={p.id}>
                        {p.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                    Variable Type *
                  </label>
                  <select
                    value={formData.dataType}
                    onChange={(e) => setFormData({ ...formData, dataType: e.target.value })}
                    className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  >
                    <option value="text">Text</option>
                    <option value="number">Number</option>
                    <option value="date">Date</option>
                    <option value="email">Email</option>
                    <option value="url">URL</option>
                    <option value="select">Select</option>
                  </select>
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  Variable Name *
                </label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value, placeholder: e.target.value })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="e.g., topic, tone, audience"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  Default Value
                </label>
                <input
                  type="text"
                  value={formData.defaultValue}
                  onChange={(e) => setFormData({ ...formData, defaultValue: e.target.value })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="Optional default value"
                />
              </div>
              <div>
                <label className="flex items-center gap-2 text-sm font-medium text-slate-700 dark:text-slate-300">
                  <input
                    type="checkbox"
                    checked={formData.isRequired}
                    onChange={(e) => setFormData({ ...formData, isRequired: e.target.checked })}
                    className="w-4 h-4 rounded border-slate-300"
                  />
                  Required field
                </label>
              </div>
              <div className="flex gap-2">
                <button
                  type="submit"
                  disabled={createMutation.isPending || updateMutation.isPending}
                  className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors"
                >
                  {createMutation.isPending || updateMutation.isPending ? 'Saving...' : 'Save'}
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

        {/* Error State */}
        {variablesError && (
          <ErrorState
            message="Failed to load variables"
            onRetry={() => refetchVariables()}
          />
        )}

        {/* Variables List */}
        {variables.length === 0 ? (
          <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-12 text-center">
            <p className="text-slate-600 dark:text-slate-400 mb-4">No variables yet</p>
            <button
              onClick={() => {
                resetForm();
                setIsFormOpen(true);
              }}
              className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              <Plus size={20} />
              <span>Create First Variable</span>
            </button>
          </div>
        ) : (
          <>
            {/* Mobile: stacked cards */}
            <div className="grid grid-cols-1 gap-3 sm:hidden">
              {variables.map((variable) => (
                <div
                  key={variable.id}
                  className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-4"
                >
                  <div className="flex items-start justify-between gap-2 mb-2">
                    <span className="font-mono text-sm font-semibold text-slate-900 dark:text-white break-all">
                      {variable.name}
                    </span>
                    <div className="flex gap-1 flex-shrink-0">
                      <button
                        onClick={() => handleEdit(variable)}
                        className="text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 p-1.5 rounded transition-colors"
                        title="Edit"
                      >
                        <Edit2 size={16} />
                      </button>
                      <button
                        onClick={() => {
                          if (confirm('Are you sure?')) {
                            deleteMutation.mutate(variable.id);
                          }
                        }}
                        disabled={deleteMutation.isPending}
                        className="text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 p-1.5 rounded transition-colors disabled:opacity-50"
                        title="Delete"
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </div>
                  <div className="flex flex-wrap items-center gap-2 text-sm text-slate-600 dark:text-slate-400">
                    <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400">
                      {variable.dataType || 'text'}
                    </span>
                    <span>{getPromptTitle(variable.promptId)}</span>
                    {variable.isRequired ? (
                      <span className="text-green-600 dark:text-green-400 text-xs">Required</span>
                    ) : (
                      <span className="text-slate-400 text-xs">Optional</span>
                    )}
                  </div>
                </div>
              ))}
            </div>

            {/* Desktop: table */}
            <div className="hidden sm:block overflow-x-auto bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800">
              <table className="w-full">
                <thead className="bg-slate-100 dark:bg-slate-800 border-b border-slate-200 dark:border-slate-700">
                  <tr>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-slate-900 dark:text-white">Name</th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-slate-900 dark:text-white">Type</th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-slate-900 dark:text-white">Prompt</th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-slate-900 dark:text-white">Required</th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-slate-900 dark:text-white">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                  {variables.map((variable) => (
                    <tr key={variable.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors">
                      <td className="px-6 py-3 text-sm text-slate-900 dark:text-white font-mono">
                        {variable.name}
                      </td>
                      <td className="px-6 py-3 text-sm">
                        <span className="px-2 py-1 rounded-full text-xs font-medium bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400">
                          {variable.dataType || 'text'}
                        </span>
                      </td>
                      <td className="px-6 py-3 text-sm text-slate-600 dark:text-slate-400">
                        {getPromptTitle(variable.promptId)}
                      </td>
                      <td className="px-6 py-3 text-sm">
                        {variable.isRequired ? (
                          <span className="text-green-600 dark:text-green-400">✓</span>
                        ) : (
                          <span className="text-slate-400">○</span>
                        )}
                      </td>
                      <td className="px-6 py-3 text-sm">
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleEdit(variable)}
                            className="text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 p-1 rounded transition-colors"
                            title="Edit"
                          >
                            <Edit2 size={16} />
                          </button>
                          <button
                            onClick={() => {
                              if (confirm('Are you sure?')) {
                                deleteMutation.mutate(variable.id);
                              }
                            }}
                            disabled={deleteMutation.isPending}
                            className="text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 p-1 rounded transition-colors disabled:opacity-50"
                            title="Delete"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        )}
        </div>
      </main>
    </div>
  );
}
