import { afterEach, describe, expect, it, vi } from 'vitest';
import { CompletionViewModel } from './CompletionViewModel';
import { joinStreamedStrings } from './ChatViewModel';
import { isHealthy } from './OverviewViewModel';

afterEach(() => vi.unstubAllGlobals());

describe('CompletionViewModel', () => {
  it('sends the previous context on the second turn', async () => {
    const bodies: any[] = [];
    vi.stubGlobal('fetch', vi.fn(async (_p: string, init: RequestInit) => {
      bodies.push(JSON.parse(init.body as string));
      return new Response(JSON.stringify({ context: [1, 2, 3], response: 'ok' }), { status: 200 });
    }));
    const vm = new CompletionViewModel('/api/Ollama');
    vm.prompt.value = 'one';
    await vm.send.execute();
    vm.prompt.value = 'two';
    await vm.send.execute();
    expect(bodies[0].context).toBeNull();
    expect(bodies[1].context).toEqual([1, 2, 3]);
    expect(vm.turns.value).toHaveLength(4);
  });

  it('cannot send an empty prompt', () => {
    expect(new CompletionViewModel('/x').send.canExecute.value).toBe(false);
  });
});

describe('isHealthy', () => {
  it('treats 401 as healthy and 500 as not', () => {
    const p = { name: '', method: 'GET' as const, path: '', to: '', ms: 1 };
    expect(isHealthy({ ...p, status: 401 })).toBe(true);
    expect(isHealthy({ ...p, status: 500 })).toBe(false);
  });
});

describe('joinStreamedStrings', () => {
  it('joins complete and partial JSON arrays', () => {
    expect(joinStreamedStrings('["Hello","!"]')).toBe('Hello!');
    expect(joinStreamedStrings('["Hello",')).toBe('Hello');
    expect(joinStreamedStrings('["Hello","wor')).toBe('Hello');
    expect(joinStreamedStrings('[')).toBe('');
  });
});
