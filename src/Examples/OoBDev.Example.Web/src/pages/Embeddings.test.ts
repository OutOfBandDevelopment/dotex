import { describe, expect, it } from 'vitest';
import { cosine } from './EmbeddingsViewModel';

describe('cosine', () => {
  it('cosine_IdenticalVectors_ReturnsOne', () => {
    expect(cosine([1, 2, 3], [1, 2, 3])).toBeCloseTo(1, 6);
  });
  it('cosine_OrthogonalVectors_ReturnsZero', () => {
    expect(cosine([1, 0], [0, 1])).toBe(0);
  });
  it('cosine_ZeroVector_ReturnsZero', () => {
    expect(cosine([0, 0], [1, 1])).toBe(0);
  });
});
