import { describe, expect, it } from 'vitest';
import { OverviewViewModel, isHealthy } from './OverviewViewModel';

describe('OverviewViewModel', () => {
  it('probes_Default_EachTargetsARoutedPage', () => {
    const vm = new OverviewViewModel();
    expect(vm.probes.value.length).toBeGreaterThanOrEqual(8);
    expect(new Set(vm.probes.value.map(p => p.path)).size).toBe(vm.probes.value.length);
  });
  it('isHealthy_UnauthorizedIsHealthy_ServerErrorAndUnansweredAreNot', () => {
    const p = new OverviewViewModel().probes.value[0];
    expect(isHealthy({ ...p, status: 401 })).toBe(true);
    expect(isHealthy({ ...p, status: 200 })).toBe(true);
    expect(isHealthy({ ...p, status: 500 })).toBe(false);
    expect(isHealthy({ ...p, status: null })).toBe(false);
  });
});
