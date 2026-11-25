import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface GeocodeResult { matched: boolean; latitude: number | null; longitude: number | null; quality: string; normalizedAddress: string | null; }

export class GeocodeViewModel {
  readonly line1 = ref('1600 Pennsylvania Ave NW');
  readonly city = ref('Washington');
  readonly region = ref('DC');
  readonly postalCode = ref('20500');
  readonly result = ref<GeocodeResult | null>(null);
  readonly lookup = new GeocodeCommand(this);
}

export class GeocodeCommand extends Command {
  constructor(private readonly vm: GeocodeViewModel) { super(); }
  protected isAllowed() { return this.vm.line1.value.trim() !== ''; }
  protected async run() {
    this.vm.result.value = await http.postJson<GeocodeResult>('/api/Geocode', {
      line1: this.vm.line1.value, city: this.vm.city.value, region: this.vm.region.value, postalCode: this.vm.postalCode.value,
    });
  }
}
