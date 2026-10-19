import { ref } from 'vue';
import { Command } from '../core/command';

export interface Probe { name: string; method: 'GET' | 'POST'; path: string; to: string; status: number | null; ms: number | null; }

const probes = (): Probe[] => [
  { name: 'OpenAPI document', method: 'GET', path: '/openapi/all.json', to: '/developer/api' },
  { name: 'AsyncAPI document', method: 'GET', path: '/asyncapi/all.json', to: '/developer/events' },
  { name: 'Embeddings (MiniLM)', method: 'GET', path: '/api/AllMiniLM?text=ping', to: '/ai/embeddings' },
  { name: 'Templates', method: 'POST', path: '/api/TextTemplate/SupportedTemplates', to: '/content/templates' },
  { name: 'Current user', method: 'GET', path: '/api/User/claims', to: '/identity/me' },
].map(p => ({ ...p, method: p.method as 'GET' | 'POST', status: null, ms: null }));

/** Checks that each example endpoint answers; 401 is healthy (the endpoint is secured). */
export class OverviewViewModel {
  readonly probes = ref<Probe[]>(probes());
  readonly check = new CheckEndpointsCommand(this);
}

export class CheckEndpointsCommand extends Command {
  constructor(private readonly vm: OverviewViewModel) { super(); }
  protected async run() {
    await Promise.all(this.vm.probes.value.map(async p => {
      const start = performance.now();
      try {
        const r = await fetch(p.path, { method: p.method, headers: p.method === 'POST' ? { 'Content-Type': 'application/json' } : {} });
        p.status = r.status;
      } catch {
        p.status = 0;
      }
      p.ms = Math.round(performance.now() - start);
    }));
  }
}

export const isHealthy = (p: Probe) => p.status !== null && (p.status < 400 || p.status === 401);
