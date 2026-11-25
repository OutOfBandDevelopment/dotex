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

describe('stored document commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('refresh_Execute_FillsStoredList', async () => {
    const docs = [{ fileName: 'a.txt', contentType: 'text/plain' }];
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify(docs), { status: 200 })));
    const vm = new DocumentsViewModel();
    await vm.refresh.execute();
    expect(vm.stored.value).toEqual(docs);
  });
  it('pick_Document_SetsStoreName', async () => {
    const vm = new DocumentsViewModel();
    await vm.pick.execute({ fileName: 'a.txt', contentType: 'text/plain' });
    expect(vm.storeName.value).toBe('a.txt');
  });
});
