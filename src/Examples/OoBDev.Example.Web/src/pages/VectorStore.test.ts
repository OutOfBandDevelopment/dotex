import { afterEach, describe, expect, it, vi } from 'vitest';
import { VectorStoreViewModel } from './VectorStoreViewModel';

const ok = (body: unknown) => new Response(JSON.stringify(body), { status: 200, headers: { 'Content-Type': 'application/json' } });

describe('VectorStoreViewModel', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('lines_BlankLines_AreDropped', () => {
    const vm = new VectorStoreViewModel();
    vm.texts.value = ' a \n\n b\n';
    expect(vm.lines()).toEqual(['a', 'b']);
  });
  it('index_NoTexts_CannotExecute', () => {
    const vm = new VectorStoreViewModel();
    vm.texts.value = '  ';
    expect(vm.index.canExecute.value).toBe(false);
  });
  it('index_Valid_PostsLinesAndRecordsStore', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => ok({ ids: ['1', '2'], store: 'memory' }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new VectorStoreViewModel();
    vm.texts.value = 'one\ntwo';
    await vm.index.execute();
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string).texts).toEqual(['one', 'two']);
    expect(vm.store.value).toBe('memory');
    expect(vm.stored.value).toHaveLength(2);
  });
  it('search_Valid_StoresHits', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => ok([{ id: '3', score: 0.2, text: 'kitten' }])));
    const vm = new VectorStoreViewModel();
    await vm.search.execute();
    expect(vm.hits.value[0].text).toBe('kitten');
  });
  it('list_Valid_GetsCollection', async () => {
    const fetchMock = vi.fn(async (_p: string) => ok([]));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new VectorStoreViewModel();
    vm.collection.value = 'my notes';
    await vm.list.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/VectorStore/my%20notes');
  });
});
