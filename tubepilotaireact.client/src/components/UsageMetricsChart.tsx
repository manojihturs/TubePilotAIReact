import { TrendingUp } from 'lucide-react';

interface MetricData {
  date: string;
  value: number;
  label: string;
}

interface UsageMetricsChartProps {
  data?: MetricData[];
  title?: string;
  subtitle?: string;
  isLoading?: boolean;
  timeRange?: '7d' | '30d' | '90d';
  onTimeRangeChange?: (range: '7d' | '30d' | '90d') => void;
}



export function UsageMetricsChart({
  data = [],
  title = 'Usage Metrics',
  subtitle = 'Prompt usage over time',
  isLoading = false,
  timeRange = '7d',
  onTimeRangeChange,
}: UsageMetricsChartProps) {
  // Require data to be passed in - no mock data generation
  const chartData = data;
  const maxValue = chartData.length > 0 ? Math.max(...chartData.map((d) => d.value), 100) : 100;

  if (isLoading) {
    return (
      <div className="bg-white dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-800 p-6 animate-pulse">
        <div className="h-6 w-40 bg-gray-200 dark:bg-gray-700 rounded mb-6" />
        <div className="space-y-4">
          {[...Array(5)].map((_, i) => (
            <div key={i} className="flex items-center gap-3">
              <div className="w-12 h-12 bg-gray-200 dark:bg-gray-700 rounded" />
              <div className="flex-1">
                <div className="h-4 w-3/4 bg-gray-200 dark:bg-gray-700 rounded" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-800 p-6 shadow-sm hover:shadow-md transition-shadow">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
        <div>
          <h3 className="text-lg md:text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <TrendingUp size={24} className="text-blue-600" />
            {title}
          </h3>
          <p className="text-xs md:text-sm text-gray-600 dark:text-gray-400 mt-1">
            {subtitle}
          </p>
        </div>

        {/* Time Range Selector */}
        <div className="flex gap-2">
          {(['7d', '30d', '90d'] as const).map((range) => (
            <button
              key={range}
              onClick={() => onTimeRangeChange?.(range)}
              className={`px-3 py-1 rounded-md text-xs md:text-sm font-medium transition-colors ${
                timeRange === range
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700'
              }`}
            >
              {range === '7d' ? '7 Days' : range === '30d' ? '30 Days' : '90 Days'}
            </button>
          ))}
        </div>
      </div>

      {/* Chart Area */}
      {chartData.length === 0 ? (
        <div className="text-center py-12">
          <TrendingUp size={48} className="mx-auto mb-4 text-gray-400 opacity-50" />
          <p className="text-gray-600 dark:text-gray-400">No data available for the selected period</p>
          <p className="text-sm text-gray-500 dark:text-gray-500 mt-2">Create some prompts to see usage metrics</p>
        </div>
      ) : (
        <div className="space-y-4">
          {/* Simple bar chart */}
          <div className="space-y-3">
            {chartData.map((item, index) => (
            <div key={index} className="flex items-end gap-2">
              {/* Date label */}
              <div className="w-12 text-right">
                <span className="text-xs text-gray-500 dark:text-gray-500">{item.date}</span>
              </div>

              {/* Bar */}
              <div className="flex-1 flex items-end gap-2">
                <div className="w-full flex items-end gap-1 h-12 bg-gray-100 dark:bg-gray-800 rounded-t p-2">
                  <div
                    className="flex-1 bg-gradient-to-t from-blue-600 to-blue-400 rounded-t transition-all duration-300 hover:from-blue-700 hover:to-blue-500"
                    style={{
                      height: `${(item.value / maxValue) * 100}%`,
                    }}
                  />
                </div>

                {/* Value label */}
                <span className="text-xs md:text-sm font-semibold text-gray-900 dark:text-white w-12 text-right">
                  {item.value}
                </span>
              </div>
            </div>
          ))}
        </div>

        {/* Stats Summary */}
        <div className="grid grid-cols-3 gap-3 pt-4 border-t border-gray-200 dark:border-gray-800 mt-6">
          <div className="text-center p-3 bg-gray-50 dark:bg-gray-800/50 rounded-lg">
            <p className="text-xs text-gray-600 dark:text-gray-400 mb-1">Total</p>
            <p className="text-lg md:text-xl font-bold text-gray-900 dark:text-white">
              {chartData.reduce((sum, item) => sum + item.value, 0)}
            </p>
          </div>
          <div className="text-center p-3 bg-gray-50 dark:bg-gray-800/50 rounded-lg">
            <p className="text-xs text-gray-600 dark:text-gray-400 mb-1">Average</p>
            <p className="text-lg md:text-xl font-bold text-gray-900 dark:text-white">
              {Math.round(chartData.reduce((sum, item) => sum + item.value, 0) / chartData.length)}
            </p>
          </div>
          <div className="text-center p-3 bg-gray-50 dark:bg-gray-800/50 rounded-lg">
            <p className="text-xs text-gray-600 dark:text-gray-400 mb-1">Peak</p>
            <p className="text-lg md:text-xl font-bold text-gray-900 dark:text-white">
              {Math.max(...chartData.map((d) => d.value))}
            </p>
          </div>
        </div>
      </div>
      )}

      {/* Info */}
      <div className="mt-6 p-3 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
        <p className="text-xs md:text-sm text-blue-800 dark:text-blue-300">
          💡 <strong>Pro Tip:</strong> Use this chart to track your content generation activity and identify trends
          in your prompt usage.
        </p>
      </div>
    </div>
  );
}
