import { afterEach, describe, expect, it, vi } from 'vitest';
import { CommunicationsViewModel } from './CommunicationsViewModel';

describe('communications commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('send_NoRecipient_CannotExecute', () => {
    const vm = new CommunicationsViewModel();
    vm.to.value = ' ';
    expect(vm.send.canExecute.value).toBe(false);
  });
  it('send_Valid_PostsMessageToChosenChannel', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response('sent', { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new CommunicationsViewModel();
    vm.channel.value = 'queued';
    await vm.send.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Communications/queued');
    const body = JSON.parse(fetchMock.mock.calls[0][1].body as string);
    expect(body).toMatchObject({ toAddresses: ['someone@example.test'], subject: 'Hello', textContent: 'Message text' });
    expect(vm.result.value).toBe('sent');
  });
  it('poll_MailFetched_ListsNewestFirstAndMarksPolled', async () => {
    const batch = (id: string) => [{ messageId: id, from: 'a@example.test', to: ['b@example.test'], subject: id, preview: 'p', timestamp: '2026-11-20T00:00:00Z' }];
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response(JSON.stringify(batch('m2')), { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new CommunicationsViewModel();
    vm.inbox.value = batch('m1');
    await vm.poll.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Communications/inbox/poll?max=10');
    expect(vm.inbox.value.map(m => m.messageId)).toEqual(['m2', 'm1']);
    expect(vm.polled.value).toBe(true);
  });
});
