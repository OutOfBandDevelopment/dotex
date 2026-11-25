import { afterEach, describe, expect, it, vi } from 'vitest';
import { DataAccessViewModel } from './DataAccessViewModel';

describe('data access command', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('run_EmptyText_CannotExecute', () => {
    const vm = new DataAccessViewModel();
    vm.sql.value = '  ';
    expect(vm.run.canExecute.value).toBe(false);
  });
  it('run_Select_PostsTheTextAndKeepsTheResult', async () => {
    const body = { columns: ['n'], rows: [['1']], truncated: false, elapsedMs: 3 };
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => new Response(JSON.stringify(body), { status: 200, headers: { 'content-type': 'application/json' } }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new DataAccessViewModel();
    vm.sql.value = 'SELECT 1 AS n';
    await vm.run.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/DataAccess/query');
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string)).toEqual({ sql: 'SELECT 1 AS n' });
    expect(vm.result.value).toEqual(body);
  });
  it('run_Refused_ShowsTheProblemText', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify({ title: 'Query refused', detail: 'Only a SELECT statement is allowed.', status: 400 }), { status: 400, headers: { 'content-type': 'application/problem+json' } })));
    const vm = new DataAccessViewModel();
    await vm.run.execute();
    expect(vm.run.error.value).toContain('Only a SELECT');
  });
});
