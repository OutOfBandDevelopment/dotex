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
});
