import { afterEach, describe, expect, it, vi } from 'vitest';
import { SearchViewModel } from './SearchViewModel';

describe('SearchCommand', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('search_NoQuery_CannotExecute', () => {
    const vm = new SearchViewModel();
    vm.query.value = ' ';
    expect(vm.search.canExecute.value).toBe(false);
  });
  it('search_NoPassages_CannotExecute', () => {
    const vm = new SearchViewModel();
    vm.passages.value = '\n\n';
    expect(vm.search.canExecute.value).toBe(false);
  });
  it('search_Valid_PostsQueryAndStoresAllThreeLists', async () => {
    const hit = { id: 'p3', score: 0.7, text: 't' };
    const fetchMock = vi.fn(async (_path: string, _init: RequestInit) =>
      new Response(JSON.stringify({ lexical: [], semantic: [hit], hybrid: [hit] }), { status: 200, headers: { 'Content-Type': 'application/json' } }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new SearchViewModel();
    await vm.search.execute();
    expect(fetchMock.mock.calls[0][0]).toContain('/api/Search');
    const body = JSON.parse(fetchMock.mock.calls[0][1].body as string);
    expect(body.passages).toHaveLength(4);
    expect(body.limit).toBe(3);
    expect(vm.result.value?.lexical).toEqual([]);
    expect(vm.result.value?.hybrid[0].id).toBe('p3');
  });
});
