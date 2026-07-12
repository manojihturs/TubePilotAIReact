import { Bell, Search, Settings, Sun, Moon } from 'lucide-react';
import { useTheme } from '../contexts/ThemeContext';

export function TopNav() {
  const { theme, toggleTheme } = useTheme();

  return (
    <header className="sticky top-0 h-16 bg-white dark:bg-slate-900 border-b border-slate-200 dark:border-slate-800 z-20 flex-shrink-0">
      <div className="flex items-center justify-between px-4 sm:px-6 lg:px-8 h-full gap-2">
        {/* Search */}
        <div className="hidden sm:flex flex-1 max-w-md">
          <div className="relative w-full">
            <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              placeholder="Search projects, prompts..."
              className="w-full pl-10 pr-4 py-2 rounded-lg bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 text-sm"
            />
          </div>
        </div>

        {/* Right Actions */}
        <div className="flex items-center gap-1 sm:gap-2 sm:ml-6 ml-auto">
          <button
            onClick={toggleTheme}
            className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-400"
            title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
            aria-label="Toggle color theme"
          >
            {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
          </button>

          <button className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 relative hidden sm:inline-flex">
            <Bell size={20} className="text-slate-600 dark:text-slate-400" />
            <span className="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full" />
          </button>

          <button className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 hidden sm:inline-flex">
            <Settings size={20} className="text-slate-600 dark:text-slate-400" />
          </button>

          <button className="flex items-center gap-2 px-2 sm:px-3 py-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800">
            <div className="w-8 h-8 bg-indigo-600 rounded-full flex items-center justify-center text-white text-sm font-bold flex-shrink-0">
              A
            </div>
          </button>
        </div>
      </div>
    </header>
  );
}
