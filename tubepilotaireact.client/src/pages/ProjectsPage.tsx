import { useQuery } from '@tanstack/react-query';
import { Plus, ChevronLeft, Clapperboard, ArrowRight } from 'lucide-react';
import { Link } from 'react-router-dom';
import { projectService } from '../services';
import { LoadingSkeleton } from '../components/LoadingCard';
import { ErrorState } from '../components/ErrorState';

export function ProjectsPage() {
  const { data: projects = [], isLoading, isError, refetch } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectService.getAll(),
  });

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
                  Projects
                </h1>
                <p className="text-sm sm:text-base text-slate-600 dark:text-slate-400 mt-1">
                  Review generated projects — images, thumbnails, and video rendering all happen here
                </p>
              </div>
            </div>
            <Link
              to="/generate"
              className="flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors whitespace-nowrap flex-shrink-0"
            >
              <Plus size={20} />
              <span className="hidden sm:inline">New Project</span>
              <span className="sm:hidden">New</span>
            </Link>
          </div>

          {isError && <ErrorState message="Failed to load projects" onRetry={() => refetch()} />}

          {projects.length === 0 ? (
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-12 text-center">
              <Clapperboard className="mx-auto text-slate-400 mb-3" size={32} />
              <p className="text-slate-600 dark:text-slate-400 mb-4">No projects yet</p>
              <Link
                to="/generate"
                className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
              >
                <Plus size={20} />
                <span>Generate Your First Project</span>
              </Link>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {projects.map((project) => (
                <Link
                  key={project.id}
                  to={`/projects/${project.id}`}
                  className="group bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-5 hover:shadow-lg hover:border-indigo-300 dark:hover:border-indigo-700 transition-all"
                >
                  <div className="flex items-start justify-between mb-3">
                    <div className="w-10 h-10 rounded-lg bg-indigo-50 dark:bg-indigo-500/10 flex items-center justify-center">
                      <Clapperboard size={20} className="text-indigo-600 dark:text-indigo-400" />
                    </div>
                    <ArrowRight size={18} className="text-slate-300 dark:text-slate-600 group-hover:text-indigo-500 group-hover:translate-x-0.5 transition-all" />
                  </div>
                  <h3 className="font-bold text-slate-900 dark:text-white line-clamp-2">{project.title}</h3>
                  <p className="text-xs text-slate-500 dark:text-slate-500 mt-2">
                    {new Date(project.createdAt).toLocaleDateString()}
                  </p>
                </Link>
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
