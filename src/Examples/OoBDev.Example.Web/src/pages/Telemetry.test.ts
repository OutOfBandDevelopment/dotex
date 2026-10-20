import { afterEach, describe, expect, it, vi } from 'vitest';
import { TelemetryViewModel, traceUrl } from './TelemetryViewModel';

describe('traceUrl', () => {
  it('traceUrl_TrailingSlash_BuildsDetailLink', () =>
    expect(traceUrl('http://localhost:18888/', 'abc')).toBe('http://localhost:18888/traces/detail/abc'));
});

describe('TelemetryViewModel', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('refresh_Success_StoresTraces', async () => {
    const body = { dashboardUrl: 'http://localhost:18888', traces: [{ traceId: 'a', name: 'GET', started: '2026-10-20T00:00:00Z', durationMs: 2 }] };
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify(body), { status: 200, headers: { 'Content-Type': 'application/json' } })));
    const vm = new TelemetryViewModel();
    await vm.refresh.execute();
    expect(vm.recent.value?.traces).toHaveLength(1);
  });
});
