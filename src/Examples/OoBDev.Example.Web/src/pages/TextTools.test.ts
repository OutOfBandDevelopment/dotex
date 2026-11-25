// @vitest-environment jsdom
import { afterEach, describe, expect, it, vi } from 'vitest';
import { TextToolsViewModel } from './TextToolsViewModel';

describe('TextToolsViewModel', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('useSample_YamlChosen_ReplacesDocumentAndQuery', () => {
    const vm = new TextToolsViewModel();
    vm.format.value = 'yaml';
    vm.useSample();
    expect(vm.content.value).toContain('orders:');
    expect(vm.result.value).toBeNull();
  });
  it('query_EmptyXPath_CannotExecute', () => {
    const vm = new TextToolsViewModel();
    vm.xpath.value = ' ';
    expect(vm.query.canExecute.value).toBe(false);
  });
  it('query_Valid_PostsFormatAndStoresMatches', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) =>
      new Response(JSON.stringify({ count: 2, matches: ['<total>12.5</total>', '<total>40</total>'], xml: '<x/>' }), { status: 200, headers: { 'Content-Type': 'application/json' } }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new TextToolsViewModel();
    await vm.query.execute();
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string).format).toBe('json');
    expect(vm.result.value?.count).toBe(2);
  });
  it('render_Valid_StoresHtml', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify({ html: '<h1>Title</h1>' }), { status: 200, headers: { 'Content-Type': 'application/json' } })));
    const vm = new TextToolsViewModel();
    await vm.render.execute();
    expect(vm.html.value).toBe('<h1>Title</h1>');
  });
  it('safeHtml_ScriptAndHandler_AreRemoved', () => {
    const vm = new TextToolsViewModel();
    vm.html.value = '<p onclick="x()">hi</p><script>alert(1)</script>';
    expect(vm.safeHtml.value).toBe('<p>hi</p>');
  });
});
