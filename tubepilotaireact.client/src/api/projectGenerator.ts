export async function sendGenerateRequest(payload: any) {
    try {
        const res = await fetch('/api/projectgenerator/generate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (!res.ok) return null;
        return await res.json();
    }
    catch {
        return null;
    }
}

export async function getStatus(projectId: string) {
    try {
        const res = await fetch(`/api/projectgenerator/${projectId}/status`);
        if (!res.ok) return null;
        return await res.json();
    }
    catch {
        return null;
    }
}
