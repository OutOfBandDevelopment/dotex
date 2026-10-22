import { afterEach, describe, expect, it, vi } from 'vitest';
import { TemplatesViewModel } from './TemplatesViewModel';

describe('templates commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('apply_BlankName_CannotExecute', () => {
    const vm = new TemplatesViewModel();
    vm.name.value = ' ';
    expect(vm.apply.canExecute.value).toBe(false);
  });
  it('apply_InvalidJson_CannotExecute', () => {
    const vm = new TemplatesViewModel();
    vm.data.value = '{ nope';
    expect(vm.apply.canExecute.value).toBe(false);
  });
  it('apply_Valid_PostsEncodedNameAndShowsOutput', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response('Hello Ada', { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new TemplatesViewModel();
    vm.name.value = 'a b.hbs';
    await vm.apply.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/TextTemplate/Apply?templateName=a%20b.hbs');
    expect(vm.output.value).toBe('Hello Ada');
  });
});
