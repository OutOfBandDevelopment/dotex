import { afterEach, describe, expect, it, vi } from 'vitest';
import { ModelCompareViewModel } from './ModelCompareViewModel';

describe('ModelCompareViewModel', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('compare_NoModel_CannotExecute', () => {
    const vm = new ModelCompareViewModel();
    vm.selected.value = [];
    expect(vm.compare.canExecute.value).toBe(false);
  });
  it('compare_Valid_PostsTextsAndModels', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) =>
      new Response(JSON.stringify([{ model: 'all-minilm-l6-v2', length: 384, similarity: 0.7, milliseconds: 12 }]), { status: 200, headers: { 'Content-Type': 'application/json' } }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new ModelCompareViewModel();
    await vm.compare.execute();
    const body = JSON.parse(fetchMock.mock.calls[0][1].body as string);
    expect(body.models).toEqual(['all-minilm-l6-v2']);
    expect(vm.scores.value[0].length).toBe(384);
  });
});
