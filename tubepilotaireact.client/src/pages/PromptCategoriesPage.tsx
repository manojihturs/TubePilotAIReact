import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Edit2, Trash2, ChevronLeft } from 'lucide-react';
import { Link } from 'react-router-dom';
import { promptCategoryService, type PromptCategory, type CreatePromptCategoryDto } from '../services';
import { LoadingSkeleton } from '../components/LoadingCard';
import { ErrorState } from '../components/ErrorState';
import { useToast } from '../contexts/ToastContext';
import { logActivity } from '../services/logger';

export function PromptCategoriesPage() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState<CreatePromptCategoryDto>({ name: '', description: '' });

  const { data: categories = [], isLoading, isError, refetch } = useQuery({
    queryKey: ['prompt-categories'],
    queryFn: () => promptCategoryService.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreatePromptCategoryDto) => promptCategoryService.create(data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['prompt-categories'] });
      showToast(`Category "${variables.name}" created`, 'success');
      logActivity({ action: 'create', entity: 'PromptCategory', label: `Created category "${variables.name}"`, status: 'success' });
      resetForm();
    },
    onError: (err: unknown, variables) => {
      showToast(`Failed to create category "${variables.name}"`, 'error');
      logActivity({ action: 'create', entity: 'PromptCategory', label: `Create category "${variables.name}"`, status: 'error', detail: String(err) });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreatePromptCategoryDto }) =>
      promptCategoryService.update(id, { id, ...data }),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['prompt-categories'] });
      showToast(`Category "${variables.data.name}" updated`, 'success');
      logActivity({ action: 'update', entity: 'PromptCategory', label: `Updated category "${variables.data.name}"`, status: 'success' });
      resetForm();
    },
    onError: (err: unknown, variables) => {
      showToast(`Failed to update category "${variables.data.name}"`, 'error');
      logActivity({ action: 'update', entity: 'PromptCategory', label: `Update category "${variables.data.name}"`, status: 'error', detail: String(err) });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => promptCategoryService.delete(id),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['prompt-categories'] });
      const name = categories.find((c) => c.id === id)?.name ?? id;
      showToast(`Category "${name}" deleted`, 'success');
      logActivity({ action: 'delete', entity: 'PromptCategory', label: `Deleted category "${name}"`, status: 'success' });
    },
    onError: (err: unknown, id) => {
      const name = categories.find((c) => c.id === id)?.name ?? id;
      showToast(`Failed to delete category "${name}"`, 'error');
      logActivity({ action: 'delete', entity: 'PromptCategory', label: `Delete category "${name}"`, status: 'error', detail: String(err) });
    },
  });

  const resetForm = () => {
    setFormData({ name: '', description: '' });
    setEditingId(null);
    setIsFormOpen(false);
  };

  const handleEdit = (category: PromptCategory) => {
    setFormData({ name: category.name, description: category.description || '' });
    setEditingId(category.id);
    setIsFormOpen(true);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name.trim()) return;

    if (editingId) {
      updateMutation.mutate({ id: editingId, data: formData });
    } else {
      createMutation.mutate(formData);
    }
  };

  if (isLoading) return <LoadingSkeleton />;

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
                Prompt Categories
              </h1>
              <p className="text-sm sm:text-base text-slate-600 dark:text-slate-400 mt-1">
                Manage your content categories
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
            <span className="hidden sm:inline">New Category</span>
            <span className="sm:hidden">New</span>
          </button>
        </div>

        {/* Form */}
        {isFormOpen && (
          <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6 mb-8">
            <h2 className="text-xl font-bold text-slate-900 dark:text-white mb-4">
              {editingId ? 'Edit Category' : 'Create New Category'}
            </h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  Category Name *
                </label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="e.g., Marketing, Social Media, Branding"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  Description
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="Describe this category..."
                  rows={3}
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
        {isError && (
          <ErrorState
            message="Failed to load categories"
            onRetry={() => refetch()}
          />
        )}

        {/* Categories List */}
        {categories.length === 0 ? (
          <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-12 text-center">
            <p className="text-slate-600 dark:text-slate-400 mb-4">No categories yet</p>
            <button
              onClick={() => {
                resetForm();
                setIsFormOpen(true);
              }}
              className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              <Plus size={20} />
              <span>Create First Category</span>
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {categories.map((category) => (
              <div
                key={category.id}
                className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6 hover:shadow-lg transition-shadow"
              >
                <div className="flex items-start justify-between mb-4">
                  <h3 className="text-lg font-bold text-slate-900 dark:text-white">
                    {category.name}
                  </h3>
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleEdit(category)}
                      className="p-2 text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 rounded-lg transition-colors"
                      title="Edit"
                    >
                      <Edit2 size={18} />
                    </button>
                    <button
                      onClick={() => {
                        if (confirm('Are you sure?')) {
                          deleteMutation.mutate(category.id);
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
                {category.description && (
                  <p className="text-sm text-slate-600 dark:text-slate-400">
                    {category.description}
                  </p>
                )}
              </div>
            ))}
          </div>
        )}
        </div>
      </main>
    </div>
  );
}
