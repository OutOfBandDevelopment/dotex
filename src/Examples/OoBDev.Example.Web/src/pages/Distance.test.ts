import { afterEach, describe, expect, it, vi } from 'vitest';
import { DistanceViewModel } from './DistanceViewModel';

const answer = { meters: 343600, kilometers: 343.6, miles: 213.5, bearingDegrees: 148.1 };
const reply = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status, headers: { 'content-type': 'application/json' } });

describe('distance command', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('measure_NonNumericCoordinate_CannotExecute', () => {
    const vm = new DistanceViewModel();
    vm.toLatitude.value = 'north';
    expect(vm.measure.canExecute.value).toBe(false);
  });
  it('measure_Coordinates_PostsNumbersAndKeepsTheResult', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => reply(answer));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new DistanceViewModel();
    await vm.measure.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Distance');
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string)).toEqual({ fromLatitude: 51.5074, fromLongitude: -0.1278, toLatitude: 48.8566, toLongitude: 2.3522 });
    expect(vm.result.value).toEqual(answer);
  });
  it('measure_BadRange_ShowsTheProblemText', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => reply({ title: 'Coordinates refused', detail: 'Latitude must be within -90..90', status: 400 }, 400)));
    const vm = new DistanceViewModel();
    await vm.measure.execute();
    expect(vm.measure.error.value).toContain('-90..90');
  });
});
