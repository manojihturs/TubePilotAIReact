import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { ChevronLeft, Sparkles, KeyRound, AlertTriangle, CheckCircle2, Loader2 } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { aiProviderService, aiToolService, promptService, projectService, FOOTAGE_PROVIDERS } from '../services';
import { useToast } from '../contexts/ToastContext';
import { logActivity } from '../services/logger';

type Step = 'idle' | 'project' | 'content' | 'images' | 'thumbnail' | 'done' | 'error';

export function GeneratePage() {
  const navigate = useNavigate();
  const { showToast } = useToast();
  // Empty string = automatic (try the user's enabled AI tools in priority order).
  const [preferredAiToolId, setPreferredAiToolId] = useState('');
  const [promptId, setPromptId] = useState('');
  const [title, setTitle] = useState('');
  const [step, setStep] = useState<Step>('idle');
  const [imageProgress, setImageProgress] = useState({ done: 0, total: 0 });
  const [errorMessage, setErrorMessage] = useState('');

  const { data: keys = [] } = useQuery({
    queryKey: ['api-keys'],
    queryFn: () => aiProviderService.getKeys(),
  });

  const { data: aiTools = [] } = useQuery({
    queryKey: ['ai-tools'],
    queryFn: () => aiToolService.getAll(),
  });

  const { data: prompts = [] } = useQuery({
    queryKey: ['prompts'],
    queryFn: () => promptService.getAll(),
  });

  const hasEnabledAiTool = aiTools.some((t) => t.isEnabled);
  const isRunning = step !== 'idle' && step !== 'done' && step !== 'error';
  const hasFootageKey = keys.some((k) => (FOOTAGE_PROVIDERS as readonly string[]).includes(k.providerName));
  const [useVideoClips, setUseVideoClips] = useState(true);

  // A prompt with no Output Spec silently degrades to a single text blob — no table rows,
  // no per-row images, no video. Warn before the user commits a generation to it.
  const selectedPrompt = prompts.find((p) => p.id === promptId);
  const promptLacksSpec = !!selectedPrompt && !selectedPrompt.outputSpecJson?.trim();

  const runGeneration = async () => {
    setErrorMessage('');
    try {
      setStep('project');
      const project = await projectService.create({ title, promptId: promptId || undefined });

      setStep('content');
      const contentResult = await projectService.generate(project.id, preferredAiToolId || undefined);
      contentResult.warnings.forEach((w) => showToast(w, 'info'));

      setStep('images');
      const rows = await projectService.getRows(project.id);
      const imageRows = rows.filter((r) => r.imageStatus !== 'NotRequired');
      const fetchClips = useVideoClips && hasFootageKey;
      setImageProgress({ done: 0, total: imageRows.length });
      for (let i = 0; i < imageRows.length; i++) {
        try {
          if (fetchClips) {
            // Real motion footage from a free source (Pexels/Pixabay) — genuine filmed video.
            await projectService.autoFetchRowClip(project.id, imageRows[i].id);
          } else {
            // Real photo from a free source (Wikipedia/Commons) — not an AI drawing.
            await projectService.autoFetchRowImage(project.id, imageRows[i].id);
          }
        } catch {
          // One row failing shouldn't block the rest — it stays reviewable on the Projects page.
        }
        setImageProgress({ done: i + 1, total: imageRows.length });
      }

      setStep('thumbnail');
      try {
        await projectService.generateThumbnail(project.id);
      } catch {
        // Thumbnail can be regenerated later from the review page too.
      }

      setStep('done');
      showToast(`"${title}" generated — review it on the Projects page`, 'success');
      logActivity({ action: 'create', entity: 'Project', label: `Generated project "${title}"`, status: 'success' });
      navigate('/projects');
    } catch (err: unknown) {
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Generation failed';
      setErrorMessage(message);
      setStep('error');
      showToast(message, 'error');
      logActivity({ action: 'create', entity: 'Project', label: `Generate project "${title}"`, status: 'error', detail: message });
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim() || !hasEnabledAiTool || isRunning) return;
    runGeneration();
  };

  const steps: { key: Step; label: string }[] = [
    { key: 'project', label: 'Creating project' },
    { key: 'content', label: 'Generating data & text content' },
    { key: 'images', label: useVideoClips && hasFootageKey ? 'Fetching video clips' : 'Fetching images' },
    { key: 'thumbnail', label: 'Generating thumbnail' },
  ];
  const stepOrder: Step[] = ['project', 'content', 'images', 'thumbnail', 'done'];
  const currentIndex = stepOrder.indexOf(step);

  return (
    <div className="w-full h-full flex flex-col bg-slate-50 dark:bg-slate-950 overflow-hidden">
      <main className="flex-1 overflow-auto">
        <div className="p-4 sm:p-6 lg:p-8 max-w-3xl mx-auto">
          {/* Header */}
          <div className="flex items-center gap-4 mb-8">
            <Link to="/" className="p-2 hover:bg-slate-200 dark:hover:bg-slate-800 rounded-lg">
              <ChevronLeft size={24} className="text-slate-600 dark:text-slate-400" />
            </Link>
            <div className="min-w-0">
              <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 dark:text-white truncate">
                Generate
              </h1>
              <p className="text-sm sm:text-base text-slate-600 dark:text-slate-400 mt-1">
                Pick any topic and a prompt — this creates the project and generates its data, images, and thumbnail in one go.
              </p>
            </div>
          </div>

          <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
            {!hasEnabledAiTool && (
              <div className="flex items-start gap-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-900/40 rounded-lg p-3 mb-4">
                <AlertTriangle size={18} className="text-amber-600 dark:text-amber-400 flex-shrink-0 mt-0.5" />
                <p className="text-sm text-amber-800 dark:text-amber-300">
                  No AI tool connected.{' '}
                  <Link to="/api-keys" className="font-semibold underline inline-flex items-center gap-1">
                    <KeyRound size={14} /> Add one first
                  </Link>
                </p>
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  Title *
                </label>
                <input
                  type="text"
                  required
                  disabled={isRunning}
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-60"
                  placeholder="Any topic — e.g., Brad Pitt Filmography, Top 10 Hiking Trails, Weird Facts About Octopuses"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  Prompt
                </label>
                <select
                  value={promptId}
                  disabled={isRunning}
                  onChange={(e) => setPromptId(e.target.value)}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-60"
                >
                  <option value="">No prompt (generic single-text project)</option>
                  {prompts.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name}
                    </option>
                  ))}
                </select>
                <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                  The prompt's Output Spec decides what gets generated — it works for any topic, not just movies.
                </p>
                {promptLacksSpec && (
                  <div className="flex items-start gap-2 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-900/40 rounded-lg p-3 mt-2">
                    <AlertTriangle size={16} className="text-amber-600 dark:text-amber-400 flex-shrink-0 mt-0.5" />
                    <p className="text-xs text-amber-800 dark:text-amber-300">
                      This prompt has no Output Spec, so it will only produce a single block of text — no data table,
                      no per-row images, and no video rendering. Pick a prompt with an Output Spec (or add one to this
                      prompt) if you want images and video.
                    </p>
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                  AI Tool
                </label>
                <select
                  value={preferredAiToolId}
                  disabled={isRunning}
                  onChange={(e) => setPreferredAiToolId(e.target.value)}
                  className="w-full px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-60"
                >
                  <option value="">Auto (try my tools in priority order)</option>
                  {aiTools.filter((t) => t.isEnabled).map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name}
                    </option>
                  ))}
                </select>
                <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                  Manage your AI tools in <Link to="/api-keys" className="underline font-medium">Settings</Link>.
                </p>
              </div>

              <div className="rounded-lg border border-slate-200 dark:border-slate-700 p-3">
                <label className="flex items-start gap-2.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={useVideoClips && hasFootageKey}
                    disabled={isRunning || !hasFootageKey}
                    onChange={(e) => setUseVideoClips(e.target.checked)}
                    className="mt-0.5 rounded border-slate-300 dark:border-slate-600 text-indigo-600 focus:ring-indigo-500 disabled:opacity-60"
                  />
                  <span className="text-sm text-slate-700 dark:text-slate-300">
                    Use real video clips instead of animated photos
                    <span className="block text-xs text-slate-500 dark:text-slate-500 mt-0.5">
                      {hasFootageKey
                        ? 'Genuine motion footage from Pexels/Pixabay for each row, no Ken Burns needed.'
                        : (
                          <>
                            Requires a free Pexels or Pixabay API key —{' '}
                            <Link to="/api-keys" className="underline font-medium">add one in Settings</Link>.
                          </>
                        )}
                    </span>
                  </span>
                </label>
              </div>

              <button
                type="submit"
                disabled={isRunning || !hasEnabledAiTool}
                className="flex items-center justify-center gap-2 w-full px-4 py-2.5 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-semibold"
              >
                {isRunning ? <Loader2 size={18} className="animate-spin" /> : <Sparkles size={18} />}
                {isRunning ? 'Generating...' : 'Generate'}
              </button>
            </form>

            {step !== 'idle' && (
              <div className="mt-6 border-t border-slate-200 dark:border-slate-800 pt-4 space-y-2">
                {steps.map((s) => {
                  const idx = stepOrder.indexOf(s.key);
                  const isDone = currentIndex > idx || step === 'done';
                  const isActive = step === s.key;
                  return (
                    <div key={s.key} className="flex items-center gap-2 text-sm">
                      {isDone ? (
                        <CheckCircle2 size={16} className="text-green-500 flex-shrink-0" />
                      ) : isActive ? (
                        <Loader2 size={16} className="animate-spin text-indigo-500 flex-shrink-0" />
                      ) : (
                        <div className="w-4 h-4 rounded-full border-2 border-slate-300 dark:border-slate-700 flex-shrink-0" />
                      )}
                      <span className={isDone || isActive ? 'text-slate-900 dark:text-white' : 'text-slate-400 dark:text-slate-600'}>
                        {s.label}
                        {s.key === 'images' && isActive && imageProgress.total > 0 && ` (${imageProgress.done}/${imageProgress.total})`}
                      </span>
                    </div>
                  );
                })}
                {step === 'error' && (
                  <p className="text-sm text-red-600 dark:text-red-400 mt-2">{errorMessage}</p>
                )}
              </div>
            )}
          </div>
        </div>
      </main>
    </div>
  );
}
