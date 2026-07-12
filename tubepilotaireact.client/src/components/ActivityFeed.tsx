import { ChevronRight, Clock, Sparkles, Hash } from 'lucide-react';

interface ActivityItem {
  id: number;
  title: string;
  category?: string;
  timestamp: Date | string;
  type: 'prompt' | 'category' | 'variable';
  action: 'created' | 'updated' | 'used';
  description?: string;
}

interface ActivityFeedProps {
  items: ActivityItem[];
  isLoading?: boolean;
  onViewAll?: () => void;
  maxItems?: number;
}

const getActionColor = (action: string) => {
  switch (action) {
    case 'created':
      return 'bg-green-50 dark:bg-green-900/20 text-green-700 dark:text-green-300';
    case 'updated':
      return 'bg-indigo-50 dark:bg-indigo-900/20 text-indigo-700 dark:text-indigo-300';
    case 'used':
      return 'bg-purple-50 dark:bg-purple-900/20 text-purple-700 dark:text-purple-300';
    default:
      return 'bg-slate-50 dark:bg-slate-900/20 text-slate-700 dark:text-slate-300';
  }
};

const getTypeIcon = (type: string) => {
  switch (type) {
    case 'prompt':
      return <Sparkles size={16} />;
    case 'category':
      return <Hash size={16} />;
    case 'variable':
      return <Sparkles size={16} />;
    default:
      return <Clock size={16} />;
  }
};

const formatTime = (date: Date | string) => {
  const d = typeof date === 'string' ? new Date(date) : date;
  const now = new Date();
  const diffMs = now.getTime() - d.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return 'just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  return d.toLocaleDateString();
};

export function ActivityFeed({
  items,
  isLoading = false,
  onViewAll,
  maxItems = 5,
}: ActivityFeedProps) {
  const displayItems = items.slice(0, maxItems);
  const hasMore = items.length > maxItems;

  if (isLoading) {
    return (
      <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 overflow-hidden">
        <div className="p-6 border-b border-slate-200 dark:border-slate-800">
          <h3 className="text-lg md:text-xl font-bold text-slate-900 dark:text-white">
            Recent Activity
          </h3>
        </div>
        <div className="divide-y divide-slate-200 dark:divide-slate-800">
          {[...Array(3)].map((_, i) => (
            <div key={i} className="p-4 md:p-6 animate-pulse">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-slate-200 dark:bg-slate-700 rounded-full" />
                <div className="flex-1">
                  <div className="h-4 w-3/4 bg-slate-200 dark:bg-slate-700 rounded mb-2" />
                  <div className="h-3 w-1/2 bg-slate-200 dark:bg-slate-700 rounded" />
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (displayItems.length === 0) {
    return (
      <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 overflow-hidden">
        <div className="p-6 border-b border-slate-200 dark:border-slate-800">
          <h3 className="text-lg md:text-xl font-bold text-slate-900 dark:text-white">
            Recent Activity
          </h3>
        </div>
        <div className="p-12 text-center">
          <Clock size={48} className="mx-auto mb-4 text-slate-400 opacity-50" />
          <p className="text-slate-600 dark:text-slate-400">
            No recent activity yet. Create a prompt to get started!
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 overflow-hidden shadow-sm hover:shadow-md transition-shadow">
      <div className="p-4 md:p-6 border-b border-slate-200 dark:border-slate-800 flex items-center justify-between">
        <div>
          <h3 className="text-lg md:text-xl font-bold text-slate-900 dark:text-white">
            Recent Activity
          </h3>
          <p className="text-xs md:text-sm text-slate-600 dark:text-slate-400 mt-1">
            Latest changes to your prompts and content
          </p>
        </div>
      </div>

      <div className="divide-y divide-slate-200 dark:divide-slate-800">
        {displayItems.map((item) => (
          <div
            key={item.id}
            className="p-4 md:p-6 hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors"
          >
            <div className="flex items-start gap-3 md:gap-4">
              {/* Icon */}
              <div className="flex-shrink-0 mt-1">
                <div className="w-10 h-10 rounded-full bg-indigo-50 dark:bg-indigo-900/20 flex items-center justify-center text-indigo-600 dark:text-indigo-400">
                  {getTypeIcon(item.type)}
                </div>
              </div>

              {/* Content */}
              <div className="flex-1 min-w-0">
                <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-1 sm:gap-2">
                  <div className="min-w-0">
                    <p className="text-sm md:text-base font-semibold text-slate-900 dark:text-white truncate">
                      {item.title}
                    </p>
                    {item.description && (
                      <p className="text-xs md:text-sm text-slate-600 dark:text-slate-400 truncate mt-1">
                        {item.description}
                      </p>
                    )}
                  </div>
                  <span className={`text-xs md:text-sm font-medium px-2 py-1 rounded-md flex-shrink-0 ${getActionColor(item.action)}`}>
                    {item.action.charAt(0).toUpperCase() + item.action.slice(1)}
                  </span>
                </div>
                <div className="flex items-center gap-2 mt-2 text-xs text-slate-500 dark:text-slate-400">
                  {item.category && (
                    <>
                      <Hash size={12} />
                      <span>{item.category}</span>
                    </>
                  )}
                  <span className="flex items-center gap-1">
                    <Clock size={12} />
                    {formatTime(item.timestamp)}
                  </span>
                </div>
              </div>

              {/* Arrow */}
              <div className="flex-shrink-0 text-slate-400 hidden sm:block">
                <ChevronRight size={20} />
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* View All Button */}
      {hasMore && (
        <div className="p-4 md:p-6 bg-slate-50 dark:bg-slate-800/50 border-t border-slate-200 dark:border-slate-800">
          <button
            onClick={onViewAll}
            className="w-full flex items-center justify-center gap-2 text-sm md:text-base font-semibold text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 transition-colors"
          >
            View All Activity
            <ChevronRight size={16} />
          </button>
        </div>
      )}
    </div>
  );
}
