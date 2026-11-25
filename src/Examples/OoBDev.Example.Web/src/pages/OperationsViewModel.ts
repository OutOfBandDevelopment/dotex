import { computed, ref } from 'vue';
import { Command } from '../core/command';
import { api } from '../core/api';

export interface ConfigurationEntry { key: string; value?: string | null; }

/** Entries whose key or value contains the filter text (case-insensitive). */
export function filterEntries<T extends ConfigurationEntry>(entries: T[], filter: string): T[] {
  const f = filter.trim().toLowerCase();
  return f === '' ? entries : entries.filter(e => e.key.toLowerCase().includes(f) || (e.value ?? '').toLowerCase().includes(f));
}

export class OperationsViewModel {
  readonly info = ref<Awaited<ReturnType<typeof loadInfo>> | null>(null);
  readonly filter = ref('');
  readonly entries = computed(() => filterEntries(this.info.value?.configuration ?? [], this.filter.value));
  readonly refresh = new RefreshCommand(this);
}

const loadInfo = () => api.get('/api/Operations/info');

export class RefreshCommand extends Command {
  constructor(private readonly vm: OperationsViewModel) { super(); }
  protected async run() {
    this.vm.info.value = await loadInfo();
  }
}
