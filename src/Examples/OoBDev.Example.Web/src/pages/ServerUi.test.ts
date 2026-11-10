import { afterEach, describe, expect, it, vi } from 'vitest';
import { ServerUiViewModel, type UiControl, type UiDefinitionInfo } from './ServerUiViewModel';

const info: UiDefinitionInfo = { key: 'bench-supply', name: 'Bench power supply', source: 'hand-built' };
const definition = {
  name: 'Bench power supply',
  sections: [
    { label: 'Output', controls: [{ kind: 'toggle', id: 'output', label: 'Output enabled' }] },
    { label: 'Limit', controls: [{ kind: 'button', id: 'reset', label: 'Reset limit', commandId: 'reset-limit' }] },
  ],
};
const states = [
  { id: 'output', visible: true, enabled: true, valid: true, value: 'false' },
  { id: 'reset', visible: false, enabled: true, valid: true, value: '' },
];

function stub(responses: Record<string, unknown>) {
  const fetchMock = vi.fn(async (path: string) => new Response(JSON.stringify(responses[path]), { status: 200 }));
  vi.stubGlobal('fetch', fetchMock);
  return fetchMock;
}

describe('server-driven ui commands', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('list_Execute_FillsDefinitions', async () => {
    stub({ '/api/UiDefinitions': [info] });
    const vm = new ServerUiViewModel();
    await vm.list.execute();
    expect(vm.definitions.value).toEqual([info]);
  });

  it('open_Execute_LoadsDefinitionAndEvaluatesDefaults', async () => {
    stub({ '/api/UiDefinitions/bench-supply': definition, '/api/UiDefinitions/bench-supply/evaluate': states });
    const vm = new ServerUiViewModel();
    await vm.open.execute(info);
    expect(vm.definition.value?.sections).toHaveLength(2);
    expect(vm.state('output')?.value).toBe('false');
    expect(vm.display('output')).toBe('false');
  });

  it('setValue_SendsValuesAndShowsServerState', async () => {
    const fetchMock = stub({ '/api/UiDefinitions/bench-supply': definition, '/api/UiDefinitions/bench-supply/evaluate': states });
    const vm = new ServerUiViewModel();
    await vm.open.execute(info);
    await vm.setValue('output', 'true');
    const last = fetchMock.mock.calls.at(-1) as unknown as [string, RequestInit];
    expect(JSON.parse(last[1].body as string)).toEqual({ values: { output: 'true' } });
    expect(vm.display('output')).toBe('true');
  });

  it('sectionVisible_AllControlsHidden_False', async () => {
    stub({ '/api/UiDefinitions/bench-supply': definition, '/api/UiDefinitions/bench-supply/evaluate': states });
    const vm = new ServerUiViewModel();
    await vm.open.execute(info);
    expect(vm.sectionVisible(vm.definition.value!.sections[0])).toBe(true);
    expect(vm.sectionVisible(vm.definition.value!.sections[1])).toBe(false);
  });

  it('evaluate_NothingOpen_CannotExecute', () => {
    expect(new ServerUiViewModel().evaluate.canExecute.value).toBe(false);
  });

  it('press_Button_RecordsTheCommand', async () => {
    const vm = new ServerUiViewModel();
    await vm.press.execute({ kind: 'button', id: 'reset', label: 'Reset', commandId: 'reset-limit' } as UiControl);
    expect(vm.lastAction.value).toBe('Button reset would send command reset-limit.');
  });

  it('open_Failure_SetsError', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response('{}', { status: 404 })));
    const vm = new ServerUiViewModel();
    await vm.open.execute(info);
    expect(vm.open.error.value).not.toBeNull();
    expect(vm.definition.value).toBeNull();
  });
});
