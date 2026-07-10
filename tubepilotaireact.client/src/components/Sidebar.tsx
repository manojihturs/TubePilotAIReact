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
      ? 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400'
      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800';

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
            <div className="ml-4 mt-2 space-y-1 border-l-2 border-gray-200 dark:border-gray-700 pl-2">
              {item.submenu?.map((subitem) => (
                <Link
                  key={subitem.label}
                  to={subitem.path!}
                  onClick={() => setIsOpen(false)}
                  className={`flex items-center gap-3 px-4 py-2 rounded-lg transition-colors text-sm ${
                    isActive(subitem.path)
                      ? 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400 font-medium'
                      : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800'
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
        className="fixed top-4 left-4 z-50 p-2 rounded-lg bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 lg:hidden"
      >
        {isOpen ? <X size={24} /> : <Menu size={24} />}
      </button>

      {/* Sidebar */}
      <aside
        className={`${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        } lg:translate-x-0 fixed lg:relative top-0 left-0 z-40 w-64 h-screen bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-800 transition-transform duration-300 overflow-y-auto`}
      >
        {/* Logo */}
        <div className="h-16 flex items-center px-4 border-b border-gray-200 dark:border-gray-800">
          <Link to="/" className="flex items-center gap-2 font-bold text-xl text-gray-900 dark:text-white">
            <LayoutDashboard size={24} className="text-blue-600" />
            TubePilot
          </Link>
        </div>

        {/* Navigation */}
        <nav className="p-4 space-y-2">
          {navItems.map((item) => (
            <div key={item.label}>
              <NavItemComponent item={item} />
            </div>
          ))}
        </nav>

        {/* Footer */}
        <div className="absolute bottom-0 left-0 right-0 p-4 border-t border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50">
          <p className="text-xs text-gray-600 dark:text-gray-400">
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
