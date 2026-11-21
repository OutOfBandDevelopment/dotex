import { afterEach, describe, expect, it, vi } from 'vitest';
import { GeocodeViewModel } from './GeocodeViewModel';

const match = { matched: true, latitude: 38.8977, longitude: -77.0365, quality: 'Interpolated', normalizedAddress: '1600 PENNSYLVANIA AVE NW, WASHINGTON, DC, 20500' };
const reply = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status, headers: { 'content-type': 'application/json' } });

describe('geocode command', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('lookup_BlankStreet_CannotExecute', () => {
    const vm = new GeocodeViewModel();
    vm.line1.value = ' ';
    expect(vm.lookup.canExecute.value).toBe(false);
  });
  it('lookup_Address_PostsItAndKeepsTheResult', async () => {
    const fetchMock = vi.fn(async (_p: string, _i: RequestInit) => reply(match));
    vi.stubGlobal('fetch', fetchMock);
    const vm = new GeocodeViewModel();
    await vm.lookup.execute();
    expect(fetchMock.mock.calls[0][0]).toBe('/api/Geocode');
    expect(JSON.parse(fetchMock.mock.calls[0][1].body as string)).toEqual({ line1: '1600 Pennsylvania Ave NW', city: 'Washington', region: 'DC', postalCode: '20500' });
    expect(vm.result.value).toEqual(match);
  });
  it('lookup_Forbidden_ShowsTheProblemText', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => reply({ title: 'Forbidden', detail: 'Needs geocode.use', status: 403 }, 403)));
    const vm = new GeocodeViewModel();
    await vm.lookup.execute();
    expect(vm.lookup.error.value).toContain('geocode.use');
  });
});
