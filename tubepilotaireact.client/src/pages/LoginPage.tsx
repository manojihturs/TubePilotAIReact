import { useState } from 'react';
import { LayoutDashboard } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);
    try {
      await login(email, password);
      navigate('/');
    } catch (err) {
      setError('Failed to log in. Please check your credentials.');
      console.error(err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="flex items-center justify-center w-full h-screen bg-slate-50 dark:bg-slate-950 px-4">
      <div className="w-full max-w-md">
        <div className="flex flex-col items-center gap-2 mb-8">
          <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-indigo-600 to-violet-600 flex items-center justify-center shadow-lg shadow-indigo-600/20">
            <LayoutDashboard size={24} className="text-white" />
          </div>
          <h1 className="text-xl font-bold text-slate-900 dark:text-white mt-2">TubePilotAI</h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">Sign in to your workspace</p>
        </div>

        <div className="p-8 space-y-6 bg-white rounded-2xl border border-slate-200 shadow-xl shadow-slate-200/50 dark:bg-slate-900 dark:border-slate-800 dark:shadow-none">
          <form className="space-y-5" onSubmit={handleSubmit}>
            <div>
              <label
                htmlFor="email"
                className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5"
              >
                Email Address
              </label>
              <input
                id="email"
                name="email"
                type="email"
                autoComplete="email"
                required
                placeholder="you@company.com"
                className="block w-full px-3.5 py-2.5 text-slate-900 bg-slate-50 border border-slate-200 rounded-lg shadow-sm placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm transition-colors dark:bg-slate-800 dark:border-slate-700 dark:placeholder-slate-500 dark:text-white"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>
            <div>
              <label
                htmlFor="password"
                className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5"
              >
                Password
              </label>
              <input
                id="password"
                name="password"
                type="password"
                autoComplete="current-password"
                required
                placeholder="••••••••"
                className="block w-full px-3.5 py-2.5 text-slate-900 bg-slate-50 border border-slate-200 rounded-lg shadow-sm placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm transition-colors dark:bg-slate-800 dark:border-slate-700 dark:placeholder-slate-500 dark:text-white"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
            {error && (
              <p className="text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-900/40 rounded-lg px-3 py-2">
                {error}
              </p>
            )}
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex justify-center w-full px-4 py-2.5 text-sm font-semibold text-white bg-indigo-600 border border-transparent rounded-lg shadow-sm hover:bg-indigo-700 disabled:opacity-60 disabled:cursor-not-allowed focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors"
            >
              {isSubmitting ? 'Signing in...' : 'Sign in'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
