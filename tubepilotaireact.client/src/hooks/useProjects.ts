import { useState, useCallback } from 'react';
import { getProjects, getProject, createProject, updateProject, deleteProject } from '../services/api/projectsApi';
import type { ProjectPayload, ProjectQuery } from '../types/project';

export function useProjects() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchProjects = useCallback(async (query: ProjectQuery) => {
    setLoading(true);
    setError(null);
    try {
      return await getProjects(query);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch projects');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchProject = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      return await getProject(id);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch project');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const saveProject = useCallback(async (payload: ProjectPayload, id?: string) => {
    setLoading(true);
    setError(null);
    try {
      return id ? await updateProject(id, payload) : await createProject(payload);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save project');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const removeProject = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await deleteProject(id);
      return true;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete project');
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  return { loading, error, fetchProjects, fetchProject, saveProject, removeProject };
}
