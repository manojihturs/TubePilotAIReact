export type LogAction = 'create' | 'update' | 'delete';

export interface ActivityLogEntry {
  id: string;
  action: LogAction;
  entity: string;
  label: string;
  timestamp: string;
  status: 'success' | 'error';
  detail?: string;
}

const STORAGE_KEY = 'tubepilot-activity-log';
const MAX_ENTRIES = 100;

function readLog(): ActivityLogEntry[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

function writeLog(entries: ActivityLogEntry[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(entries.slice(0, MAX_ENTRIES)));
}

export function logActivity(entry: Omit<ActivityLogEntry, 'id' | 'timestamp'>) {
  const full: ActivityLogEntry = {
    ...entry,
    id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
    timestamp: new Date().toISOString(),
  };

  const tag = `[${full.action.toUpperCase()}] ${full.entity}`;
  if (full.status === 'error') {
    console.error(tag, full.label, full.detail ?? '');
  } else {
    console.info(tag, full.label);
  }

  writeLog([full, ...readLog()]);
  window.dispatchEvent(new CustomEvent('tubepilot-activity-log'));
  return full;
}

export function getActivityLog(): ActivityLogEntry[] {
  return readLog();
}
