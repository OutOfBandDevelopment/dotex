import { afterEach, describe, expect, it, vi } from 'vitest';
import { ScheduleViewModel } from './ScheduleViewModel';

const state = { pending: [{ key: 'a', dueAt: '2026-11-20T08:00:10Z', message: 'hi', firedAt: null }], fired: [] };
const reply = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status, headers: { 'content-type': 'application/json' } });

describe('schedule commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('add_BlankKey_CannotExecute', () => {
    const vm = new ScheduleViewModel();
    vm.key.value = ' ';
    expect(vm.add.canExecute.value).toBe(false);
  });
  it('add_Item_PostsItAndKeepsTheReturnedState', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => reply(state));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new ScheduleViewModel();
    vm.key.value = 'a'; vm.inSeconds.value = 10; vm.message.value = 'hi';
    await vm.add.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Schedule');
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string)).toEqual({ key: 'a', inSeconds: 10, message: 'hi' });
    expect(vm.state.value).toEqual(state);
  });
  it('release_Key_DeletesItThenReloads', async () => {
    const fetchMock = vi.fn(async (_p: string, i: RequestInit) => (i?.method === 'DELETE' ? new Response(null, { status: 200 }) : reply({ pending: [], fired: [] })));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new ScheduleViewModel();
    await vm.release.execute('a b');
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Schedule/a%20b');
    expect(fetchMock.mock.calls[0][1].method).toBe('DELETE');
    expect(vm.state.value.pending).toEqual([]);
  });
  it('refresh_Failure_ShowsTheProblemText', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => reply({ title: 'Forbidden', detail: 'Needs schedule.use', status: 403 }, 403)));
    const vm = new ScheduleViewModel();
    await vm.refresh.execute();
    expect(vm.refresh.error.value).toContain('schedule.use');
  });
});
