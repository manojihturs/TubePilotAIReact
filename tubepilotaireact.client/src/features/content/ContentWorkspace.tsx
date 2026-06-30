import { useState } from 'react';

interface GeneratedContent {
  title: string;
  description: string;
  hashtags: string;
  thumbnailText: string;
  thumbnailPrompt: string;
  narrationScript: string;
  sceneBreakdown: string;
  voiceoverScript: string;
}

export function ContentWorkspace({ content }: { content: GeneratedContent }) {
  const [activeTab, setActiveTab] = useState<'Script' | 'Metadata' | 'Visuals'>('Script');

  return (
    <div className="content-workspace">
      <nav className="tabs">
        <button onClick={() => setActiveTab('Script')}>Script</button>
        <button onClick={() => setActiveTab('Metadata')}>Metadata</button>
        <button onClick={() => setActiveTab('Visuals')}>Visuals</button>
      </nav>

      <section className="tab-content">
        {activeTab === 'Script' && (
          <div>
            <h3>Narration Script</h3>
            <p>{content.narrationScript}</p>
            <h3>Voiceover Script</h3>
            <p>{content.voiceoverScript}</p>
          </div>
        )}
        {activeTab === 'Metadata' && (
          <div>
            <h3>{content.title}</h3>
            <p>{content.description}</p>
            <p><strong>Hashtags:</strong> {content.hashtags}</p>
          </div>
        )}
        {activeTab === 'Visuals' && (
          <div>
            <h3>Thumbnail Text</h3>
            <p>{content.thumbnailText}</p>
            <h3>Thumbnail Prompt</h3>
            <p>{content.thumbnailPrompt}</p>
            <h3>Scene Breakdown</h3>
            <p>{content.sceneBreakdown}</p>
          </div>
        )}
      </section>
    </div>
  );
}
