import { useEffect, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useParams, Link } from 'react-router-dom';
import {
  ChevronLeft,
  Sparkles,
  ImageIcon,
  CheckCircle2,
  AlertTriangle,
  KeyRound,
  Loader2,
  RefreshCw,
  Search,
  Film,
  Clapperboard,
  Video,
} from 'lucide-react';
import {
  projectService,
  aiProviderService,
  aiToolService,
  FOOTAGE_PROVIDERS,
  toRelativePath,
  type DataRow,
  type ImageCandidate,
  type VideoClipCandidate,
  type TextOutput,
  type RenderJob,
} from '../services';
import { LoadingSkeleton } from '../components/LoadingCard';
import { useToast } from '../contexts/ToastContext';
import { logActivity } from '../services/logger';

function useObjectUrl(projectId: string, relativePath: string | null | undefined) {
  const [url, setUrl] = useState<string | null>(null);

  useEffect(() => {
    let revoke: string | null = null;
    if (relativePath) {
      projectService.getFileObjectUrl(projectId, relativePath).then((objectUrl) => {
        revoke = objectUrl;
        setUrl(objectUrl);
      });
    } else {
      setUrl(null);
    }
    return () => {
      if (revoke) URL.revokeObjectURL(revoke);
    };
  }, [projectId, relativePath]);

  return url;
}

