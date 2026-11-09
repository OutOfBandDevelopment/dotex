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

describe('template management commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  const info = { name: 'order-shipped', file: 'order-shipped.html.hbs', templateContentType: 'text/x-handlebars-template', targetExtension: '.html' };

  it('list_Execute_FillsTemplates', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify([info]), { status: 200 })));
    const vm = new TemplatesViewModel();
    await vm.list.execute();
    expect(vm.templates.value).toEqual([info]);
  });
  it('open_Template_LoadsTextAndSelectsIt', async () => {
    const fetchMock = vi.fn(async (_p: string) => new Response(JSON.stringify({ info, text: '<p>{{name}}</p>' }), { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new TemplatesViewModel();
    await vm.open.execute(info);
    expect(fetchMock.mock.calls[0][0]).toBe('/api/TextTemplate/Read?file=order-shipped.html.hbs');
    expect(vm.text.value).toBe('<p>{{name}}</p>');
    expect(vm.selected.value?.file).toBe(info.file);
  });
  it('preview_EmptyText_CannotExecute', () => {
    const vm = new TemplatesViewModel();
    expect(vm.preview.canExecute.value).toBe(false);
  });
  it('preview_Draft_PostsTextTargetAndData', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response('<p>Ada</p>', { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new TemplatesViewModel();
    vm.selected.value = info;
    vm.text.value = '<p>{{name}}</p>';
    await vm.preview.execute();
    const body = JSON.parse(String(fetchMock.mock.calls[0][1].body));
    expect(fetchMock.mock.calls[0][0]).toBe('/api/TextTemplate/Preview');
    expect(body.targetExtension).toBe('.html');
    expect(body.data.name).toBe('Ada');
    expect(vm.output.value).toBe('<p>Ada</p>');
  });
});
