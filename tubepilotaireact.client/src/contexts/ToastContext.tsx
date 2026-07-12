import { createContext, useCallback, useContext, useState, type ReactNode } from 'react';
import { CheckCircle2, XCircle, Info, X } from 'lucide-react';

type ToastType = 'success' | 'error' | 'info';

interface Toast {
  id: number;
  type: ToastType;
  message: string;
}

interface ToastContextValue {
  showToast: (message: string, type?: ToastType) => void;
}

const ToastContext = createContext<ToastContextValue | undefined>(undefined);

let nextId = 1;

const ICONS: Record<ToastType, React.ElementType> = {
  success: CheckCircle2,
  error: XCircle,
  info: Info,
};

const STYLES: Record<ToastType, string> = {
  success: 'bg-white dark:bg-slate-900 border-green-200 dark:border-green-900/50 text-green-700 dark:text-green-400',
  error: 'bg-white dark:bg-slate-900 border-red-200 dark:border-red-900/50 text-red-700 dark:text-red-400',
  info: 'bg-white dark:bg-slate-900 border-indigo-200 dark:border-indigo-900/50 text-indigo-700 dark:text-indigo-400',
};

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const showToast = useCallback((message: string, type: ToastType = 'success') => {
    const id = nextId++;
    setToasts((prev) => [...prev, { id, type, message }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 4000);
  }, []);

  const dismiss = (id: number) => setToasts((prev) => prev.filter((t) => t.id !== id));

  return (
    <ToastContext.Provider value={{ showToast }}>
      {children}
      <div className="fixed bottom-4 right-4 z-[100] flex flex-col gap-2 w-[calc(100%-2rem)] max-w-sm">
        {toasts.map((toast) => {
          const Icon = ICONS[toast.type];
          return (
            <div
              key={toast.id}
              role="status"
              className={`flex items-start gap-3 px-4 py-3 rounded-lg border shadow-lg shadow-slate-900/5 dark:shadow-none animate-in fade-in slide-in-from-bottom-2 ${STYLES[toast.type]}`}
            >
              <Icon size={20} className="flex-shrink-0 mt-0.5" />
              <p className="text-sm font-medium flex-1 text-slate-900 dark:text-white">{toast.message}</p>
              <button onClick={() => dismiss(toast.id)} className="flex-shrink-0 text-slate-400 hover:text-slate-600 dark:hover:text-slate-200">
                <X size={16} />
              </button>
            </div>
          );
        })}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within a ToastProvider');
  return ctx;
}
