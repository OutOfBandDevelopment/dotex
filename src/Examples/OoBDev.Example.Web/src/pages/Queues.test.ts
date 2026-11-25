import { afterEach, describe, expect, it, vi } from 'vitest';
import { QueuesViewModel } from './QueuesViewModel';

describe('queues commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('enqueue_InvalidJson_CannotExecute', () => {
    const vm = new QueuesViewModel();
    vm.data.value = '{ nope';
    expect(vm.enqueue.canExecute.value).toBe(false);
  });
  it('enqueue_Valid_PostsToChosenQueue', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response('42', { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new QueuesViewModel();
    vm.queue.value = 'private';
    await vm.enqueue.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/MessageQueueing/private');
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string)).toEqual({ input: 'hello', data: { text: 'hello' } });
    expect(vm.result.value).toBe('Queued (message 42)');
  });
});

describe('queue monitor commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('refreshChannels_Execute_FillsChannels', async () => {
    const channels = [{ channel: 'A', provider: 'rabbit-mq', receiverEnabled: false }];
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify(channels), { status: 200 })));
    const vm = new QueuesViewModel();
    await vm.refreshChannels.execute();
    expect(vm.channels.value).toEqual(channels);
  });
  it('refreshReceived_Execute_FillsMessages', async () => {
    const messages = [{ receivedAt: '2026-11-09T12:00:00Z', message: 'x' }];
    const fetchMock = vi.fn(async (_p: string) => new Response(JSON.stringify(messages), { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new QueuesViewModel();
    await vm.refreshReceived.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/MessageQueueing/received');
    expect(vm.received.value).toEqual(messages);
  });
});
