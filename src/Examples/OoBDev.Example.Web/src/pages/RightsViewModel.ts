import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface EndpointRight {
  method: string;
  route: string;
  access: 'anonymous' | 'authenticated' | 'rights';
  rights: string[];
}

export interface RightCheck {
  required: string[];
  held: string[];
  result: string;
}

export class RightsViewModel {
  readonly endpoints = ref<EndpointRight[]>([]);
  readonly filter = ref('');
  readonly rights = ref('rights.demo');
  readonly check = ref<RightCheck | null>(null);
  readonly load = new LoadEndpointsCommand(this);
  readonly run = new CheckRightsCommand(this);

  visible(): EndpointRight[] {
    const text = this.filter.value.trim().toLowerCase();
    return text === '' ? this.endpoints.value : this.endpoints.value.filter(e => e.route.toLowerCase().includes(text) || e.rights.some(r => r.toLowerCase().includes(text)));
  }
}

export class LoadEndpointsCommand extends Command {
  constructor(private readonly vm: RightsViewModel) { super(); }
  protected async run() {
    this.vm.endpoints.value = await http.getJson<EndpointRight[]>('/api/Rights/endpoints');
  }
}

export class CheckRightsCommand extends Command {
  constructor(private readonly vm: RightsViewModel) { super(); }
  protected isAllowed() { return this.vm.rights.value.trim() !== ''; }
  protected async run() {
    const names = this.vm.rights.value.split(/[\s,]+/).filter(n => n !== '');
    const query = names.map(n => `rights=${encodeURIComponent(n)}`).join('&');
    this.vm.check.value = await http.getJson<RightCheck>(`/api/Rights/check?${query}`);
  }
}
