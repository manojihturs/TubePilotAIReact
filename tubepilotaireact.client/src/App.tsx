import './App.css';
import { AISettingsPage } from './features/aiSettings/AISettingsPage';
import { ContentGenerationPage } from './features/content/ContentGenerationPage';
import { PromptTemplatesPage } from './features/promptTemplates/PromptTemplatesPage';
import { ProjectsPage } from './features/projects/ProjectsPage';
import { useState } from 'react';

type Module = 'projects' | 'promptTemplates' | 'aiSettings' | 'content';

function App() {
  const [activeModule, setActiveModule] = useState<Module>('projects');

  return (
    <>
      <nav className="app-nav" aria-label="TubePilotAI modules">
        <button
          className={activeModule === 'projects' ? 'active' : ''}
          type="button"
          onClick={() => setActiveModule('projects')}
        >
          Projects
        </button>
        <button
          className={activeModule === 'promptTemplates' ? 'active' : ''}
          type="button"
          onClick={() => setActiveModule('promptTemplates')}
        >
          Prompt Templates
        </button>
        <button
          className={activeModule === 'aiSettings' ? 'active' : ''}
          type="button"
          onClick={() => setActiveModule('aiSettings')}
        >
          AI Settings
        </button>
        <button
          className={activeModule === 'content' ? 'active' : ''}
          type="button"
          onClick={() => setActiveModule('content')}
        >
          Content Generation
        </button>
      </nav>
      {activeModule === 'projects' ? (
        <ProjectsPage />
      ) : activeModule === 'promptTemplates' ? (
        <PromptTemplatesPage />
      ) : activeModule === 'aiSettings' ? (
        <AISettingsPage />
      ) : (
        <ContentGenerationPage />
      )}
    </>
  );
}

export default App;
