export interface NavItem { label: string; to: string; }
export interface NavGroup { title: string; items: NavItem[]; }

/** Left navigation groups. Entries appear as their screens are built. */
export const navGroups: NavGroup[] = [
  { title: 'AI', items: [{ label: 'Chat', to: '/ai/chat' }] },
  { title: 'Identity and security', items: [{ label: 'Current user', to: '/identity/me' }] },
  { title: 'Developer', items: [
    { label: 'API reference', to: '/developer/api' },
    { label: 'Event reference', to: '/developer/events' },
  ] },
];
