import { afterEach, describe, expect, it, vi } from 'vitest';
import { DocumentsViewModel } from './DocumentsViewModel';

describe('ViewStoredCommand', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('viewSummary_NoName_CannotExecute', () => {
    expect(new DocumentsViewModel().viewSummary.canExecute.value).toBe(false);
  });
  it('viewHtml_Named_GetsHtmlAction', async () => {
    const fetchMock = vi.fn(async (_p: string) => new Response('<p>hi</p>', { status: 200, headers: { 'Content-Type': 'text/html' } }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new DocumentsViewModel();
    vm.storeName.value = 'a/b.docx';
    await vm.viewHtml.execute();
    expect(fetchMock.mock.calls[0][0]).toContain('/Document/Html/a/b.docx');
    expect(vm.output.value).toBe('<p>hi</p>');
  });
});
