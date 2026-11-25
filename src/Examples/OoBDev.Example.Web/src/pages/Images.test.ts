import { afterEach, describe, expect, it, vi } from 'vitest';
import { ImagesViewModel } from './ImagesViewModel';

describe('ImagesViewModel', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('classify_NoFile_CannotExecute', () => expect(new ImagesViewModel().classify.canExecute.value).toBe(false));
  it('classify_OneLabel_CannotExecute', () => {
    const vm = new ImagesViewModel();
    vm.file.value = new File(['x'], 'a.png');
    vm.labels.value = 'a cat';
    expect(vm.classify.canExecute.value).toBe(false);
  });
  it('classify_Valid_PostsFormAndStoresRanking', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) =>
      new Response(JSON.stringify([{ label: 'a cat', probability: 0.9 }]), { status: 200, headers: { 'Content-Type': 'application/json' } }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new ImagesViewModel();
    vm.file.value = new File(['x'], 'a.png', { type: 'image/png' });
    await vm.classify.execute();
    const form = fetchMock.mock.calls[0][1].body as FormData;
    expect(form.get('labels')).toBe(vm.labels.value);
    expect(vm.ranked.value[0].label).toBe('a cat');
  });
});
