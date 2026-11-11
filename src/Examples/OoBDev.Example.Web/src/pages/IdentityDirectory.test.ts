import { afterEach, describe, expect, it, vi } from 'vitest';
import { IdentityDirectoryViewModel } from './IdentityDirectoryViewModel';

describe('identity directory', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('load_Execute_FillsUsersAndRoles', async () => {
    const fetchMock = vi.fn(async (path: string) => new Response(JSON.stringify(path.includes('users')
      ? [{ id: '1', userName: 'ann', email: null, displayName: 'Ann', enabled: true, roles: ['editor'], rights: ['docs.read'] }]
      : [{ name: 'editor', rights: ['docs.read'] }, { name: 'viewer', rights: [] }]), { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new IdentityDirectoryViewModel();
    await vm.load.execute();
    expect(vm.users.value).toHaveLength(1);
    expect(vm.roles.value).toHaveLength(2);
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Identity/users');
  });
  it('load_SearchText_IsEscapedInTheQuery', async () => {
    const fetchMock = vi.fn(async (_p: string) => new Response('[]', { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new IdentityDirectoryViewModel();
    vm.search.value = ' a&b ';
    await vm.load.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Identity/users?search=a%26b');
  });
  it('rightsOf_NoRights_SaysNone', () => {
    expect(new IdentityDirectoryViewModel().rightsOf({ name: 'viewer', rights: [] })).toBe('none');
  });
});
