import { AlertCircle, RotateCcw } from 'lucide-react';

interface ErrorStateProps {
  message?: string;
  description?: string;
  onRetry?: () => void;
  fullPage?: boolean;
}

export function ErrorState({
  message = 'Something went wrong',
  description = 'Please try again or contact support if the problem persists',
  onRetry,
  fullPage = false,
}: ErrorStateProps) {
  const containerClass = fullPage
    ? 'w-full h-full flex flex-col bg-gray-50 dark:bg-gray-950 items-center justify-center p-4'
    : 'bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-6';

  return (
    <div className={containerClass}>
      <div className="flex flex-col items-center gap-4 text-center max-w-md">
        <AlertCircle size={48} className="text-red-600 dark:text-red-400" />
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
          {message}
        </h3>
        <p className="text-sm text-gray-600 dark:text-gray-400">
          {description}
        </p>
        {onRetry && (
          <button
            onClick={onRetry}
            className="flex items-center gap-2 mt-4 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors"
          >
            <RotateCcw size={18} />
            <span>Try Again</span>
          </button>
        )}
      </div>
    </div>
  );
}

export function EmptyState({
  icon: Icon,
  title = 'No data found',
  description = 'Get started by creating your first item',
  action,
  fullPage = false,
}: {
  icon?: React.ElementType;
  title?: string;
  description?: string;
  action?: {
    label: string;
    onClick: () => void;
  };
  fullPage?: boolean;
}) {
  const containerClass = fullPage
    ? 'w-full h-full flex flex-col bg-gray-50 dark:bg-gray-950 items-center justify-center p-4'
    : 'bg-white dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-800 p-12';

  return (
    <div className={containerClass}>
      <div className="flex flex-col items-center gap-4 text-center max-w-md">
        {Icon && <Icon size={48} className="text-gray-400 opacity-50" />}
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
          {title}
        </h3>
        <p className="text-sm text-gray-600 dark:text-gray-400">
          {description}
        </p>
        {action && (
          <button
            onClick={action.onClick}
            className="mt-4 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
          >
            {action.label}
          </button>
        )}
      </div>
    </div>
  );
}
