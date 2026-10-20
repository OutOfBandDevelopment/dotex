import { afterEach, describe, expect, it, vi } from 'vitest';
import { CacheViewModel } from './CacheViewModel';

describe('cache commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('store_NoKey_CannotExecute', () => {
    const vm = new CacheViewModel();
    vm.key.value = '';
    expect(vm.store.canExecute.value).toBe(false);
  });
  it('store_Valid_PutsKeyValueAndLifetime', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response(null, { status: 204 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new CacheViewModel();
    await vm.store.execute();
    expect(fetchMock.mock.calls[0][1].method).toBe('PUT');
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string)).toEqual({ key: 'greeting', value: 'hello from the cache', seconds: 30 });
  });
  it('flush_Key_DeletesEncodedKey', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response(null, { status: 204 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new CacheViewModel();
    vm.key.value = 'a b';
    await vm.flush.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Cache?key=a%20b');
    expect(fetchMock.mock.calls[0][1].method).toBe('DELETE');
  });
});
