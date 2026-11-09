import { afterEach, describe, expect, it, vi } from 'vitest';
import { CgiViewModel } from './CgiViewModel';
import { WebhooksViewModel } from './WebhooksViewModel';

describe('cgi commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('load_Execute_FillsProgramsAndSelectsFirst', async () => {
    const programs = [{ name: 'greet.exe', path: '/cgi-bin/greet.exe' }];
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify(programs), { status: 200 })));
    const vm = new CgiViewModel();
    await vm.load.execute();
    expect(vm.programs.value).toEqual(programs);
    expect(vm.selected.value).toBe('/cgi-bin/greet.exe');
  });
  it('open_Execute_GetsProgramWithEncodedName', async () => {
    const fetchMock = vi.fn(async (_p: string) => new Response('<p>Hello Ada</p>', { status: 200, statusText: 'OK' }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new CgiViewModel();
    vm.selected.value = '/cgi-bin/greet.exe';
    vm.name.value = 'Ada & Bob';
    await vm.open.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/cgi-bin/greet.exe?name=Ada%20%26%20Bob');
    expect(vm.status.value).toBe('200 OK');
    expect(vm.body.value).toContain('Hello Ada');
  });
  it('post_Execute_SendsFormAndShowsFailureStatus', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response('could not start', { status: 500, statusText: 'Internal Server Error' }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new CgiViewModel();
    vm.selected.value = '/cgi-bin/greet.cgi';
    await vm.post.execute();
    expect(fetchMock.mock.calls[0][1].method).toBe('POST');
    expect(vm.status.value).toBe('500 Internal Server Error');
  });
  it('open_NothingSelected_CannotExecute', () => {
    expect(new CgiViewModel().open.canExecute.value).toBe(false);
  });
});

describe('webhook command', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('send_Execute_PostsBodyToChannelAndShowsStatus', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response('{"title":"Webhook rejected"}', { status: 400, statusText: 'Bad Request' }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new WebhooksViewModel();
    await vm.send.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/webhooks/email');
    expect(fetchMock.mock.calls[0][1].body).toBe('{ "event": "delivered" }');
    expect(vm.status.value).toBe('400 Bad Request');
    expect(vm.result.value).toContain('Webhook rejected');
  });
  it('send_EmptyChannel_CannotExecute', () => {
    const vm = new WebhooksViewModel();
    vm.channel.value = ' ';
    expect(vm.send.canExecute.value).toBe(false);
  });
});
