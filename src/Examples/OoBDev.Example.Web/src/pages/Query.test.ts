import { afterEach, describe, expect, it, vi } from 'vitest';
import { QueryViewModel } from './QueryViewModel';

const reply = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status, headers: { 'content-type': 'application/json' } });

describe('query command', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('queryPath_FilledOptions_EncodesThemAndAsksForTheCount', () => {
    const vm = new QueryViewModel();
    vm.filter.value = "Name eq 'a b'";
    expect(vm.queryPath()).toBe("/api/SampleCatalog?$count=true&$filter=Name%20eq%20'a%20b'&$orderby=Price%20desc&$top=5");
  });
  it('queryPath_EmptyFilter_LeavesItOut', () => {
    const vm = new QueryViewModel();
    vm.filter.value = ' ';
    vm.orderBy.value = '';
    expect(vm.queryPath()).toBe('/api/SampleCatalog?$count=true&$top=5');
  });
  it('run_BadTop_CannotExecute', () => {
    const vm = new QueryViewModel();
    vm.top.value = '0';
    expect(vm.run.canExecute.value).toBe(false);
  });
  it('run_ObjectWithCount_KeepsRowsAndTotal', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => reply({ '@odata.count': 7, value: [{ Id: 1, Name: 'x' }] })));
    const vm = new QueryViewModel();
    await vm.run.execute();
    expect(vm.rows.value).toEqual([{ Id: 1, Name: 'x' }]);
    expect(vm.total.value).toBe(7);
  });
  it('run_PlainArray_KeepsRowsWithoutTotal', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => reply([{ Id: 2 }])));
    const vm = new QueryViewModel();
    await vm.run.execute();
    expect(vm.rows.value).toEqual([{ Id: 2 }]);
    expect(vm.total.value).toBeNull();
  });
  it('run_BadQuery_ShowsTheProblemText', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => reply({ title: 'Bad query', detail: 'Could not find a property named Nope', status: 400 }, 400)));
    const vm = new QueryViewModel();
    await vm.run.execute();
    expect(vm.run.error.value).toContain('Nope');
  });
});
