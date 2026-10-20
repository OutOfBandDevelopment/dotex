export interface NavItem { label: string; to: string; }
export interface NavGroup { title: string; items: NavItem[]; }

/** Left navigation groups. Entries appear as their screens are built. */
export const navGroups: NavGroup[] = [
  { title: 'AI', items: [
    { label: 'Chat', to: '/ai/chat' },
    { label: 'Embeddings', to: '/ai/embeddings' },
    { label: 'Retrieval (RAG)', to: '/ai/rag' },
    { label: 'Ollama', to: '/ai/ollama' },
    { label: 'Groq cloud', to: '/ai/groq' },
    { label: 'Image labels', to: '/ai/images' },
  ] },
  { title: 'Content', items: [
    { label: 'Documents', to: '/content/documents' },
    { label: 'Templates', to: '/content/templates' },
  ] },
  { title: 'Data', items: [{ label: 'Cache lab', to: '/data/cache' }] },
  { title: 'Utilities', items: [{ label: 'Toolbox', to: '/utilities/toolbox' }, { label: 'Text tools', to: '/utilities/text' }, { label: 'Vector store', to: '/utilities/vectors' }, { label: 'Vector lab', to: '/utilities/vector-lab' } ] },
  { title: 'Messaging', items: [
    { label: 'Communications', to: '/messaging/communications' },
    { label: 'Queues', to: '/messaging/queues' },
  ] },
  { title: 'Identity and security', items: [{ label: 'Current user', to: '/identity/me' }] },
  { title: 'Operations', items: [{ label: 'Runtime and configuration', to: '/operations/info' }] },
  { title: 'Developer', items: [
    { label: 'API reference', to: '/developer/api' },
    { label: 'Event reference', to: '/developer/events' },
  ] },
];
