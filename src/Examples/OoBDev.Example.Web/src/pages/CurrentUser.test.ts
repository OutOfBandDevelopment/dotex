import { afterEach, describe, expect, it, vi } from 'vitest';
import { CurrentUserViewModel } from './CurrentUserViewModel';

describe('current user commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('refresh_Claims_FillsList', async () => {
    const claims = [{ type: 'name', value: 'ada', issuer: 'kc' }];
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response(JSON.stringify(claims), { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new CurrentUserViewModel();
    await vm.refresh.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/User/claims');
    expect(vm.claims.value).toEqual(claims);
  });
});
