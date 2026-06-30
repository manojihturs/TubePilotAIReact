import { useEffect, useState } from 'react';
import { deleteAIProviderSetting, getAIProviderSettings, saveAIProviderSetting } from '../../services/api/aiSettingsApi';
import type { AIProviderSetting, ContentGenerationProviderKind } from '../../types/aiSettings';

const providers: ContentGenerationProviderKind[] = ['OpenAI', 'Gemini', 'Claude', 'DeepSeek', 'Ollama'];

export function AISettingsPage() {
  const [settings, setSettings] = useState<AIProviderSetting[]>([]);
  const [selectedProvider, setSelectedProvider] = useState<ContentGenerationProviderKind>('Gemini');
  const [apiKey, setApiKey] = useState('');
  const [errors, setErrors] = useState<string[]>([]);
  const [message, setMessage] = useState('');

  useEffect(() => {
    void load();
  }, []);

  async function load() {
    try {
      setSettings(await getAIProviderSettings());
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    }
  }

  async function handleSave() {
    if (!apiKey.trim()) {
      setErrors(['API key is required.']);
      return;
    }

    setErrors([]);
    try {
      await saveAIProviderSetting(selectedProvider, apiKey.trim());
      setApiKey('');
      setMessage(`${selectedProvider} saved.`);
      await load();
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    }
  }

  async function handleDelete(provider: ContentGenerationProviderKind) {
    setErrors([]);
    try {
      await deleteAIProviderSetting(provider);
      setMessage(`${provider} removed.`);
      await load();
    } catch (error) {
      setErrors([getErrorMessage(error)]);
    }
  }

  return (
    <main className="prompt-page">
      <header className="prompt-header">
        <div>
          <p className="eyebrow">TubePilotAI</p>
          <h1>AI Settings</h1>
        </div>
      </header>

      {errors.length > 0 && (
        <div className="notice error" role="alert">
          {errors.map((error) => <p key={error}>{error}</p>)}
        </div>
      )}
      {message && <div className="notice success">{message}</div>}

      <section className="content-layout">
        <aside className="content-panel">
          <h2>Configure API key</h2>
          <label>
            AI tool
            <select value={selectedProvider} onChange={(event) => setSelectedProvider(event.target.value as ContentGenerationProviderKind)}>
              {providers.map((provider) => (
                <option key={provider} value={provider}>{provider}</option>
              ))}
            </select>
          </label>
          <label>
            API key
            <input value={apiKey} onChange={(event) => setApiKey(event.target.value)} placeholder="Paste API key" />
          </label>
          <div className="form-actions">
            <button type="button" onClick={handleSave}>Save Key</button>
          </div>
        </aside>

        <section className="content-workspace">
          <h2>Configured tools</h2>
          {settings.filter((setting) => setting.isConfigured).length === 0 ? (
            <p className="empty-state">No AI tools configured yet.</p>
          ) : (
            <div className="content-stack">
              {settings.filter((setting) => setting.isConfigured).map((setting) => (
                <div key={setting.provider} className="history-row">
                  <span>
                    <strong>{setting.provider}</strong>
                    <small>{setting.maskedApiKey}</small>
                  </span>
                  <button className="secondary-button" type="button" onClick={() => handleDelete(setting.provider)}>
                    Remove
                  </button>
                </div>
              ))}
            </div>
          )}
        </section>
      </section>
    </main>
  );
}

function getErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : 'Something went wrong.';
}
