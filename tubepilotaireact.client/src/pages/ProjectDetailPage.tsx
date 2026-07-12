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
} from 'lucide-react';
import {
  projectService,
  aiProviderService,
  AI_PROVIDERS,
  toRelativePath,
  type DataRow,
  type ImageCandidate,
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

  const [providerName, setProviderName] = useState<string>(AI_PROVIDERS[0]);
  const [candidatesByRow, setCandidatesByRow] = useState<Record<string, ImageCandidate[]>>({});
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

  const { data: rows = [], isLoading: rowsLoading } = useQuery({
    queryKey: ['project-rows', projectId],
    queryFn: () => projectService.getRows(projectId),
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

  const hasKey = keys.some((k) => k.providerName === providerName);
  const relativeThumbnailPath = project?.thumbnailPath && project.folderPath
    ? toRelativePath(project.thumbnailPath, project.folderPath)
    : null;
  const thumbnailUrl = useObjectUrl(projectId, relativeThumbnailPath);

  const generateMutation = useMutation({
    mutationFn: () => projectService.generate(projectId, providerName),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['project-rows', projectId] });
      queryClient.invalidateQueries({ queryKey: ['ready-for-video', projectId] });
      const rowTotal = result.outputs.reduce((sum, o) => sum + o.rowCount, 0);
      showToast(`Generated ${result.outputs.length} output(s), ${rowTotal} row(s)`, 'success');
      logActivity({ action: 'create', entity: 'Generation', label: `Generated content for project "${project?.title}"`, status: 'success' });
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

  const generateImageMutation = useMutation({
    mutationFn: (rowId: string) => projectService.generateRowImage(projectId, rowId),
    onSuccess: (_row, rowId) => {
      queryClient.invalidateQueries({ queryKey: ['project-rows', projectId] });
      queryClient.invalidateQueries({ queryKey: ['ready-for-video', projectId] });
      setCandidatesByRow((prev) => {
        const next = { ...prev };
        delete next[rowId];
        return next;
      });
      showToast('AI image generated and confirmed', 'success');
      logActivity({ action: 'create', entity: 'DataRow', label: 'Generated AI image for row', status: 'success' });
    },
    onError: (err: unknown) => {
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Image generation failed';
      showToast(message, 'error');
      logActivity({ action: 'create', entity: 'DataRow', label: 'Generate AI image for row', status: 'error', detail: message });
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
              {!hasKey && (
                <div className="flex items-start gap-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-900/40 rounded-lg p-3 mb-4">
                  <AlertTriangle size={18} className="text-amber-600 dark:text-amber-400 flex-shrink-0 mt-0.5" />
                  <p className="text-sm text-amber-800 dark:text-amber-300">
                    No {providerName} API key connected.{' '}
                    <Link to="/api-keys" className="font-semibold underline inline-flex items-center gap-1">
                      <KeyRound size={14} /> Add one first
                    </Link>
                  </p>
                </div>
              )}
              <div className="flex flex-col sm:flex-row gap-3">
                <select
                  value={providerName}
                  onChange={(e) => setProviderName(e.target.value)}
                  className="px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                >
                  {AI_PROVIDERS.map((provider) => (
                    <option key={provider} value={provider}>
                      {provider}
                    </option>
                  ))}
                </select>
                <button
                  onClick={() => generateMutation.mutate()}
                  disabled={generateMutation.isPending || !hasKey}
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
                    isGenerating={generateImageMutation.isPending && generateImageMutation.variables === row.id}
                    onFetchCandidates={() => candidatesMutation.mutate(row.id)}
                    onSelect={(url) => selectImageMutation.mutate({ rowId: row.id, fullSizeUrl: url })}
                    onGenerate={() => generateImageMutation.mutate(row.id)}
                  />
                ))}
              </div>
            </div>
          ) : rows.length === 0 ? (
            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 p-12 text-center text-slate-500 dark:text-slate-400">
              No content generated yet — click Generate above to produce this project's rows and text.
            </div>
          ) : null}

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
  isGenerating,
  onFetchCandidates,
  onSelect,
  onGenerate,
}: {
  row: DataRow;
  candidates?: ImageCandidate[];
  isFetching: boolean;
  isSelecting: boolean;
  isGenerating: boolean;
  onFetchCandidates: () => void;
  onSelect: (url: string) => void;
  onGenerate: () => void;
}) {
  const label = Object.values(row.data)[0] ?? `Row ${row.rowIndex + 1}`;

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
            <CheckCircle2 size={14} /> Confirmed
          </span>
        ) : (
          <div className="flex-shrink-0 flex items-center gap-2">
            <button
              onClick={onGenerate}
              disabled={isGenerating || isFetching}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg transition-colors disabled:opacity-50"
              title="Generate a relevant AI image for this row — works even when real-photo search finds nothing"
            >
              {isGenerating ? <Loader2 size={14} className="animate-spin" /> : <Sparkles size={14} />}
              AI Image
            </button>
            <button
              onClick={onFetchCandidates}
              disabled={isFetching || isGenerating}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 rounded-lg transition-colors disabled:opacity-50"
              title="Search Wikimedia for a real photo"
            >
              {isFetching ? <Loader2 size={14} className="animate-spin" /> : <Search size={14} />}
              {candidates ? 'Refresh' : 'Find Real Photo'}
            </button>
          </div>
        )}
      </div>

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
