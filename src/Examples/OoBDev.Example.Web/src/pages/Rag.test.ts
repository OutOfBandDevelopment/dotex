import { afterEach, describe, expect, it, vi } from 'vitest';
import { RagViewModel, parsePassages } from './RagViewModel';

describe('parsePassages', () => {
  it('parsePassages_BlankLineSeparated_NamesInOrder', () => {
    expect(parsePassages('one\nstill one\n\n\ntwo\r\n\r\nthree')).toEqual([
      { id: 'p1', text: 'one\nstill one' },
      { id: 'p2', text: 'two' },
      { id: 'p3', text: 'three' },
    ]);
  });
  it('parsePassages_Empty_ReturnsNone', () => {
    expect(parsePassages('  \n\n ')).toEqual([]);
  });
});

describe('AskCommand', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('ask_NoQuestion_CannotExecute', () => {
    const vm = new RagViewModel();
    vm.question.value = ' ';
    expect(vm.ask.canExecute.value).toBe(false);
  });
  it('ask_Valid_PostsPassagesAndStoresResult', async () => {
    const fetchMock = vi.fn(async (_path: string, _init: RequestInit) =>
      new Response(JSON.stringify({ answer: 'Queues use adapters.', sources: [{ id: 'p3', score: 0.8, text: 't' }] }), { status: 200, headers: { 'Content-Type': 'application/json' } }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new RagViewModel();
    await vm.ask.execute();
    const body = JSON.parse(fetchMock.mock.calls[0][1].body as string);
    expect(body.passages).toHaveLength(4);
    expect(body.topK).toBe(2);
    expect(vm.result.value?.answer).toBe('Queues use adapters.');
  });
});
