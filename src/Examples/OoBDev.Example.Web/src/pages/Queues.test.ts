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
