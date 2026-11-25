import { describe, expect, it } from 'vitest';
import { ToolCatalogViewModel, cliTools } from './ToolCatalogViewModel';

describe('ToolCatalogViewModel', () => {
  it('tools_NoFilter_ListsEveryTool', () => expect(new ToolCatalogViewModel().tools.value).toHaveLength(cliTools.length));
  it('tools_FilterByCommand_KeepsMatches', () => {
    const vm = new ToolCatalogViewModel();
    vm.filter.value = 'file-';
    expect(vm.tools.value.map(t => t.command)).toEqual(['file-convert', 'file-rag']);
  });
  it('tools_NoMatch_IsEmpty', () => {
    const vm = new ToolCatalogViewModel();
    vm.filter.value = 'zzz';
    expect(vm.tools.value).toEqual([]);
  });
  it('install_NotPackable_UsesDotnetRun', () =>
    expect(cliTools.find(t => t.command === 'migrationhelper')!.install).toContain('dotnet run'));
  it('commands_AreUnique', () => expect(new Set(cliTools.map(t => t.command)).size).toBe(cliTools.length));
});
