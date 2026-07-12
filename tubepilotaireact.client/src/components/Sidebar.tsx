import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  Layers,
  BookOpen,
  Code,
  Menu,
  X,
  ChevronDown,
  Sparkles,
  KeyRound,
  Clapperboard,
} from 'lucide-react';

interface NavItem {
  label: string;
  path?: string;
  icon: React.ElementType;
  badge?: number;
  submenu?: NavItem[];
}

const navItems: NavItem[] = [
  {
    label: 'Dashboard',
    path: '/',
    icon: LayoutDashboard,
  },
  {
    label: 'Projects',
    path: '/projects',
    icon: Clapperboard,
  },
  {
    label: 'Content Management',
    icon: BookOpen,
    submenu: [
      {
        label: 'Prompts',
        path: '/prompts',
        icon: BookOpen,
      },
      {
        label: 'Categories',
        path: '/prompt-categories',
        icon: Layers,
      },
      {
        label: 'Variables',
        path: '/prompt-variables',
        icon: Code,
      },
    ],
  },
  {
    label: 'AI Generation',
    icon: Sparkles,
    submenu: [
      {
        label: 'Generate',
        path: '/generate',
        icon: Sparkles,
      },
      {
        label: 'API Keys',
        path: '/api-keys',
        icon: KeyRound,
      },
    ],
  },
];

export function Sidebar() {
  const [isOpen, setIsOpen] = useState(false);
  const [expandedMenu, setExpandedMenu] = useState<string | null>(null);
  const location = useLocation();

  const toggleMenu = (label: string) => {
    setExpandedMenu(expandedMenu === label ? null : label);
  };

  const isActive = (path?: string) => {
    if (!path) return false;
    return location.pathname === path;
  };

  const isMenuActive = (submenu?: NavItem[]) => {
    if (!submenu) return false;
    return submenu.some((item) => isActive(item.path));
  };

  const NavItemComponent = ({ item }: { item: NavItem }) => {
    const Icon = item.icon;
    const active = isActive(item.path) || isMenuActive(item.submenu);
    const hasSubmenu = item.submenu && item.submenu.length > 0;

    const baseClasses =
      'flex items-center justify-between px-4 py-3 rounded-lg transition-colors group relative';

    const activeClasses = active
      ? 'bg-indigo-50 dark:bg-indigo-500/10 text-indigo-700 dark:text-indigo-400 font-semibold shadow-sm ring-1 ring-indigo-100 dark:ring-indigo-500/20'
      : 'text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800/60';

    if (hasSubmenu) {
      return (
        <div>
          <button
            onClick={() => toggleMenu(item.label)}
            className={`${baseClasses} ${activeClasses} w-full`}
          >
            <div className="flex items-center gap-3">
              <Icon size={20} />
              <span className="font-medium">{item.label}</span>
            </div>
            <ChevronDown
              size={16}
              className={`transition-transform ${expandedMenu === item.label ? 'rotate-180' : ''}`}
            />
          </button>

          {expandedMenu === item.label && (
            <div className="ml-4 mt-2 space-y-1 border-l-2 border-slate-200 dark:border-slate-700 pl-2">
              {item.submenu?.map((subitem) => (
                <Link
                  key={subitem.label}
                  to={subitem.path!}
                  onClick={() => setIsOpen(false)}
                  className={`flex items-center gap-3 px-4 py-2 rounded-lg transition-colors text-sm ${
                    isActive(subitem.path)
                      ? 'bg-indigo-50 dark:bg-indigo-500/10 text-indigo-700 dark:text-indigo-400 font-semibold'
                      : 'text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800/60'
                  }`}
                >
                  {subitem.icon && <subitem.icon size={16} />}
                  <span>{subitem.label}</span>
                </Link>
              ))}
            </div>
          )}
        </div>
      );
    }

    return (
      <Link
        to={item.path!}
        onClick={() => setIsOpen(false)}
        className={`${baseClasses} ${activeClasses}`}
      >
        <div className="flex items-center gap-3">
          <Icon size={20} />
          <span className="font-medium">{item.label}</span>
        </div>
        {item.badge && (
          <span className="inline-flex items-center justify-center px-2.5 py-1 rounded-full text-xs font-bold bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-400">
            {item.badge}
          </span>
        )}
      </Link>
    );
  };

  return (
    <>
      {/* Mobile Menu Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="fixed top-4 left-4 z-50 p-2 rounded-lg bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 lg:hidden"
      >
        {isOpen ? <X size={24} /> : <Menu size={24} />}
      </button>

      {/* Sidebar */}
      <aside
        className={`${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        } lg:translate-x-0 fixed lg:relative top-0 left-0 z-40 w-64 h-screen bg-white dark:bg-slate-900 border-r border-slate-200 dark:border-slate-800 transition-transform duration-300 overflow-y-auto`}
      >
        {/* Logo */}
        <div className="h-16 flex items-center px-4 border-b border-slate-200 dark:border-slate-800">
          <Link to="/" className="flex items-center gap-2.5 font-bold text-lg text-slate-900 dark:text-white">
            <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-indigo-600 to-violet-600 flex items-center justify-center shadow-sm shadow-indigo-600/20">
              <LayoutDashboard size={18} className="text-white" />
            </div>
            TubePilot
          </Link>
        </div>

        {/* Navigation */}
        <nav className="p-3 space-y-1">
          {navItems.map((item) => (
            <div key={item.label}>
              <NavItemComponent item={item} />
            </div>
          ))}
        </nav>

        {/* Footer */}
        <div className="absolute bottom-0 left-0 right-0 p-4 border-t border-slate-200 dark:border-slate-800 bg-slate-50/80 dark:bg-slate-800/30">
          <p className="text-xs text-slate-500 dark:text-slate-500">
            © 2024 TubePilot. All rights reserved.
          </p>
        </div>
      </aside>

      {/* Mobile Overlay */}
      {isOpen && (
        <div
          onClick={() => setIsOpen(false)}
          className="fixed inset-0 bg-black/50 z-30 lg:hidden"
        />
      )}
    </>
  );
}
