import { afterEach, describe, expect, it, vi } from 'vitest';
import { DataLoadViewModel } from './DataLoadViewModel';

const preview = { read: 3, written: 2, replaced: 0, skipped: 1, errors: [], columns: ['id', 'name', 'city'], rows: [['1', 'Ann', 'Oslo'], ['2', 'Bo', 'Lima']] };
const reply = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status, headers: { 'content-type': 'application/json' } });

describe('data load command', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('load_BlankKeyColumn_CannotExecute', () => {
    const vm = new DataLoadViewModel();
    vm.keyColumn.value = ' ';
    expect(vm.load.canExecute.value).toBe(false);
  });
  it('load_Sample_PostsTheFormValuesAndKeepsThePreview', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => reply(preview));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new DataLoadViewModel();
    vm.duplicates.value = 'Replace';
    await vm.load.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/DataLoad/preview');
    const body = JSON.parse(fetchMock.mock.calls[0][1].body as string);
    expect(body).toMatchObject({ format: 'csv', keyColumn: 'id', duplicates: 'Replace' });
    expect(vm.preview.value).toEqual(preview);
  });
  it('load_Refused_ShowsTheProblemText', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => reply({ title: 'Load refused', detail: 'Name the key column.', status: 400 }, 400)));
    const vm = new DataLoadViewModel();
    await vm.load.execute();
    expect(vm.load.error.value).toContain('key column');
  });
});
