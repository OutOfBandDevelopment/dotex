import { afterEach, describe, expect, it, vi } from 'vitest';
import { ToolboxViewModel, parseNumbers, parseVariables } from './ToolboxViewModel';

describe('parseNumbers', () => {
  it('parseNumbers_MixedSeparators_ReturnsNumbers', () => expect(parseNumbers('1, 2;3  4.5')).toEqual([1, 2, 3, 4.5]));
  it('parseNumbers_NotANumber_ReturnsNull', () => expect(parseNumbers('1, x')).toBeNull());
  it('parseNumbers_Empty_ReturnsNull', () => expect(parseNumbers(' ')).toBeNull());
});

describe('parseVariables', () => {
  it('parseVariables_PairsOnLines_ReturnsRecord', () => expect(parseVariables('a = 4\nb=2.5')).toEqual({ a: 4, b: 2.5 }));
  it('parseVariables_Malformed_ReturnsNull', () => expect(parseVariables('a 4')).toBeNull());
  it('parseVariables_Empty_ReturnsEmptyRecord', () => expect(parseVariables('')).toEqual({}));
});

describe('toolbox commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('distance_DifferentLengths_CannotExecute', () => {
    const vm = new ToolboxViewModel();
    vm.vectorB.value = '1, 2';
    expect(vm.distance.canExecute.value).toBe(false);
  });
  it('hash_Valid_PostsAlgorithmAndStoresResult', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) =>
      new Response(JSON.stringify({ algorithm: 'sha512', result: 'abc' }), { status: 200, headers: { 'Content-Type': 'application/json' } }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new ToolboxViewModel();
    await vm.hash.execute();
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string)).toEqual({ algorithm: 'sha512', value: 'hello' });
    expect(vm.hashResult.value).toBe('abc');
  });
});
