import { Plus, BookOpen, Layers, Code, ArrowRight } from 'lucide-react';
import { Link } from 'react-router-dom';

interface QuickAction {
  label: string;
  description: string;
  icon: React.ElementType;
  path: string;
  color: 'blue' | 'purple' | 'green' | 'pink';
}

interface QuickActionPanelProps {
  totalPrompts?: number;
  totalCategories?: number;
  totalVariables?: number;
  isLoading?: boolean;
}

const getColorClasses = (color: string) => {
  switch (color) {
    case 'blue':
      return 'bg-indigo-50 dark:bg-indigo-900/20 text-indigo-600 dark:text-indigo-400 hover:bg-indigo-100 dark:hover:bg-indigo-900/30';
    case 'purple':
      return 'bg-purple-50 dark:bg-purple-900/20 text-purple-600 dark:text-purple-400 hover:bg-purple-100 dark:hover:bg-purple-900/30';
    case 'green':
      return 'bg-green-50 dark:bg-green-900/20 text-green-600 dark:text-green-400 hover:bg-green-100 dark:hover:bg-green-900/30';
    case 'pink':
      return 'bg-pink-50 dark:bg-pink-900/20 text-pink-600 dark:text-pink-400 hover:bg-pink-100 dark:hover:bg-pink-900/30';
    default:
      return 'bg-slate-50 dark:bg-slate-900/20 text-slate-600 dark:text-slate-400';
  }
};

export function QuickActionPanel({
  totalPrompts = 0,
  totalCategories = 0,
  totalVariables = 0,
  isLoading = false,
}: QuickActionPanelProps) {
  const actions: QuickAction[] = [
    {
      label: 'New Prompt',
      description: 'Create a new content prompt',
      icon: Plus,
      path: '/prompts',
      color: 'blue',
    },
    {
      label: 'Manage Categories',
      description: 'Organize your prompt categories',
      icon: Layers,
      path: '/prompt-categories',
      color: 'purple',
    },
    {
      label: 'Define Variables',
      description: 'Create dynamic content variables',
      icon: Code,
      path: '/prompt-variables',
      color: 'green',
    },
  ];

  const stats = [
    { label: 'Active Prompts', value: totalPrompts, icon: BookOpen },
    { label: 'Categories', value: totalCategories, icon: Layers },
    { label: 'Variables', value: totalVariables, icon: Code },
  ];

  if (isLoading) {
    return (
      <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6 animate-pulse">
        <div className="h-6 w-40 bg-slate-200 dark:bg-slate-700 rounded mb-6" />
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {[...Array(3)].map((_, i) => (
            <div key={i} className="h-32 bg-slate-200 dark:bg-slate-700 rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Quick Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <div
              key={stat.label}
              className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-4"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-xs md:text-sm text-slate-600 dark:text-slate-400">
                    {stat.label}
                  </p>
                  <p className="text-2xl md:text-3xl font-bold text-slate-900 dark:text-white mt-1">
                    {stat.value}
                  </p>
                </div>
                <div className="p-2 bg-indigo-50 dark:bg-indigo-900/20 rounded-lg">
                  <Icon size={20} className="text-indigo-600 dark:text-indigo-400" />
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Quick Actions */}
      <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 overflow-hidden">
        <div className="p-4 md:p-6 border-b border-slate-200 dark:border-slate-800">
          <h3 className="text-lg md:text-xl font-bold text-slate-900 dark:text-white">
            Quick Actions
          </h3>
          <p className="text-xs md:text-sm text-slate-600 dark:text-slate-400 mt-1">
            Get started with common tasks
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 p-4 md:p-6">
          {actions.map((action) => {
            const Icon = action.icon;
            const colorClasses = getColorClasses(action.color);
            return (
              <Link
                key={action.label}
                to={action.path}
                className={`group relative overflow-hidden rounded-lg p-4 md:p-5 transition-all duration-300 ${colorClasses}`}
              >
                {/* Background gradient on hover */}
                <div className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity" />

                <div className="relative flex flex-col h-full">
                  <div className="flex items-center justify-between mb-3">
                    <div className="w-10 h-10 rounded-lg bg-white/20 flex items-center justify-center group-hover:bg-white/30 transition-colors">
                      <Icon size={20} />
                    </div>
                    <ArrowRight
                      size={20}
                      className="opacity-0 group-hover:opacity-100 transform group-hover:translate-x-1 transition-all duration-300"
                    />
                  </div>

                  <h4 className="font-semibold text-sm md:text-base">{action.label}</h4>
                  <p className="text-xs md:text-sm opacity-80 mt-1">{action.description}</p>
                </div>
              </Link>
            );
          })}
        </div>

        {/* Suggested Next Steps */}
        <div className="p-4 md:p-6 bg-gradient-to-r from-indigo-50 to-indigo-50 dark:from-indigo-900/20 dark:to-indigo-900/20 border-t border-slate-200 dark:border-slate-800">
          <h4 className="font-semibold text-sm md:text-base text-slate-900 dark:text-white mb-2">
            💡 Suggested Next Steps
          </h4>
          <ul className="space-y-2 text-xs md:text-sm text-slate-700 dark:text-slate-300">
            {totalPrompts === 0 && (
              <li className="flex items-start gap-2">
                <span className="text-indigo-600 dark:text-indigo-400 font-bold">→</span>
                <span>Create your first prompt to start generating content</span>
              </li>
            )}
            {totalCategories === 0 && (
              <li className="flex items-start gap-2">
                <span className="text-indigo-600 dark:text-indigo-400 font-bold">→</span>
                <span>Organize content by creating categories (e.g., "Video Titles", "Instagram Captions")</span>
              </li>
            )}
            {totalVariables === 0 && (
              <li className="flex items-start gap-2">
                <span className="text-indigo-600 dark:text-indigo-400 font-bold">→</span>
                <span>Define variables to make your prompts dynamic and reusable</span>
              </li>
            )}
            {totalPrompts > 0 && totalCategories > 0 && totalVariables > 0 && (
              <li className="flex items-start gap-2">
                <span className="text-green-600 dark:text-green-400 font-bold">✓</span>
                <span>Great! You're all set. Start using prompts to generate content!</span>
              </li>
            )}
          </ul>
        </div>
      </div>
    </div>
  );
}
