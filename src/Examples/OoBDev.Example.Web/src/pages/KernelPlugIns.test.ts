import { afterEach, describe, expect, it, vi } from 'vitest';
import { KernelPlugInsViewModel } from './KernelPlugInsViewModel';

const fn = { plugIn: 'TimePlugIn', name: 'adjusted_time', description: 'time', parameters: { timeOffset: 'offset' } };
const json = (body: unknown) => new Response(JSON.stringify(body), { status: 200, headers: { 'Content-Type': 'application/json' } });

describe('KernelPlugInsViewModel', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('load_Success_FillsTheFunctions', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => json([fn])));
    const vm = new KernelPlugInsViewModel();
    await vm.load.execute();
    expect(vm.functions.value).toEqual([fn]);
  });
  it('invoke_NothingSelected_CannotExecute', () => expect(new KernelPlugInsViewModel().invoke.canExecute.value).toBe(false));
  it('select_Function_PreparesEmptyArguments', () => {
    const vm = new KernelPlugInsViewModel();
    vm.select(fn);
    expect(vm.arguments.value).toEqual({ timeOffset: '' });
    expect(vm.invoke.canExecute.value).toBe(true);
  });
  it('invoke_Selected_PostsArgumentsAndShowsTheResult', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => json({ algorithm: 'TimePlugIn.adjusted_time', result: 'noon' }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new KernelPlugInsViewModel();
    vm.select(fn);
    vm.arguments.value.timeOffset = '01:00:00';
    await vm.invoke.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/KernelPlugIns/invoke');
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string)).toEqual({ plugIn: 'TimePlugIn', function: 'adjusted_time', arguments: { timeOffset: '01:00:00' } });
    expect(vm.result.value).toBe('noon');
  });
});
