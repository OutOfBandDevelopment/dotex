import { readFileSync } from 'node:fs';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { trackScreen } from './analytics';

describe('analytics', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('posts the screen path to the counter endpoint', () => {
    const fetchMock = vi.fn(async () => new Response(null, { status: 204 }));
    vi.stubGlobal('fetch', fetchMock);
    trackScreen('/data/access');
    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [url, init] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
    expect(url).toBe('/api/v1/analytics/screen-views');
    expect(init.body).toBe(JSON.stringify({ screen: '/data/access' }));
  });

  it('ignores the sign-in callback and swallows failures', async () => {
    const fetchMock = vi.fn(async () => new Response('x', { status: 500 }));
    vi.stubGlobal('fetch', fetchMock);
    trackScreen('/signin-callback');
    expect(fetchMock).not.toHaveBeenCalled();
    expect(() => trackScreen('/')).not.toThrow();
  });

  it('keeps the API screen list equal to the router paths', () => {
    const router = readFileSync(new URL('../router/index.ts', import.meta.url), 'utf8');
    const paths = [...router.matchAll(/path: '([^']*)'/g)].map(m => m[1]).filter(p => p !== '/signin-callback').sort();
    const raw = readFileSync(new URL('../../../OoBDev.Example.WebApi/appsettings.json', import.meta.url), 'utf8');
    const settings = JSON.parse(raw.replace(/^\s*\/\/.*$/gm, ''));
    expect([...settings.OoBDev.Analytics.Screens].sort()).toEqual(paths);
  });
});
