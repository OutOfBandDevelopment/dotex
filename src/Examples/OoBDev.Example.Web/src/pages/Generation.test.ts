import { afterEach, describe, expect, it, vi } from 'vitest';
import { GenerationViewModel } from './GenerationViewModel';

const answer = { sampleType: 'customer', seed: 42, value: { Person: { Name: 'Ada' } } };
const reply = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status, headers: { 'content-type': 'application/json' } });

describe('generate command', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('generate_NonIntegerSeed_CannotExecute', () => {
    const vm = new GenerationViewModel();
    vm.seed.value = 'abc';
    expect(vm.generate.canExecute.value).toBe(false);
  });
  it('generate_TypeAndSeed_PostsThemAndKeepsTheResult', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => reply(answer));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new GenerationViewModel();
    await vm.generate.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Generation');
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string)).toEqual({ sampleType: 'customer', seed: 42 });
    expect(vm.result.value).toEqual(answer);
  });
  it('generate_UnknownType_ShowsTheProblemText', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => reply({ title: 'Sample type refused', detail: 'Unknown sample type', status: 400 }, 400)));
    const vm = new GenerationViewModel();
    await vm.generate.execute();
    expect(vm.generate.error.value).toContain('Unknown sample type');
  });
});
