import { ref } from 'vue';
import { Command } from '../core/command';
import { auth } from '../core/auth';

export interface Probe { name: string; method: 'GET' | 'POST'; path: string; to: string; status: number | null; ms: number | null; }

/** One configured provider as reported by `GET api/Operations/health` (needs the right `operations.read`). */
export interface ProviderHealth { name: string; status: 'Healthy' | 'Degraded' | 'Unhealthy'; description?: string | null; durationMs: number; }

export const healthPath = '/api/Operations/health';

const probes = (): Probe[] => [
  { name: 'OpenAPI document', method: 'GET', path: '/openapi/all.json', to: '/developer/api' },
  { name: 'AsyncAPI document', method: 'GET', path: '/asyncapi/all.json', to: '/developer/events' },
  { name: 'Embeddings (MiniLM)', method: 'GET', path: '/api/AllMiniLM?text=ping', to: '/ai/embeddings' },
  { name: 'Templates', method: 'POST', path: '/api/TextTemplate/SupportedTemplates', to: '/content/templates' },
  { name: 'Vector store collections', method: 'GET', path: '/api/VectorStore/collections', to: '/utilities/vectors' },
  { name: 'Runtime and configuration', method: 'GET', path: '/api/Operations/info', to: '/operations/info' },
  { name: 'Endpoint rights', method: 'GET', path: '/api/Rights/endpoints', to: '/identity/rights' },
  { name: 'Current user', method: 'GET', path: '/api/User/claims', to: '/identity/me' },
].map(p => ({ ...p, method: p.method as 'GET' | 'POST', status: null, ms: null }));

/** Checks that each example endpoint answers (401 is healthy: the endpoint is secured) and lists the health of each configured provider. */
export class OverviewViewModel {
  readonly probes = ref<Probe[]>(probes());
  readonly providers = ref<ProviderHealth[]>([]);
  /** Why the provider list is empty, or null when it loaded. */
  readonly providersNote = ref<string | null>('Not checked yet.');
  readonly check = new CheckEndpointsCommand(this);
}

const headers = (method: string) => ({ ...(method === 'POST' ? { 'Content-Type': 'application/json' } : {}), ...(auth.accessToken ? { Authorization: `Bearer ${auth.accessToken}` } : {}) });

export class CheckEndpointsCommand extends Command {
  constructor(private readonly vm: OverviewViewModel) { super(); }
  protected async run() {
    await Promise.all([this.checkProbes(), this.loadProviders()]);
  }

  private checkProbes() {
    return Promise.all(this.vm.probes.value.map(async p => {
      const start = performance.now();
      try {
        const r = await fetch(p.path, { method: p.method, headers: headers(p.method) });
        p.status = r.status;
      } catch {
        p.status = 0;
      }
      p.ms = Math.round(performance.now() - start);
    }));
  }

  private async loadProviders() {
    try {
      const r = await fetch(healthPath, { headers: headers('GET') });
      if (r.status === 401 || r.status === 403) {
        this.vm.providers.value = [];
        this.vm.providersNote.value = 'Sign in with the right operations.read to see the providers.';
        return;
      }
      if (!r.ok) {
        this.vm.providers.value = [];
        this.vm.providersNote.value = `The health endpoint answered ${r.status}.`;
        return;
      }
      const body = await r.json() as { entries: ProviderHealth[] };
      this.vm.providers.value = body.entries;
      this.vm.providersNote.value = body.entries.length === 0 ? 'No provider is configured.' : null;
    } catch {
      this.vm.providers.value = [];
      this.vm.providersNote.value = 'The health endpoint is unreachable.';
    }
  }
}

export const isHealthy = (p: Probe) => p.status !== null && (p.status < 400 || p.status === 401);
