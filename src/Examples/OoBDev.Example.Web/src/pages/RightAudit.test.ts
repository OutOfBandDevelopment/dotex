import { describe, expect, it } from 'vitest';
import { ApiError } from '../core/http';
import { auditQuery, isNotConfigured } from './RightAuditViewModel';

describe('auditQuery', () => {
  it('auditQuery_EmptyFilters_OnlyMaxCount', () => expect(auditQuery(' ', '', 50)).toBe('maxCount=50'));
  it('auditQuery_Filters_AreTrimmedAndEncoded', () => expect(auditQuery(' grace ', 'Forbidden', 10)).toBe('subject=grace&result=Forbidden&maxCount=10'));
});

describe('isNotConfigured', () => {
  it('isNotConfigured_NotFound_True', () => expect(isNotConfigured(new ApiError({}, 404))).toBe(true));
  it('isNotConfigured_Forbidden_False', () => expect(isNotConfigured(new ApiError({}, 403))).toBe(false));
  it('isNotConfigured_OtherError_False', () => expect(isNotConfigured(new Error('x'))).toBe(false));
});
