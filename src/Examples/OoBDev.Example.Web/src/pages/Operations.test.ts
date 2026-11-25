import { describe, expect, it } from 'vitest';
import { filterEntries } from './OperationsViewModel';

const entries = [{ key: 'Logging:LogLevel', value: 'Debug' }, { key: 'Jwt:Secret', value: '***' }, { key: 'Mail:Host', value: 'smtp' }];

describe('filterEntries', () => {
  it('filterEntries_Empty_ReturnsAll', () => expect(filterEntries(entries, ' ')).toHaveLength(3));
  it('filterEntries_MatchesKeyOrValueIgnoringCase', () => {
    expect(filterEntries(entries, 'LOGGING').map(e => e.key)).toEqual(['Logging:LogLevel']);
    expect(filterEntries(entries, 'smtp').map(e => e.key)).toEqual(['Mail:Host']);
  });
});
