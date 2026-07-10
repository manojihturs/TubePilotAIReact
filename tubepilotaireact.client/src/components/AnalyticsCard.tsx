import { TrendingUp, TrendingDown } from 'lucide-react';

interface AnalyticsCardProps {
  icon: React.ElementType;
  label: string;
  value: string | number;
  trend?: {
    direction: 'up' | 'down' | 'neutral';
    percentage: number;
    label: string;
  };
  description?: string;
  onClick?: () => void;
  className?: string;
}

export function AnalyticsCard({
  icon: Icon,
  label,
  value,
  trend,
  description,
  onClick,
  className = '',
}: AnalyticsCardProps) {
  const getTrendColor = () => {
    if (!trend) return '';
    switch (trend.direction) {
      case 'up':
        return 'text-green-600 dark:text-green-400';
      case 'down':
        return 'text-red-600 dark:text-red-400';
      default:
        return 'text-gray-600 dark:text-gray-400';
    }
  };

  const getTrendBgColor = () => {
    if (!trend) return '';
    switch (trend.direction) {
      case 'up':
        return 'bg-green-50 dark:bg-green-900/20';
      case 'down':
        return 'bg-red-50 dark:bg-red-900/20';
      default:
        return 'bg-gray-50 dark:bg-gray-800/20';
    }
  };

  const TrendIcon = trend?.direction === 'up' ? TrendingUp : TrendingDown;

  return (
    <div
      onClick={onClick}
      className={`bg-white dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-800 p-6 hover:shadow-lg transition-all duration-200 ${
        onClick ? 'cursor-pointer hover:scale-105' : ''
      } ${className}`}
    >
      {/* Header with icon */}
      <div className="flex items-start justify-between mb-4">
        <div>
          <p className="text-sm font-medium text-gray-600 dark:text-gray-400 mb-1">
            {label}
          </p>
          <h3 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white">
            {value}
          </h3>
        </div>
        <div className="p-3 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
          <Icon size={24} className="text-blue-600 dark:text-blue-400" />
        </div>
      </div>

      {/* Description */}
      {description && (
        <p className="text-xs md:text-sm text-gray-600 dark:text-gray-400 mb-3">
          {description}
        </p>
      )}

      {/* Trend indicator */}
      {trend && (
        <div className={`flex items-center gap-2 ${getTrendBgColor()} px-3 py-2 rounded-md w-fit`}>
          <TrendIcon size={16} className={getTrendColor()} />
          <span className={`text-sm font-semibold ${getTrendColor()}`}>
            {trend.direction === 'up' ? '+' : '-'}
            {trend.percentage}% {trend.label}
          </span>
        </div>
      )}
    </div>
  );
}
