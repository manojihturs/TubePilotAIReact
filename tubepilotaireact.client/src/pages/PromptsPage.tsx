import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Edit2, Trash2, ChevronLeft } from 'lucide-react';
import { Link } from 'react-router-dom';
import { promptService, promptCategoryService, type Prompt, type CreatePromptDto } from '../services';
import { LoadingSkeleton } from '../components/LoadingCard';
import { ErrorState } from '../components/ErrorState';
import { useToast } from '../contexts/ToastContext';
import { logActivity } from '../services/logger';

export function PromptsPage() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState<CreatePromptDto>({ name: '', promptText: '', categoryId: '' });

  const { data: prompts = [], isLoading: promptsLoading, isError: promptsError, refetch: refetchPrompts } = useQuery({
    queryKey: ['prompts'],
    queryFn: () => promptService.getAll(),
  });

  const { data: categories = [], isLoading: categoriesLoading } = useQuery({
    queryKey: ['prompt-categories'],
    queryFn: () => promptCategoryService.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreatePromptDto) => promptService.create(data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['prompts'] });
      showToast(`Prompt "${variables.name}" created`, 'success');
      logActivity({ action: 'create', entity: 'Prompt', label: `Created prompt "${variables.name}"`, status: 'success' });
      resetForm();
    },
    onError: (err: unknown, variables) => {
      showToast(`Failed to create prompt "${variables.name}"`, 'error');
      logActivity({ action: 'create', entity: 'Prompt', label: `Create prompt "${variables.name}"`, status: 'error', detail: String(err) });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreatePromptDto }) =>
      promptService.update(id, { id, ...data }),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['prompts'] });
      showToast(`Prompt "${variables.data.name}" updated`, 'success');
      logActivity({ action: 'update', entity: 'Prompt', label: `Updated prompt "${variables.data.name}"`, status: 'success' });
      resetForm();
    },
    onError: (err: unknown, variables) => {
      showToast(`Failed to update prompt "${variables.data.name}"`, 'error');
      logActivity({ action: 'update', entity: 'Prompt', label: `Update prompt "${variables.data.name}"`, status: 'error', detail: String(err) });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => promptService.delete(id),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['prompts'] });
      const name = prompts.find((p) => p.id === id)?.name ?? id;
      showToast(`Prompt "${name}" deleted`, 'success');
      logActivity({ action: 'delete', entity: 'Prompt', label: `Deleted prompt "${name}"`, status: 'success' });
    },
    onError: (err: unknown, id) => {
      const name = prompts.find((p) => p.id === id)?.name ?? id;
      showToast(`Failed to delete prompt "${name}"`, 'error');
      logActivity({ action: 'delete', entity: 'Prompt', label: `Delete prompt "${name}"`, status: 'error', detail: String(err) });
    },
  });

  const resetForm = () => {
    setFormData({ name: '', promptText: '', categoryId: '' });
    setEditingId(null);
    setIsFormOpen(false);
  };

  const handleEdit = (prompt: Prompt) => {
    setFormData({ name: prompt.name, promptText: prompt.promptText, categoryId: prompt.categoryId });
    setEditingId(prompt.id);
    setIsFormOpen(true);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name.trim() || !formData.promptText.trim() || !formData.categoryId) return;

    if (editingId) {
      updateMutation.mutate({ id: editingId, data: formData });
    } else {
      createMutation.mutate(formData);
    }
  };

  const isLoading = promptsLoading || categoriesLoading;

  if (isLoading) return <LoadingSkeleton />;

  const getCategoryName = (categoryId: string) => {
    return categories.find(c => c.id === categoryId)?.name || 'Unknown';
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
                Prompts
              </h1>
              <p className="text-sm sm:text-base text-slate-600 dark:text-slate-400 mt-1">
                Create and manage prompts for content generation
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
            <span className="hidden sm:inline">New Prompt</span>
            <span className="sm:hidden">New</span>
          </button>
        </div>

        {/* Form */}
        {isFormOpen && (
          <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6 mb-8">
            <h2 className="text-xl font-bold text-slate-900 dark:text-white mb-4">
              {editingId ? 'Edit Prompt' : 'Create New Prompt'}
            </h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  Category *
                </label>
                <select
                  required
                  value={formData.categoryId}
                  onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="">Select a category</option>
                  {categories.map((cat) => (
                    <option key={cat.id} value={cat.id}>
                      {cat.name}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  Prompt Name *
                </label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="e.g., Instagram Caption Generator"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  Prompt Text *
                </label>
                <textarea
                  required
                  value={formData.promptText}
                  onChange={(e) => setFormData({ ...formData, promptText: e.target.value })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono text-sm"
                  placeholder="Write your prompt here. Use {variable_name} for dynamic variables..."
                  rows={6}
                />
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
        {promptsError && (
          <ErrorState
            message="Failed to load prompts"
            onRetry={() => refetchPrompts()}
          />
        )}

        {/* Prompts List */}
        {prompts.length === 0 ? (
          <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-12 text-center">
            <p className="text-slate-600 dark:text-slate-400 mb-4">No prompts yet</p>
            <button
              onClick={() => {
                resetForm();
                setIsFormOpen(true);
              }}
              className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              <Plus size={20} />
              <span>Create First Prompt</span>
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            {prompts.map((prompt) => (
              <div
                key={prompt.id}
                className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6 hover:shadow-lg transition-shadow"
              >
                <div className="flex items-start justify-between mb-3">
                  <div className="flex-1">
                    <h3 className="text-lg font-bold text-slate-900 dark:text-white">
                      {prompt.name}
                    </h3>
                    <p className="text-sm text-indigo-600 dark:text-indigo-400 mt-1">
                      Category: {getCategoryName(prompt.categoryId)}
                    </p>
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleEdit(prompt)}
                      className="p-2 text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 rounded-lg transition-colors"
                      title="Edit"
                    >
                      <Edit2 size={18} />
                    </button>
                    <button
                      onClick={() => {
                        if (confirm('Are you sure?')) {
                          deleteMutation.mutate(prompt.id);
                        }
                      }}
                      disabled={deleteMutation.isPending}
                      className="p-2 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors disabled:opacity-50"
                      title="Delete"
                    >
                      <Trash2 size={18} />
                    </button>
                  </div>
                </div>
                <p className="text-sm text-slate-600 dark:text-slate-400 line-clamp-3 font-mono bg-slate-50 dark:bg-slate-800 p-3 rounded">
                  {prompt.promptText}
                </p>
              </div>
            ))}
          </div>
        )}
        </div>
      </main>
    </div>
  );
}
