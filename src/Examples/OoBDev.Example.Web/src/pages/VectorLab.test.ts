import { afterEach, describe, expect, it, vi } from 'vitest';
import { VectorLabViewModel, parseRows } from './VectorLabViewModel';

describe('parseRows', () => {
  it('parseRows_CommasAndSpaces_ReturnsRows', () => expect(parseRows('1, 2\n3 4')).toEqual([[1, 2], [3, 4]]));
  it('parseRows_UnevenLengths_ReturnsNull', () => expect(parseRows('1,2\n3')).toBeNull());
  it('parseRows_NotNumeric_ReturnsNull', () => expect(parseRows('a,b')).toBeNull());
  it('parseRows_Empty_ReturnsNull', () => expect(parseRows('  ')).toBeNull());
});

describe('VectorLabViewModel', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('compute_BadInput_CannotExecute', () => {
    const vm = new VectorLabViewModel();
    vm.rows.value = '1,2\n3';
    expect(vm.compute.canExecute.value).toBe(false);
  });
  it('compute_Valid_PostsVectorsAndMetric', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) =>
      new Response(JSON.stringify({ matrix: [[0]], magnitudes: [1], centroid: [1] }), { status: 200, headers: { 'Content-Type': 'application/json' } }));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new VectorLabViewModel();
    vm.metric.value = 'Euclidean';
    await vm.compute.execute();
    const body = JSON.parse(fetchMock.mock.calls[0][1].body as string);
    expect(body.metric).toBe('Euclidean');
    expect(body.vectors).toHaveLength(4);
    expect(vm.result.value?.centroid).toEqual([1]);
  });
});