export function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>();
  const projectId = id!;
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  const [preferredAiToolId, setPreferredAiToolId] = useState('');
  const [candidatesByRow, setCandidatesByRow] = useState<Record<string, ImageCandidate[]>>({});
  const [clipCandidatesByRow, setClipCandidatesByRow] = useState<Record<string, VideoClipCandidate[]>>({});
  const [renderFormat, setRenderFormat] = useState<'Desktop' | 'Shorts'>('Desktop');
  const [renderLanguage, setRenderLanguage] = useState<'en' | 'ta'>('en');
  const [desktopMinutes, setDesktopMinutes] = useState(1);
  const [desktopSeconds, setDesktopSeconds] = useState(30);
  const [shortsSeconds, setShortsSeconds] = useState(45);

  const { data: project, isLoading: projectLoading } = useQuery({
    queryKey: ['project', projectId],
    queryFn: () => projectService.getById(projectId),
  });

  const { data: keys = [] } = useQuery({
    queryKey: ['api-keys'],
    queryFn: () => aiProviderService.getKeys(),
  });

  const { data: aiTools = [] } = useQuery({
    queryKey: ['ai-tools'],
    queryFn: () => aiToolService.getAll(),
  });

  const { data: rows = [], isLoading: rowsLoading } = useQuery({
    queryKey: ['project-rows', projectId],
    queryFn: () => projectService.getRows(projectId),
  });

  const { data: textOutputs = [], isLoading: textOutputsLoading } = useQuery({
    queryKey: ['project-text-outputs', projectId],
    queryFn: () => projectService.getTextOutputs(projectId),
  });

  const { data: readyForVideo = false } = useQuery({
    queryKey: ['ready-for-video', projectId],
    queryFn: () => projectService.isReadyForVideo(projectId),
    enabled: rows.length > 0,
  });

  const { data: ffmpegAvailable = false } = useQuery({
    queryKey: ['render-availability', projectId],
    queryFn: () => projectService.checkRenderAvailability(projectId),
    enabled: readyForVideo,
  });

  const { data: renderJobs = [] } = useQuery({
    queryKey: ['render-jobs', projectId],
    queryFn: () => projectService.getRenderJobs(projectId),
    enabled: readyForVideo,
    refetchInterval: (query) => {
      const jobs = query.state.data as RenderJob[] | undefined;
      return jobs?.some((j) => j.status === 'Queued' || j.status === 'Rendering') ? 3000 : false;
    },
  });

  const hasEnabledAiTool = aiTools.some((t) => t.isEnabled);
  const hasFootageKey = keys.some((k) => (FOOTAGE_PROVIDERS as readonly string[]).includes(k.providerName));
  const relativeThumbnailPath = project?.thumbnailPath && project.folderPath
    ? toRelativePath(project.thumbnailPath, project.folderPath)
    : null;
  const thumbnailUrl = useObjectUrl(projectId, relativeThumbnailPath);

  const generateMutation = useMutation({
    mutationFn: () => projectService.generate(projectId, preferredAiToolId || undefined),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['project-rows', projectId] });
      queryClient.invalidateQueries({ queryKey: ['project-text-outputs', projectId] });
      queryClient.invalidateQueries({ queryKey: ['ready-for-video', projectId] });
      const rowTotal = result.outputs.reduce((sum, o) => sum + o.rowCount, 0);
      showToast(`Generated ${result.outputs.length} output(s), ${rowTotal} row(s)`, 'success');
      logActivity({ action: 'create', entity: 'Generation', label: `Generated content for project "${project?.title}"`, status: 'success' });
      if (result.warnings.length > 0) {
        result.warnings.forEach((w) => showToast(w, 'info'));
        logActivity({ action: 'create', entity: 'Generation', label: `Some parts skipped for project "${project?.title}"`, status: 'error', detail: result.warnings.join(' | ') });
      }
    },
    onError: (err: unknown) => {
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Generation failed';
      showToast(message, 'error');
      logActivity({ action: 'create', entity: 'Generation', label: `Generate for project "${project?.title}"`, status: 'error', detail: message });
    },
  });

  const thumbnailMutation = useMutation({
    mutationFn: () => projectService.generateThumbnail(projectId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['project', projectId] });
      showToast('Thumbnail generated', 'success');
      logActivity({ action: 'create', entity: 'Thumbnail', label: `Generated thumbnail for "${project?.title}"`, status: 'success' });
    },
    onError: (err: unknown) => {
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Thumbnail generation failed';
      showToast(message, 'error');
      logActivity({ action: 'create', entity: 'Thumbnail', label: `Generate thumbnail for "${project?.title}"`, status: 'error', detail: message });
    },
  });

  const candidatesMutation = useMutation({
    mutationFn: (rowId: string) => projectService.getImageCandidates(projectId, rowId),
    onSuccess: (result) => {
      setCandidatesByRow((prev) => ({ ...prev, [result.rowId]: result.candidates }));
      queryClient.invalidateQueries({ queryKey: ['project-rows', projectId] });
      if (result.candidates.length === 0) {
        showToast(`No image candidates found for "${result.query}"`, 'info');
      }
    },
    onError: (err: unknown) => {
      showToast('Failed to fetch image candidates', 'error');
      logActivity({ action: 'create', entity: 'ImageCandidates', label: 'Fetch image candidates', status: 'error', detail: String(err) });
    },
  });

  const selectImageMutation = useMutation({
    mutationFn: ({ rowId, fullSizeUrl }: { rowId: string; fullSizeUrl: string }) =>
      projectService.selectImage(projectId, rowId, fullSizeUrl),
    onSuccess: (_row, variables) => {
      queryClient.invalidateQueries({ queryKey: ['project-rows', projectId] });
      queryClient.invalidateQueries({ queryKey: ['ready-for-video', projectId] });
      setCandidatesByRow((prev) => {
        const next = { ...prev };
        delete next[variables.rowId];
        return next;
      });
      showToast('Image confirmed', 'success');
      logActivity({ action: 'update', entity: 'DataRow', label: 'Confirmed row image', status: 'success' });
    },
    onError: (err: unknown) => {
      showToast('Failed to confirm image', 'error');
      logActivity({ action: 'update', entity: 'DataRow', label: 'Confirm row image', status: 'error', detail: String(err) });
    },
  });

  const autoFetchImageMutation = useMutation({
    mutationFn: (rowId: string) => projectService.autoFetchRowImage(projectId, rowId),
    onSuccess: (row, rowId) => {
      queryClient.invalidateQueries({ queryKey: ['project-rows', projectId] });
      queryClient.invalidateQueries({ queryKey: ['ready-for-video', projectId] });
      setCandidatesByRow((prev) => {
        const next = { ...prev };
        delete next[rowId];
        return next;
      });
      if (row.imageStatus === 'Confirmed') {
        showToast('Real photo fetched and confirmed', 'success');
      } else {
        showToast('No free photo found — try "Find Real Photo" to search manually', 'info');
      }
      logActivity({ action: 'update', entity: 'DataRow', label: 'Auto-fetched real photo for row', status: 'success' });
    },
    onError: (err: unknown) => {
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Photo fetch failed';
      showToast(message, 'error');
      logActivity({ action: 'update', entity: 'DataRow', label: 'Auto-fetch real photo for row', status: 'error', detail: message });
    },
  });

  const clipCandidatesMutation = useMutation({
    mutationFn: (rowId: string) => projectService.getClipCandidates(projectId, rowId),
    onSuccess: (result) => {
      setClipCandidatesByRow((prev) => ({ ...prev, [result.rowId]: result.candidates }));
      queryClient.invalidateQueries({ queryKey: ['project-rows', projectId] });
      if (result.candidates.length === 0) {
        showToast(`No video clips found for "${result.query}"`, 'info');
      }
    },
    onError: (err: unknown) => {
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Failed to fetch video clip candidates';
      showToast(message, 'error');
      logActivity({ action: 'create', entity: 'ClipCandidates', label: 'Fetch clip candidates', status: 'error', detail: String(err) });
    },
  });

  const selectClipMutation = useMutation({
    mutationFn: ({ rowId, downloadUrl }: { rowId: string; downloadUrl: string }) =>
      projectService.selectClip(projectId, rowId, downloadUrl),
    onSuccess: (_row, variables) => {
      queryClient.invalidateQueries({ queryKey: ['project-rows', projectId] });
      queryClient.invalidateQueries({ queryKey: ['ready-for-video', projectId] });
      setClipCandidatesByRow((prev) => {
        const next = { ...prev };
        delete next[variables.rowId];
        return next;
      });
      showToast('Video clip confirmed', 'success');
      logActivity({ action: 'update', entity: 'DataRow', label: 'Confirmed row clip', status: 'success' });
    },
    onError: (err: unknown) => {
      showToast('Failed to confirm video clip', 'error');
      logActivity({ action: 'update', entity: 'DataRow', label: 'Confirm row clip', status: 'error', detail: String(err) });
    },
  });

  const autoFetchClipMutation = useMutation({
    mutationFn: (rowId: string) => projectService.autoFetchRowClip(projectId, rowId),
    onSuccess: (row, rowId) => {
      queryClient.invalidateQueries({ queryKey: ['project-rows', projectId] });
      queryClient.invalidateQueries({ queryKey: ['ready-for-video', projectId] });
      setClipCandidatesByRow((prev) => {
        const next = { ...prev };
        delete next[rowId];
        return next;
      });
      if (row.imageStatus === 'Confirmed') {
        showToast('Real video clip fetched and confirmed', 'success');
      } else {
        showToast('No free clip found — try "Find Clip" to search manually', 'info');
      }
      logActivity({ action: 'update', entity: 'DataRow', label: 'Auto-fetched real clip for row', status: 'success' });
    },
    onError: (err: unknown) => {
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Clip fetch failed';
      showToast(message, 'error');
      logActivity({ action: 'update', entity: 'DataRow', label: 'Auto-fetch real clip for row', status: 'error', detail: message });
    },
  });

  const renderMutation = useMutation({
    mutationFn: () => {
      const durationSeconds = renderFormat === 'Desktop' ? desktopMinutes * 60 + desktopSeconds : shortsSeconds;
      return projectService.startRender(projectId, renderFormat, durationSeconds, renderLanguage);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['render-jobs', projectId] });
      showToast(`${renderFormat} render started`, 'success');
      logActivity({ action: 'create', entity: 'RenderJob', label: `Started ${renderFormat} render for "${project?.title}"`, status: 'success' });
    },
    onError: (err: unknown) => {
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Render failed to start';
      showToast(message, 'error');
      logActivity({ action: 'create', entity: 'RenderJob', label: `Start render for "${project?.title}"`, status: 'error', detail: message });
    },
  });

  if (projectLoading) return <LoadingSkeleton />;

  if (!project) {
    return (
      <div className="p-8 text-center text-slate-500">Project not found.</div>
    );
  }

  const imageRows = rows.filter((r) => r.imageStatus !== 'NotRequired');
  const confirmedCount = imageRows.filter((r) => r.imageStatus === 'Confirmed').length;

  return (
    <div className="w-full h-full flex flex-col bg-slate-50 dark:bg-slate-950 overflow-hidden">
      <main className="flex-1 overflow-auto">
        <div className="p-4 sm:p-6 lg:p-8 max-w-7xl mx-auto space-y-6">
          {/* Header */}
          <div className="flex items-center gap-4">
            <Link to="/projects" className="p-2 hover:bg-slate-200 dark:hover:bg-slate-800 rounded-lg">
              <ChevronLeft size={24} className="text-slate-600 dark:text-slate-400" />
            </Link>
            <div className="min-w-0 flex-1">
              <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 dark:text-white truncate">
                {project.title}
              </h1>
              <p className="text-sm text-slate-500 dark:text-slate-500 mt-1 truncate">{project.folderPath}</p>
            </div>
            {imageRows.length > 0 && (
              <span
                className={`flex-shrink-0 inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-semibold ${
                  readyForVideo
                    ? 'bg-green-50 dark:bg-green-500/10 text-green-700 dark:text-green-400'
                    : 'bg-amber-50 dark:bg-amber-500/10 text-amber-700 dark:text-amber-400'
                }`}
              >
                {readyForVideo ? <CheckCircle2 size={14} /> : <AlertTriangle size={14} />}
                {readyForVideo ? 'Ready for video' : `${confirmedCount}/${imageRows.length} images confirmed`}
              </span>
            )}
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Generate panel */}
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
              <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Generate Content</h2>
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
              <div className="flex flex-col sm:flex-row gap-3">
                <select
                  value={preferredAiToolId}
                  onChange={(e) => setPreferredAiToolId(e.target.value)}
                  className="px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="">Auto (priority order)</option>
                  {aiTools.filter((t) => t.isEnabled).map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name}
                    </option>
                  ))}
                </select>
                <button
                  onClick={() => generateMutation.mutate()}
                  disabled={generateMutation.isPending || !hasEnabledAiTool}
                  className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-semibold"
                >
                  {generateMutation.isPending ? <Loader2 size={18} className="animate-spin" /> : <Sparkles size={18} />}
                  {generateMutation.isPending ? 'Generating...' : 'Generate'}
                </button>
              </div>
              {rows.length > 0 && (
                <p className="text-xs text-slate-500 dark:text-slate-500 mt-3">
                  {rows.length} row(s) already generated. Re-generating will add another AI call per output item.
                </p>
              )}
            </div>

            {/* Thumbnail panel */}
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
              <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Video Thumbnail</h2>
              <div className="flex flex-col sm:flex-row gap-4 items-start">
                <div className="w-full sm:w-40 aspect-video bg-slate-100 dark:bg-slate-800 rounded-lg overflow-hidden flex items-center justify-center flex-shrink-0">
                  {thumbnailUrl ? (
                    <img src={thumbnailUrl} alt="Thumbnail" className="w-full h-full object-cover" />
                  ) : (
                    <ImageIcon size={24} className="text-slate-400" />
                  )}
                </div>
                <div className="flex-1">
                  <button
                    onClick={() => thumbnailMutation.mutate()}
                    disabled={thumbnailMutation.isPending}
                    className="flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors font-semibold"
                  >
                    {thumbnailMutation.isPending ? <Loader2 size={18} className="animate-spin" /> : <Sparkles size={18} />}
                    {thumbnailMutation.isPending ? 'Generating...' : project.thumbnailPath ? 'Regenerate Thumbnail' : 'Generate Thumbnail'}
                  </button>
                  <p className="text-xs text-slate-500 dark:text-slate-500 mt-2">
                    Free AI image generation — no API key required.
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Rows / image review */}
          {rowsLoading ? (
            <LoadingSkeleton />
          ) : imageRows.length > 0 ? (
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
              <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-4">Image Review</h2>
              <div className="space-y-4">
                {imageRows.map((row) => (
                  <RowImageReview
                    key={row.id}
                    row={row}
                    candidates={candidatesByRow[row.id]}
                    isFetching={candidatesMutation.isPending && candidatesMutation.variables === row.id}
                    isSelecting={selectImageMutation.isPending && selectImageMutation.variables?.rowId === row.id}
                    isFetchingPhoto={autoFetchImageMutation.isPending && autoFetchImageMutation.variables === row.id}
                    onFetchCandidates={() => candidatesMutation.mutate(row.id)}
                    onSelect={(url) => selectImageMutation.mutate({ rowId: row.id, fullSizeUrl: url })}
                    onAutoFetch={() => autoFetchImageMutation.mutate(row.id)}
                    hasFootageKey={hasFootageKey}
                    clipCandidates={clipCandidatesByRow[row.id]}
                    isFetchingClips={clipCandidatesMutation.isPending && clipCandidatesMutation.variables === row.id}
                    isSelectingClip={selectClipMutation.isPending && selectClipMutation.variables?.rowId === row.id}
                    isFetchingClip={autoFetchClipMutation.isPending && autoFetchClipMutation.variables === row.id}
                    onFetchClipCandidates={() => clipCandidatesMutation.mutate(row.id)}
                    onSelectClip={(url) => selectClipMutation.mutate({ rowId: row.id, downloadUrl: url })}
                    onAutoFetchClip={() => autoFetchClipMutation.mutate(row.id)}
                  />
                ))}
              </div>
            </div>
          ) : rows.length === 0 && textOutputs.length === 0 && !textOutputsLoading ? (
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-12 text-center text-slate-500 dark:text-slate-400">
              No content generated yet — click Generate above to produce this project's rows and text.
            </div>
          ) : null}

          {/* Text-only outputs (narration, captions, or a whole no-prompt generic project) —
              these never produce reviewable DataRows, so they need their own preview. */}
          {textOutputs.length > 0 && (
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
              <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-4">Generated Text</h2>
              <div className="space-y-4">
                {textOutputs.map((output) => (
                  <div key={output.id}>
                    <p className="text-xs font-semibold text-slate-500 dark:text-slate-500 mb-1.5">{output.outputItemName}</p>
                    <pre className="whitespace-pre-wrap text-sm text-slate-800 dark:text-slate-200 bg-slate-50 dark:bg-slate-800 rounded-lg p-4 font-mono max-h-96 overflow-auto">
                      {output.content || '(empty)'}
                    </pre>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Render video */}
          {readyForVideo && (
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-6">
              <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Render Video</h2>

              {!ffmpegAvailable && (
                <div className="flex items-start gap-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-900/40 rounded-lg p-3 mb-4">
                  <AlertTriangle size={18} className="text-amber-600 dark:text-amber-400 flex-shrink-0 mt-0.5" />
                  <p className="text-sm text-amber-800 dark:text-amber-300">
                    FFmpeg is not installed on the server. Rendering is unavailable until it's installed and the API is restarted.
                  </p>
                </div>
              )}

              <div className="flex flex-wrap gap-4 items-end">
                <div>
                  <label className="block text-xs font-medium text-slate-500 dark:text-slate-500 mb-1">Format</label>
                  <div className="flex rounded-lg border border-slate-300 dark:border-slate-600 overflow-hidden">
                    <button
                      onClick={() => setRenderFormat('Desktop')}
                      className={`flex items-center gap-1.5 px-3 py-2 text-sm font-medium ${renderFormat === 'Desktop' ? 'bg-indigo-600 text-white' : 'bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300'}`}
                    >
                      <Film size={14} /> Desktop
                    </button>
                    <button
                      onClick={() => setRenderFormat('Shorts')}
                      className={`flex items-center gap-1.5 px-3 py-2 text-sm font-medium ${renderFormat === 'Shorts' ? 'bg-indigo-600 text-white' : 'bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300'}`}
                    >
                      <Clapperboard size={14} /> Shorts/Reels
                    </button>
                  </div>
                </div>

                {renderFormat === 'Desktop' ? (
                  <div>
                    <label className="block text-xs font-medium text-slate-500 dark:text-slate-500 mb-1">Duration</label>
                    <div className="flex items-center gap-1">
                      <input
                        type="number"
                        min={0}
                        max={60}
                        value={desktopMinutes}
                        onChange={(e) => setDesktopMinutes(Math.max(0, Number(e.target.value)))}
                        className="w-16 px-2 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white text-sm"
                      />
                      <span className="text-sm text-slate-500">min</span>
                      <input
                        type="number"
                        min={0}
                        max={59}
                        value={desktopSeconds}
                        onChange={(e) => setDesktopSeconds(Math.min(59, Math.max(0, Number(e.target.value))))}
                        className="w-16 px-2 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white text-sm"
                      />
                      <span className="text-sm text-slate-500">sec</span>
                    </div>
                  </div>
                ) : (
                  <div>
                    <label className="block text-xs font-medium text-slate-500 dark:text-slate-500 mb-1">Duration (max 60s)</label>
                    <div className="flex items-center gap-1">
                      <input
                        type="number"
                        min={1}
                        max={60}
                        value={shortsSeconds}
                        onChange={(e) => setShortsSeconds(Math.min(60, Math.max(1, Number(e.target.value))))}
                        className="w-16 px-2 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white text-sm"
                      />
                      <span className="text-sm text-slate-500">sec</span>
                    </div>
                  </div>
                )}

                <div>
                  <label className="block text-xs font-medium text-slate-500 dark:text-slate-500 mb-1">Language</label>
                  <select
                    value={renderLanguage}
                    onChange={(e) => setRenderLanguage(e.target.value as 'en' | 'ta')}
                    className="px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white text-sm"
                  >
                    <option value="en">English</option>
                    <option value="ta">Tamil</option>
                  </select>
                </div>

                <button
                  onClick={() => renderMutation.mutate()}
                  disabled={renderMutation.isPending || !ffmpegAvailable}
                  className="flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-semibold"
                >
                  {renderMutation.isPending ? <Loader2 size={18} className="animate-spin" /> : <Sparkles size={18} />}
                  Generate Video
                </button>
              </div>

              {renderJobs.length > 0 && (
                <div className="mt-5 space-y-2">
                  {renderJobs
                    .slice()
                    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
                    .map((job) => (
                      <div key={job.id} className="flex items-center justify-between gap-3 border border-slate-200 dark:border-slate-800 rounded-lg px-3 py-2 text-sm">
                        <div className="flex items-center gap-2 min-w-0">
                          {job.format === 'Desktop' ? <Film size={14} className="text-slate-400 flex-shrink-0" /> : <Clapperboard size={14} className="text-slate-400 flex-shrink-0" />}
                          <span className="text-slate-700 dark:text-slate-300 truncate">
                            {job.format} · {job.durationSeconds}s · {job.language === 'ta' ? 'Tamil' : 'English'}
                          </span>
                        </div>
                        <span
                          className={`flex-shrink-0 inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ${
                            job.status === 'Complete'
                              ? 'bg-green-50 dark:bg-green-500/10 text-green-700 dark:text-green-400'
                              : job.status === 'Failed'
                              ? 'bg-red-50 dark:bg-red-500/10 text-red-700 dark:text-red-400'
                              : 'bg-amber-50 dark:bg-amber-500/10 text-amber-700 dark:text-amber-400'
                          }`}
                          title={job.errorMessage ?? undefined}
                        >
                          {(job.status === 'Queued' || job.status === 'Rendering') && <Loader2 size={12} className="animate-spin" />}
                          {job.status === 'Complete' && <CheckCircle2 size={12} />}
                          {job.status === 'Failed' && <AlertTriangle size={12} />}
                          {job.status}
                        </span>
                      </div>
                    ))}
                </div>
              )}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}

function RowImageReview({
  row,
  candidates,
  isFetching,
  isSelecting,
  isFetchingPhoto,
  onFetchCandidates,
  onSelect,
  onAutoFetch,
  hasFootageKey,
  clipCandidates,
  isFetchingClips,
  isSelectingClip,
  isFetchingClip,
  onFetchClipCandidates,
  onSelectClip,
  onAutoFetchClip,
}: {
  row: DataRow;
  candidates?: ImageCandidate[];
  isFetching: boolean;
  isSelecting: boolean;
  isFetchingPhoto: boolean;
  onFetchCandidates: () => void;
  onSelect: (url: string) => void;
  onAutoFetch: () => void;
  hasFootageKey: boolean;
  clipCandidates?: VideoClipCandidate[];
  isFetchingClips: boolean;
  isSelectingClip: boolean;
  isFetchingClip: boolean;
  onFetchClipCandidates: () => void;
  onSelectClip: (url: string) => void;
  onAutoFetchClip: () => void;
}) {
  const label = Object.values(row.data)[0] ?? `Row ${row.rowIndex + 1}`;
  const anyBusy = isFetching || isFetchingPhoto || isFetchingClips || isFetchingClip;

  return (
    <div className="border border-slate-200 dark:border-slate-800 rounded-lg p-4">
      <div className="flex items-center justify-between gap-3 mb-2">
        <div className="min-w-0">
          <p className="font-semibold text-slate-900 dark:text-white truncate">{label}</p>
          <p className="text-xs text-slate-500 dark:text-slate-500">
            {Object.entries(row.data).map(([k, v]) => `${k}: ${v}`).join(' · ')}
          </p>
        </div>
        {row.imageStatus === 'Confirmed' ? (
          <span className="flex-shrink-0 inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-green-50 dark:bg-green-500/10 text-green-700 dark:text-green-400">
            {row.isVideoClip ? <Video size={14} /> : <CheckCircle2 size={14} />}
            {row.isVideoClip ? 'Confirmed (video clip)' : 'Confirmed'}
          </span>
        ) : null}
      </div>

      {row.imageStatus !== 'Confirmed' && (
        <div className="space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs font-medium text-slate-500 dark:text-slate-500 w-16 flex-shrink-0">Clip</span>
            <button
              onClick={onAutoFetchClip}
              disabled={anyBusy || !hasFootageKey}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg transition-colors disabled:opacity-50"
              title={hasFootageKey ? 'Fetch the best real video clip for this row (Pexels/Pixabay)' : 'Add a free Pexels or Pixabay API key in Settings to fetch video clips'}
            >
              {isFetchingClip ? <Loader2 size={14} className="animate-spin" /> : <Video size={14} />}
              Get Clip
            </button>
            <button
              onClick={onFetchClipCandidates}
              disabled={anyBusy || !hasFootageKey}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 rounded-lg transition-colors disabled:opacity-50"
              title={hasFootageKey ? 'Browse and manually pick a real video clip' : 'Add a free Pexels or Pixabay API key in Settings to browse video clips'}
            >
              {isFetchingClips ? <Loader2 size={14} className="animate-spin" /> : <Search size={14} />}
              {clipCandidates ? 'Refresh' : 'Find Clip'}
            </button>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs font-medium text-slate-500 dark:text-slate-500 w-16 flex-shrink-0">Photo</span>
            <button
              onClick={onAutoFetch}
              disabled={anyBusy}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-slate-700 dark:text-slate-300 bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 rounded-lg transition-colors disabled:opacity-50"
              title="Fetch the best real photo for this row from a free source (Wikipedia / Wikimedia Commons)"
            >
              {isFetchingPhoto ? <Loader2 size={14} className="animate-spin" /> : <ImageIcon size={14} />}
              Get Photo
            </button>
            <button
              onClick={onFetchCandidates}
              disabled={anyBusy}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 rounded-lg transition-colors disabled:opacity-50"
              title="Browse and manually pick a real photo"
            >
              {isFetching ? <Loader2 size={14} className="animate-spin" /> : <Search size={14} />}
              {candidates ? 'Refresh' : 'Find Real Photo'}
            </button>
          </div>
        </div>
      )}

      {clipCandidates && row.imageStatus !== 'Confirmed' && (
        <div className="grid grid-cols-3 sm:grid-cols-6 gap-2 mt-3">
          {clipCandidates.length === 0 && (
            <p className="col-span-full text-sm text-slate-500 dark:text-slate-500">No video clips found for this row.</p>
          )}
          {clipCandidates.map((c) => (
            <button
              key={c.downloadUrl}
              onClick={() => onSelectClip(c.downloadUrl)}
              disabled={isSelectingClip}
              className="group relative aspect-square rounded-lg overflow-hidden border-2 border-transparent hover:border-indigo-500 transition-colors disabled:opacity-50"
              title={`${c.license} · ${Math.round(c.durationSeconds)}s`}
            >
              <img src={c.previewImageUrl} alt="" className="w-full h-full object-cover" />
              <div className="absolute top-1 right-1 bg-black/60 rounded-full p-1">
                <Video size={10} className="text-white" />
              </div>
              <div className="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors" />
            </button>
          ))}
        </div>
      )}

      {candidates && row.imageStatus !== 'Confirmed' && (
        <div className="grid grid-cols-3 sm:grid-cols-6 gap-2 mt-3">
          {candidates.length === 0 && (
            <p className="col-span-full text-sm text-slate-500 dark:text-slate-500">No candidates found for this row.</p>
          )}
          {candidates.map((c) => (
            <button
              key={c.fullSizeUrl}
              onClick={() => onSelect(c.fullSizeUrl)}
              disabled={isSelecting}
              className="group relative aspect-square rounded-lg overflow-hidden border-2 border-transparent hover:border-indigo-500 transition-colors disabled:opacity-50"
              title={c.license}
            >
              <img src={c.thumbnailUrl} alt="" className="w-full h-full object-cover" />
              <div className="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors" />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
