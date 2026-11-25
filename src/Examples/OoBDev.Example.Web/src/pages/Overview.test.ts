import { afterEach, describe, expect, it, vi } from 'vitest';
import { OverviewViewModel, healthPath, isHealthy } from './OverviewViewModel';

afterEach(() => vi.unstubAllGlobals());

describe('OverviewViewModel', () => {
  it('probes_Default_EachTargetsARoutedPage', () => {
    const vm = new OverviewViewModel();
    expect(vm.probes.value.length).toBeGreaterThanOrEqual(8);
    expect(new Set(vm.probes.value.map(p => p.path)).size).toBe(vm.probes.value.length);
  });
  it('isHealthy_UnauthorizedIsHealthy_ServerErrorAndUnansweredAreNot', () => {
    const p = new OverviewViewModel().probes.value[0];
    expect(isHealthy({ ...p, status: 401 })).toBe(true);
    expect(isHealthy({ ...p, status: 200 })).toBe(true);
    expect(isHealthy({ ...p, status: 500 })).toBe(false);
    expect(isHealthy({ ...p, status: null })).toBe(false);
  });
});

describe('OverviewViewModel providers', () => {
  const answer = (status: number, body?: unknown) => (url: string) =>
    Promise.resolve(url === healthPath ? new Response(body ? JSON.stringify(body) : null, { status }) : new Response(null, { status: 200 }));

  it('check_HealthAnswers_ListsEachProvider', async () => {
    vi.stubGlobal('fetch', vi.fn(answer(200, { status: 'Healthy', entries: [{ name: 'ollama', status: 'Healthy', durationMs: 4 }] })));
    const vm = new OverviewViewModel();
    await vm.check.execute();
    expect(vm.providers.value.map(p => p.name)).toEqual(['ollama']);
    expect(vm.providersNote.value).toBeNull();
  });
  it('check_Forbidden_ExplainsTheRightNeeded', async () => {
    vi.stubGlobal('fetch', vi.fn(answer(403)));
    const vm = new OverviewViewModel();
    await vm.check.execute();
    expect(vm.providers.value).toEqual([]);
    expect(vm.providersNote.value).toContain('operations.read');
  });
  it('check_NoProvider_SaysSo', async () => {
    vi.stubGlobal('fetch', vi.fn(answer(200, { status: 'Healthy', entries: [] })));
    const vm = new OverviewViewModel();
    await vm.check.execute();
    expect(vm.providersNote.value).toBe('No provider is configured.');
  });
});
