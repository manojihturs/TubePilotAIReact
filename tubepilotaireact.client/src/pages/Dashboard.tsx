import { useState, useEffect } from 'react';
import { sendGenerateRequest, getStatus } from '../api/projectGenerator';

export default function Dashboard() {
    const [topic, setTopic] = useState('Best Selling Car Brands');
    const [provider, setProvider] = useState('OpenAI');
    const [template, setTemplate] = useState('Top 10');
    const [language, setLanguage] = useState('en');
    const [outputFolder, setOutputFolder] = useState('');
    const [projectId, setProjectId] = useState('');
    const [progress, setProgress] = useState(0);
    const [status, setStatus] = useState('Idle');
    const [logs, setLogs] = useState<string[]>([]);

    useEffect(() => {
        let timer: any;
        if (projectId) {
            timer = setInterval(async () => {
                const resp = await getStatus(projectId);
                if (resp) {
                    setProgress(resp.progressPercent);
                    setStatus(resp.status);
                    setLogs(resp.logs || []);
                    if (resp.status === 'Completed' || resp.status === 'Failed') {
                        clearInterval(timer);
                    }
                }
            }, 1000);
        }
        return () => clearInterval(timer);
    }, [projectId]);

    async function onGenerate() {
        setStatus('Starting');
        setLogs([]);
        const resp = await sendGenerateRequest({ topic, aiProvider: provider, promptTemplate: template, language, outputFolder });
        if (resp && resp.projectId) {
            setProjectId(resp.projectId);
            setStatus(resp.status);
        }
    }

    return (
        <div style={{ padding: 20 }}>
            <h2>TubePilotAI - Level 0 Dashboard</h2>
            <div style={{ marginBottom: 8 }}>
                <label>Topic: </label>
                <input value={topic} onChange={e => setTopic(e.target.value)} style={{ width: 400 }} />
            </div>

            <div style={{ marginBottom: 8 }}>
                <label>AI Provider: </label>
                <select value={provider} onChange={e => setProvider(e.target.value)}>
                    <option>OpenAI</option>
                    <option>Gemini</option>
                    <option>Claude</option>
                    <option>Grok</option>
                    <option>DeepSeek</option>
                    <option>Ollama</option>
                </select>
            </div>

            <div style={{ marginBottom: 8 }}>
                <label>Prompt Template: </label>
                <select value={template} onChange={e => setTemplate(e.target.value)}>
                    <option>Biography</option>
                    <option>History</option>
                    <option>Facts</option>
                    <option>Top 10</option>
                    <option>Comparison</option>
                </select>
            </div>

            <div style={{ marginBottom: 8 }}>
                <label>Language: </label>
                <select value={language} onChange={e => setLanguage(e.target.value)}>
                    <option value="en">English</option>
                    <option value="es">Spanish</option>
                    <option value="fr">French</option>
                </select>
            </div>

            <div style={{ marginBottom: 8 }}>
                <label>Output Folder: </label>
                <input value={outputFolder} onChange={e => setOutputFolder(e.target.value)} style={{ width: 400 }} placeholder="(optional)" />
            </div>

            <div style={{ marginBottom: 8 }}>
                <button onClick={onGenerate}>Generate Project</button>
            </div>

            <div style={{ marginTop: 16 }}>
                <label>Progress: {progress}%</label>
                <div style={{ width: 500, height: 18, background: '#eee' }}>
                    <div style={{ width: `${progress}%`, height: '100%', background: '#4caf50' }} />
                </div>
                <div>Status: {status}</div>
            </div>

            <div style={{ marginTop: 16 }}>
                <h4>Logs</h4>
                <div style={{ maxHeight: 300, overflow: 'auto', background: '#f7f7f7', padding: 8 }}>
                    {logs.map((l, idx) => <div key={idx}><small>{l}</small></div>)}
                </div>
            </div>
        </div>
    );
}
