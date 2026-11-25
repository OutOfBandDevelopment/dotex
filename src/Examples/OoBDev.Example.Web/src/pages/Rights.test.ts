import { afterEach, describe, expect, it, vi } from 'vitest';
import { RightsViewModel } from './RightsViewModel';

const rows = [
  { method: 'GET', route: '/api/Rights/protected', access: 'rights', rights: ['rights.demo'] },
  { method: 'GET', route: '/api/Cache', access: 'anonymous', rights: [] },
];

describe('rights commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('load_Execute_FillsEndpoints', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify(rows), { status: 200 })));
    const vm = new RightsViewModel();
    await vm.load.execute();
    expect(vm.endpoints.value).toHaveLength(2);
  });
  it('visible_FilterMatchesRouteOrRight', () => {
    const vm = new RightsViewModel();
    vm.endpoints.value = rows as never;
    vm.filter.value = 'DEMO';
    expect(vm.visible().map(e => e.route)).toEqual(['/api/Rights/protected']);
    vm.filter.value = 'cache';
    expect(vm.visible().map(e => e.route)).toEqual(['/api/Cache']);
    vm.filter.value = '';
    expect(vm.visible()).toHaveLength(2);
  });
  it('run_Execute_SendsEachRightAndShowsResult', async () => {
    const fetchMock = vi.fn(async (_p: string) => new Response(JSON.stringify({ required: ['a b', 'c'], held: [], result: 'Unauthenticated' }), { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new RightsViewModel();
    vm.rights.value = 'a%b, c';
    await vm.run.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Rights/check?rights=a%25b&rights=c');
    expect(vm.check.value?.result).toBe('Unauthenticated');
  });
  it('run_NoRights_CannotExecute', () => {
    const vm = new RightsViewModel();
    vm.rights.value = '  ';
    expect(vm.run.canExecute.value).toBe(false);
  });
});
